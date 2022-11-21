using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    internal class CsvParser
    {
        static char trennzeichen = ';';
        public static Color[][] readColorPalettes(string path)
        {
            string[] rows, columns;
            Color[][] allColorPalettes = null;

            try
            {
                rows = File.ReadAllLines(path);
                allColorPalettes = new Color[rows.Length][];
                for (int i = 0; i < rows.Length; i++)
                {
                    columns = rows[i].Split(trennzeichen);
                    Color[] colorRow = new Color[columns.Length];
                    allColorPalettes[i] = new Color[columns.Length];
                    for (int j = 0; j < columns.Length; j++)
                    {
                        allColorPalettes[i][j] = System.Drawing.ColorTranslator.FromHtml(columns[j]);
                    }
                }

            }
            catch { }
            return allColorPalettes;
        }
    }
}
