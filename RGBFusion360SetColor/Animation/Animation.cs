using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBFusion390SetColor.Animation
{
    public class Animation
    {
        public readonly sbyte AreaId;
        private readonly List<AnimationPhase> phases;
        private AnimationPhase currentPhase;
        private int phaseCount;

        public Animation(sbyte areaId)
        {
            this.AreaId = areaId;
            phases = new List<AnimationPhase>();
            phaseCount = 0;
        }

        public bool Ready
        {
            get => phases.Any();
        }

        public void AddPhase(AnimationPhase phase)
        {
            phases.Add(phase);
        }

        public LedCommand Step(int stepCount)
        {
            if (phases.Any())
            {
                if (phaseCount >= phases.Count())
                {
                    phaseCount = 0;
                }

                if (currentPhase?.PhaseExpired ?? true)
                {
                    currentPhase?.Reset();
                    currentPhase = phases[phaseCount];
                    phaseCount++;
                }

                return currentPhase.Step(AreaId, stepCount);
            }

            throw new Exception("No phases available.");
        }

        public IEnumerable<AnimationPhase> Phases
        {
            get
            {
                foreach (var phase in phases)
                {
                    yield return phase;
                }
            }
        }

        public void Reset()
        {
            phaseCount = 0;
            currentPhase?.Reset();
            currentPhase = null;
        }
    }
}
