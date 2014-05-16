using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class Quantizer
    {
        double T0 { get; set; }
        double T1 { get; set; }
        double T2 { get; set; }

        public Quantizer() : this(0.0, 0.0, 0.0) { }
        public Quantizer(double t0) : this(t0, 0.0, 0.0) { }
        public Quantizer(double t0, double t1) : this(t0, t1, 0.0) { }
        public Quantizer(double t0, double t1, double t2)
        {
            T0 = t0;
            T1 = t1;
            T2 = t2;

            if (T0 > T1 || T1 > T2)
            {
                throw new Exception();
            }
        }


        public int Quantize(double value)
        {
            if (value < T1)
            {
                if (value < T0)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (value < T2)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }

        public override string ToString()
        {
            return "Quantizer(" + T0 + ", " + T1 + ", " + T2 + ")";
        }
    }
}
