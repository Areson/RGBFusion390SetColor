using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Animation
{
    public enum GradientType
    {
        SIN = 1,
        COS = 2,
        SAW = 3,
        BSAW = 4,
        CONST = 5
    }

    public class GradientInfo
    {        
        public readonly double Center;
        public readonly double Frequency;
        public readonly double Phase;
        public readonly double Width;
        public readonly GradientType GradientType;

        public GradientInfo(double frequency, double phase, double center = 0, double width = 0, GradientType gradientType = GradientType.CONST)
        {
            this.Frequency = frequency;
            this.Phase = phase;
            this.Center = center;
            this.Width = width;
            this.GradientType = gradientType;
        }

        public double Generate(int timestep)
        {
            switch(GradientType)
            {
                case GradientType.SIN:
                    return GradientGenerator.Sin(timestep, this);

                case GradientType.COS:
                    return GradientGenerator.Cos(timestep, this);

                case GradientType.SAW:
                    return GradientGenerator.Saw(timestep, this);

                case GradientType.BSAW:
                    return GradientGenerator.BSaw(timestep, this);

                default:
                    return GradientGenerator.Constant(timestep, this);
            }
        }
    }
}
