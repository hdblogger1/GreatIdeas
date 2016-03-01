using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    class Example_05L27_05L28 : GameWindow
    {
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        int _renderingProgramHandle;
        int _vaoHandle;

        Matrix4 _projMatrix;

        int _transformHandle;

        float[] _vertices = new float[] 
        {
            // back
            -0.25f, 0.25f,-0.25f,1,   -0.25f,-0.25f,-0.25f,1,    0.25f,-0.25f,-0.25f,1,
             0.25f,-0.25f,-0.25f,1,    0.25f, 0.25f,-0.25f,1,   -0.25f, 0.25f,-0.25f,1,
            // right
             0.25f,-0.25f,-0.25f,1,    0.25f,-0.25f, 0.25f,1,    0.25f, 0.25f,-0.25f,1,
             0.25f,-0.25f, 0.25f,1,    0.25f, 0.25f, 0.25f,1,    0.25f, 0.25f,-0.25f,1,
            // front
             0.25f,-0.25f, 0.25f,1,   -0.25f,-0.25f, 0.25f,1,    0.25f, 0.25f, 0.25f,1,
            -0.25f,-0.25f, 0.25f,1,   -0.25f, 0.25f, 0.25f,1,    0.25f, 0.25f, 0.25f,1,
            // left
            -0.25f,-0.25f, 0.25f,1,   -0.25f,-0.25f,-0.25f,1,   -0.25f, 0.25f, 0.25f,1,
            -0.25f,-0.25f,-0.25f,1,   -0.25f, 0.25f,-0.25f,1,   -0.25f, 0.25f, 0.25f,1,
            // bottom
            -0.25f,-0.25f, 0.25f,1,    0.25f,-0.25f, 0.25f,1,    0.25f,-0.25f,-0.25f,1,
             0.25f,-0.25f,-0.25f,1,   -0.25f,-0.25f,-0.25f,1,   -0.25f,-0.25f, 0.25f,1,
            // top
            -0.25f, 0.25f,-0.25f,1,    0.25f, 0.25f,-0.25f,1,    0.25f, 0.25f, 0.25f,1,
             0.25f, 0.25f, 0.25f,1,   -0.25f, 0.25f, 0.25f,1,   -0.25f, 0.25f,-0.25f,1
        };

        //-----------------------------------------------------------------------------------------
        public Example_05L27_05L28() 
            : base( 800, 600, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
                    // ask for an OpenGL 4.3 or higher default(core?) context
                    , 4, 3, GraphicsContextFlags.Default)
        {
        }

        //-----------------------------------------------------------------------------------------
        public int CompileShaders()
        {
            int vertexShaderHandle, fragmentShaderHandle;
            int shaderProgramHandle;
                
            //Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core

                struct vertex
                {
                    vec4 position;
                    vec3 color;
                };

                layout(binding=0, std430) buffer my_vertices
                {
                    vertex vertices[];
                };

                uniform mat4 transform_matrix;

                out VS_OUT
                {
                    vec3 color;
                } vs_out;

                void main(void)
                {
                    gl_Position  = transform_matrix * vertices[gl_VertexID].position;
                    vs_out.color = vertices[gl_VertexID].color;
                }
                ";
                
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                out vec4 color;

                in VS_OUT
                {
                    vec4 color;
                } fs_in;

                void main(void)
                {
                    color = fs_in.color;
                }
                ";

            //Create and compile vertex shader
            vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource( vertexShaderHandle, vertexShaderSource );
            GL.CompileShader( vertexShaderHandle );

            //Create and compile fragment shader
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShaderHandle, fragmentShaderSource );
            GL.CompileShader( fragmentShaderHandle );

            //Create program, attach shaders to it, and link it
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader( shaderProgramHandle, vertexShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            GL.AttachShader( shaderProgramHandle, fragmentShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));
            GL.LinkProgram( shaderProgramHandle );

            //Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderHandle );
            GL.DeleteShader( fragmentShaderHandle );

            return shaderProgramHandle;
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(50.0f), aspect, 0.1f, 1000.0f);
        }

        //-----------------------------------------------------------------------------------------
        private int initTransform()
        {
            int transformHandle;

            transformHandle = GL.GetUniformLocation( _renderingProgramHandle, "transform_matrix" );

            return transformHandle;
        }

        //-----------------------------------------------------------------------------------------
        private void initVertices()
        {
            int verticesHandle;


            GL.GenBuffers(1, out verticesHandle);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, verticesHandle);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, (IntPtr)(1024 * sizeof(float)),
                          IntPtr.Zero, BufferUsageHint.StaticDraw );

            int bbpIndex = 0;
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bbpIndex, verticesHandle);

            float[] color2 = new float[3];
            
            int bufferOffset;
            for(int vertexNum = 0; vertexNum < 36; vertexNum++)
            {
                bufferOffset = vertexNum * (8 * sizeof(float));
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)bufferOffset, 
                                 (IntPtr)(4*sizeof(float)), ref _vertices[vertexNum*4] );

                color2[0] = _vertices[(vertexNum*4)+0] * 2.0f + 0.5f;
                color2[1] = _vertices[(vertexNum*4)+1] * 2.0f + 0.5f;
                color2[2] = _vertices[(vertexNum*4)+2] * 2.0f + 0.5f;
                
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, (IntPtr)(bufferOffset + (4 * sizeof(float))), 
                                 (IntPtr)(3*sizeof(float)), color2 );

            }
          
            //IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer,BufferAccess.ReadWrite);
            //float[] managedArray = new float[100];
            //Marshal.Copy(ptr,managedArray,0,100);
            //GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);

       }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad (EventArgs e)
        {
            _renderingProgramHandle = CompileShaders();
                
            //Create VAO object to hold vertex shader inputs and attach it to our context
            GL.GenVertexArrays(1, out _vaoHandle);
            GL.BindVertexArray(_vaoHandle);

            this.initVertices();
            _transformHandle = this.initTransform();

            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteVertexArrays(1, ref _vaoHandle);
            GL.DeleteProgram(_renderingProgramHandle);
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Get elapsed time since application startup
            float elapsedSeconds = (float)(Program.ElapsedTimeSeconds);

            // Set color to green
            _color[0] = 0.0f;
            _color[1] = 0.2f;
            
            // Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);

            // Use the program object we created earlier for rendering
            GL.UseProgram(_renderingProgramHandle);

            Matrix4 transformMatrix;
            float f = elapsedSeconds * 0.3f;
            transformMatrix = Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), 
                                                   elapsedSeconds * MathHelper.DegreesToRadians(81.0f) );
            transformMatrix *= Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), 
                                                    elapsedSeconds * MathHelper.DegreesToRadians(45.0f) );
            transformMatrix *= Matrix4.CreateTranslation( (float)(Math.Sin(2.1f * f) * 0.5f), 
                                                   (float)(Math.Cos(1.7f * f) * 0.5f),
                                                   (float)(Math.Sin(1.3f * f) * Math.Cos(1.5f * f) * 2.0f));
            transformMatrix *= Matrix4.CreateTranslation( 0.0f, 0.0f, -4.0f );
            transformMatrix *= Matrix4.CreateScale( 1.0f, 1.0f, 1.0f );
            transformMatrix *= _projMatrix;

            // Set the transformation matrix
            GL.UniformMatrix4( _transformHandle, false, ref transformMatrix );

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line );

            // Draw 6 faces of 2 triangles of 3 vertices each = 36 vertices
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            
            SwapBuffers();
        }
    }
}
