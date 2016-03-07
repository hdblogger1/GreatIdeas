using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp.Framework
{
    using IOException = System.IO.IOException;
    using IOReader = System.IO.StreamReader;

    static public class Shader
    {
        private class ShaderFileException : Exception
        {
            public ShaderFileException() { }
            public ShaderFileException( string msg ) : base( msg ) { }
            public ShaderFileException( string msg, Exception inner ) : base( msg, inner ) { }
        }

        //-------------------------------------------------------------------------------------
        static public uint Load( string filename, ShaderType shaderType, bool fCheckErrors =true )
        {
            int shaderName = 0;
            IOReader file = null;

            try
            {
                file = new IOReader( filename );
                string shaderText = file.ReadToEnd();

                shaderName = GL.CreateShader( shaderType );
                if( shaderName == 0 ) 
                    { throw new ShaderFileException( "Failed allocating memory for shader object." ); }

                GL.ShaderSource( shaderName, shaderText );
                GL.CompileShader( shaderName );

                if( fCheckErrors )
                {
                    int status;
                    GL.GetShader( shaderName, ShaderParameter.CompileStatus, out status );
                        
                    if( status == 0 )
                    {
                        throw new ShaderFileException( GL.GetShaderInfoLog( shaderName ) );
                    }
                }
            }
            catch( IOException e )
            {
                Console.WriteLine( "Shader.File IO Exception: " + e.Message );
            }
            catch( ShaderFileException e )
            {
                GL.DeleteShader( shaderName );
                shaderName = 0;
                Console.WriteLine( "Shader.File.Load Exception: " + e.Message );
            }

            if( file != null ) { file.Close(); }

            return (uint)shaderName;
        }

        //-------------------------------------------------------------------------------------
        static public uint Link( int[] shaders, 
                                    int count, 
                                    bool fDeleteShaders =true, 
                                    bool fCheckErrors =true )
        {
            int shaderProgramName = 0;

            try
            {
                shaderProgramName = GL.CreateProgram();

                for( int i = 0; i < count; i++ )
                    { GL.AttachShader( shaderProgramName, shaders[i] ); }

                GL.LinkProgram( shaderProgramName );

                if( fCheckErrors )
                {
                    int status;
                    GL.GetProgram( shaderProgramName, GetProgramParameterName.LinkStatus, out status );

                    if( status == 0 )
                    {
                        throw new ShaderFileException( GL.GetShaderInfoLog( shaderProgramName ) );
                    }
                }

                if( fDeleteShaders )
                {
                    for( int i = 0; i < count; i++ ) { GL.DeleteShader( shaders[i] ); }
                }
            }
            catch( ShaderFileException e )
            {
                Console.WriteLine( "Shader.File.Link Exception: " + e.Message );
            }

            return (uint)shaderProgramName;
        }
    }
}
