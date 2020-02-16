using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RGBFusion390SetColor.Animation
{
    public class AnimationPhase
    {        
        private int length;
        public int TimeInPhase { get; private set; }

        private readonly GradientInfo red;
        private readonly GradientInfo green;
        private readonly GradientInfo blue;
        private readonly GradientInfo brightness;
        private readonly GradientInfo mode;

        public AnimationPhase(int length, GradientInfo mode, GradientInfo red, GradientInfo green, GradientInfo blue, GradientInfo brightness)
        {
            this.length = length;
            this.red = red;
            this.blue = blue;
            this.green = green;
            this.brightness = brightness;
            this.mode = mode;
        }

        public bool PhaseExpired
        {
            get
            {
                return TimeInPhase >= length;
            }
        }

        public LedCommand Step(sbyte areaId, int stepCount)
        {
            TimeInPhase++;

            return new LedCommand
            {
                AreaId = areaId,
                NewMode = (sbyte)mode.Generate(stepCount),
                Speed = 9,
                NewColor = Color.FromRgb(
                    (byte)red.Generate(stepCount),
                    (byte)green.Generate(stepCount),
                    (byte)blue.Generate(stepCount)
                ),
                Bright = (sbyte)brightness.Generate(stepCount),
            };
        }

        public void Reset()
        {
            TimeInPhase = 0;
        }
    }
}
