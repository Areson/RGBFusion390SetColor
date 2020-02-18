using RGBFusion390SetColor.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Animations
{
    public enum AnimationStepEndingType
    {
        HOLD = 1,
        OFF = 2,
        CONTINUOUS = 3
    }

    public class AnimationStep
    {
        private readonly List<TimedEffect> timedEffects;                
        public readonly List<IResetable> resetables;
        private LedState startingState;
        private readonly AnimationStepEndingType EndingType;
        private bool hasPlayed = false;
        private readonly bool playOnce;

        public readonly int StepLength;
        public int TimeInStep { get; private set; }
        private int totalEffectLength;
        private int timeInEffect;
        private int effectIndex;

        public AnimationStep(int stepLength, bool playOnce, AnimationStepEndingType endingType, params TimedEffect[] timedEffects) : this(stepLength, playOnce, endingType)
        {
            this.timedEffects.AddRange(timedEffects);
            this.resetables.AddRange(timedEffects.Where(x => x.Effect is IResetable).Select(x => (IResetable)x.Effect));
            totalEffectLength = timedEffects.Sum(x => x.Length);
            
        }

        public AnimationStep(int stepLength, bool playOnce, AnimationStepEndingType endingType)
        {
            this.EndingType = endingType;
            this.timedEffects = new List<TimedEffect>();
            this.resetables = new List<IResetable>();
            this.StepLength = stepLength;
            this.timeInEffect = 0;
            this.effectIndex = 0;
            this.totalEffectLength = 0;
            this.playOnce = playOnce;
        }
       
        public void AddEffect(Effect effect, int length)
        {
            timedEffects.Add(new TimedEffect(effect, length));
            totalEffectLength += length;

            if (effect is IResetable)
            {
                resetables.Add((IResetable)effect);
            }
        }

        public void Reset()
        {
            timeInEffect = 0;
            TimeInStep = 0;
            startingState = null;
            resetables.ForEach(r => r.Reset());
            hasPlayed = true;
        }

        public void Reinitialize()
        {
            Reset();
            hasPlayed = false;
        }

        public bool Ended
        {
            get { return TimeInStep >= StepLength || (playOnce && hasPlayed); }
        }

        public LedState Step(int clock, LedState currentState, LedState previousState)
        {            
            if (!timedEffects.Any())
            {                
                return currentState;
            }            
            else
            {
                if (EndingType == AnimationStepEndingType.CONTINUOUS || TimeInStep < totalEffectLength)
                {
                    startingState = startingState ?? currentState.Clone();

                    if (timeInEffect >= timedEffects[effectIndex].Length)
                    {
                        timeInEffect = 0;
                        effectIndex = (effectIndex + 1) % timedEffects.Count();
                        startingState = currentState.Clone();
                    }

                    var timedEffect = timedEffects[effectIndex];
                    timedEffect.Effect.Step(clock, currentState, startingState, previousState, timeInEffect, timedEffects[effectIndex].Length - 1);
                    TimeInStep++;
                    timeInEffect++;

                    return currentState;
                }
                else if (EndingType == AnimationStepEndingType.HOLD)
                {
                    TimeInStep++;
                    return currentState;
                }
                else
                {
                    TimeInStep++;

                    return new LedState
                    {
                        Mode = 8 // Off
                    };
                }
            }
        }
    }
}
