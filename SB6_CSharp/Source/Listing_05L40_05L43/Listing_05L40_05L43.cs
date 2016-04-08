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

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_05L40_05L43 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorBlack = { 0.0f, 0.0f, 0.0f, 1.0f };
        }

        private struct ShaderPrograms
        {
            public int Name;
        }
        private ShaderPrograms _shaderPrograms;

        private struct Buffers
        {
            public int rainBuffer;
        }
        private Buffers _buffer;

        private struct Textures
        {
            public int Name;
        }
        private Textures _textures;

        private int _vao;

        private static int _dropletSeed = 0x13371337;
        private float[] _dropletXOffset = new float[256];
        private float[] _dropletRotSpeed = new float[256];
        private float[] _dropletFallSpeed = new float[256];

        //-----------------------------------------------------------------------------------------
        public static float _RandomFloat()
        {
            int   tmp;

            _dropletSeed *= 16807;
            tmp = _dropletSeed ^ (_dropletSeed >> 4) ^ (_dropletSeed << 15);

            byte[] res = System.BitConverter.GetBytes( (uint)((tmp >> 9) | 0x3F800000) );

            return (System.BitConverter.ToSingle( res, 0 ) - 1.0f);
        }

        //-----------------------------------------------------------------------------------------
        public Listing_05L40_05L43() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Alien Rain", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int[] shaders = new int[2];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L40_05L43\render.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L40_05L43\render.fs.glsl", 
                                                ShaderType.FragmentShader );

            _shaderPrograms.Name = Framework.Shader.Link( shaders, shaders.Length );

            return true;
        }
 
        //-----------------------------------------------------------------------------------------
        private bool _InitVao()
        {
            // Create VAO object to hold vertex shader inputs and attach it to our context. As 
            // OpenGL requires the VAO object (whether or not it's used) we do this here.
            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitTextures()
        {
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\aliens.ktx", ref _textures.Name );
            GL.BindTexture( TextureTarget.Texture2DArray, _textures.Name );

            int filter = (int)All.LinearMipmapLinear;
            GL.TexParameterI( TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, ref filter );

            return true;
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
            GL.GenBuffers( 1, out _buffer.rainBuffer );
            GL.BindBuffer( BufferTarget.UniformBuffer, _buffer.rainBuffer );
            GL.BufferData( BufferTarget.UniformBuffer, (IntPtr)(sizeof(float) * 4 * 256), 
                           IntPtr.Zero, BufferUsageHint.DynamicDraw );
            
            return true;
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            _InitProgram();
            _InitVao();
            _InitTextures();
            _InitBuffers();

            GL.Enable( EnableCap.Blend );
            GL.BlendFunc( BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha );

            for( int i = 0; i < 256; i++ )
            {
                _dropletXOffset[i]   = _RandomFloat() * 2.0f - 1.0f;
                _dropletRotSpeed[i]  = (_RandomFloat() + 0.5f) * (((i & 1) == 1) ? -3.0f : 3.0f);
                _dropletFallSpeed[i] = _RandomFloat() + 0.2f;
            }
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderPrograms.Name );
            GL.DeleteTexture( _textures.Name );
            GL.DeleteBuffer( _buffer.rainBuffer );
            GL.DeleteVertexArray( _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float currentTime = (float)Program.ElapsedTimeSeconds;

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );

            GL.UseProgram( _shaderPrograms.Name );

            GL.BindBufferBase( BufferRangeTarget.UniformBuffer, 0, _buffer.rainBuffer );
            
            IntPtr ptr = GL.MapBufferRange( BufferTarget.UniformBuffer, IntPtr.Zero, 
                                            (IntPtr)(sizeof(float) * 4 * 256), 
                                            (BufferAccessMask) BufferAccessMask.MapInvalidateBufferBit 
                                                               | BufferAccessMask.MapWriteBit );           
            float[] droplet = new float[4];
            for( int i = 0; i < 256; i++ )
            {
                droplet[0] = _dropletXOffset[i];
                droplet[1] = 2.0f - (((currentTime + (float)i) * _dropletFallSpeed[i]) % 4.31f);
                droplet[2] = currentTime * _dropletRotSpeed[i];
                droplet[3] = 0.0f;

                System.Runtime.InteropServices.Marshal.Copy( droplet, 0, 
                                                             ptr + (sizeof(float) * droplet.Length * i), 
                                                             droplet.Length );
            }

            GL.UnmapBuffer( BufferTarget.UniformBuffer );

            for( int alienIndex = 0; alienIndex < 256; alienIndex++ )
            {
                GL.VertexAttribI1( 0, alienIndex );
                GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );
            }

            SwapBuffers();
        }

    }
}
