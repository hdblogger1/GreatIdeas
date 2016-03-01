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
    class Example_02L08_02L09 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.2f, 0.0f, 1.0f };
        }

        private int _programName;
        private int _vertexArrayName;

        //-----------------------------------------------------------------------------------------
        public Example_02L08_02L09() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listings 2.8 thru 2.9 ", 
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

                void main(void)
                {
                    // Declare a hard-coded array of positions
                    const vec4 vertices[3] = vec4[3](vec4( 0.25, -0.25, 0.5, 1.0),
                                                     vec4(-0.25, -0.25, 0.5, 1.0),
                                                     vec4( 0.25,  0.25, 0.5, 1.0));
                    // Index into our array using gl_VertexID
                    gl_Position = vertices[gl_VertexID];
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
            _programName = GL.CreateProgram();
            GL.AttachShader( _programName, vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _programName, fragmentShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShaderName ) );
            GL.LinkProgram( _programName );

            // Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShaderName );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitVertexArray()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As our
            // shader dosn't have any inputs, nothing else needs to be done with them, but OpenGL
            // still requires the VAO object to be created before drawing is allowed.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitVertexArray();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
            GL.DeleteProgram( _programName );
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            //Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
 
            //Use the program object we created earlier for rendering
            GL.UseProgram( _programName );

            //Draw one triangle
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
            
            SwapBuffers();
        }
    }
}
