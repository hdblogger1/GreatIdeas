using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_05L26 : GameWindow
    {
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        float[] _vertices = new float[] 
        {
            // back
            -0.25f, 0.25f,-0.25f,   -0.25f,-0.25f,-0.25f,    0.25f,-0.25f,-0.25f,
             0.25f,-0.25f,-0.25f,    0.25f, 0.25f,-0.25f,   -0.25f, 0.25f,-0.25f,
            // right
             0.25f,-0.25f,-0.25f,    0.25f,-0.25f, 0.25f,    0.25f, 0.25f,-0.25f,
             0.25f,-0.25f, 0.25f,    0.25f, 0.25f, 0.25f,    0.25f, 0.25f,-0.25f,
            // front
             0.25f,-0.25f, 0.25f,   -0.25f,-0.25f, 0.25f,    0.25f, 0.25f, 0.25f,
            -0.25f,-0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,    0.25f, 0.25f, 0.25f,
            // left
            -0.25f,-0.25f, 0.25f,   -0.25f,-0.25f,-0.25f,   -0.25f, 0.25f, 0.25f,
            -0.25f,-0.25f,-0.25f,   -0.25f, 0.25f,-0.25f,   -0.25f, 0.25f, 0.25f,
            // bottom
            -0.25f,-0.25f, 0.25f,    0.25f,-0.25f, 0.25f,    0.25f,-0.25f,-0.25f,
             0.25f,-0.25f,-0.25f,   -0.25f,-0.25f,-0.25f,   -0.25f,-0.25f, 0.25f,
            // top
            -0.25f, 0.25f,-0.25f,    0.25f, 0.25f,-0.25f,    0.25f, 0.25f, 0.25f,
             0.25f, 0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,   -0.25f, 0.25f,-0.25f
        };

        int _renderingProgramHandle;
        int _vaoHandle;

        int _buffer;
        
        int _mvLocation, _projLocation;
        Matrix4 _projMatrix;

        //-----------------------------------------------------------------------------------------
        public Example_05L26() 
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

                in vec4 position;

                out VS_OUT
                {
                    vec4 color;
                } vs_out;

                uniform mat4 mv_matrix;
                uniform mat4 proj_matrix;

                void main(void)
                {
                    gl_Position  = proj_matrix * mv_matrix * position;
                    vs_out.color = position * 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
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
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(50.0f), aspect, 0.1f, 1000.0f);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad (EventArgs e)
        {
            _renderingProgramHandle = CompileShaders();
                
            //Create VAO object to hold vertex shader inputs and attach it to our context
            GL.GenVertexArrays(1, out _vaoHandle);
            GL.BindVertexArray(_vaoHandle);

            _mvLocation = GL.GetUniformLocation( _renderingProgramHandle, "mv_matrix" );
            _projLocation = GL.GetUniformLocation( _renderingProgramHandle, "proj_matrix" );

            // Now generate some data and put it in a buffer object
            GL.GenBuffers(1, out _buffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * sizeof(float)), 
                          _vertices, BufferUsageHint.StaticDraw);
            
            // Set up our vertex attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray(0);

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

            // Set the projection matrices
            GL.UniformMatrix4( _projLocation, false, ref _projMatrix );

            Matrix4 mvMatrix;
            float f;
            
            for( int i=0; i<24; i++ )
            {
                // Calculate a new model-view matrix for each object
                f = (float)i + (elapsedSeconds * 0.3f);
                mvMatrix = Matrix4.CreateTranslation( (float)(Math.Sin(2.1f * f) * 2.0f), 
                                                      (float)(Math.Cos(1.7f * f) * 2.0f),
                                                      (float)(Math.Sin(1.3f * f) * Math.Cos(1.5f * f) * 2.0f));
                mvMatrix *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), 
                                                        elapsedSeconds * MathHelper.DegreesToRadians(21.0f) );
                mvMatrix *= Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), 
                                                        elapsedSeconds * MathHelper.DegreesToRadians(45.0f) );
                mvMatrix *= Matrix4.CreateTranslation( 0.0f, 0.0f, -6.0f );

                // Update the uniform
                GL.UniformMatrix4( _mvLocation, false, ref mvMatrix );

                // Draw - notice that we haven’t updated the projection matrix
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
            SwapBuffers();
        }
    }
}
