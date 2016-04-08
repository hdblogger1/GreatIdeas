//=================================================================================================
// The code herein has been adapted from the book "OpenGL SuperBible - Sixth Edition" and its
// accompanying C++ example source code. Please see 'Copyright_SB6.txt' for copyright information.
//=================================================================================================
using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

using Gnu.Getopt;

namespace SB6_CSharp
{
    //=============================================================================================
    /// <summary>
    /// Defines the entry point of our application, which initializes OpenTK, creates an 
    /// instance of our example application class and calls its Run() method, which implements the
    /// application's main execution loop.
    /// </summary>
    public class Program
    {
        //Counter for holding elapsed time since application startup
        private static Stopwatch _counter = Stopwatch.StartNew();
        public static double ElapsedTimeSeconds 
        { 
            get { return _counter.ElapsedMilliseconds / 1000.0; } 
        }

        private static string _basePath = @".\";
        public static string BasePath { get { return _basePath; } }

        //-----------------------------------------------------------------------------------------
        private static void _ParseOptions( ref string[] args )
        {
            int c;
            Getopt g = new Getopt( System.AppDomain.CurrentDomain.FriendlyName, args, "b:" );
            while( (c = g.getopt()) != -1 )
            {
                switch( c )
                {
                    case 'b':
                        _basePath = g.Optarg;
                        break;
                    
                    default:
                        Console.WriteLine( "getopt() returned unknown option '" + c + "'" );
                        break;
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        [STAThread]
        static void Main(string[] args)
        {           
            // Initialize OpenTK and make sure it's 'Disposed' of properly.
            using( Toolkit.Init() )
            {
                _ParseOptions( ref args );

                //     **************************************************
                //     *** Just uncomment the example you wish to run ***
                //     **************************************************
                
                // Initialize our Example window and make sure it's 'Disposed' of properly.

                //using( var example = new BasicAtomicCounters() )
                //using( var example = new DebugMessages() )
                //using( var example = new SBM6ModelRenderer() )
                //using( var example = new Tunnel() )
                //using( var example = new WrapModes() )

                //using( var example = new Listing_02L01() )
                //using( var example = new Listing_02L02() )
                //using( var example = new Listing_02L03_02L07() )
                //using( var example = new Listing_02L08_02L09() )
                //using( var example = new Listing_03L01_03L02() )
                //using( var example = new Listing_03L03_03L04() )
                //using( var example = new Listing_03L05_03L06() )
                //using( var example = new Listing_03L07_03L08() )
                //using( var example = new Listing_03L09() )
                //using( var example = new Listing_03L10() )
                //using( var example = new Listing_03L11_03L12() )
                //using( var example = new Listing_05L01_05L05() )
                //using( var example = new Listing_05L06_05L07() )
                //using( var example = new Listing_05L08() )
                //using( var example = new Listing_05L09_05L17A() )
                //using( var example = new Listing_05L09_05L17B() )
                //using( var example = new Listing_05L20_05L25() )
                //using( var example = new Listing_05L26() )
                //using( var example = new Listing_05L27_05L28() )
                //using( var example = new Listing_05L29_05L32() )
                //using( var example = new Listing_05L33_05L35() )
                //using( var example = new Listing_05L36_05L37() )
                //using( var example = new Listing_05L38_05L39() )
                //using( var example = new Listing_05L40_05L43() )
                //using( var example = new Listing_05L44() )
                //using( var example = new Listing_05L45_05L46() )
                //using( var example = new Listing_06L01_06L04() )
                //using( var example = new Listing_06L05_06L06() )
                //using( var example = new Listing_06L07() )
                //using( var example = new Listing_07L02_07L03() )
                //using( var example = new Listing_07L04_07L06() )
                //using( var example = new Listing_07L08_07L09() )
                //using( var example = new Listing_07L11_07L15() )
                using( var example = new Listing_07L16_07L19() )
                {
                    string strVersion = GL.GetString( StringName.Version );
                    Console.WriteLine( strVersion );
                    example.Run();
                }
            }
        }
    }
}
