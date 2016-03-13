using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class SBM6ModelRenderer : GameWindow
    {
        //-----------------------------------------------------------------------------------------
        /// <summary>
        /// The Statics container class holding class-wide static globals
        /// </summary>
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.25f, 0.0f, 1.0f };
            public static readonly float[] one = { 1.0f };
        }
        
        //-----------------------------------------------------------------------------------------
        private uint _shaderProgramName;

        private int _mvLocation;
        private int _projLocation;

        private uint _texColorName;
        private uint _texNormalName;

        private Framework.SBM6Model _model;

        //-----------------------------------------------------------------------------------------
        public SBM6ModelRenderer() 
            : base( 800, 600, GraphicsMode.Default, "OpenGL SuperBible - SBM6 Model Rendering", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {
            uint[] shaders = new uint[2];
                
            shaders[0] = Framework.Shader.Load( Program.BasePath + @"Source\SBM6ModelRenderer\render.vs.glsl", 
                                                ShaderType.VertexShader );
            shaders[1] = Framework.Shader.Load( Program.BasePath + @"Source\SBM6ModelRenderer\render.fs.glsl", 
                                                ShaderType.FragmentShader );

            _shaderProgramName = Framework.Shader.Link( shaders, shaders.Length );

            // Get uniform locations
            _mvLocation   = GL.GetUniformLocation( _shaderProgramName, "mv_matrix" );
            _projLocation = GL.GetUniformLocation( _shaderProgramName, "proj_matrix" );

            return true;
        }
 
        //-----------------------------------------------------------------------------------------
        private bool _InitModels()
        {
            _model = new Framework.SBM6Model();
            _model.Load( Program.BasePath + @"Media\Objects\ladybug.sbm" );
            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitTextures()
        {
            GL.ActiveTexture( TextureUnit.Texture0 );
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\ladybug_co.ktx", ref _texColorName );
          
            GL.ActiveTexture( TextureUnit.Texture1 );
            Framework.KTX.Load( Program.BasePath + @"Media\Textures\ladybug_nm.ktx", ref _texNormalName );
       
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
            _model.Delete();
            GL.DeleteTexture( _texColorName );
            GL.DeleteTexture( _texNormalName );
            GL.DeleteProgram( _shaderProgramName );
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            float currentTime = (float)Program.ElapsedTimeSeconds;

            GL.Viewport( 0, 0, Width, Height );
            GL.ClearBuffer( ClearBuffer.Depth, 0, Statics.one );
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );

            GL.UseProgram( _shaderProgramName );

            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView( MathHelper.DegreesToRadians( 50.0f ), 
                                                                       Width / Height, 0.1f, 1000.0f );
            GL.UniformMatrix4( _projLocation, false, ref projMatrix );

            Matrix4 mvMatrix = Matrix4.Identity
                               * Matrix4.CreateFromAxisAngle( new Vector3( 0.0f, 1.0f, 0.0f), 
                                                              MathHelper.DegreesToRadians( (float)currentTime * 20.0f) )
                               * Matrix4.CreateTranslation( 0.0f, -0.5f, -7.0f );
            GL.UniformMatrix4( _mvLocation, false, ref mvMatrix );

            _model.Render();
            
            SwapBuffers();
        }
    }
}
