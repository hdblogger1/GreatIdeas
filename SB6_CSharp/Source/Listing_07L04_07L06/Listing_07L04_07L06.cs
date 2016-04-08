//=================================================================================================
// The code herein is an adaptation from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using SB6_CSharp.Framework;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// 
    /// </summary>
    class Listing_07L04_07L06 : GameWindow
    {
        private static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
            public static readonly float[] one = { 1.0f };
            
            public static readonly float[] grassBlade = new float[] 
                { 
                    -0.3f,  0.0f,
                     0.3f,  0.0f,
                    -0.20f, 1.0f,
                     0.1f,  1.3f,
                    -0.05f, 2.3f,
                     0.0f,  3.3f
                };
        }
        
        private int _shaderProgram;
        private int _vao;

        private int _grassBuffer;
        private int _mvpMatrixUniformLoc;

        private struct Textures
        {
            public int grassColor;
            public int grassLength;
            public int grassOrientation;
            public int grassBend;
        };
        private Textures _textures;

        //-----------------------------------------------------------------------------------------
        public Listing_07L04_07L06() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Grass", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            
            int vertexShader   = Shader.Load( Program.BasePath + @"Source\Listing_07L04_07L06\grass.vs.glsl",
                                               ShaderType.VertexShader );
            int fragmentShader = Shader.Load( Program.BasePath + @"Source\Listing_07L04_07L06\grass.fs.glsl", 
                                               ShaderType.FragmentShader );

            _shaderProgram = Shader.Link( new int[] { vertexShader, fragmentShader }, 2 );
            
            _vao = GL.GenVertexArray();
            GL.BindVertexArray( _vao );

            _mvpMatrixUniformLoc = GL.GetUniformLocation( _shaderProgram, "mvpMatrix" );

            GL.ActiveTexture( TextureUnit.Texture1 );
            KTX.Load( Program.BasePath + @"Media\Textures\grass_length.ktx", ref _textures.grassLength );

            GL.ActiveTexture( TextureUnit.Texture2 );
            KTX.Load( Program.BasePath + @"Media\Textures\grass_orientation.ktx", ref _textures.grassOrientation );

            GL.ActiveTexture( TextureUnit.Texture3 );
            KTX.Load( Program.BasePath + @"Media\Textures\grass_color.ktx", ref _textures.grassColor );

            GL.ActiveTexture( TextureUnit.Texture4 );
            KTX.Load( Program.BasePath + @"Media\Textures\grass_bend.ktx", ref _textures.grassBend );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            _grassBuffer = GL.GenBuffer();
            GL.BindBuffer( BufferTarget.ArrayBuffer, _grassBuffer );
            GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * Statics.grassBlade.Length),
                           Statics.grassBlade, BufferUsageHint.StaticDraw );

            GL.VertexAttribPointer( 0, 2, VertexAttribPointerType.Float, false, 0, 0 );
            GL.EnableVertexAttribArray( 0 );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitBuffers();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderProgram );
            GL.DeleteVertexArray( _vao );
            GL.DeleteTexture( _textures.grassBend );
            GL.DeleteTexture( _textures.grassColor );
            GL.DeleteTexture( _textures.grassLength );
            GL.DeleteTexture( _textures.grassOrientation );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float t = (float) Program.ElapsedTimeSeconds * 0.02f;
            float r = 550.0f;

            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );
            GL.ClearBuffer( ClearBuffer.Depth, 0, Statics.one );

            Matrix4 mvpMatrix = Matrix4.LookAt( new Vector3( (float)Math.Sin( t ) * r, 25.0f, (float)Math.Cos( t ) * r ),
                                                new Vector3( 0.0f, -50.0f, 0.0f ),
                                                new Vector3( 0.0f, 1.0f, 0.0f ) );

            mvpMatrix *= Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 45.0f ), (float)Width / (float)Height, 0.1f, 1000.0f );

            GL.UseProgram( _shaderProgram );
            GL.UniformMatrix4( _mvpMatrixUniformLoc, false, ref mvpMatrix );

            GL.Enable( EnableCap.DepthTest );
            GL.DepthFunc( DepthFunction.Lequal );

            GL.Viewport( 0, 0, Width, Height );

            GL.BindVertexArray( _vao );

            GL.DrawArraysInstanced( PrimitiveType.TriangleStrip, 0, 6, 1024 * 1024 );
            
            SwapBuffers();
        }
    }
}
