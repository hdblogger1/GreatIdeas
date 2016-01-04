using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_03L07_03L08 : GameWindow
    {
        
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        int _renderingProgramHandle;
        int _vaoHandle;

        //-----------------------------------------------------------------------------------------
        public Example_03L07_03L08() 
            : base( 640, 480, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
                    // ask for an OpenGL 4.3 or higher default(core?) context
                    , 4, 3, GraphicsContextFlags.Default)
        {
        }

        //-----------------------------------------------------------------------------------------
        public int CompileShaders()
        {
            int vertexShaderHandle, fragmentShaderHandle;
            int tessCtrlShaderHandle, tessEvalShaderHandle;
            int shaderProgramHandle;
                
            //Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core

                void main(void)
                {
                    // Declare a hard-coded array of positions
                    const vec4 vertices[3] = vec4[3](vec4( 0.25, -0.25, 0.5, 1.0),
                                                     vec4(-0.25, -0.25, 0.5, 1.0),
                                                     vec4( 0.25,  0.25, 0.5, 1.0));
                    
                    gl_Position = vertices[gl_VertexID];
                }
                ";

            //Source code for tessellation control shader
            string tessCtrlShaderSource = @"
                #version 430 core
                
                layout (vertices = 3) out;
                
                void main(void)
                {
                    if(gl_InvocationID == 0)
                    {
                        gl_TessLevelInner[0] = 5.0;
                        gl_TessLevelOuter[0] = 5.0;
                        gl_TessLevelOuter[1] = 5.0;
                        gl_TessLevelOuter[2] = 5.0;
                    }
                    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
                }
            ";

            //Source code for tessellation control shader
            string tessEvalShaderSource = @"
                #version 430 core
                
                layout (triangles, equal_spacing, cw) in;
                
                void main(void)
                {
                    gl_Position = (gl_TessCoord.x * gl_in[0].gl_Position +
                                   gl_TessCoord.y * gl_in[1].gl_Position +
                                   gl_TessCoord.z * gl_in[2].gl_Position);
                }
            ";

            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                
                //Output to the framebuffer
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

            //Create and compile tessellation control shader
            tessCtrlShaderHandle = GL.CreateShader(ShaderType.TessControlShader);
            GL.ShaderSource( tessCtrlShaderHandle, tessCtrlShaderSource );
            GL.CompileShader( tessCtrlShaderHandle );

            //Create and compile tessellation evaluation shader
            tessEvalShaderHandle = GL.CreateShader(ShaderType.TessEvaluationShader);
            GL.ShaderSource( tessEvalShaderHandle, tessEvalShaderSource );
            GL.CompileShader( tessEvalShaderHandle );

            //Create and compile fragment shader
            fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShaderHandle, fragmentShaderSource );
            GL.CompileShader( fragmentShaderHandle );

            //Create program, attach shaders to it, and link it
            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader( shaderProgramHandle, vertexShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            GL.AttachShader( shaderProgramHandle, tessCtrlShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(tessCtrlShaderHandle));
            GL.AttachShader( shaderProgramHandle, tessEvalShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(tessEvalShaderHandle));
            GL.AttachShader( shaderProgramHandle, fragmentShaderHandle );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));
            GL.LinkProgram( shaderProgramHandle );

            //Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderHandle );
            GL.DeleteShader( tessCtrlShaderHandle );
            GL.DeleteShader( tessEvalShaderHandle );
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
            
            //Red background color
            _color[0] = 0.25f;
            _color[1] = 0.0f;

            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, _color);
 
            //Use the program object we created earlier for rendering
            GL.UseProgram(_renderingProgramHandle);

            //Tell OpenGL to draw outlines only
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line );

            //Draw patches
            GL.DrawArrays(PrimitiveType.Patches, 0, 3);
            
            SwapBuffers();
        }
    }
}
