using System;
using System.Diagnostics;
using OpenTK;

using Gnu.Getopt;

namespace OGL_SP
{
    public class Program
    {
        //Counter for holding elapsed time since application startup
        public static Stopwatch Counter = Stopwatch.StartNew();

        public static string BasePath = ".\\";

        static void ParseOptions( ref string[] args )
        {
            Getopt g = new Getopt( System.AppDomain.CurrentDomain.FriendlyName, args, "b:");

            int c;
            while((c = g.getopt()) != -1)
            {
                switch(c)
                {
                    case 'b':
                        Program.BasePath = g.Optarg;
                        break;
                    
                    default:
                        Console.WriteLine("getopt() returned " + c);
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            using(Toolkit.Init())
            {
                ParseOptions(ref args);

                // *** Just uncomment the example you wish to run ***
                using(var example = new GL430_Atomic_Counter())
                {
                    example.Run(); //Run the example
                }
            }
        }
    }
}
