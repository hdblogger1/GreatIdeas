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

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Our OpenTK GameWindow derived application class which takes care of creating a window, 
    /// handling input, and displaying the rendered results to the user.
    /// </summary>
    class Listing_05L44 : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        private struct ShaderPrograms
        {
            public int LoadAndStore;
            public int Display;
        }
        private ShaderPrograms _shaderPrograms;

        private struct Textures
        {
            public int InputImage;
            public int OutputImage;
        }
        private Textures _textures;

        private int _vao;

        //-----------------------------------------------------------------------------------------
        public Listing_05L44() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - Texture Loads and Stores", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }
        
        //-----------------------------------------------------------------------------------------
        private void _InitTextures()
        {
            // Load our input texture image
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\Tree.ktx", ref _textures.InputImage );

            // Allocate our output texture image
            GL.GenTextures( 1, out _textures.OutputImage );
            GL.BindTexture( TextureTarget.Texture2D, _textures.OutputImage );
            GL.TexStorage2D( TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba32f, 928, 906 );
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            int[] shaders = new int[3];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L44\loadstoredisplay.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L44\loadstore.fs.glsl", 
                                                ShaderType.FragmentShader );
            shaders[2] = Framework.Shader.Load( Program.BasePath + @"Source\Listing_05L44\display.fs.glsl", 
                                                ShaderType.FragmentShader );
            
            _shaderPrograms.LoadAndStore = Framework.Shader.Link( new int[] { shaders[0], shaders[1] }, 2 );
            _shaderPrograms.Display      = Framework.Shader.Link( new int[] { shaders[0], shaders[2] }, 2 );

            GL.GenVertexArrays( 1, out _vao );
            GL.BindVertexArray( _vao );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnLoad( EventArgs e )
        {
            this._InitTextures();
            this._InitProgram();
        }

        //-----------------------------------------------------------------------------------------
        protected override void OnUnload( EventArgs e )
        {
            GL.DeleteProgram( _shaderPrograms.Display );
            GL.DeleteProgram( _shaderPrograms.LoadAndStore );
            GL.DeleteVertexArrays( 1, ref _vao );
            GL.DeleteTextures( 1, ref _textures.InputImage );
            GL.DeleteTextures( 1, ref _textures.OutputImage );
        }
        
        //-----------------------------------------------------------------------------------------
        //Our rendering function
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.UseProgram( _shaderPrograms.LoadAndStore );

            GL.ActiveTexture( TextureUnit.Texture0 );
            GL.BindTexture( TextureTarget.Texture2D, _textures.InputImage );
            GL.BindImageTexture( 0, _textures.InputImage, 0, false, 0, 
                                 TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f );
            
            GL.ActiveTexture( TextureUnit.Texture1 );
            GL.BindTexture( TextureTarget.Texture2D, _textures.OutputImage );
            GL.BindImageTexture( 1, _textures.OutputImage, 0, false, 0, 
                                 TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f );
            
            GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );

            GL.UseProgram( _shaderPrograms.Display );

            GL.ActiveTexture( TextureUnit.Texture0 );
            GL.BindTexture( TextureTarget.Texture2D, _textures.OutputImage );

            GL.DrawArrays( PrimitiveType.TriangleStrip, 0, 4 );

            SwapBuffers();
        }
    }
}
