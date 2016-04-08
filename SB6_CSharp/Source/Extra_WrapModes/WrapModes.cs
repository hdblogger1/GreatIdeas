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
    class WrapModes : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.1f, 0.0f, 1.0f };
            public static readonly float[] colorYellow = { 0.4f, 0.4f, 0.0f, 1.0f };
            public static readonly int[] wrapModes = { (int)All.ClampToEdge, 
                                                       (int)All.Repeat, 
                                                       (int)All.ClampToBorder, 
                                                       (int)All.MirroredRepeat };
            public static readonly float[] offsets = { -0.5f, -0.5f,
                                                        0.5f, -0.5f,
                                                       -0.5f,  0.5f,
                                                        0.5f,  0.5f };
        }

        private struct ShaderPrograms
        {
            public int Name;
        }
        private ShaderPrograms _shaderPrograms;

        private struct Uniforms
        {
            public int Offset;
        }
        private Uniforms _uniforms;

        private struct Textures
        {
            public int Name;
        }
        private Textures _textures;

        private int _vao;

        //-----------------------------------------------------------------------------------------
        public WrapModes() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Texture Wrap Modes", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int[] shaders = new int[2];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Extra_WrapModes\render.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Extra_WrapModes\render.fs.glsl", 
                                                ShaderType.FragmentShader );

            _shaderPrograms.Name = Framework.Shader.Link( shaders, shaders.Length );

            // Get uniform locations
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
        private bool _InitTextures()
        {
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\rightarrows.ktx", ref _textures.Name );
            GL.BindTexture( TextureTarget.Texture2D, _textures.Name );

            return true;
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
            GL.DeleteTexture( _textures.Name );
            GL.DeleteVertexArray( _vao );
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float currentTime = (float)Program.ElapsedTimeSeconds;

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );

            GL.UseProgram( _shaderPrograms.Name );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, Statics.colorYellow );

            for( int i = 0; i < 4; i++ )
            {
                GL.Uniform2( _uniforms.Offset, 1, ref Statics.offsets[i * 2] );
                GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ref Statics.wrapModes[i] );
                GL.TexParameterI( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ref Statics.wrapModes[i] );

                GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );
            }

            SwapBuffers();
        }
    }
}
