using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    class BasicAtomicCounters : GameWindow
    {
        static class Buffers
        {
            public enum Type { VERTEX, ATOMIC_COUNTER, NUM_BUFFERS };
            public static uint Name( Type type ) { return Names[(int)type]; }
            public static int Count()            { return (int)Type.NUM_BUFFERS; }

            public static uint[] Names = new uint[(int)Type.NUM_BUFFERS];
        }

        //float[] _vectorData = new float[] { 0.25f, -0.25f, 0.5f, 1.0f,
        //                                   -0.25f, -0.25f, 0.5f, 1.0f,
        //                                    0.25f,  0.25f, 0.5f, 1.0f,
        //                                   -0.25f,  0.25f, 0.5f, 1.0f };
        Matrix4 _vectorData = new Matrix4( 0.25f, -0.25f, 0.5f, 1.0f,
                                           -0.25f, -0.25f, 0.5f, 1.0f,
                                            0.25f,  0.25f, 0.5f, 1.0f,
                                           -0.25f,  0.25f, 0.5f, 1.0f );

        int _programName;
        int _vertexArrayName;

        //-----------------------------------------------------------------------------------------
        public BasicAtomicCounters() 
            : base( 800, 600, GraphicsMode.Default, "OpenTK Example", 0, DisplayDevice.Default
                    // ask for an OpenGL 4.3 or higher default(core?) context
                    , 4, 3, GraphicsContextFlags.Default)
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool InitBuffers()
        {
            GL.GenBuffers( Buffers.Count(), Buffers.Names );
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Name(Buffers.Type.VERTEX));
            //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 4 * _vectorData.Length), 
            //              _vectorData, BufferUsageHint.StaticDraw);
            //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 16), 
            //              ref _vectorData, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 16), 
                          IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.BufferSubData( BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(sizeof(float) * 16), ref _vectorData );
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            uint data = 0;
            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, Buffers.Name(Buffers.Type.ATOMIC_COUNTER));
            //GL.BufferData(BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
            //              IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.BufferData(BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
                          ref data, BufferUsageHint.DynamicCopy);
            //GL.BufferSubData( BufferTarget.AtomicCounterBuffer, IntPtr.Zero, (IntPtr)sizeof(uint), ref data );
            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, 0);

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        public bool InitProgram()
        {
            int vertexShaderName, fragmentShaderName;
                
            //Source code for vertex shader
            string vertexShaderSource = @"
                #version 430 core

                layout (location = 0) in vec4 position;

                layout(binding = 1, offset = 0) uniform atomic_uint atomic_cntr;

                out vec4 vs_color;

                void main(void)
                {
                    gl_Position = position;
                    uint cntr = atomicCounterIncrement( atomic_cntr );
                    //switch( gl_VertexID )
                    switch( cntr )
                    {
                        case 0:  vs_color = vec4(0.0, 0.8, 1.0, 1.0);  break;
                        case 1:  vs_color = vec4(1.0, 0.0, 0.8, 1.0);  break;
                        case 2:  vs_color = vec4(0.8, 1.0, 0.0, 1.0);  break;
                        case 3:  vs_color = vec4(1.0, 0.8, 0.0, 1.0);  break;
                    }
                }
                ";
                
            //Source code for fragment shader
            string fragmentShaderSource = @"
                #version 430 core

                //layout(binding = 1, offset = 0) uniform atomic_uint atomic_cntr;

                out vec4 color;

                in vec4 vs_color;

                void main(void)
                {
                    //atomicCounterIncrement( atomic_cntr );
                    //uint cntr = atomicCounterIncrement( atomic_cntr );
                    color = vs_color;
                    //switch( cntr )
                    //{
                    //    case 0:  color = vec4(0.0, 0.8, 1.0, 1.0);  break;
                    //    case 1:  color = vec4(1.0, 0.0, 0.8, 1.0);  break;
                    //    case 2:  color = vec4(0.8, 1.0, 0.0, 1.0);  break;
                    //    case 3:  color = vec4(1.0, 0.8, 0.0, 1.0);  break;
                    //    default: color = vec4(0.5, 0.5, 0.5, 1.0);  break;
                    //}
                }
                ";

            //Create and compile vertex shader
            vertexShaderName = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource( vertexShaderName, vertexShaderSource );
            GL.CompileShader( vertexShaderName );

            //Create and compile fragment shader
            fragmentShaderName = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShaderName, fragmentShaderSource );
            GL.CompileShader( fragmentShaderName );

            //Create program, attach shaders to it, and link it
            _programName = GL.CreateProgram();
            GL.AttachShader( _programName, vertexShaderName );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderName));
            GL.AttachShader( _programName, fragmentShaderName );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderName));
            GL.LinkProgram( _programName );

            //Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShaderName );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool InitVertexArray()
        {
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.Name(Buffers.Type.VERTEX) );
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0 );
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            GL.EnableVertexAttribArray(0);

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad (EventArgs e)
        {
            bool success = true;

            success = this.InitBuffers();
            success = this.InitProgram();
            success = this.InitVertexArray();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteBuffers( Buffers.Count(), Buffers.Names );
            GL.DeleteProgram( _programName );
            GL.DeleteVertexArrays(1, ref _vertexArrayName);
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            
            //Clear the window with given color
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });

            uint data = 0;
            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );
            GL.ClearBufferSubData( BufferTarget.AtomicCounterBuffer, PixelInternalFormat.R8ui, IntPtr.Zero, (IntPtr)sizeof(uint),
                 PixelFormat.Rgba, All.UnsignedInt, (IntPtr)data );
            //GL.BufferSubData( BufferTarget.AtomicCounterBuffer, IntPtr.Zero, (IntPtr)sizeof(uint), ref data );
 
            //IntPtr ptr = GL.MapBuffer(BufferTarget.AtomicCounterBuffer,BufferAccess.ReadOnly);
            int[] managedArray = new int[100];
            //Marshal.Copy(ptr,(int[])managedArray,0,1);
            //GL.UnmapBuffer(BufferTarget.AtomicCounterBuffer);

            //Use the program object we created earlier for rendering
            GL.UseProgram(_programName);
            GL.BindBufferBase( BufferRangeTarget.AtomicCounterBuffer, 1, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );

            GL.PointSize(40.0f);

            //Draw one triangle
            GL.DrawArrays(PrimitiveType.Points, 0, 4);

            IntPtr ptr = GL.MapBuffer(BufferTarget.AtomicCounterBuffer,BufferAccess.ReadOnly);
            Marshal.Copy(ptr,(int[])managedArray,0,1);
            GL.UnmapBuffer(BufferTarget.AtomicCounterBuffer);
            
            SwapBuffers();
        }
    }
}
