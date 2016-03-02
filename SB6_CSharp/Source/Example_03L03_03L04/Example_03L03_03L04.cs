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
    class Example_03L03_03L04 : GameWindow
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

        //-----------------------------------------------------------------------------------------
        public Example_03L03_03L04() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 3.3 thru 3.4", 
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

                // 'offset' is an input vertex attribute
                layout (location = 0) in vec4 offset;
                layout (location = 1) in vec4 color;

                // 'vs_color' is an output that will be sent to the next shader stage
                out vec4 vs_color;

                void main(void)
                {
                    // Declare a hard-coded array of positions
                    const vec4 vertices[3] = vec4[3](vec4( 0.25, -0.25, 0.5, 1.0),
                                                     vec4(-0.25, -0.25, 0.5, 1.0),
                                                     vec4( 0.25,  0.25, 0.5, 1.0));
                    
                    // Add 'offset' to our hard-coded vertex position
                    gl_Position = vertices[gl_VertexID] + offset;
                    
                    // Output a fixed value for vs_color
                    vs_color = color;
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                
                //Input from the vertex shader
                in vec4 vs_color;

                //Output to the framebuffer
                out vec4 color;

                void main(void)
                {
                    // Simply assign the color we were given by the vertex shader to our output
                    color = vs_color;
                }
                ";

            // Create and compile vertex shader
            vertexShaderName = GL.CreateShader( ShaderType.VertexShader );
            GL.ShaderSource( vertexShaderName, vertexShaderSource );
            GL.CompileShader( vertexShaderName );

            //Create and compile fragment shader
            fragmentShaderName = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( fragmentShaderName, fragmentShaderSource );
            GL.CompileShader( fragmentShaderName );

            //Create program, attach shaders to it, and link it
            _shaderProgramName = GL.CreateProgram();
            GL.AttachShader( _shaderProgramName, vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _shaderProgramName, fragmentShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShaderName ) );
            GL.LinkProgram( _shaderProgramName );

            //Delete the shaders as the program has them now
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
            GL.DeleteProgram( _shaderProgramName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            //Get elapsed time since application startup
            double elapsedSeconds = Program.ElapsedTimeSeconds;
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, Statics.colorGreen );
 
            //Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            //Animate triangle position
            float[] vertexPos = new float[] { (float)(Math.Sin(elapsedSeconds) * 0.5f),
                                              (float)(Math.Cos(elapsedSeconds) * 0.6f),
                                              0.0f, 0.0f };
            
            //Animate triangle color
            float[] vertexColor = new float[] { (float) (Math.Cos(elapsedSeconds) * 0.5f + 0.5f), 
                                                (float) (Math.Sin(elapsedSeconds) * 0.5f + 0.5f), 
                                                0.0f, 1.0f };
            
            //Update the value of input attributes
            GL.VertexAttrib4( 0, vertexPos );
            GL.VertexAttrib4( 1, vertexColor );

            //Draw one triangle
            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
            
            SwapBuffers();
        }
    }
}
