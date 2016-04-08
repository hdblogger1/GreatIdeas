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
    class Tunnel : GameWindow
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

        private struct Uniforms
        {
            public int MVP;
            public int Offset;
        }
        private Uniforms _uniforms;

        private struct Textures
        {
            public int Wall;
            public int Ceiling;
            public int Floor;
        }
        private Textures _textures;

        private int _vao;

        //-----------------------------------------------------------------------------------------
        public Tunnel() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Tunnel", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int[] shaders = new int[2];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Extra_Tunnel\render.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Extra_Tunnel\render.fs.glsl", 
                                                ShaderType.FragmentShader );

            _shaderPrograms.Name = Framework.Shader.Link( shaders, shaders.Length );

            // Get uniform locations
            _uniforms.MVP    = GL.GetUniformLocation( _shaderPrograms.Name, "mvp" );
            _uniforms.Offset = GL.GetUniformLocation( _shaderPrograms.Name, "offset" );

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
        private void _InitTextures()
        {
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\brick.ktx", ref _textures.Wall );
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\ceiling.ktx", ref _textures.Ceiling );
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\floor.ktx", ref _textures.Floor );

            int[] textures = new int[] { _textures.Wall, _textures.Ceiling, _textures.Floor };
            foreach( int tex in textures )
            {
                GL.BindTexture( TextureTarget.Texture2D, tex );
                int[] filters = new int[] { (int)TextureMinFilter.LinearMipmapLinear, (int)TextureMinFilter.Linear };
                GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref filters[0] );
                GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref filters[1] );
            }

            //GL.BindVertexArray( _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitProgram();
            this._InitVao();
            this._InitTextures();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderPrograms.Name );
            GL.DeleteTexture( _textures.Ceiling );
            GL.DeleteTexture( _textures.Wall );
            GL.DeleteTexture( _textures.Floor );
            GL.DeleteVertexArray( _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float currentTime = (float)Program.ElapsedTimeSeconds;

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorBlack );

            GL.UseProgram( _shaderPrograms.Name );

            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 60.0f ), 
                                                                       Width / Height, 0.1f, 100.0f );
            GL.Uniform1( _uniforms.Offset, currentTime * 0.003f );

            int[] textures = new int[] 
            { 
                _textures.Wall, 
                _textures.Floor, 
                _textures.Wall, 
                _textures.Ceiling 
            };

            for( uint i = 0; i < 4; i++ )
            {
                Matrix4 mvMatrix = Matrix4.CreateScale( 30.0f, 1.0f, 1.0f )
                                   * Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 1.0f, 0.0f), 
                                                                  MathHelper.DegreesToRadians( 90.0f) )
                                   * Matrix4.CreateTranslation( -0.5f, 0.0f, -10.0f )
                                   * Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 0.0f, 1.0f), 
                                                                  MathHelper.DegreesToRadians( 90.0f * (float)i ) );
                Matrix4 mvp = mvMatrix * projMatrix;

                GL.UniformMatrix4( _uniforms.MVP, false, ref mvp );

                GL.BindTexture( TextureTarget.Texture2D, textures[i] );
                GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );
            }

            SwapBuffers();
        }
    }
}
