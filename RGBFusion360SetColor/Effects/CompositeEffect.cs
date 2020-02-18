using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects
{
    public class CompositeEffect : Effect
    {
        private readonly List<Effect> effects;

        public CompositeEffect(string name) : base(name)
        {
            effects = new List<Effect>();
        }

        public CompositeEffect(string name, params Effect[] args) : base(name)
        {
            effects = args.ToList();
        }

        public void AddEffect(Effect effect)
        {
            effects.Add(effect);
        }

        public override LedState Step(int clock, LedState currentState, LedState startingState, LedState previousState, int timeInEffect, int effectLength)
        {
            effects.ForEach(x => x.Step(clock, currentState, startingState, previousState, timeInEffect, effectLength));
            return currentState;
        }

        public override Effect Clone()
        {
            return new CompositeEffect(Name, effects.Select(e => e.Clone()).ToArray());
        }
    }
}
