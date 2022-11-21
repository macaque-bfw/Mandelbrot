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
        static int totalIterations; //counter for all interations from all threads
        static int width;
        static int height;
        //static int maxIterations; 

        static decimal zoom;
        static decimal posX;
        static decimal posY;

        static int threadCount;

        static string pathPng;
        static Bitmap output; //final Bitmap to be written as PNG
        static Bitmap[] bitmapsFromThreads; //Bitmap sections written by threads
        static Color[] colorPalette;
        public int[][] mandelbrotInThreads(int widthUser, int heightUser, decimal zoomUser, decimal posXUser, decimal posYUser, int threadCountUser, Color[] colorPaletteUser, string pathUser)
        {
            width = widthUser;
            height = heightUser;
            zoom= zoomUser;
            posX = posXUser;
            posY = posYUser;
            threadCount = threadCountUser;
            colorPalette = colorPaletteUser;
            pathPng = pathUser;

            mandelbrot = new int[height][];
            output = new Bitmap(width, height, PixelFormat.Format16bppRgb555);
            totalIterations = 0;

            var threads = new List<Thread>();

            //create Threads for Mandelbrot to int[][] calculation
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
                thread.Join(); //wait for all threads to complete
            }

            Console.WriteLine("MandelbrotThreads finished");
            Console.WriteLine();
            Console.WriteLine("Converting Mandelbrot-Matrix to Bitmap");

            bitmapsFromThreads = new Bitmap[threadCount];

            //create Threads for int[][] to Bitmap conversion
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
                thread.Join(); //wait for all threads to complete
            }
            Console.WriteLine("BitmapCreationThreads finished");
            Console.WriteLine();


            //stich Bitmap sections into one Bitmap and save as PNG
            Console.WriteLine("Saving Bitmap to PNG");
            Graphics g = Graphics.FromImage(output);
            int yPos = 0;
            for (int i = 0; i < bitmapsFromThreads.Length; i++)
            {
                g.DrawImage(bitmapsFromThreads[i],0,yPos);
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
                        int maxIteration = 1000; //TODO: Allow user to specify
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
                if (y % 10 == 0) {
                    int percentComplete = Convert.ToInt32(Convert.ToDouble(y) / Convert.ToDouble(height) * 100.0);
                    Console.WriteLine("MandelbrotThread" + thread + " (" + percentComplete +"%) " +totalIterations); 
                }
            }
            return mandelbrot;
        }
        static Bitmap MatrixToBitmap(int[][] matrix, int threadCount, int thread)
        {
            int yStart = thread * matrix.Length / threadCount;
            int yEnd = yStart + (matrix.Length / threadCount);    
            Bitmap output = new Bitmap(matrix[0].Length, matrix.Length/threadCount);
            for (int i = 0; i < matrix.Length/threadCount; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Color pixel = ColorFromIterations(matrix[i+yStart][j], 1000);
                    output.SetPixel(j, i, pixel);
                }
            }
            return output;
        }

        static Color ColorFromIterations(int iterations, int maxIterations)
        {
            Color output = new Color();

            int colorindex = iterations % colorPalette.Length;
            
            output = colorPalette[colorindex];

            return output;
        }

    }
}
