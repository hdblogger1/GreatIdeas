using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_05L08 : GameWindow
    {
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        int _renderingProgramHandle;
        int _vaoHandle;

        int _buffer;

        struct Vertex
        {
            public float x, y, z; // Position
            public float r, g, b; // Color

            public const int Size = 6 * sizeof(float);
            public const int ColorOffset = 3 * sizeof(float);
        };

        //-----------------------------------------------------------------------------------------
        public Example_05L08() 
            : base( 640, 480, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
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

                layout (location = 0) in vec3 position;
                layout (location = 1) in vec3 color;

                out vec4 vs_color;

                void main(void)
                {
                    gl_Position = vec4(position.x, position.y, position.z, 1.0);
                    vs_color = vec4(color.r, color.g, color.b, 1.0 );
                }
                ";
                
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                
                in vec4 vs_color;

                out vec4 color;

                void main(void)
                {
                    color = vs_color;
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
        protected override void OnLoad (EventArgs e)
        {
            _renderingProgramHandle = CompileShaders();
                
            //Create VAO object to hold vertex shader inputs and attach it to our context
            GL.GenVertexArrays(1, out _vaoHandle);
            GL.BindVertexArray(_vaoHandle);

            Vertex[] vertices = new Vertex[] {
                          /* --------------------------   ---------------------- */
                          /*         positions                    colors         */
                          /* --------------------------   ---------------------- */
               new Vertex {  x= 0.25f, y=-0.25f, z=0.5f,  r=1.0f, g=0.0f, b=0.0f },
               new Vertex {  x=-0.25f, y=-0.25f, z=0.5f,  r=0.0f, g=1.0f, b=0.0f },
               new Vertex {  x= 0.25f, y= 0.25f, z=0.5f,  r=0.0f, g=0.0f, b=1.0f }
            };
           
            // Allocate and initialize a buffer object
            GL.GenBuffers(1, out _buffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vertex.Size), vertices, BufferUsageHint.StaticDraw);
            
            // Set up two vertex attributes - first positions
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Size, 0 );
            GL.EnableVertexAttribArray(0);

            // Now colors
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.Size, Vertex.ColorOffset );
            GL.EnableVertexAttribArray(1);
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
            //Set color to green
            _color[0] = 0.0f;
            _color[1] = 0.2f;
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            //Use the program object we created earlier for rendering
            GL.UseProgram(_renderingProgramHandle);

            //Draw one triangle
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            
            SwapBuffers();
        }
    }
}
