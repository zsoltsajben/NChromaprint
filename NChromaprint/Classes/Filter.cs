using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class Filter
    {
        public int Type { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }


        public Filter() : this(0, 0, 0, 0) { }
        public Filter(int type) : this(type, 0, 0, 0) { }
        public Filter(int type, int y) : this(type, y, 0, 0) { }
        public Filter(int type, int y, int height) : this(type, y, height, 0) { }
        public Filter(int type, int y, int height, int width)
        {
            Type = type;
            Y = y;
            Height = height;
            Width = width;
        }


        public override string ToString()
        {
            return "Filter(" + Type + ", " + Y + ", " + Height + ", " + Width + ")";
        }

        public double Apply(IntegralImage image, int x)
        {
            switch (Type)
            {
                case 0:
                    return Filter0(image, x, Y, Width, Height, SubtractLog);
                case 1:
                    return Filter1(image, x, Y, Width, Height, SubtractLog);
                case 2:
                    return Filter2(image, x, Y, Width, Height, SubtractLog);
                case 3:
                    return Filter3(image, x, Y, Width, Height, SubtractLog);
                case 4:
                    return Filter4(image, x, Y, Width, Height, SubtractLog);
                case 5:
                    return Filter5(image, x, Y, Width, Height, SubtractLog);
            }
            return 0.0;
        }

        // ------------------------------------------------------------------------------------
        // helper methods

        double SubtractLog(double a, double b)
        {
            double r = Math.Log(1.0 + a) - Math.Log(1.0 + b);
            if (double.IsNaN(r))
                throw new Exception();
            return r;
        }

        // oooooooooooooooo
        // oooooooooooooooo
        // oooooooooooooooo
        // oooooooooooooooo
        double Filter0(IntegralImage image, int x, int y, int w, int h, Func<double, double, double> cmp)
        {
            double a = image.Area(x, y, x + w - 1, y + h - 1);
            double b = 0;
            return cmp(a, b);
        }

        // ................
        // ................
        // oooooooooooooooo
        // oooooooooooooooo
        double Filter1(IntegralImage image, int x, int y, int w, int h, Func<double, double, double> cmp)
        {
            int h_2 = h / 2;

            double a = image.Area(x, y + h_2, x + w - 1, y + h - 1);
            double b = image.Area(x, y, x + w - 1, y + h_2 - 1);

            return cmp(a, b);
        }

        // .......ooooooooo
        // .......ooooooooo
        // .......ooooooooo
        // .......ooooooooo
        double Filter2(IntegralImage image, int x, int y, int w, int h, Func<double, double, double> cmp)
        {
            int w_2 = w / 2;

            double a = image.Area(x + w_2, y, x + w - 1, y + h - 1);
            double b = image.Area(x, y, x + w_2 - 1, y + h - 1);

            return cmp(a, b);
        }

        // .......ooooooooo
        // .......ooooooooo
        // ooooooo.........
        // ooooooo.........
        double Filter3(IntegralImage image, int x, int y, int w, int h, Func<double, double, double> cmp)
        {
            int w_2 = w / 2;
            int h_2 = h / 2;

            double a = image.Area(x, y + h_2, x + w_2 - 1, y + h - 1) +
                       image.Area(x + w_2, y, x + w - 1, y + h_2 - 1);
            double b = image.Area(x, y, x + w_2 - 1, y + h_2 - 1) +
                       image.Area(x + w_2, y + h_2, x + w - 1, y + h - 1);

            return cmp(a, b);
        }

        // ................
        // oooooooooooooooo
        // ................
        double Filter4(IntegralImage image, int x, int y, int w, int h, Func<double, double, double> cmp)
        {
            int h_3 = h / 3;

            double a = image.Area(x, y + h_3, x + w - 1, y + 2 * h_3 - 1);
            double b = image.Area(x, y, x + w - 1, y + h_3 - 1) +
                       image.Area(x, y + 2 * h_3, x + w - 1, y + h - 1);

            return cmp(a, b);
        }

        // .....oooooo.....
        // .....oooooo.....
        // .....oooooo.....
        // .....oooooo.....
        double Filter5(IntegralImage image, int x, int y, int w, int h, Func<double, double, double> cmp)
        {
            int w_3 = w / 3;

            double a = image.Area(x + w_3, y, x + 2 * w_3 - 1, y + h - 1);
            double b = image.Area(x, y, x + w_3 - 1, y + h - 1) +
                       image.Area(x + 2 * w_3, y, x + w - 1, y + h - 1);

            return cmp(a, b);
        }
    }
}
