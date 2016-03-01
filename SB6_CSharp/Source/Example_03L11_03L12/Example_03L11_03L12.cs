using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_03L11_03L12 : GameWindow
    {
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        int _renderingProgramHandle;
        int _vaoHandle;

        //-----------------------------------------------------------------------------------------
        public Example_03L11_03L12() 
            : base( 640, 480, GraphicsMode.Default, "OpenGL SuperBible - Simple Triangle", 0, DisplayDevice.Default
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

                // 'vs_color' is an output that will be sent to the next shader stage
                out vec4 vs_color;

                void main(void)
                {
                    const vec4 vertices[3] = vec4[3](vec4( 0.25, -0.25, 0.5, 1.0),
                                                     vec4(-0.25, -0.25, 0.5, 1.0),
                                                     vec4( 0.25,  0.25, 0.5, 1.0));
                    const vec4 colors[] = vec4[3](vec4(1.0, 0.0, 0.0, 1.0),
                                                  vec4(0.0, 1.0, 0.0, 1.0),
                                                  vec4(0.0, 0.0, 1.0, 1.0));                    

                    // Index into our array using gl_VertexID
                    gl_Position = vertices[gl_VertexID];
                    
                    // Output an indexed value for vs_color
                    vs_color = colors[gl_VertexID];
                }
                ";
                
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                //'vs_color' is the color produced by the vertex shader
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
