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
    class Example_05L29_05L32 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static float[] colorGreen = new float[] { 0.0f, 0.3f, 0.0f, 1.0f };
            public static float[]        one = new float[] { 1.0f };
        }
        
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// A management class for OpenGL buffer object names
        /// </summary>
        static class Buffers
        {
            public enum Type { VERTEX, ATOMIC_COUNTER, NUM_BUFFERS };
            public static uint[] Names = new uint[(int)Type.NUM_BUFFERS];
            
            public static uint   GetName( Type type ) { return Names[(int)type]; }
            public static int    GetCount()           { return (int)Type.NUM_BUFFERS; }
        }

        private int _vertexArrayName;
        private int[] _shaderProgramName = new int[2];
        private int[] _uniformNames = new int[2];

        private Matrix4 _projMatrix;
        private Matrix4 _transformMatrix;

        //-----------------------------------------------------------------------------------------
        public Example_05L29_05L32() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Listing 5.29 thru 5.32 (Atomic Counters)", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShaderName, fragmentShader1Name, fragmentShader2Name;
                
            // Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core

                layout(location = 0) in vec3 position;
                out vec4 vs_color;

                uniform mat4 transform_matrix;

                void main(void)
                {
                    vec4 pos = vec4(position.x, position.y, position.z, 1.0);
                    gl_Position  = transform_matrix * pos;
                    vs_color = pos * 2.0 + vec4(0.5, 0.5, 0.5, 0.0);
                }
                ";
                
            // Source code for first fragment shaders
            string fragmentShader1Source = @"
                #version 430 core

                layout(binding = 0, offset = 0) uniform atomic_uint area;

                void main(void)
                {
                    atomicCounterIncrement(area);
                }
                ";

            // Source code for second fragment shaders
            string fragmentShader2Source = @"
                #version 430 core

                layout(binding = 0) uniform area_block
                {
                    uint counter_value;
                };

                out vec4 color;
                in vec4 vs_color;

                uniform float max_area;
                
                float max_area2;
                void main(void)
                {
                    max_area2 = 10000;
                    float brightness = clamp(float(counter_value) / max_area2, 0.0, 1.0);
                    
                    color = vec4( vs_color.r * brightness, 
                                  vs_color.b * brightness, 
                                  vs_color.g * brightness, 1.0);
                }
                ";

            // Create and compile vertex shader
            vertexShaderName = GL.CreateShader( ShaderType.VertexShader );
            GL.ShaderSource( vertexShaderName, vertexShaderSource );
            GL.CompileShader( vertexShaderName );

            // Create and compile fragment shaders
            fragmentShader1Name = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( fragmentShader1Name, fragmentShader1Source );
            GL.CompileShader( fragmentShader1Name );
            fragmentShader2Name = GL.CreateShader( ShaderType.FragmentShader );
            GL.ShaderSource( fragmentShader2Name, fragmentShader2Source );
            GL.CompileShader( fragmentShader2Name );

            // Create programs, attach shaders, and link it
            _shaderProgramName[0] = GL.CreateProgram();
            GL.AttachShader( _shaderProgramName[0], vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _shaderProgramName[0], fragmentShader1Name );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShader1Name ) );
            GL.LinkProgram( _shaderProgramName[0] );

            _shaderProgramName[1] = GL.CreateProgram();
            GL.AttachShader( _shaderProgramName[1], vertexShaderName );
            Console.WriteLine( GL.GetShaderInfoLog( vertexShaderName ) );
            GL.AttachShader( _shaderProgramName[1], fragmentShader2Name );
            Console.WriteLine( GL.GetShaderInfoLog( fragmentShader2Name ) );
            GL.LinkProgram( _shaderProgramName[1] );

            // Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShader1Name );
            GL.DeleteShader( fragmentShader2Name );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            float[] vertexData = new float[] 
            {
                // back
                -0.25f, 0.25f,-0.25f,   -0.25f,-0.25f,-0.25f,    0.25f,-0.25f,-0.25f,
                 0.25f,-0.25f,-0.25f,    0.25f, 0.25f,-0.25f,   -0.25f, 0.25f,-0.25f,
                // right
                 0.25f,-0.25f,-0.25f,    0.25f,-0.25f, 0.25f,    0.25f, 0.25f,-0.25f,
                 0.25f,-0.25f, 0.25f,    0.25f, 0.25f, 0.25f,    0.25f, 0.25f,-0.25f,
                // front
                 0.25f,-0.25f, 0.25f,   -0.25f,-0.25f, 0.25f,    0.25f, 0.25f, 0.25f,
                -0.25f,-0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,    0.25f, 0.25f, 0.25f,
                // left
                -0.25f,-0.25f, 0.25f,   -0.25f,-0.25f,-0.25f,   -0.25f, 0.25f, 0.25f,
                -0.25f,-0.25f,-0.25f,   -0.25f, 0.25f,-0.25f,   -0.25f, 0.25f, 0.25f,
                // bottom
                -0.25f,-0.25f, 0.25f,    0.25f,-0.25f, 0.25f,    0.25f,-0.25f,-0.25f,
                 0.25f,-0.25f,-0.25f,   -0.25f,-0.25f,-0.25f,   -0.25f,-0.25f, 0.25f,
                // top
                -0.25f, 0.25f,-0.25f,    0.25f, 0.25f,-0.25f,    0.25f, 0.25f, 0.25f,
                 0.25f, 0.25f, 0.25f,   -0.25f, 0.25f, 0.25f,   -0.25f, 0.25f,-0.25f
            };

            GL.GenBuffers( Buffers.GetCount(), Buffers.Names );

            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.GetName( Buffers.Type.VERTEX ) );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 4 * vertexData.Length), 
                           vertexData, BufferUsageHint.StaticDraw );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );
            GL.BufferData( BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
                           IntPtr.Zero, BufferUsageHint.DynamicCopy );
            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.GetName( Buffers.Type.VERTEX ) );
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            // Enable the vertex attribute. Note we don't have to be bound to do this!
            GL.EnableVertexAttribArray( 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitUniforms()
        {
            _uniformNames[0] = GL.GetUniformLocation( _shaderProgramName[0], "transform_matrix" );
            _uniformNames[1] = GL.GetUniformLocation( _shaderProgramName[1], "transform_matrix" );
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private void _UpdateTransform( int cubeId )
        {
            float elapsedSeconds = (float)Program.ElapsedTimeSeconds;

            float f = (float)cubeId + (elapsedSeconds * 0.3f);

            _transformMatrix = Matrix4.CreateTranslation( (float)(Math.Sin( 2.1f * f ) * 2.0f), 
                                                          (float)(Math.Cos( 1.7f * f ) * 2.0f),
                                                          (float)(Math.Sin( 1.3f * f ) * Math.Cos( 1.5f * f ) * 2.0f) );
            _transformMatrix *= Matrix4.CreateFromAxisAngle( new Vector3( 1.0f, 0.0f, 0.0f ), 
                                                             elapsedSeconds * MathHelper.DegreesToRadians( 21.0f ) );
            _transformMatrix *= Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 1.0f, 0.0f ), 
                                                             elapsedSeconds * MathHelper.DegreesToRadians( 45.0f ) );
            _transformMatrix *= Matrix4.CreateTranslation( 0.0f, 0.0f, -6.0f );
            _transformMatrix *= _projMatrix;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnResize( EventArgs e )
        {
            GL.Viewport( ClientRectangle.X, ClientRectangle.Y, 
                         ClientRectangle.Width, ClientRectangle.Height );
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians(50.0f), 
                                                                aspect, 0.1f, 1000.0f );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();             /* note: this has to come before _InitVao() */
            this._InitVao();
            this._InitUniforms();

            GL.Enable( EnableCap.CullFace );
            GL.FrontFace( FrontFaceDirection.Cw );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgramName[0] );
            GL.DeleteProgram( _shaderProgramName[1] );
            GL.DeleteBuffers( Buffers.GetCount(), Buffers.Names );
            GL.DeleteVertexArrays( 1, ref _vertexArrayName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );

            for( int cubeId = 0; cubeId < 24; cubeId++ )
            {
                this._UpdateTransform( cubeId );

                // Reset atomic counter
                uint data = 0;
                GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );
                GL.ClearBufferSubData( BufferTarget.AtomicCounterBuffer, PixelInternalFormat.R8ui, IntPtr.Zero, 
                                       (IntPtr)sizeof(uint), PixelFormat.Rgba, All.UnsignedInt, ref data );
                GL.BindBuffer( BufferTarget.AtomicCounterBuffer, 0 );

                // Attach our atomic buffer to our contexts atomic counter buffer binding point
                GL.BindBufferBase( BufferRangeTarget.AtomicCounterBuffer, 0, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );

                // Use first program object to calculate object area
                GL.UseProgram( _shaderProgramName[0] );
                //GL.ColorMask( false, false, false, false );  // disable writing to framebuffer

                // Set the transformation matrix for current program object
                GL.UniformMatrix4( _uniformNames[0], false, ref _transformMatrix );

                // Run our shader to update our atomic counter
                GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );

                GL.BindBufferBase( BufferRangeTarget.UniformBuffer, 0, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );

                // Switch to our second program to actually draw the cube
                GL.UseProgram( _shaderProgramName[1] );
                //GL.ColorMask( true, true, true, true );  // enable writing to framebuffer
                
                GL.UniformMatrix4( _uniformNames[1], false, ref _transformMatrix );

                GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );
            }            
            SwapBuffers();
        }
    }
}
