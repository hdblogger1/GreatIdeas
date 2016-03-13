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
    class Example_05L08 : GameWindow
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

        private int _vertexArrayBufferName;

        struct Vertex
        {
            public float x, y, z; // Position
            public float r, g, b; // Color

            public const int Size = 6 * sizeof(float);
            public const int ColorOffset = 3 * sizeof(float);
        };

        //-----------------------------------------------------------------------------------------
        public Example_05L08() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 5.8", 
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
            Vertex[] vertices = new Vertex[] {
                          /* --------------------------   ---------------------- */
                          /*         positions                    colors         */
                          /* --------------------------   ---------------------- */
               new Vertex {  x= 0.25f, y=-0.25f, z=0.5f,  r=1.0f, g=0.0f, b=0.0f },
               new Vertex {  x=-0.25f, y=-0.25f, z=0.5f,  r=0.0f, g=1.0f, b=0.0f },
               new Vertex {  x= 0.25f, y= 0.25f, z=0.5f,  r=0.0f, g=0.0f, b=1.0f }
            };
           
            // Allocate and initialize a buffer object
            GL.GenBuffers( 1, out _vertexArrayBufferName );

            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferName );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vertex.Size), 
                           vertices, BufferUsageHint.StaticDraw );
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

            // Set up two vertex attributes - first positions, then colors
            GL.BindBuffer( BufferTarget.ArrayBuffer, _vertexArrayBufferName );
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, Vertex.Size, 0 );
            GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, false, Vertex.Size, Vertex.ColorOffset );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            // Now enable the vertex attributes. Note: This can be done without the array buffer actually
            // bound to the context.
            GL.EnableVertexAttribArray( 0 );
            GL.EnableVertexAttribArray( 1 );
            
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
            GL.DeleteBuffers( 1, ref _vertexArrayBufferName );
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
