using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    /// <summary>
    /// http://www.dwmkerr.com/importing-opengl-extensions-functions-with-wglgetprocaddress/
    /// </summary>
    class GLDirect
    {
        private static HashSet<string> _extentions = null;
        private static Dictionary<string, Delegate> extensionFunctions = new Dictionary<string, Delegate>();

        //-----------------------------------------------------------------------------------------
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary( string dllToLoad );

        //-----------------------------------------------------------------------------------------
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary( IntPtr hModule );

        //-----------------------------------------------------------------------------------------
        [DllImport("kernel32", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=true)]
        static extern IntPtr GetProcAddress( IntPtr hModule, string procName );

        //-----------------------------------------------------------------------------------------
        [DllImport("opengl32.dll")]  
        private static extern IntPtr wglGetProcAddress( string name );


        
        //-----------------------------------------------------------------------------------------
        [DllImport("opengl32.dll")]  
        private static extern void glEnable( uint cap );
        //.........................................................................................
        public static void Enable( uint cap ) { glEnable( cap ); }

        //-----------------------------------------------------------------------------------------
        [DllImport("opengl32.dll")]  
        private static extern void glDisable( uint cap );
        //.........................................................................................
        public static void Disable( uint cap ) { glDisable( cap ); }

        //-----------------------------------------------------------------------------------------
        [DllImport("opengl32.dll")]  
        private static extern byte glIsEnabled( uint cap );
        //.........................................................................................
        public static bool IsEnabled( uint cap ) { return glIsEnabled( cap ) == 1; }



        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DebugProc( uint source, uint type, uint id, uint severity, int length, 
                                        IntPtr message, IntPtr userParam );

        //-----------------------------------------------------------------------------------------
        private delegate void glDebugMessageCallback( [MarshalAs( UnmanagedType.FunctionPtr)] DebugProc callback,
                                                      IntPtr userParam );
        //.........................................................................................
        public static void DebugMessageCallback( DebugProc callback, IntPtr userParam )
        {
            _InvokeExtensionFunction<glDebugMessageCallback>( callback, userParam );
        }

        //-----------------------------------------------------------------------------------------
        private static object _InvokeExtensionFunction<T>( params object[] args )  
        {
            // Get the type of the extension function.
            Type delegateType = typeof(T);

            // Get the name of the extension function.
            string name = delegateType.Name;

            // Does the dictionary contain our extension function?
            Delegate del = null;
            if( extensionFunctions.ContainsKey( name ) == false )
            {
                // We haven't loaded it yet. Load it now.
                IntPtr proc = wglGetProcAddress( name );
                if( proc == IntPtr.Zero
                    || (int)proc == 1 || (int)proc == 2 || (int)proc == 3
                    || (int)proc == -1 )
                { 
                    // This may be an OpenGL 1.1 exported function so try loading it directly.
                    IntPtr hModule = LoadLibrary( "opengl32.dll" );
                    proc = GetProcAddress( hModule, name );
                    FreeLibrary( hModule );
                    if( proc == IntPtr.Zero )
                        { throw new Exception( "Extension function " + name + " not supported" ); }
                }

                // Get the delegate for the function pointer.
                del = Marshal.GetDelegateForFunctionPointer( (IntPtr)proc, delegateType );
                if( del == null )
                    { throw new Exception( "Extension function " + name + " not supported" ); }

                // Add to the dictionary.
                extensionFunctions.Add( name, del );
            }
            else
            {
                //  Get the delegate.
                del = extensionFunctions[name];
            }

            //  Try and invoke it.
            object result = null;
            try   { result = del.DynamicInvoke( args ); }
            catch { throw new Exception( "Cannot invoke extension function " + name ); }

            return result;
        }

        public static HashSet<string> Extentions()
        {
            if( _extentions == null )
            {
                _extentions = new HashSet<string>();
                // For OpenGL 3.0 and higher glGetStringi() must be used
                int count = GL.GetInteger( GetPName.NumExtensions );
                for( int i = 0; i < count; i++ )
                    { _extentions.Add( GL.GetString( StringNameIndexed.Extensions, i ) ); }
            }
            return _extentions;
        }

        public static bool IsExtAvailable(string ext)
        {
            return Extentions().Contains(ext);
        }

        public static void PrintAllExtentions()
        {
            foreach( var ext in Extentions() ) { Console.WriteLine(ext); }
        }
    }
}
