using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RGBFusion390SetColor.Effects
{
    public class LedState
    {
        public sbyte Mode { get; set; } = 0;
        public sbyte Speed { get; set; } = 5;
        public byte Red { get; set; } = 0;
        public byte Green { get; set; } = 0;
        public byte Blue { get; set; } = 0;
        public double Brightness { get; set; } = 9;

        public LedCommand ToCommand(sbyte areaId)
        {
            return new LedCommand
            {
                AreaId = areaId,
                NewMode = this.Mode,
                NewColor = Color.FromRgb(this.Red, this.Green, this.Blue),
                Bright = this.Brightness,
                Speed = this.Speed
            };
        }

        public LedState Clone()
        {
            return new LedState
            {
                Mode = this.Mode,
                Speed = this.Speed,
                Red = this.Red,
                Green = this.Green,
                Blue = this.Blue,
                Brightness = this.Brightness,
            };
        }

        public static LedState FromCommand(LedCommand command)
        {
            return new LedState
            {
                Mode = command.NewMode,
                Speed = command.Speed,
                Red = command.NewColor.R,
                Green = command.NewColor.G,
                Blue = command.NewColor.B,
                Brightness = command.Bright
            };
        }
    }
}
