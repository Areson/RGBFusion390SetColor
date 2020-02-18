using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects.Channels
{
    public class PassThroughChannel : IChannel
    {
        public double Step(int clock, double currentValue, double startingValue, double previousValue, int timeInEffect, int effectLength)
        {
            return currentValue;
        }
    }
}
