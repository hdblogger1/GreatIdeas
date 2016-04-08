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
    /// 
    /// </summary>
    class Listing_00L00_00L00 : GameWindow
    {
        static class Statics
        {
            public static readonly float[] colorGreen = { 0.0f, 0.25f, 0.0f, 1.0f };
        }
        
        private int _shaderProgram;
        private int _vao;

        //-----------------------------------------------------------------------------------------
        public Listing_00L00_00L00() 
            : base( 800, 600, GraphicsMode.Default, "SB6_CSharp - Template File", 
                    0, DisplayDevice.Default, 4, 3, GraphicsContextFlags.Default )
        {
        }

        //-----------------------------------------------------------------------------------------
        private bool _InitProgram()
        {

            _vao = GL.GenVertexArray();
            GL.BindVertexArray( _vao );

            return true;
        }
        
        //-----------------------------------------------------------------------------------------
        private bool _InitBuffers()
        {
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
        }
        
        //-----------------------------------------------------------------------------------------
        protected override void OnRenderFrame( FrameEventArgs e )
        {
            GL.ClearBuffer( ClearBuffer.Color, 0, Statics.colorGreen );
 
            GL.UseProgram( _shaderProgram );

            GL.DrawArrays( PrimitiveType.Triangles, 0, 3 );
            
            SwapBuffers();
        }
    }
}
