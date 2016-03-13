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
    class Listing_03L09 : GameWindow
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
        public Listing_03L09() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Tessellation and Geometry Shaders", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShaderName, fragmentShaderName;
            int tessCtrlShaderName, tessEvalShaderName;
            int geomShaderName;

                
            // Source code for vertex shader
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

            // Source code for tessellation control shader
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

            // Source code for tessellation control shader
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

            // Source code for tessellation control shader
            string geomShaderSource = @"
                #version 430 core

                layout (triangles) in;
                layout (points, max_vertices = 3) out;

                void main(void)
                {
                    int i;
                    for (i = 0; i < gl_in.length(); i++)
                    {
                        gl_Position = gl_in[i].gl_Position;
                        EmitVertex();
                    }
                }
            ";

            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core
                
                //Output to the framebuffer
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

            // Create and compile tessellation control shader
            tessCtrlShaderName = GL.CreateShader( ShaderType.TessControlShader );
            GL.ShaderSource( tessCtrlShaderName, tessCtrlShaderSource );
            GL.CompileShader( tessCtrlShaderName );

            // Create and compile tessellation evaluation shader
            tessEvalShaderName = GL.CreateShader( ShaderType.TessEvaluationShader );
            GL.ShaderSource( tessEvalShaderName, tessEvalShaderSource );
            GL.CompileShader( tessEvalShaderName );

            // Create and compile geometry shader
            geomShaderName = GL.CreateShader( ShaderType.GeometryShader );
            GL.ShaderSource( geomShaderName, geomShaderSource );
            GL.CompileShader( geomShaderName );

            // Create and compile fragment shader
            fragmentShaderName = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( fragmentShaderName, fragmentShaderSource );
            GL.CompileShader( fragmentShaderName );

            // Create program, attach shaders to it, and link it
            _shaderProgramName = GL.CreateProgram();
            GL.AttachShader( _shaderProgramName, vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _shaderProgramName, tessCtrlShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( tessCtrlShaderName ) );
            GL.AttachShader( _shaderProgramName, tessEvalShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( tessEvalShaderName ) );
            GL.AttachShader( _shaderProgramName, geomShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( geomShaderName ) );
            GL.AttachShader( _shaderProgramName, fragmentShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShaderName ) );
            GL.LinkProgram( _shaderProgramName );

            // Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( tessCtrlShaderName );
            GL.DeleteShader( tessEvalShaderName );
            GL.DeleteShader( geomShaderName );
            GL.DeleteShader( fragmentShaderName );

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
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitVao();
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
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
 
            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );

            // Set point size to something we can actually see
            GL.PointSize( 5.0f );

            // Draw patches
            GL.DrawArrays( PrimitiveType.Patches, 0, 3 );
            
            SwapBuffers();
        }
    }
}
