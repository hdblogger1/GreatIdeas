using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    class Example_05L29_05L32 : GameWindow
    {
        static class Statics
        {
            public static float[] colorGreen = new float[] { 0.0f, 0.3f, 0.0f, 1.0f };
            public static float[]        one = new float[] { 1.0f };
        }
        
        static class Buffers
        {
            public enum Type { VERTEX, ATOMIC_COUNTER, NUM_BUFFERS };
            public static uint[] Names = new uint[(int)Type.NUM_BUFFERS];
            
            public static uint   GetName( Type type ) { return Names[(int)type]; }
            public static int    GetCount()           { return (int)Type.NUM_BUFFERS; }
        }

        int _vertexArrayName;
        int[] _programNames = new int[2];
        int[] _uniformNames = new int[2];

        Matrix4 _projMatrix;

        float[] _vectorData = new float[] 
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

        //-----------------------------------------------------------------------------------------
        public Example_05L29_05L32() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Atomic Counters", 0, 
                    DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default)
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool InitProgram()
        {
            int vertexShaderName, fragmentShader1Name, fragmentShader2Name;
                
            //Source code for vertex shader
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
                
            //Source code for fragment shaders
            string fragmentShader1Source = @"
                #version 430 core

                layout(binding = 0, offset = 0) uniform atomic_uint area;

                void main(void)
                {
                    atomicCounterIncrement(area);
                }
                ";

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

            //Create and compile vertex shader
            vertexShaderName = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource( vertexShaderName, vertexShaderSource );
            GL.CompileShader( vertexShaderName );

            //Create and compile fragment shaders
            fragmentShader1Name = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShader1Name, fragmentShader1Source );
            GL.CompileShader( fragmentShader1Name );
            fragmentShader2Name = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShader2Name, fragmentShader2Source );
            GL.CompileShader( fragmentShader2Name );

            //Create programs, attach shaders, and link it
            _programNames[0] = GL.CreateProgram();
            GL.AttachShader( _programNames[0], vertexShaderName );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderName));
            GL.AttachShader( _programNames[0], fragmentShader1Name );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShader1Name));
            GL.LinkProgram( _programNames[0] );

            _programNames[1] = GL.CreateProgram();
            GL.AttachShader( _programNames[1], vertexShaderName );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderName));
            GL.AttachShader( _programNames[1], fragmentShader2Name );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShader2Name));
            GL.LinkProgram( _programNames[1] );

            //Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShader1Name );
            GL.DeleteShader( fragmentShader2Name );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool InitBuffers()
        {
            GL.GenBuffers( Buffers.GetCount(), Buffers.Names );

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.GetName(Buffers.Type.VERTEX));
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 4 * _vectorData.Length), 
                          _vectorData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, Buffers.GetName(Buffers.Type.ATOMIC_COUNTER));
            GL.BufferData(BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
                          IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, 0);

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool InitVertexArray()
        {
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.GetName(Buffers.Type.VERTEX) );
            GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, 0, 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            GL.EnableVertexAttribArray( 0 );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool InitUniforms()
        {
            _uniformNames[0] = GL.GetUniformLocation( _programNames[0], "transform_matrix" );
            _uniformNames[1] = GL.GetUniformLocation( _programNames[1], "transform_matrix" );
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            float aspect = (float)Width / (float)Height;
            _projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(50.0f), aspect, 0.1f, 1000.0f);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad (EventArgs e)
        {
            bool success = true;

            success = this.InitBuffers();
            success = this.InitProgram();
            success = this.InitVertexArray();
            success = this.InitUniforms();

            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);

            //GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Lequal);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteBuffers( Buffers.GetCount(), Buffers.Names );
            GL.DeleteProgram( _programNames[0] );
            GL.DeleteProgram( _programNames[1] );
            GL.DeleteVertexArrays(1, ref _vertexArrayName);
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Get elapsed time since application startup
            float elapsedSeconds = (float)(Program.ElapsedTimeSeconds);

            // Clear the window with given color
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
            GL.ClearBuffer( ClearBuffer.Depth, 0, Statics.one );

//            uint[] tmp = new uint[] { 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1};
//            //uint[] tmp = new uint[] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
//            GL.UseProgram(_renderingProgramHandles[0]);
//            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, _counterHandle);
//            //GL.BindBufferBase(BufferTarget.AtomicCounterBuffer, 0, _counterHandle);
//            GL.BufferSubData(BufferTarget.AtomicCounterBuffer, IntPtr.Zero, (IntPtr)sizeof(uint), tmp );
            
            Matrix4 transformMatrix;
            float f;
            for(int i = 0; i < 24; i++)
            {
                f = (float)i + (elapsedSeconds * 0.3f);
                transformMatrix = Matrix4.CreateTranslation( (float)(Math.Sin(2.1f * f) * 2.0f), 
                                                              (float)(Math.Cos(1.7f * f) * 2.0f),
                                                              (float)(Math.Sin(1.3f * f) * Math.Cos(1.5f * f) * 2.0f));
                transformMatrix *= Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), 
                                                       elapsedSeconds * MathHelper.DegreesToRadians(21.0f) );
                transformMatrix *= Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), 
                                                        elapsedSeconds * MathHelper.DegreesToRadians(45.0f) );
                transformMatrix *= Matrix4.CreateTranslation( 0.0f, 0.0f, -6.0f );
                //transformMatrix *= Matrix4.CreateScale( 1.0f, 1.0f, 1.0f );
                transformMatrix *= _projMatrix;

                uint data = 0;
                GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );
                GL.ClearBufferSubData( BufferTarget.AtomicCounterBuffer, PixelInternalFormat.R8ui, IntPtr.Zero, 
                                       (IntPtr)sizeof(uint), PixelFormat.Rgba, All.UnsignedInt, ref data);
                GL.BindBuffer( BufferTarget.AtomicCounterBuffer, 0 );

                //Use first program object to calculate object area
                GL.UseProgram( _programNames[0] );
                //GL.ColorMask( false, false, false, false );  // disable writing to framebuffer
                //GL.DepthMask( true );
                
                GL.BindBufferBase( BufferRangeTarget.AtomicCounterBuffer, 0, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );
                GL.UniformMatrix4( _uniformNames[0], false, ref transformMatrix );

                GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );

                int[] managedArray = new int[100];
                IntPtr ptr = GL.MapBuffer( BufferTarget.AtomicCounterBuffer, BufferAccess.ReadOnly );
                Marshal.Copy( ptr, (int[])managedArray, 0, 1 );
                GL.UnmapBuffer( BufferTarget.AtomicCounterBuffer );

                // Now actually draw the object
                GL.UseProgram( _programNames[1] );
                //GL.ColorMask( true, true, true, true );  // enable writing to framebuffer
                
                //GL.BindBuffer( BufferTarget.UniformBuffer, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );
                GL.BindBufferBase( BufferRangeTarget.UniformBuffer, 0, Buffers.GetName( Buffers.Type.ATOMIC_COUNTER ) );
                GL.UniformMatrix4( _uniformNames[1], false, ref transformMatrix );

                GL.DrawArrays( PrimitiveType.Triangles, 0, 36 );

                int[] managedArray2 = new int[100];
                IntPtr ptr2 = GL.MapBuffer( BufferTarget.AtomicCounterBuffer, BufferAccess.ReadOnly );
                Marshal.Copy( ptr2, (int[])managedArray2, 0, 1 );
                GL.UnmapBuffer( BufferTarget.AtomicCounterBuffer );
                Console.WriteLine( "Cntr = {0:d}", managedArray2[0] );

            }            
            SwapBuffers();
        }
    }
}
