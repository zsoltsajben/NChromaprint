using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    /**
     * Image transformation that allows us to quickly calculate the sum of
     * values in a rectangular area.
     *
     * http://en.wikipedia.org/wiki/Summed_area_table
     */
    public class IntegralImage
    {
        public Image Image { get; private set; }

        public int NumCols { get { return Image.NumCols; } }
        public int NumRows { get { return Image.NumRows; } }


        /**
         * Construct the integral image. Note that will modify the original
         * image in-place, so it will not be usable afterwards.
         */
        public IntegralImage(Image img)
        {
            Image = img;
            Transform();
        }


        public Vector<double> this[int i]
        {
            get
            {
                return GetRow(i);
            }
            set
            {
                SetRow(i, value);
            }
        }

        public Vector<double> GetRow(int i)
        {
            return Image.GetRow(i);
        }
        public void SetRow(int i, Vector<double> row)
        {
            Image.SetRow(i, row);
        }

        public double Area(int x1, int y1, int x2, int y2)
        {
            double area = Image[x2][y2];

            if (x1 > 0)
            {
                area -= Image[x1 - 1][y2];
                if (y1 > 0)
                {
                    area += Image[x1 - 1][y1 - 1];
                }
            }
            if (y1 > 0)
            {
                area -= Image[x2][y1 - 1];
            }

            //System.Diagnostics.Debug.WriteLine("Area(" + x1 + "," + y1 + "," + x2 + "," + y2 + ") = " + area);
            return area;
        }

        private void Transform()
        {
            for (int c = 1; c < NumCols; c++)
            {
                // First column - add value on top
                Image[0, c] += Image[0, c - 1];
            }

            for (int r = 1; r < NumRows; r++)
            {
                // First row - add value on left
                Image[r, 0] += Image[r - 1, 0];

                // Add values on left, up, and up-left
                for (int c = 1; c < NumCols; c++)
                {
                    Image[r, c] += Image[r, c - 1] + Image[r - 1, c] - Image[r - 1, c - 1];
                }
            }
        }
    }
}
