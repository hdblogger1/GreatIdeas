using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

        //-----------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            // Initialize OpenTK and make sure it's 'Disposed' of properly.
            using( Toolkit.Init() )
            {
                //     **************************************************
                //     *** Just uncomment the example you wish to run ***
                //     **************************************************
                
                // Initialize our Example window and make sure it's 'Disposed' of properly.

                //using( var example = new BasicAtomicCounters() )
                //using( var example = new Example_02L01() )
                //using( var example = new Example_02L02() )
                //using( var example = new Example_02L03_02L07() )
                //using( var example = new Example_02L08_02L09() )
                //using( var example = new Example_03L01_03L02() )
                //using( var example = new Example_03L03_03L04() )
                //using( var example = new Example_03L05_03L06() )
                //using( var example = new Example_03L07_03L08() )
                //using( var example = new Example_03L09() )
                //using( var example = new Example_03L10() )
                //using( var example = new Example_03L11_03L12() )
                //using( var example = new Example_05L01_05L05() )
                //using( var example = new Example_05L06_05L07() )
                //using( var example = new Example_05L08() )
                using( var example = new Example_05L09_05L17() )
                //using( var example = new Example_05L20_05L25() )
                //using( var example = new Example_05L26() )
                //using( var example = new Example_05L27_05L28() )
                //using( var example = new Example_05L29_05L32() )
                {
                    string strVersion = GL.GetString( StringName.Version );
                    Console.WriteLine( strVersion );
                    example.Run();
                }
                
            }
        }
    }
}
