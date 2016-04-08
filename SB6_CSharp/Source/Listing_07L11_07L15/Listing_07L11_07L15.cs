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

using SB6_CSharp.Framework;
using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// 
    /// </summary>
    class Listing_07L11_07L15 : GameWindow
    {
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
            public static readonly float[] one = { 1.0f };
            public static readonly int numberOfDraws = 50000;
        }
        
        private int _shaderProgram;

        private enum Mode
        {
            Multidraw,
            SeparateDraws
        };

        private SBM6Model _asteroidsModel;

        private bool _pausedFlag = false;
        private bool _vsyncFlag = false;
        private double _lastTime = 0.0f;
        private double _totalTime = 0.0f;
        private Mode   _drawMode = Mode.Multidraw;

        private struct UniformNames
        {
            public int time;
            public int viewMatrix;
            public int projMatrix;
            public int viewProjMatrix;
        };
        private UniformNames _uniforms;

        private struct Buffers
        {
            public int indirectDraw;
            public int drawIndex;
        };
        private Buffers _buffers;

        [StructLayout(LayoutKind.Sequential)]
        private struct DrawArraysIndirectCommand
        {
            public uint count;
            public uint primCount;
            public uint first;
            public uint baseInstance;

            public static int Size { get { return sizeof(uint) * 4; } }
        };

        //-----------------------------------------------------------------------------------------
        public Listing_07L11_07L15() 
            : base( 800, 600, GraphicsMode.Default, "SB6_CSharp - Asteroids", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShader   = Shader.Load( Program.BasePath + @"Source\Listing_07L11_07L15\asteroids.vs.glsl",
                                              ShaderType.VertexShader );
            int fragmentShader = Shader.Load( Program.BasePath + @"Source\Listing_07L11_07L15\asteroids.fs.glsl", 
                                              ShaderType.FragmentShader );

            _shaderProgram = Shader.Link( new int[] { vertexShader, fragmentShader }, 2 );

            _asteroidsModel = new SBM6Model();
            _asteroidsModel.Load( Program.BasePath + @"Media\Objects\asteroids.sbm" );

            _uniforms.time           = GL.GetUniformLocation( _shaderProgram, "time" );
            _uniforms.viewMatrix     = GL.GetUniformLocation( _shaderProgram, "view_matrix" );
            _uniforms.projMatrix     = GL.GetUniformLocation( _shaderProgram, "proj_matrix" );
            _uniforms.viewProjMatrix = GL.GetUniformLocation( _shaderProgram, "viewproj_matrix" );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            _buffers.indirectDraw = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.DrawIndirectBuffer, _buffers.indirectDraw );
            GL.BufferData( BufferTarget.DrawIndirectBuffer, 
                           (IntPtr)(Statics.numberOfDraws * DrawArraysIndirectCommand.Size),
                           IntPtr.Zero, BufferUsageHint.StaticDraw );

            IntPtr cmdBufferPtr = GL.MapBufferRange( BufferTarget.DrawIndirectBuffer, IntPtr.Zero, 
                                                     (IntPtr)(Statics.numberOfDraws * DrawArraysIndirectCommand.Size),
                                                     BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit );
            DrawArraysIndirectCommand cmd;
            for( uint i = 0; i < Statics.numberOfDraws; i++ )
            {
                _asteroidsModel.GetSubObjectInfo( i % _asteroidsModel.NumSubObject, out cmd.first, out cmd.count );
                cmd.primCount = 1;
                cmd.baseInstance = i;
            
                Marshal.StructureToPtr( cmd, cmdBufferPtr + (int)(i * DrawArraysIndirectCommand.Size), false );
            }
            GL.UnmapBuffer( BufferTarget.DrawIndirectBuffer );

            GL.BindVertexArray( _asteroidsModel.Vao );
            
            _buffers.drawIndex = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, _buffers.drawIndex );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(Statics.numberOfDraws * sizeof(int)),
                           IntPtr.Zero, BufferUsageHint.StaticDraw );

            IntPtr drawIndexBufferPtr = GL.MapBufferRange( BufferTarget.ArrayBuffer, IntPtr.Zero,
                                                           (IntPtr)(Statics.numberOfDraws * sizeof(uint)), 
                                                           BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit );
            int[] drawIndex = new int[1];
            for( int i = 0; i < Statics.numberOfDraws; i++ )
            {
                drawIndex[0] = i;
                Marshal.Copy( drawIndex, 0, drawIndexBufferPtr + (i * sizeof(int)), 1 );
            }
            GL.UnmapBuffer( BufferTarget.ArrayBuffer );

            GL.VertexAttribIPointer( 10, 1, VertexAttribIPointerType.UnsignedInt, 0, IntPtr.Zero );
            GL.VertexAttribDivisor( 10, 1 );
            GL.EnableVertexAttribArray( 10 );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();

            GL.Enable( EnableCap.DepthTest );
            GL.DepthFunc( DepthFunction.Lequal );
            GL.Enable( EnableCap.CullFace );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgram );
            GL.DeleteBuffer( _buffers.indirectDraw );
            GL.DeleteBuffer( _buffers.drawIndex );
            _asteroidsModel.Delete();
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnKeyPress( OpenTK.KeyPressEventArgs e )
        {
            switch( e.KeyChar )
            {
                case 'P': case 'p':
                    _pausedFlag = !_pausedFlag;
                    break;

                case 'V': case 'v':
                    _vsyncFlag = !_vsyncFlag;
                    this.VSync = _vsyncFlag ? VSyncMode.On : VSyncMode.Off;
                    break;

                case 'D': case 'd':
                    switch( _drawMode )
                    {
                        case Mode.Multidraw:     _drawMode = Mode.SeparateDraws; break;
                        case Mode.SeparateDraws: _drawMode = Mode.Multidraw; break;
                    }
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            double currentTime = Program.ElapsedTimeSeconds;
            
            if( !_pausedFlag ) { _totalTime += ( currentTime - _lastTime); }
            _lastTime = currentTime;

            float t = (float)_totalTime;
            int i = (int)(_totalTime * 3.0f);

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );
            GL.ClearBuffer( ClearBuffer.Depth, 0, Statics.one );

            Matrix4 viewMatrix = Matrix4.LookAt( new Vector3( 100.0f * (float)Math.Cos( t * 0.023f),
                                                              100.0f * (float)Math.Cos( t * 0.023f),
                                                              300.0f * (float)Math.Sin( t * 0.037f) - 600.0f ),
                                                 new Vector3( 0.0f, 0.0f, 260.0f ),
                                                 Vector3.Normalize( new Vector3( 0.1f - (float)Math.Cos( t * 0.1f ) * 0.3f,
                                                                                 1.0f, 0.0f ) ) );
            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 50.0f ),
                                                                       (float)Width / (float)Height,
                                                                       1.0f, 2000.0f );
            Matrix4 viewprojMatrix = viewMatrix * projMatrix;

            GL.UseProgram( _shaderProgram );

            GL.Uniform1( _uniforms.time, t );
            GL.UniformMatrix4( _uniforms.viewMatrix, false, ref viewMatrix );
            GL.UniformMatrix4( _uniforms.projMatrix, false, ref projMatrix );
            GL.UniformMatrix4( _uniforms.viewProjMatrix, false, ref viewprojMatrix );

            GL.BindVertexArray( _asteroidsModel.Vao );

            switch( _drawMode )
            {
                case Mode.Multidraw:
                    GL.MultiDrawArraysIndirect( PrimitiveType.Triangles, IntPtr.Zero, Statics.numberOfDraws, 0 );
                    break;

                case Mode.SeparateDraws:
                    for( uint j = 0; j < Statics.numberOfDraws; j++ )
                    {
                        uint first, count;
                        _asteroidsModel.GetSubObjectInfo( j % _asteroidsModel.NumSubObject, out first, out count );
                        GL.DrawArraysInstancedBaseInstance( PrimitiveType.Triangles, (int)first, (int)count, 1, j );
                    }
                    break;
            }

            SwapBuffers();
        }
    }
}
