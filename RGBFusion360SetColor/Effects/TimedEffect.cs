using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects
{
    public class TimedEffect
    {
        public readonly Effect Effect;
        public readonly int Length;

        public TimedEffect(Effect effect, int length)
        {
            this.Effect = effect;
            this.Length = length;
        }
    }
}
