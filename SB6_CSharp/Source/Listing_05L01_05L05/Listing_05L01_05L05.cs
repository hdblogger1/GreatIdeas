using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_05L01_05L05 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.25f, 0.0f, 1.0f };
        }
        
        private int _shaderProgramName;
        private int _vertexArrayName;
        private int _arrayBufferName;

        //-----------------------------------------------------------------------------------------
        public Listing_05L01_05L05() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 5.1 thru 5.5", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShaderName, fragmentShaderName;
                
            // Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core

                layout (location = 0) in vec4 position;

                void main(void)
                {
                    gl_Position = position;
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                out vec4 color;

                void main(void)
                {
                    color = vec4(0.0, 0.8, 1.0, 1.0);
                }
                ";

            // Create and compile vertex shader
            vertexShaderName = GL.CreateShader( ShaderType.VertexShader );
            GL.ShaderSource( vertexShaderName, vertexShaderSource );
            GL.CompileShader( vertexShaderName );

            // Create and compile fragment shader
            fragmentShaderName = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( fragmentShaderName, fragmentShaderSource );
            GL.CompileShader( fragmentShaderName );

            // Create program, attach shaders to it, and link it
            _shaderProgramName = GL.CreateProgram();
            GL.AttachShader( _shaderProgramName, vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _shaderProgramName, fragmentShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShaderName ) );
            GL.LinkProgram( _shaderProgramName );

            // Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShaderName );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            // Generate a name for the buffer
            GL.GenBuffers( 1, out _arrayBufferName );

            // Now bind it to the context using the GL_ARRAY_BUFFER binding point
            GL.BindBuffer( BufferTarget.ArrayBuffer, _arrayBufferName );

            // Specify the amount of storage we want to use for the buffer
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(1024 * 1024), 
                           IntPtr.Zero, BufferUsageHint.StaticDraw );

            // This is the data that we will place into the buffer object
            float[] data = new float[] { 0.25f, -0.25f, 0.5f, 1.0f,
                                        -0.25f, -0.25f, 0.5f, 1.0f,
                                         0.25f,  0.25f, 0.5f, 1.0f };
            
            // *** set useAlternateCode to false to use alternate method to copy data to buffer object ***
            bool useAlternateCode = true;
            if( useAlternateCode )
            {
                // LISTING 5.2
                // Put the data into the buffer at offset zero
                GL.BufferSubData( BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(data.Length * sizeof(float)), data );
            }
            else
            {
                // LISTING 5.3
                // Get a pointer to the buffer’s data store
                IntPtr ptr = GL.MapBuffer( BufferTarget.ArrayBuffer, BufferAccess.WriteOnly );

                // Copy our data into it...
                unsafe
                {
                    fixed ( float* src = &data[0] )
                    {
                        float* dst = (float*) ptr.ToPointer();
                        for( int i=0; i<data.Length; i++ ) { dst[i] = src[i]; }
                    }
                }
                
                // Tell OpenGL that we’re done with the pointer
                GL.UnmapBuffer( BufferTarget.ArrayBuffer );
            }

            // Unbind the array buffer from the context
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As our
            // shader dosn't have any inputs, nothing else needs to be done with them, but OpenGL
            // still requires the VAO object to be created before drawing is allowed.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );

            // Bind our array buffer that we created earlier to the context using the 
            // GL_ARRAY_BUFFER binding point
            GL.BindBuffer( BufferTarget.ArrayBuffer, _arrayBufferName );
            
            // Now, describe the data to OpenGL, tell it where it is, and turn on automatic vertex 
            // fetching for the specified attribute. Note: This can't be done untill after the 
            // call to GL.BindVertexArray() above.
            GL.VertexAttribPointer( 0, 4, VertexAttribPointerType.Float, false, 0, 0 );

            // Unbind the array buffer from the context
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            // Enable the vertex attribute. Note we don't have to be bound to do this!
            GL.EnableVertexAttribArray( 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();             /* note: this has to come before _InitVao() */
            this._InitVao();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName );

            // Make sure we release the buffer once were done with it.
            GL.DeleteBuffers( 1, ref _arrayBufferName );
            
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
 
            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            // Draw one triangle
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
            
            SwapBuffers();
        }
    }
}
