using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_03L01_03L02 : GameWindow
    {
        
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        int _renderingProgramHandle;
        int _vaoHandle;

        //-----------------------------------------------------------------------------------------
        public Example_03L01_03L02() 
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

                // 'offset' is an input vertex attribute
                layout (location = 0) in vec4 offset;

                void main(void)
                {
                    // Declare a hard-coded array of positions
                    const vec4 vertices[3] = vec4[3](vec4( 0.25, -0.25, 0.5, 1.0),
                                                     vec4(-0.25, -0.25, 0.5, 1.0),
                                                     vec4( 0.25,  0.25, 0.5, 1.0));
                    // Add 'offset' to our hard-coded vertex position
                    gl_Position = vertices[gl_VertexID] + offset;
                }
                ";
                
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                out vec4 color;

                void main(void)
                {
                    color = vec4(0.0, 0.8, 1.0, 1.0);
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
            //Get elapsed time since application startup
            double elapsedSeconds = Program.Counter.ElapsedMilliseconds / 1000.0;
            
            //Animate color
            _color[0] = (float) (Math.Sin(elapsedSeconds) * 0.5f + 0.5f);
            _color[1] = (float) (Math.Cos(elapsedSeconds) * 0.5f + 0.5f);
          
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            //Use the program object we created earlier for rendering
            GL.UseProgram(_renderingProgramHandle);

            float[] attrib = new float[4] { (float)(Math.Sin(elapsedSeconds) * 0.5f),
                                            (float)(Math.Cos(elapsedSeconds) * 0.6f),
                                            0.0f, 0.0f };
            
            //Update the value of input attribute 0
            GL.VertexAttrib4(0, attrib);

            //Draw one triangle
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            
            SwapBuffers();
        }
    }
}
