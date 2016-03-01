using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OGL_SP
{
    class Test : GameWindow
    {

        //-----------------------------------------------------------------------------------------
        public Test( int majorVersionRequested, int minorVersionRequested ) 
            : base(640, 480, GraphicsMode.Default, "OpenTK Test Window", 0, DisplayDevice.Default
                   , majorVersionRequested, minorVersionRequested, GraphicsContextFlags.Default)
        {
            this.CheckGLVersion(majorVersionRequested, minorVersionRequested);
            Console.WriteLine( GL.GetString(StringName.Version) );
        }

        //-----------------------------------------------------------------------------------------
        public float MakeVersion( int major, int minor )
        {
            return Convert.ToSingle(String.Format("{0:d}.{1:d}", major, minor));
        }

        //-----------------------------------------------------------------------------------------
        public bool CheckGLVersion( int majorVersionRequire, int minorVersionRequire )
        {
            int majorVersionContext;
            int minorVersionContext;
            GL.GetInteger(GetPName.MajorVersion, out majorVersionContext);
            GL.GetInteger(GetPName.MinorVersion, out minorVersionContext);
            
            Console.WriteLine("OpenGL Version Needed {0:d}.{1:d} ({2:d}.{3:d} Found)",
                              majorVersionRequire, minorVersionRequire,
                              majorVersionContext, minorVersionContext);
            
            return MakeVersion(majorVersionContext, minorVersionContext)
                     >= MakeVersion(majorVersionRequire, majorVersionRequire);
        }

        //-----------------------------------------------------------------------------------------
        public bool CheckExtension( string extensionName )
        {
            int extensionCount;
            GL.GetInteger(GetPName.NumExtensions, out extensionCount);
            for(int i=0; i < extensionCount; i++)
            {
                if(GL.GetString(StringNameIndexed.Extensions, i) == extensionName) { return true; }
            }
            Console.WriteLine("Faild to find Extension: {0}", extensionName);
            return false;
        }

        //-----------------------------------------------------------------------------------------
        public bool CheckProgram( int programName )
        {
            if( programName == 0 ) { return false; }

            int result;
            GL.GetProgram(programName, GetProgramParameterName.LinkStatus, out result);

            int infoLogLength;
            GL.GetProgram(programName, GetProgramParameterName.InfoLogLength, out infoLogLength);
            if( infoLogLength > 0 )
            {
                string infoLogString;
                GL.GetProgramInfoLog(programName, out infoLogString);
                Console.WriteLine(infoLogString);
            }
            return result != 0;
        }

        //-----------------------------------------------------------------------------------------
        public string LoadShader( string fileName )
        {
            string tmp = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            using(System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
