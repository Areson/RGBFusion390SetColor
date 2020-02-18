using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects
{
    public interface IChannel
    {
        double Step(int clock, double currentValue, double startingValue, double previousValue, int timeInEffect, int effectLength);
    }
}
