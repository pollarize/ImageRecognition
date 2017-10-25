using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PixelClusteringProgram
{
    class TestPixelClustering
    {
        static void Main(string[] args)
        {
            if (args.Length > 3)
            {

                PixelClusterEngine pixelEngine = new PixelClusterEngine(args[0], int.Parse(args[1]));
                pixelEngine.CalculateClusterCenters();
                pixelEngine.CreateSegmentationImage(args[2]);
            }
            else
            {
                Console.WriteLine("Invalid arguments...");
                Console.WriteLine("Number of clusters:");
                int clusters = int.Parse(Console.ReadLine());
                Console.WriteLine("Input Image path:");
                string input = Console.ReadLine();
                Console.WriteLine("Output Image path:");
                string output = Console.ReadLine();

                PixelClusterEngine pixelEngine = new PixelClusterEngine(input, clusters);
                pixelEngine.CalculateClusterCenters();
                pixelEngine.CreateSegmentationImage(output);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
