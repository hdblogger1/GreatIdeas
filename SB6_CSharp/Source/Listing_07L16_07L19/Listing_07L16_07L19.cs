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
using OpenTK.Input;

using SB6_CSharp.Framework;
using System.Runtime.InteropServices;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// 
    /// </summary>
    class Listing_07L16_07L19 : GameWindow
    {
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
            public static readonly string[] tfVaryings = { "tf_position_mass", "tf_velocity" };
            
            public static readonly int pointsX = 50;
            public static readonly int pointsY = 50;
            public static readonly int numPoints = pointsX * pointsY;
            public static readonly int numConnections = (pointsX - 1) * pointsY + (pointsY - 1) * pointsX;
        }
        
        private int _updateProgram;
        private int _renderProgram;
        private int[] _vaos = new int[2];  // vertex array objects
        private int[] _vbos = new int[5];  // vertex buffer objects
        private int _vboIndex;             //    ...
        private int[] _tbos = new int[2];  // texture buffer objects

        private bool _drawLinesFlag = true;
        private bool _drawPointsFlag = true;
        private int  _iterationsPerFrame = 16;

        private enum BufferType
        {
            PositionA,
            PositionB,
            VelocityA,
            VelocityB,
            Connection
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Vector4i
        {
            public int X;
            public int Y;
            public int Z;
            public int W;

            public Vector4i( int value )
            {
                X = value;
                Y = value;
                Z = value;
                W = value;
            }
            
            public int this[int index]
            {
                get
                {
                    if( index == 0 )      { return X; }
                    else if( index == 1 ) { return Y; }
                    else if( index == 2 ) { return Z; }
                    else if( index == 3 ) { return W; }
                    throw new IndexOutOfRangeException( "You tried to access this vector at index: " + index );
                }
                set
                {
                    if( index == 0 )      { X = value; }
                    else if( index == 1 ) { Y = value; }
                    else if( index == 2 ) { Z = value; }
                    else if( index == 3 ) { W = value; }
                    else throw new IndexOutOfRangeException( "You tried to access this vector at index: " + index );
                }
            }
        };

        //-----------------------------------------------------------------------------------------
        public Listing_07L16_07L19() 
            : base( 800, 600, GraphicsMode.Default, "SB6_CSharp - Spring-Mass Simulator", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int vertexShader, fragmentShader;

            vertexShader = Shader.Load( Program.BasePath + @"Source\Listing_07L16_07L19\update.vs.glsl", 
                                        ShaderType.VertexShader );

            _updateProgram = GL.CreateProgram();
            GL.AttachShader( _updateProgram, vertexShader );
            GL.TransformFeedbackVaryings( _updateProgram, 2, Statics.tfVaryings, TransformFeedbackMode.SeparateAttribs );
            GL.LinkProgram( _updateProgram );

            string slog = GL.GetShaderInfoLog( vertexShader );
            string plog = GL.GetProgramInfoLog( _updateProgram );

            GL.DeleteShader( vertexShader );

            vertexShader = Shader.Load( Program.BasePath + @"Source\Listing_07L16_07L19\render.vs.glsl", 
                                        ShaderType.VertexShader );
            fragmentShader = Shader.Load( Program.BasePath + @"Source\Listing_07L16_07L19\render.fs.glsl", 
                                        ShaderType.FragmentShader );
            _renderProgram = Shader.Link( new int[] { vertexShader, fragmentShader }, 2 );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            Vector4[] initialPostions    = new Vector4[Statics.numPoints];
            Vector3[] initialVelocities  = new Vector3[Statics.numPoints];
            Vector4i[] connectionVectors = new Vector4i[Statics.numPoints];

            int n = 0;
            for( int j = 0; j < Statics.pointsY; j++ )
            {
                float fj = (float)j / (float)Statics.pointsY;
                for( int i = 0; i < Statics.pointsX; i++ )
                {
                    float fi = (float)i / (float)Statics.pointsX;

                    initialPostions[n] = new Vector4( (fi - 0.5f) * (float)Statics.pointsX,
                                                      (fj - 0.5f) * (float)Statics.pointsY,
                                                      0.6f * (float)Math.Sin( fi ) * (float)Math.Cos( fj ),
                                                      1.0f );
                    initialVelocities[n] = new Vector3( 0.0f );
                    connectionVectors[n] = new Vector4i( -1 );

                    if( j != (Statics.pointsY - 1) )
                    {
                        if( i != 0 )                     { connectionVectors[n][0] = n - 1; }
                        if( j != 0 )                     { connectionVectors[n][1] = n - Statics.pointsX; }
                        if( i != (Statics.pointsX - 1) ) { connectionVectors[n][2] = n + 1; }
                        if( j != (Statics.pointsY - 1) ) { connectionVectors[n][3] = n + Statics.pointsX; }
                    }
                    n++;
                }
            }

            //SB6Debug.ToOutput<Vector4>( initialPostions, "initialPostions.bin" );
            //SB6Debug.ToOutput<Vector3>( initialVelocities, "initialVelocities.bin" );
            //SB6Debug.ToOutput<Vector4i>( connectionVectors, "connectionVectors.bin" );
            
            GL.GenVertexArrays( _vaos.Length, _vaos );
            GL.GenBuffers( _vbos.Length, _vbos );

            //int s1 = Statics.numPoints * 4 * sizeof(float);
            //int s2 = Statics.numPoints * 3 * sizeof(float);
            //int s3 = Statics.numPoints * 4 * sizeof(int);


            for( int i = 0; i < _vaos.Length; i++ )
            {
                GL.BindVertexArray( _vaos[i] );

                GL.BindBuffer( BufferTarget.ArrayBuffer, _vbos[ (int)BufferType.PositionA + i] );
                GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(Statics.numPoints * 4 * sizeof(float)), 
                               initialPostions, BufferUsageHint.DynamicCopy );
                GL.VertexAttribPointer( 0, 4, VertexAttribPointerType.Float, false, 0, 0 );
                GL.EnableVertexAttribArray( 0 );

                GL.BindBuffer( BufferTarget.ArrayBuffer, _vbos[ (int)BufferType.VelocityA + i] );
                GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(Statics.numPoints * 3 * sizeof(float)), 
                               initialVelocities, BufferUsageHint.DynamicCopy );
                GL.VertexAttribPointer( 1, 3, VertexAttribPointerType.Float, false, 0, 0 );
                GL.EnableVertexAttribArray( 1 );

                GL.BindBuffer( BufferTarget.ArrayBuffer, _vbos[ (int)BufferType.Connection] );
                GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(Statics.numPoints * 4 * sizeof(int)), 
                               initialVelocities, BufferUsageHint.StaticDraw );
                GL.VertexAttribPointer( 2, 4, VertexAttribPointerType.Int, false, 0, 0 );
                GL.EnableVertexAttribArray( 2 );
            }

            GL.GenTextures( _tbos.Length, _tbos );
            GL.BindTexture( TextureTarget.TextureBuffer, _tbos[0] );
            GL.TexBuffer( TextureBufferTarget.TextureBuffer, SizedInternalFormat.Rgba32f, 
                          _vbos[(int)BufferType.PositionA] );
            GL.BindTexture( TextureTarget.TextureBuffer, _tbos[1] );
            GL.TexBuffer( TextureBufferTarget.TextureBuffer, SizedInternalFormat.Rgba32f, 
                          _vbos[(int)BufferType.PositionB] );

            int lines = (Statics.pointsX - 1) * Statics.pointsY + (Statics.pointsY - 1) * Statics.pointsX;

            _vboIndex = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ElementArrayBuffer, _vboIndex );
            GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(lines * 2 * sizeof(int)), 
                           IntPtr.Zero, BufferUsageHint.StaticDraw );

            IntPtr eBufferPtr = GL.MapBufferRange( BufferTarget.ElementArrayBuffer, IntPtr.Zero, 
                                                   (IntPtr)(lines * 2 * sizeof(int)), 
                                                   BufferAccessMask.MapWriteBit | BufferAccessMask.MapInvalidateBufferBit );
            int eBufferOffset = 0;
            int[] e = new int[2];

            for( int j = 0; j < Statics.pointsY; j++ )
            {
                for( int i = 0; i < Statics.pointsX - 1; i++ )
                {
                    e[0] = i + j * Statics.pointsX;
                    e[1] = 1 + i + j * Statics.pointsX;
                    Marshal.Copy( e, 0, eBufferPtr + eBufferOffset, e.Length );
                    eBufferOffset += e.Length * sizeof(int);
                }
            }

            for( int i = 0; i < Statics.pointsX; i++ )
            {
                for( int j = 0; j < Statics.pointsY - 1; j++ )
                {
                    e[0] = i + j * Statics.pointsX;
                    e[1] = Statics.pointsX + i + j * Statics.pointsX;
                    Marshal.Copy( e, 0, eBufferPtr + eBufferOffset, e.Length );
                    eBufferOffset += e.Length * sizeof(int);
                }
            }


            GL.UnmapBuffer( BufferTarget.ElementArrayBuffer );

            //byte[] bufferbytes = SB6Debug.GetBuffer( BufferTarget.ElementArrayBuffer, lines * 2 * sizeof(int) );
            //SB6Debug.ToOutput<byte>( bufferbytes , "indexpoints.bin" );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        void _HandleKeyDown( object sender, KeyboardKeyEventArgs e )
        {
            switch( e.Key )
            {
                case Key.L:            
                    _drawLinesFlag = !_drawLinesFlag; 
                    break;

                case Key.P:            
                    _drawPointsFlag = !_drawPointsFlag; 
                    break;

                case Key.KeypadPlus:  
                    _iterationsPerFrame++; 
                    break;

                case Key.KeypadMinus:  
                    _iterationsPerFrame--; 
                    break;
            }
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            Keyboard.KeyDown += _HandleKeyDown;
            
            this._InitProgram();
            this._InitBuffers();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _updateProgram );
            GL.DeleteProgram( _renderProgram );
            GL.DeleteVertexArrays( _vaos.Length, _vaos );
            GL.DeleteBuffers( _vbos.Length, _vbos );
            GL.DeleteTextures( _tbos.Length, _tbos );
            GL.DeleteBuffer( _vboIndex );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.UseProgram( _updateProgram );
            GL.Enable( EnableCap.RasterizerDiscard );

            int iterationIndex = 0;
            for( int i = _iterationsPerFrame; i != 0; --i )
            {
                GL.BindVertexArray( _vaos[iterationIndex & 1] );
                GL.BindTexture( TextureTarget.TextureBuffer, _tbos[iterationIndex & 1] );
                iterationIndex++;
                GL.BindBufferBase( BufferRangeTarget.TransformFeedbackBuffer, 0, 
                                   _vbos[(int)BufferType.PositionA + (iterationIndex & 1)] );
                GL.BindBufferBase( BufferRangeTarget.TransformFeedbackBuffer, 1, 
                                   _vbos[(int)BufferType.VelocityA + (iterationIndex & 1)] );
                GL.BeginTransformFeedback( TransformFeedbackPrimitiveType.Points );
                GL.DrawArrays( PrimitiveType.Points, 0, Statics.numPoints );
                GL.EndTransformFeedback();
            }
            GL.Flush();
            GL.Disable( EnableCap.RasterizerDiscard );

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );
 
            GL.UseProgram( _renderProgram );

            if( _drawPointsFlag )
            {
                GL.PointSize( 4.0f );
                GL.DrawArrays( PrimitiveType.Points, 0, Statics.numPoints );
            }

            if( _drawLinesFlag )
            {
                GL.BindBuffer( BufferTarget.ElementArrayBuffer, _vboIndex );
                GL.DrawElements( BeginMode.Lines, Statics.numConnections * 2, DrawElementsType.UnsignedInt, 0 );
            }
            
            SwapBuffers();
        }
    }
}
