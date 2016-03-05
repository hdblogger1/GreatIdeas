using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class BasicAtomicCounters : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
        }
        
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// A management class for OpenGL buffer object names
        /// </summary>
        static class Buffers
        {
            public enum Type { VERTEX, ATOMIC_COUNTER, NUM_BUFFERS };
            public static uint Name( Type type ) { return Names[(int)type]; }
            public static int Count()            { return (int)Type.NUM_BUFFERS; }

            public static uint[] Names = new uint[(int)Type.NUM_BUFFERS];
        }

        private int _shaderProgramName;
        private int _vertexArrayName;

        Matrix4 _vectorData = new Matrix4( 0.25f, -0.25f, 0.5f, 1.0f,
                                           -0.25f, -0.25f, 0.5f, 1.0f,
                                            0.25f,  0.25f, 0.5f, 1.0f,
                                           -0.25f,  0.25f, 0.5f, 1.0f );

        //-----------------------------------------------------------------------------------------
        public BasicAtomicCounters() 
            : base( 800, 600, GraphicsMode.Default, "SB6_CSharp - Atomic Counters (Basic)", 
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

                layout(binding = 1, offset = 0) uniform atomic_uint atomic_cntr;

                out vec4 vs_color;

                void main(void)
                {
                    gl_Position = position;
                    uint cntr = atomicCounterIncrement( atomic_cntr );
                    switch( cntr )
                    {
                        case 0:  vs_color = vec4(0.0, 0.8, 1.0, 1.0);  break;
                        case 1:  vs_color = vec4(1.0, 0.0, 0.8, 1.0);  break;
                        case 2:  vs_color = vec4(0.8, 1.0, 0.0, 1.0);  break;
                        case 3:  vs_color = vec4(1.0, 0.8, 0.0, 1.0);  break;
                    }
                }
                ";
                
            // Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                out vec4 color;
                in vec4 vs_color;

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
            GL.GenBuffers( Buffers.Count(), Buffers.Names );
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.Name( Buffers.Type.VERTEX ) );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 16), 
                           ref _vectorData, BufferUsageHint.StaticDraw );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );
            GL.BufferData( BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
                           IntPtr.Zero, BufferUsageHint.DynamicCopy );
            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, 0 );

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
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.Name(Buffers.Type.VERTEX) );
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

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
            GL.DeleteBuffers( Buffers.Count(), Buffers.Names );
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );

            uint data = 0;
            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );
            GL.BufferSubData( BufferTarget.AtomicCounterBuffer, IntPtr.Zero, (IntPtr)sizeof(uint), ref data );
 
            // Use the program object we created earlier for rendering
            GL.UseProgram( _shaderProgramName );
            GL.BindBufferBase( BufferRangeTarget.AtomicCounterBuffer, 1, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );

            GL.PointSize( 40.0f );

            // Draw one triangle
            GL.DrawArrays( PrimitiveType.Points, 0, 4 );
            
            SwapBuffers();
        }
    }
}
