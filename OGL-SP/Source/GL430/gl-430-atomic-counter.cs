using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using System.Runtime.InteropServices;

namespace OGL_SP
{
    class GL430_Atomic_Counter : Test
    {
        //Requested OpenGL context used in this example
        static int OGL_MAJOR_VERSION_REQUESTED = 4;
        static int OGL_MINOR_VERSION_REQUESTED = 3;

        public enum Locations { POSITION =0, TEXCOORD =4, FRAG_COLOR =0 };
        public enum Bindings  { TRANSFORM0 =1 };

        static class Buffers
        {
            public enum Type { VERTEX, ELEMENT, TRANSFORM, ATOMIC_COUNTER, NUM_BUFFERS };
            public static uint Name( Type type ) { return Names[(int)type]; }
            public static int Count()            { return (int)Type.NUM_BUFFERS; }

            public static uint[] Names = new uint[(int)Type.NUM_BUFFERS];
        }

        static ushort[] _ElementData = new ushort[]
        {
            0, 1, 2,
            2, 3, 0
        };

        static Matrix2[] _VectorData = new Matrix2[]
        {
            new Matrix2(new Vector2(-1.0f, -1.0f), new Vector2(0.0f, 1.0f)),
            new Matrix2(new Vector2( 1.0f, -1.0f), new Vector2(1.0f, 1.0f)),
            new Matrix2(new Vector2( 1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
            new Matrix2(new Vector2(-1.0f,  1.0f), new Vector2(0.0f, 0.0f))
        };

        //int _pipelineName;
        int _programName;
        int _vertexArrayName;

        Matrix4 _projectionMatrix;
        
        static readonly float[] _red = new float[] { 1.0f, 0.0f, 0.0f, 1.0f };
        
        //-----------------------------------------------------------------------------------------
        public GL430_Atomic_Counter() 
            : base( OGL_MAJOR_VERSION_REQUESTED, OGL_MINOR_VERSION_REQUESTED ) { } 

        //-----------------------------------------------------------------------------------------
        private bool InitBuffers()
        {
            int maxVertexAtomicCounterBuffers;
            int maxControlAtomicCounterBuffers;
            int maxEvaluationAtomicCounterBuffers;
            int maxGeometryAtomicCounterBuffers;
            int maxFragmentAtomicCounterBuffers;
            int maxCombinedAtomicCounterBuffers;

            GL.GetInteger((GetPName)All.MaxVertexAtomicCounterBuffers, out maxVertexAtomicCounterBuffers);
            GL.GetInteger((GetPName)All.MaxTessControlAtomicCounterBuffers, out maxControlAtomicCounterBuffers);
            GL.GetInteger((GetPName)All.MaxTessEvaluationAtomicCounterBuffers, out maxEvaluationAtomicCounterBuffers);
            GL.GetInteger((GetPName)All.MaxGeometryAtomicCounterBuffers, out maxGeometryAtomicCounterBuffers);
            GL.GetInteger((GetPName)All.MaxFragmentAtomicCounterBuffers, out maxFragmentAtomicCounterBuffers);
            GL.GetInteger((GetPName)All.MaxCombinedAtomicCounterBuffers, out maxCombinedAtomicCounterBuffers);

            GL.GenBuffers( Buffers.Count(), Buffers.Names );

            GL.BindBuffer(BufferTarget.UniformBuffer, Buffers.Name(Buffers.Type.TRANSFORM));
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(sizeof(float) * 16), 
                          IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffers.Name(Buffers.Type.ELEMENT));
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * _ElementData.Length), 
                          _ElementData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffers.Name(Buffers.Type.VERTEX));
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * 4 * _VectorData.Length), 
                          _VectorData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, Buffers.Name(Buffers.Type.ATOMIC_COUNTER));
            GL.BufferData(BufferTarget.AtomicCounterBuffer, (IntPtr)sizeof(uint), 
                          IntPtr.Zero, BufferUsageHint.DynamicCopy);
            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, 0);

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool InitProgram()
        {
//            GL.GenProgramPipelines(1, out _pipelineName);

            //Create and compile vertex shader
            int vertexShaderName = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource( vertexShaderName, LoadShader(Program.BasePath + "Data\\GL430\\atomic-counter.vert") );
            GL.CompileShader( vertexShaderName );

            //Create and compile fragment shader
            int fragmentShaderName = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource( fragmentShaderName, LoadShader(Program.BasePath + "Data\\GL430\\atomic-counter.frag") );
            GL.CompileShader( fragmentShaderName );

            //Create program, attach shaders, and link it
            _programName = GL.CreateProgram();
            GL.AttachShader( _programName, vertexShaderName );
            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderName));
            GL.AttachShader( _programName, fragmentShaderName );
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderName));
            GL.LinkProgram( _programName );

            bool success = CheckProgram( _programName );

            if( success )
            {
//                GL.UseProgramStages( _pipelineName, 
//                                     ProgramStageMask.VertexShaderBit | ProgramStageMask.FragmentShaderBit, 
//                                     _programName );
            }

            //Delete the shaders as the program has them now
            GL.DeleteShader( vertexShaderName );
            GL.DeleteShader( fragmentShaderName );

            return success;
        }

        //-----------------------------------------------------------------------------------------
        private bool InitVertexArray()
        {
            GL.GenVertexArrays( 1, out _vertexArrayName );
            GL.BindVertexArray( _vertexArrayName );
            
            GL.BindBuffer( BufferTarget.ArrayBuffer, Buffers.Name(Buffers.Type.VERTEX) );
            GL.VertexAttribPointer( (int)Locations.POSITION, 2, VertexAttribPointerType.Float, 
                                    false, sizeof(float) * 4, 0 );
            GL.VertexAttribPointer( (int)Locations.TEXCOORD, 2, VertexAttribPointerType.Float, 
                                    false, sizeof(float) * 4, sizeof(float) * 2);
            GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

            GL.EnableVertexAttribArray( (int)Locations.POSITION );
            GL.EnableVertexAttribArray( (int)Locations.TEXCOORD );

            GL.BindBuffer( BufferTarget.ElementArrayBuffer, Buffers.Name(Buffers.Type.ELEMENT) );
            GL.BindVertexArray(0);

            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            bool success = true;

            success = this.CheckExtension("GL_ARB_clear_buffer_object");

            success = this.InitBuffers();
            success = this.InitProgram();
            success = this.InitVertexArray();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteBuffers( Buffers.Count(), Buffers.Names );
            GL.DeleteProgram( _programName );
            GL.DeleteVertexArrays(1, ref _vertexArrayName);
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport( ClientRectangle.X, ClientRectangle.Y, 
                         ClientRectangle.Width, ClientRectangle.Height );
            float aspect = (float)Width / (float)Height;
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians(50.0f), 
                                                                      aspect, 0.1f, 1000.0f );
        }

        public float[] GetTransform()
        {
        //{
        //    float transCurrentY = 1.0f;
        //    float rotateCurrentY = 1.0f;
        //    Matrix4 viewTranslate = Matrix4.CreateTranslation(new Vector3( 0.0f, 0.0f, transCurrentY ) );
        //    Matrix4 viewRotateX = Matrix4.CreateRotationX( rotateCurrentY );
        //    Matrix4 view = 
        //    Matrix4 ret = new Matrix4( -0.2852312f,  0.0f,       -0.9843464f,  -0.9841495f,
        //                                1.5774110f, -0.1782147f, -0.1767626f,  -0.1767273f,
        //                                0.1315424f,  2.137089f,  -0.01474047f, -0.01473752f,
        //                                -0.661469f,  1.034733f,   5.339466f,    5.538378f);
        //    Matrix4 ret = new Matrix4( 1.4648548f,  0.83409023f,  0.47648028f,  0.47552827f,
        //                               0.0f,        1.9531397f,  -0.58896202f, -0.58778524f,
        //                               1.0642793f, -1.1480267f,  -0.65581888f, -0.65450853f,
        //                               0.0f,        0.0f,        -0.2002002f,   0.0f);
            float[] ret = new float[16] { 1.4648548f,   0.0f,        1.0642793f,    0.0f,
                                          0.83409023f,  1.9531397f, -1.1480267f,    0.0f,
                                          0.47648028f, -0.58896202f, -0.65581888f, -0.2002002f,
                                          0.47552827f, -0.58778524f, -0.65450853f,  0.0f };
        //    float[] ret = new float[16] { 1.4648548f,  0.83409023f,  0.47648028f,  0.47552827f,
        //                                  0.0f,        1.9531397f,  -0.58896202f, -0.58778524f,
        //                                  1.0642793f, -1.1480267f,  -0.65581888f, -0.65450853f,
        //                                  0.0f,        0.0f,        -0.2002002f,   0.0f };
            return ret;
        }

        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Enable( EnableCap.Blend );
            GL.BlendEquation( BlendEquationMode.FuncAdd );
            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );

 //           Matrix4 mvp = _projectionMatrix * ... * Matrix4.Identity;
            //Matrix4 mvp = GetTransform();
            //mvp.Transpose();

            GL.BindBuffer( BufferTarget.UniformBuffer, Buffers.Name( Buffers.Type.TRANSFORM ) );
            //GL.BufferSubData( BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)(sizeof(float) * 16), ref mvp.Row0.X );
            GL.BufferSubData( BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)(sizeof(float) * 16), GetTransform() );

            uint data = 0;
            GL.BindBuffer( BufferTarget.AtomicCounterBuffer, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );
            GL.ClearBufferSubData( BufferTarget.AtomicCounterBuffer, PixelInternalFormat.R8ui, IntPtr.Zero, (IntPtr)sizeof(uint),
                 PixelFormat.Rgba, All.UnsignedInt, (IntPtr)data );

            GL.ViewportIndexed( 0, 0, 0, ClientRectangle.X, ClientRectangle.Y );
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f } );

            GL.UseProgram( _programName );
            GL.BindVertexArray( _vertexArrayName );
            GL.BindBufferBase( BufferRangeTarget.AtomicCounterBuffer, 0, Buffers.Name( Buffers.Type.ATOMIC_COUNTER ) );
            GL.BindBufferBase( BufferRangeTarget.UniformBuffer, (int)Bindings.TRANSFORM0, Buffers.Name( Buffers.Type.TRANSFORM ) );

            GL.DrawElementsInstancedBaseVertexBaseInstance( PrimitiveType.Triangles, _ElementData.Length, 
                                                            DrawElementsType.UnsignedShort, (IntPtr)0,
                                                            5, 0, 0 ); 
            SwapBuffers();
        }
    }
}
