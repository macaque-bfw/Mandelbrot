using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

namespace Mandelbrot
{
    internal class MandelbrotInThreads
    {
        static int[][] mandelbrot;
        static int totalIterations;
        static int width;
        static int height;

        static decimal zoom;
        static decimal posX;
        static decimal posY;

        static int threadCount;

        static string pathPng = @"..\..\output\mandelbrot.png";
        static Bitmap output;
        static Bitmap[] bitmapsFromThreads;
        public int[][] mandelbrotInThreads(int widthUser, int heightUser, decimal zoomUser, decimal posXUser, decimal posYUser, int threadCountUser)
        {
            width = widthUser;
            height = heightUser;
            zoom = zoomUser;
            posX = posXUser;
            posY = posYUser;
            threadCount = threadCountUser;

            mandelbrot = new int[height][];
            output = new Bitmap(width, height, PixelFormat.Format16bppRgb555);
            totalIterations = 0;
            var threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                int copy = i;
                Thread thread = new Thread(() => MandelbrotThread(copy));
                thread.Start();
                thread.Name = "MandelbrotThread" + i;
                threads.Add(thread);
            }

            Console.WriteLine("MainThread blocking");
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Console.WriteLine("MandelbrotThreads finished");
            Console.WriteLine();
            Console.WriteLine("Converting Mandelbrot-Matrix to Bitmap");

            bitmapsFromThreads = new Bitmap[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int copy = i;
                Thread thread = new Thread(() => BitmapCreationThread(copy));
                thread.Name = "BitmapCreationThread" + copy;
                thread.Start();
                threads.Add(thread);
            }
            Console.WriteLine("MainThread blocking");
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            Console.WriteLine("BitmapCreationThreads finished");
            Console.WriteLine();

            Console.WriteLine("Saving Bitmap to PNG");
            Graphics g = Graphics.FromImage(output);
            int yPos = 0;
            for (int i = 0; i < bitmapsFromThreads.Length; i++)
            {
                g.DrawImage(bitmapsFromThreads[i], 0, yPos);
                yPos += bitmapsFromThreads[i].Height;
            }
            output.Save(pathPng, ImageFormat.Png);
            Console.WriteLine("PNG saved.");

            return mandelbrot;
        }


        static void MandelbrotThread(int thread)
        {
            Console.WriteLine("MandelbrotThread{0} doing work", thread);
            MandelbrotMatrix(width, height, threadCount, thread, posX, posY, zoom);
            Console.WriteLine("MandelbrotThread{0} done", thread);
        }
        static void BitmapCreationThread(int thread)
        {
            Console.WriteLine("BitmapCreationThread{0} doing work", thread);
            bitmapsFromThreads[thread] = MatrixToBitmap(mandelbrot, threadCount, thread);
            Console.WriteLine("BitmapCreationThread{0} done", thread);
        }


