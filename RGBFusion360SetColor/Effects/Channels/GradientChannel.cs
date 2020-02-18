using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects.Channels
{
    public enum GradientType
    {
        SIN = 1,
        COS = 2,
        SAW = 3,
        BSAW = 4        
    }

    public class GradientChannel : IChannel
    {
        private readonly GradientType GradientType ;
        private readonly double Center;
        private readonly double Width;
        private readonly double Frequency;
        private readonly double Phase;

        public GradientChannel(GradientType gradientType, double center, double width, double frequency, double phase)
        {
            this.GradientType = gradientType;
            this.Center = center;
            this.Width = width;
            this.Frequency = frequency;
            this.Phase = phase;
        }

        public double Step(int clock, double currentValue, double startingValue, double previousValue, int timeInEffect, int effectLength)
        {            
            switch (GradientType)
            {
                case GradientType.SIN:
                    return GradientChannel.Sin(clock, this);

                case GradientType.COS:
                    return GradientChannel.Cos(clock, this);

                case GradientType.SAW:
                    return GradientChannel.Saw(clock, this);

                case GradientType.BSAW:
                    return GradientChannel.BSaw(clock, this);

                default:
                    throw new Exception("Unknown gradient type.");
            }
        }

        public static double Sin(int step, GradientChannel gradient)
        {
            double dStep = step;
            return Math.Sin(dStep * gradient.Frequency + gradient.Phase) * gradient.Width + gradient.Center;
        }

        public static double Cos(int step, GradientChannel gradient)
        {
            double dStep = step;
            return Math.Sin(dStep * gradient.Frequency + gradient.Phase) * gradient.Width + gradient.Center;
        }

        public static double Saw(int step, GradientChannel gradient)
        {
            double dStep = step;
            return ((Math.Round(Math.Sin(dStep * gradient.Frequency + gradient.Phase) * .5 + .5, 0) - .5) * 2.0) * gradient.Width + gradient.Center;
        }

        public static double BSaw(int step, GradientChannel gradient)
        {
            double dStep = step;
            return Math.Round(Math.Sin(dStep * gradient.Frequency + gradient.Phase) * .5 + .5, 0) * gradient.Width + gradient.Center;
        }
    }
}
