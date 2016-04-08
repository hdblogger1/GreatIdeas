//=================================================================================================
// The code herein has been adapted from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
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
    class Listing_05L06_05L07 : GameWindow
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

        private int[] _vertexArrayBufferNames = new int[2];

        //-----------------------------------------------------------------------------------------
        public Listing_05L06_05L07() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 5.6 thru 5.7", 
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

                layout (location = 0) in vec3 position;
                layout (location = 1) in vec3 color;

                out vec4 vs_color;

                void main(void)
                {
                    gl_Position = vec4(position.x, position.y, position.z, 1.0);
                    vs_color = vec4(color.r, color.g, color.b, 1.0 );
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                
                in vec4 vs_color;

                out vec4 color;

                void main(void)
                {
                    color = vs_color;
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
            float[] positions = new float[] {  0.25f, -0.25f, 0.5f,
                                              -0.25f, -0.25f, 0.5f,
                                               0.25f,  0.25f, 0.5f };
            float[] colors = new float[] { 1.0f, 0.0f, 0.0f,
                                           0.0f, 1.0f, 0.0f,
                                           0.0f, 0.0f, 1.0f };
            
            // Get names for two buffers
            GL.GenBuffers( 2, _vertexArrayBufferNames );

            // Bind the first and initialize it
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferNames[0] );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(positions.Length * sizeof(float)), positions, 
                           BufferUsageHint.StaticDraw );

            // Bind the second and initialize it
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferNames[1] );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(colors.Length * sizeof(float)), colors, 
                           BufferUsageHint.StaticDraw );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );

            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferNames[0] );
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray( 0 );
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferNames[1] );
            GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray( 1 );

            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

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
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
            GL.DeleteBuffers( _vertexArrayBufferNames.Length, _vertexArrayBufferNames );
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