        //mandelbrot with decimals and zoom
        static int[][] MandelbrotMatrix(int width, int height, int threadCount, int thread, decimal posX, decimal posY, decimal zoom)
        {
            for (int y = 0; y < height; y++)
            {
                if (y % threadCount == thread)
                {
                    mandelbrot[y] = new int[width];
                    for (int x = 0; x < width; x++)
                    {
                        decimal x0 = (x - width / 2) * zoom / width + posX;
                        decimal y0 = (y - height / 2) * zoom / width + posY;
                        decimal xZ = (decimal)0.0;
                        decimal yZ = (decimal)0.0;
                        int iteration = 0;
                        int maxIteration = 1000;
                        while (xZ * xZ + yZ * yZ <= 2 * 2 && iteration < maxIteration)
                        {
                            decimal temp = xZ * xZ - yZ * yZ + x0;
                            yZ = 2 * xZ * yZ + y0;
                            xZ = temp;
                            iteration++;
                            totalIterations++;
                        }
                        mandelbrot[y][x] = iteration;
                        //Console.WriteLine("Thread"+thread+ " (" + x + "," + y + ") :" + iteration.ToString()); 
                    }
                }
                if (y % 10 == 0)
                {
                    int percentComplete = Convert.ToInt32(Convert.ToDouble(y) / Convert.ToDouble(height) * 100.0);
                    Console.WriteLine("MandelbrotThread" + thread + " (" + percentComplete + "%) " + totalIterations);
                }
            }


            return mandelbrot;
        }
        static Bitmap MatrixToBitmap(int[][] matrix, int threadCount, int thread)
        {
            int yStart = thread * matrix.Length / threadCount;
            int yEnd = yStart + (matrix.Length / threadCount);
            Bitmap output = new Bitmap(matrix[0].Length, matrix.Length / threadCount);
            for (int i = 0; i < matrix.Length / threadCount; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Color pixel = ColorFromIterations(matrix[i + yStart][j], 1000);
                    output.SetPixel(j, i, pixel);
                }
            }
            return output;
        }

        static Color ColorFromIterations(int iterations, int maxIterations)
        {
            //TODO: Color mapping from iterations
            Color output = new Color();


            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#262932"),
            //                                System.Drawing.ColorTranslator.FromHtml("#710100"),
            //                                System.Drawing.ColorTranslator.FromHtml("#fdf500"),
            //                                System.Drawing.ColorTranslator.FromHtml("#19c5af"),
            //                                System.Drawing.ColorTranslator.FromHtml("#36ebf2"),
            //                                System.Drawing.ColorTranslator.FromHtml("#9470dc"),
            //                                System.Drawing.ColorTranslator.FromHtml("#e455ad"),
            //                                System.Drawing.ColorTranslator.FromHtml("#cb1dce"),
            //                                System.Drawing.ColorTranslator.FromHtml("#d0c5bf"),
            //};
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#006668"),
            //                                System.Drawing.ColorTranslator.FromHtml("#1a5e5d"),
            //                                System.Drawing.ColorTranslator.FromHtml("#335556"),
            //                                System.Drawing.ColorTranslator.FromHtml("#4d4d4d"),
            //                                System.Drawing.ColorTranslator.FromHtml("#654342"),
            //                                System.Drawing.ColorTranslator.FromHtml("#803c3d"),
            //};

            //gradiant
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#1500CE"),
            //                                System.Drawing.ColorTranslator.FromHtml("#2900B1"),
            //                                System.Drawing.ColorTranslator.FromHtml("#3E0093"),
            //                                System.Drawing.ColorTranslator.FromHtml("#520076"),
            //                                System.Drawing.ColorTranslator.FromHtml("#660058"),
            //                                System.Drawing.ColorTranslator.FromHtml("#7A003B"),
            //                                System.Drawing.ColorTranslator.FromHtml("#8F001D"),
            //                                System.Drawing.ColorTranslator.FromHtml("#A30000"),
            //};
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#FB8B24"),
            //                                System.Drawing.ColorTranslator.FromHtml("#D90368"),
            //                                System.Drawing.ColorTranslator.FromHtml("#820263"),
            //                                System.Drawing.ColorTranslator.FromHtml("#291720"),
            //                                System.Drawing.ColorTranslator.FromHtml("#04A777"),
            //};
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#000000"),
            //                                System.Drawing.ColorTranslator.FromHtml("#14213d"),
            //                                System.Drawing.ColorTranslator.FromHtml("#fca311"),
            //                                System.Drawing.ColorTranslator.FromHtml("#e63946"),
            //                                System.Drawing.ColorTranslator.FromHtml("#023e8a"),
            //};

            //black->red->yellow
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#03071E"),
            //                                System.Drawing.ColorTranslator.FromHtml("#370617"),
            //                                System.Drawing.ColorTranslator.FromHtml("#6A040F"),
            //                                System.Drawing.ColorTranslator.FromHtml("#9D0208"),
            //                                System.Drawing.ColorTranslator.FromHtml("#D00000"),
            //                                System.Drawing.ColorTranslator.FromHtml("#DC2F02"),
            //                                System.Drawing.ColorTranslator.FromHtml("#E85D04"),
            //                                System.Drawing.ColorTranslator.FromHtml("#F48C06"),
            //                                System.Drawing.ColorTranslator.FromHtml("#FAA307"),
            //                                System.Drawing.ColorTranslator.FromHtml("#FFBA08"),
            //};

            //blue -> red
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#09005b"),
            //                                System.Drawing.ColorTranslator.FromHtml("#2a005d"),
            //                                System.Drawing.ColorTranslator.FromHtml("#3f005e"),
            //                                System.Drawing.ColorTranslator.FromHtml("#51005e"),
            //                                System.Drawing.ColorTranslator.FromHtml("#62005d"),
            //                                System.Drawing.ColorTranslator.FromHtml("#71005b"),
            //                                System.Drawing.ColorTranslator.FromHtml("#800058"),
            //                                System.Drawing.ColorTranslator.FromHtml("#8e0054"),
            //                                System.Drawing.ColorTranslator.FromHtml("#9b004f"),
            //                                System.Drawing.ColorTranslator.FromHtml("#a8004a"),
            //                                System.Drawing.ColorTranslator.FromHtml("#b30044"),
            //                                System.Drawing.ColorTranslator.FromHtml("#bd003d"),
            //                                System.Drawing.ColorTranslator.FromHtml("#c60036"),
            //                                System.Drawing.ColorTranslator.FromHtml("#cd002e"),
            //                                System.Drawing.ColorTranslator.FromHtml("#d40024"),
            //                                System.Drawing.ColorTranslator.FromHtml("#d90019"),
            //};

            //yellow -> blue
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#d6d900"),
            //                                System.Drawing.ColorTranslator.FromHtml("#e1c200"),
            //                                System.Drawing.ColorTranslator.FromHtml("#e8ab00"),
            //                                System.Drawing.ColorTranslator.FromHtml("#ec9400"),
            //                                System.Drawing.ColorTranslator.FromHtml("#ec7c02"),
            //                                System.Drawing.ColorTranslator.FromHtml("#e9631c"),
            //                                System.Drawing.ColorTranslator.FromHtml("#e34a2a"),
            //                                System.Drawing.ColorTranslator.FromHtml("#d92e36"),
            //                                System.Drawing.ColorTranslator.FromHtml("#cb0341"),
            //                                System.Drawing.ColorTranslator.FromHtml("#bb004a"),
            //                                System.Drawing.ColorTranslator.FromHtml("#a70051"),
            //                                System.Drawing.ColorTranslator.FromHtml("#910058"),
            //                                System.Drawing.ColorTranslator.FromHtml("#78005c"),
            //                                System.Drawing.ColorTranslator.FromHtml("#5c005e"),
            //                                System.Drawing.ColorTranslator.FromHtml("#3b005e"),
            //                                System.Drawing.ColorTranslator.FromHtml("#09005b"),
            //};

            //pinterest: futuristic color palette
            Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#17ecb2"),
                                            System.Drawing.ColorTranslator.FromHtml("#18e4ed"),
                                            System.Drawing.ColorTranslator.FromHtml("#bde6ec"),
                                            System.Drawing.ColorTranslator.FromHtml("#5bd4ef"),
                                            System.Drawing.ColorTranslator.FromHtml("#23a1ed"),
                                            System.Drawing.ColorTranslator.FromHtml("#6e98ae"),
                                            System.Drawing.ColorTranslator.FromHtml("#1b82c4"),
                                            System.Drawing.ColorTranslator.FromHtml("#4ca5e7"),
                                            System.Drawing.ColorTranslator.FromHtml("#2554a6"),
                                            System.Drawing.ColorTranslator.FromHtml("#416bbf"),
                                            System.Drawing.ColorTranslator.FromHtml("#4a6ee9"),
                                            System.Drawing.ColorTranslator.FromHtml("#232989"),
                                            System.Drawing.ColorTranslator.FromHtml("#4333b7"),
                                            System.Drawing.ColorTranslator.FromHtml("#2203da"),
                                            System.Drawing.ColorTranslator.FromHtml("#674cd7"),
                                            System.Drawing.ColorTranslator.FromHtml("#443391"),
                                            System.Drawing.ColorTranslator.FromHtml("#1a0665"),
                                            System.Drawing.ColorTranslator.FromHtml("#3506a0"),
                                            System.Drawing.ColorTranslator.FromHtml("#6f5499"),
                                            System.Drawing.ColorTranslator.FromHtml("#1b0738"),
                                            System.Drawing.ColorTranslator.FromHtml("#6908d4"),
                                            System.Drawing.ColorTranslator.FromHtml("#40087a"),
                                            System.Drawing.ColorTranslator.FromHtml("#630fab"),
                                            System.Drawing.ColorTranslator.FromHtml("#9a08d7"),
                                            System.Drawing.ColorTranslator.FromHtml("#3f0354"),
                                            System.Drawing.ColorTranslator.FromHtml("#630784"),
                                            System.Drawing.ColorTranslator.FromHtml("#8e06a4"),
                                            System.Drawing.ColorTranslator.FromHtml("#e522bc"),
                                            System.Drawing.ColorTranslator.FromHtml("#9d1168"),
                                            System.Drawing.ColorTranslator.FromHtml("#c92f61"),
            };

            //more cyberpunk
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#ff124f"),
            //                                System.Drawing.ColorTranslator.FromHtml("#ff00a0"),
            //                                System.Drawing.ColorTranslator.FromHtml("#fe75fe"),
            //                                System.Drawing.ColorTranslator.FromHtml("#7a04eb"),
            //                                System.Drawing.ColorTranslator.FromHtml("#120458"),
            //};
            //mooore cyberpunk
            //Color[] myCustomPalette = {    System.Drawing.ColorTranslator.FromHtml("#490109"),
            //                                System.Drawing.ColorTranslator.FromHtml("#d40010"),
            //                                System.Drawing.ColorTranslator.FromHtml("#fd7495"),
            //                                System.Drawing.ColorTranslator.FromHtml("#5e4ef8"),
            //                                System.Drawing.ColorTranslator.FromHtml("#14029a"),
            //};

            int colorindex = (maxIterations - iterations) % myCustomPalette.Length;
            output = myCustomPalette[colorindex];
            //int colorindex = ((iterations*13)% 234)+20;
            //output = Color.FromArgb(BitmapPalettes.Halftone256.Colors[colorindex].R, BitmapPalettes.Halftone256.Colors[colorindex].G, BitmapPalettes.Halftone256.Colors[colorindex].B);

            return output;
        }

    }
}
