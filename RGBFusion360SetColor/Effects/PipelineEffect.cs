using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Effects
{
    public class PipelineEffect : Effect, IResetable
    {
        private readonly List<TimedEffect> timedEffects;        
        private int pipelineTimeInEffect;
        private int effectIndex;
        private LedState pipelineStartingState;
        private int maxEffectTime;
        private Effect currentEffect;
        
        private double[] relativeTiming;
        private double[] cumulativeTimings;
        private int totalEffectTime;

        public PipelineEffect(string name, params TimedEffect[] effects) : base(name)
        {
            this.timedEffects = effects.ToList();            

            double totalTime = totalEffectTime = effects.Sum(e => e.Length);
            relativeTiming = effects.Select(e => e.Length / totalTime).ToArray();

            cumulativeTimings = new double[relativeTiming.Length];
            cumulativeTimings[0] = relativeTiming[0];
            cumulativeTimings[cumulativeTimings.Length - 1] = 1.0;

            for (var i = 1; i < cumulativeTimings.Length - 1; i++)
            {
                cumulativeTimings[i] = cumulativeTimings[i - 1] + relativeTiming[i];
            }

            this.Reset();
        }

        public void Reset()
        {
            currentEffect = null;
            effectIndex = -1;
            pipelineTimeInEffect = 0;
            pipelineStartingState = null;
            maxEffectTime = -1;
        }

        public override LedState Step(int clock, LedState currentState, LedState startingState, LedState previousState, int timeInEffect, int effectLength)
        {            
            if (!timedEffects.Any())
            {
                return currentState;
            }
            else
            {
                return RelativeStep(clock, currentState, startingState, previousState, timeInEffect, effectLength);

                pipelineStartingState = pipelineStartingState ?? startingState;

                if (pipelineTimeInEffect >= maxEffectTime)
                {
                    effectIndex = (effectIndex + 1) % timedEffects.Count();
                    pipelineTimeInEffect = 0;
                    pipelineStartingState = currentState.Clone();
                    maxEffectTime = timedEffects[effectIndex].Length;
                    currentEffect = timedEffects[effectIndex].Effect;                    
                }
                
                currentEffect.Step(clock, currentState, pipelineStartingState, previousState, pipelineTimeInEffect, timedEffects[effectIndex].Length);
                pipelineTimeInEffect++;

                return currentState;
            }
        }

        private LedState RelativeStep(int clock, LedState currentState, LedState startingState, LedState previousState, int timeInEffect, int effectLength)
        {            
            if (pipelineTimeInEffect >= maxEffectTime)
            {
                effectIndex = (effectIndex + 1) % timedEffects.Count();
                var percentageTimeInEffect = (double)timeInEffect / (double)effectLength;
                var localEffectIndex = getTimingIndex(percentageTimeInEffect);

                //effectIndex = localEffectIndex;
                pipelineStartingState = currentState.Clone();
                var timedEffect = timedEffects[effectIndex];
                currentEffect = timedEffect.Effect;
                pipelineTimeInEffect = 0;

                if (effectIndex == timedEffects.Count - 1)
                {
                    maxEffectTime = effectLength - timeInEffect;
                }
                else
                {
                    maxEffectTime = (int)Math.Ceiling(relativeTiming[localEffectIndex] * effectLength);
                }
            }

            currentEffect.Step(clock, currentState, pipelineStartingState, previousState, pipelineTimeInEffect, maxEffectTime - 1);
            pipelineTimeInEffect++;

            return currentState;
        }

        private int getTimingIndex(double percentageTimeInEffect)
        {
            var result = Array.BinarySearch(cumulativeTimings, percentageTimeInEffect);

            if (result < 0)
            {
                return ~result;
            }
            else
            {
                return result;
            }
        }

        public override Effect Clone()
        {
            return new PipelineEffect(Name, timedEffects.Select(x => new TimedEffect(x.Effect.Clone(), x.Length)).ToArray());
        }
    }
}
