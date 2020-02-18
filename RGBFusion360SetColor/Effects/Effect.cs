using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects
{
    public abstract class Effect
    {
        public readonly string Name;

        public Effect(string name)
        {
            this.Name = name;
        }

        public abstract LedState Step(int clock, LedState currentState, LedState startingState, LedState previousState, int timeInEffect, int effectLength);

        public abstract Effect Clone();
    }
}
