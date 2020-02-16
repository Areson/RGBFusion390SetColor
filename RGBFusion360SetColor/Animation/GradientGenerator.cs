using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Animation
{
    public static class GradientGenerator
    {
        public static double Sin(int step, GradientInfo gradient)
        {
            double dStep = step;
            return Math.Sin(dStep * gradient.Frequency + gradient.Phase) * gradient.Width + gradient.Center;
        }

        public static double Cos(int step, GradientInfo gradient)
        {
            double dStep = step;
            return Math.Sin(dStep * gradient.Frequency + gradient.Phase) * gradient.Width + gradient.Center;
        }

        public static double Saw(int step, GradientInfo gradient)
        {
            double dStep = step;
            return ((Math.Round(Math.Sin(dStep * gradient.Frequency + gradient.Phase) * .5 + .5, 0) - .5) * 2.0) * gradient.Width + gradient.Center;
        }

        public static double BSaw(int step, GradientInfo gradient)
        {
            double dStep = step;
            return Math.Round(Math.Sin(dStep * gradient.Frequency + gradient.Phase) * .5 + .5, 0) * gradient.Width + gradient.Center;
        }

        public static double Constant(int step, GradientInfo gradient)
        {
            return gradient.Center;
        }
    }
}
