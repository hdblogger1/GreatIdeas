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
            // *** Just uncomment the example you wish to run ***
            
            //using(var example = new Example_02L01() )
            //using(var example = new Example_02L02() )
            //using(var example = new Example_02L03_02L07() )
            //using(var example = new Example_02L08_02L09() )
            using(var example = new Example_03L01_03L02() )
            {
                string version_string = GL.GetString(StringName.Version);
                Console.WriteLine( version_string );

                //Run the example
                example.Run();
            }
        }
    }
}
