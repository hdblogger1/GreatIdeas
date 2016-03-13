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
    class Example_05L38_05L39 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.2f, 0.0f, 1.0f };
            public static readonly float[] one = { 1.0f };
            private static uint B = 0x00000000;
            private static uint W = 0xFFFFFFFF;
            public static readonly uint[] textureData = 
            {
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
                B, W, B, W, B, W, B, W, B, W, B, W, B, W, B, W,
                W, B, W, B, W, B, W, B, W, B, W, B, W, B, W, B,
            };
        }

        private struct ShaderPrograms
        {
            public uint Name;
        }
        private ShaderPrograms _shaderPrograms;

        private struct Uniforms
        {
            public int MVLocation;
            public int ProjLocation;
        }
        private Uniforms _uniforms;

        private struct Textures
        {
            public uint[] Array;
        }
        private Textures _textures;
        private int _textureIndex = 0;

        private Framework.SBM6Model _model;

        //-----------------------------------------------------------------------------------------
        public Example_05L38_05L39() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Texture Coordinates", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            uint[] shaders = new uint[2];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Example_05L38_05L39\render.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Example_05L38_05L39\render.fs.glsl", 
                                                ShaderType.FragmentShader );

            _shaderPrograms.Name = Framework.Shader.Link( shaders, shaders.Length );

            // Get uniform locations
            _uniforms.MVLocation   = GL.GetUniformLocation( _shaderPrograms.Name, "mv_matrix" );
            _uniforms.ProjLocation = GL.GetUniformLocation( _shaderPrograms.Name, "proj_matrix" );

            return true;
        }
 
        //-----------------------------------------------------------------------------------------
        private void _InitTextures()
        {
            _textures.Array = new uint[2];

            // Generate a name for texture #1 and manually create it
            GL.GenTextures( 1, out _textures.Array[0] );
            GL.BindTexture( TextureTarget.Texture2D, _textures.Array[0] );
            GL.TexStorage2D( TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, 16, 16 );
            GL.TexSubImage2D( TextureTarget.Texture2D, 0, 0, 0, 16, 16, PixelFormat.Rgba, 
                              PixelType.UnsignedByte, Statics.textureData );
            int nearestFilter = (int)TextureMinFilter.Nearest;
            GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref nearestFilter );
            GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref nearestFilter );

            // Load texture #2 from file
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\pattern1.ktx", ref _textures.Array[1] );

//            GL.GenTextures( 1, out _textures.Array[0] );
//            GL.BindTexture( TextureTarget.Texture2D, _textures.Array[0] );
//            GL.TexStorage2D( TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, 0x400, 0x400 );
//            GL.TexSubImage2D( TextureTarget.Texture2D, 0, 0, 0, 0x400, 0x400, PixelFormat.Bgra, 
//                              PixelType.UnsignedByte, SB6Debug.ByteArrayFromOutput() );

        }

        //-----------------------------------------------------------------------------------------
        private bool _InitModels()
        {
            _model = new Framework.SBM6Model();
            _model.Load( Program.BasePath + @"Media\Objects\torus_nrms_tc.sbm" );
            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitModels();
            this._InitTextures();

            GL.Enable( EnableCap.DepthTest );
            GL.DepthFunc( DepthFunction.Lequal );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderPrograms.Name );
            GL.DeleteTextures( 2, _textures.Array );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnKeyPress( OpenTK.KeyPressEventArgs e )
        {
            if (e.KeyChar == 't' || e.KeyChar == 'T')
            {
                _textureIndex++;
                if( _textureIndex > 1 ) { _textureIndex = 0; }
            }
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float currentTime = (float)Program.ElapsedTimeSeconds;

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Depth, 0, Statics.one );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );

            GL.BindTexture( TextureTarget.Texture2D, _textures.Array[_textureIndex] );

            GL.UseProgram( _shaderPrograms.Name );

            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 60.0f ), 
                                                                       Width / Height, 0.1f, 1000.0f );
            Matrix4 mvMatrix = Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 0.0f, 1.0f), 
                                                              MathHelper.DegreesToRadians( (float)currentTime * 21.1f) )
                               * Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 1.0f, 0.0f), 
                                                              MathHelper.DegreesToRadians( (float)currentTime * 19.3f) )
                               * Matrix4.CreateTranslation( 0.0f, 0.0f, -3.0f );
            
            GL.UniformMatrix4( _uniforms.ProjLocation, false, ref projMatrix );
            GL.UniformMatrix4( _uniforms.MVLocation, false, ref mvMatrix );

            _model.Render();
            
            SwapBuffers();
        }
    }
}
