using RGBFusion390SetColor.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Animations
{
    public class Animation
    {
        public readonly sbyte AreaId;
        private readonly List<AnimationStep> steps;
        private AnimationStep currentStep;
        private int stepIndex;
        private LedState previousState;

        public Animation(sbyte areaId)
        {
            this.AreaId = areaId;
            steps = new List<AnimationStep>();
            stepIndex = 0;
            previousState = new LedState();
        }

        public Animation(sbyte areaId, params AnimationStep[] args) : this(areaId)
        {
            steps.AddRange(args);
        }

        public bool Ready
        {
            get => steps.Any();
        }

        public void AddStep(AnimationStep step)
        {
            steps.Add(step);
        }

        public LedCommand Step(int clock)
        {
            if (steps.Any())
            {
                if (stepIndex >= steps.Count())
                {
                    stepIndex = 0;
                }

                while(currentStep?.Ended ?? true)
                {
                    if (stepIndex >= steps.Count())
                    {
                        // Something went wrong and we have no playable steps.
                        // Abort and send the last state
                        stepIndex = 0;
                        return previousState.ToCommand(AreaId);
                    }

                    currentStep?.Reset();
                    currentStep = steps[stepIndex];
                    stepIndex++;
                }

                previousState = currentStep.Step(clock, previousState.Clone(), previousState);
                return previousState.ToCommand(AreaId);
            }

            throw new Exception("No steps available.");
        }

        public IEnumerable<AnimationStep> Steps
        {
            get
            {
                foreach (var step in steps)
                {
                    yield return step;
                }
            }
        }

        public void Reset()
        {
            stepIndex = 0;
            steps.ForEach(s => s.Reinitialize());
            currentStep = null;
            previousState = new LedState();
        }
    }
}
