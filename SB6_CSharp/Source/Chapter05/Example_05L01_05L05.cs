using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    class Example_05L01_05L05 : GameWindow
    {
        float[] _color = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };

        int _renderingProgramHandle;
        int _vaoHandle;

        int _buffer;

        //-----------------------------------------------------------------------------------------
        public Example_05L01_05L05() 
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

                layout (location = 0) in vec4 position;

                void main(void)
                {
                    gl_Position = position;
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

            //Generate a name for the buffer
            GL.GenBuffers(1, out _buffer);

            //Now bind it to the context using the GL_ARRAY_BUFFER binding point
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            //Specify the amount of storage we want to use for the buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(1024 * 1024), IntPtr.Zero, BufferUsageHint.StaticDraw);

            //This is the data that we will place into the buffer object
            float[] data = new float[] { 0.25f, -0.25f, 0.5f, 1.0f,
                                        -0.25f, -0.25f, 0.5f, 1.0f,
                                         0.25f,  0.25f, 0.5f, 1.0f };
            
            // *** set useAlternateCode to false to use alternate method to copy data to buffer object ***
            bool useAlternateCode = true;
            if( useAlternateCode )
            {
                //LISTING 5.2
                //Put the data into the buffer at offset zero
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(data.Length * sizeof(float)), data );
            }
            else
            {
                //LISTING 5.3
                //Get a pointer to the buffer’s data store
                IntPtr ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly );

                //Copy our data into it...
                unsafe
                {
                    fixed ( float* src = &data[0] )
                    {
                        float* dst = (float*) ptr.ToPointer();
                        for( int i=0; i<data.Length; i++ ) { dst[i] = src[i]; }
                    }
                }
                
                //Tell OpenGL that we’re done with the pointer
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            }

            //Now, describe the data to OpenGL, tell it where it is, and turn on automatic vertex 
            // fetching for the specified attribute
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray(0);
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
