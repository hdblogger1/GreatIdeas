using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SB6_CSharp
{
    public class Program
    {
        //Counter for holding elapsed time since application startup
        public static Stopwatch Counter = Stopwatch.StartNew();

        static void Main(string[] args)
        {
            //make sure any OS resource is cleaned up after our sub-classed GameWindow example's
            //Run() method returns.

            using(Toolkit.Init())
            {
                // *** Just uncomment the example you wish to run ***
            
                //using(var example = new Example_02L01() )
                //using(var example = new Example_02L02() )
                //using(var example = new Example_02L03_02L07() )
                //using(var example = new Example_02L08_02L09() )
                //using(var example = new Example_03L01_03L02() )
                //using(var example = new Example_03L03_03L04() )
                //using(var example = new Example_03L05_03L06() )
                //using(var example = new Example_03L07_03L08() )
                //using(var example = new Example_03L09() )
                //using(var example = new Example_03L10() )
                //using(var example = new Example_03L11_03L12() )
                //using(var example = new Example_05L01_05L05() )
                //using(var example = new Example_05L06_05L07() )
                //using(var example = new Example_05L08() )
                //using(var example = new Example_05L09_05L17() )
                using(var example = new Example_05L20_05L25() )
                //using(var example = new Example_05L26() )
                {
                    string strVersion = GL.GetString(StringName.Version);
                    Console.WriteLine( strVersion );

                    //Run the example
                    example.Run();
                }
                
            }
        }
    }
}
