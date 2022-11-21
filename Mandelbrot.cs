using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Mandelbrot
{
    internal class Mandelbrot
    {
        static string pathCsv = @"..\..\output\mandelbrot.csv";
        static string pathPng = @"..\..\output\mandelbrot.png";
        static string pathColorPalettes = @"..\..\output\colorPalettes.csv";

        static bool showProgress = true;
        static DateTime start;
        static void Main(string[] args)
        {
            MainMenu();
            Console.ReadLine();
        }


        static void MainMenu()
        {
            int width;
            int height;
            int threads;
            decimal posX;
            decimal posY;
            decimal zoom;
            Color[] colorPalette;
            posX = 0; posY = 0; zoom = (decimal)4.0; //default

            Console.WriteLine("***** Welcome to the Mandelbrot Bakery *****");
            Console.WriteLine();
            Console.Write("Mandelbrot Width (default: 1920px): ");
            string inputWidth = Console.ReadLine();
            if (inputWidth == "") { width = 1920; }
            else
            {
                width = Int32.Parse(inputWidth);
            }
            height = width / 16 * 9;
            Console.Write("Mandelbrot Height (default: " + height +"px): ");
            string inputHeight = Console.ReadLine();
            if (inputHeight != "") { height = Int32.Parse(inputHeight); }
            Console.WriteLine();
            Console.WriteLine("a) Singlethreaded Mandelbrot CSV");
            Console.WriteLine("b) Singlethreaded Mandelbrot PNG");
            Console.WriteLine("c) Multi-Threaded Mandelbrot PNG");
            Console.Write("selection: ");
            string singleMulti = Console.ReadLine();
            switch (singleMulti)
            {
                case "a":
                    Console.WriteLine("A fresh {0}x{1} Mandelbrot is being prepared.", width, height);
                    start = DateTime.Now;
                    writeToCSV(MandelbrotMatrix(width, height));
                    break;
                case "b":
                    Console.WriteLine("A fresh {0}x{1} Mandelbrot is being prepared.", width, height);
                    Console.Clear();
                    start = DateTime.Now;
                    MandelbrotToPNG(width, height);
                    break;
                case "c":
                    Console.Clear();
                    Color[][] allColorPalettes = CsvParser.readColorPalettes(pathColorPalettes);
                    Color[] inputColorPalette;
                    MandelbrotInThreads myMandelbrot = new MandelbrotInThreads();
                    Console.WriteLine("How many threads (default: 4): ");
                    string inputThreads = Console.ReadLine();
                    if (inputThreads == "") { threads = 4; }
                    else { threads = Int32.Parse(inputThreads); }
                    Console.WriteLine("Which Color Palette? (0-" + (allColorPalettes.Length-1) + ")(default: 0): ");
                    string inputColorPaletteId = Console.ReadLine();
                    if (inputColorPaletteId == "") { inputColorPalette = allColorPalettes[0]; }
                    else { inputColorPalette = allColorPalettes[Int32.Parse(inputColorPaletteId)]; }
                    Console.WriteLine("a) default Mandelbrot Set");
                    Console.WriteLine("b) an interesting set");
                    Console.WriteLine("c) another interesting set");
                    Console.WriteLine("d) custom coordinates");
                    string inputCoordinates = Console.ReadLine();
                    switch (inputCoordinates)
                    {
                        case "a":
                            //just use default
                            break;
                        case "b":
                            posX = (decimal)-1.789; posY = 0; zoom = (decimal)0.00000000000001;
                            break;
                        case "c":
                            posX = (decimal)-0.903019109095; posY = (decimal)0.2619000001; zoom = (decimal)0.00000000001;
                            break;
                        case "d":
                            Console.WriteLine("not yet implemented. Using default...");
                            break;
                    }
                    Console.WriteLine("A fresh {0}x{1} Mandelbrot is being prepared.", width, height);
                    start = DateTime.Now;
                    myMandelbrot.mandelbrotInThreads(width, height, zoom, posX, posY, threads, inputColorPalette, pathPng);

                    break;
                default:
                    Console.WriteLine("Invald selection.");
                    break;
            }
            System.Console.WriteLine("Done. That took " + Convert.ToInt32((DateTime.Now - start).TotalSeconds) + " Seconds.");
        }



        static int[][] MandelbrotMatrix(int width, int height)
        {
            int[][] mandelbrot = new int[height][];

            for (int y = 0; y < height; y++)
            {
                mandelbrot[y] = new int[width];
                for (int x = 0; x < width; x++)
                {
                    double x0 = (x - width / 2) * 4.0 / width;
                    double y0 = (y - height / 2) * 4.0 / width;
                    double xZ = 0.0;
                    double yZ = 0.0;
                    int iteration = 0;
                    int maxIteration = 1000;
                    while (xZ * xZ + yZ * yZ <= 2 * 2 && iteration < maxIteration)
                    {
                        double temp = xZ * xZ - yZ * yZ + x0;
                        yZ = 2 * xZ * yZ + y0;
                        xZ = temp;
                        iteration++;
                    }
                    mandelbrot[y][x] = iteration;
                    //Console.WriteLine(" (" + x + "," + y + ") :" + iteration.ToString());
                }
            }


            return mandelbrot;
        }
        static void writeToCSV(int[][] matrix)
        {
            string[] zeilen = new string[matrix.Length];
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    zeilen[i] += matrix[i][j];
                    if (j != matrix[i].Length - 1)
                    {
                        zeilen[i] += ";";
                    }
                }
            }
            try
            {
                File.WriteAllLines(pathCsv, zeilen);
            }
            catch (Exception)
            {
                throw;
            }
        }

        static void MandelbrotToPNG(int width, int height)
        {
            Bitmap output = new Bitmap(width, height, PixelFormat.Format16bppRgb555);

            int progressWidth = Console.WindowWidth - 20;
            int progressPosition = 1;
            TimeSpan progressTime;
            Console.SetCursorPosition(0, 1);
            Console.Write("[");
            Console.SetCursorPosition(progressWidth + 3, 1);
            Console.Write("]");


            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double x0 = (x - width / 2) * 4.0 / width;
                    double y0 = (y - height / 2) * 4.0 / width;
                    double xZ = 0.0;
                    double yZ = 0.0;
                    int iteration = 0;
                    int maxIteration = 1000;
                    while (xZ * xZ + yZ * yZ <= 2 * 2 && iteration < maxIteration)
                    {
                        double temp = xZ * xZ - yZ * yZ + x0;
                        yZ = 2 * xZ * yZ + y0;
                        xZ = temp;
                        iteration++;
                    }

                    Color pixel = ColorFromIterations(iteration, maxIteration);
                    output.SetPixel(x, y, pixel);
                }
                if (showProgress && y % (height / progressWidth) == 0)
                {
                    Console.SetCursorPosition(progressPosition, 1);
                    Console.Write("*");
                    progressPosition++;
                }
                if (showProgress && (y % (height / 10) == height / 10 - 1))
                {
                    Console.SetCursorPosition(0, 2);
                    int factor = y / (height / 10) + 1;
                    progressTime = DateTime.Now - start;
                    int estimatedTime = Convert.ToInt32(progressTime.TotalSeconds) * 10 / factor;
                    int remainingTime = estimatedTime - Convert.ToInt32(progressTime.TotalSeconds);
                    Console.WriteLine("Elapsed Time: " + Convert.ToInt32(progressTime.TotalSeconds) + " seconds");
                    Console.WriteLine("Estimated Time Remaining: " + remainingTime + " seconds");
                }
            }
            Console.WriteLine();
            output.Save(pathPng, ImageFormat.Png);
        }

        //only used for single-threaded PNG 
        static Color ColorFromIterations(int iterations, int maxIterations)
        {
            Color output = new Color();
            int colorIndex = iterations % 256;

            output = Color.FromArgb(BitmapPalettes.Halftone256.Colors[colorIndex].R, BitmapPalettes.Halftone256.Colors[colorIndex].G, BitmapPalettes.Halftone256.Colors[colorIndex].B);

            return output;
        }
    }
}
