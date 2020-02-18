using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RGBFusion390SetColor.Effects
{
    public class ChannelEffect : Effect
    {
        private readonly IChannel redChannel;
        private readonly IChannel greenChannel;
        private readonly IChannel blueChannel;
        private readonly IChannel brightnessChannel;
        private readonly IChannel modeChannel;
        private readonly IChannel speedChannel;

        public ChannelEffect(string name, IChannel mode, IChannel speed, IChannel red, IChannel green, IChannel blue, IChannel brightness) : base(name)
        {
            this.modeChannel = mode;
            this.speedChannel = speed;
            this.redChannel = red;
            this.greenChannel = green;
            this.blueChannel = blue;
            this.brightnessChannel = brightness;
        }

        public override LedState Step(int clock, LedState currentState, LedState startingState, LedState previousState, int timeInEffect, int effectLength)
        {
            currentState.Mode = (sbyte)modeChannel.Step(clock, currentState.Mode, startingState.Mode, previousState.Mode, timeInEffect, effectLength);
            currentState.Speed = (sbyte)speedChannel.Step(clock, currentState.Speed, startingState.Speed, previousState.Speed, timeInEffect, effectLength);
            currentState.Red = (byte)redChannel.Step(clock, currentState.Red, startingState.Red, previousState.Red, timeInEffect, effectLength);
            currentState.Green = (byte)greenChannel.Step(clock, currentState.Green, startingState.Green, previousState.Green, timeInEffect, effectLength);
            currentState.Blue = (byte)blueChannel.Step(clock, currentState.Blue, startingState.Blue, previousState.Blue, timeInEffect, effectLength);
            currentState.Brightness = brightnessChannel.Step(clock, currentState.Brightness, startingState.Brightness, previousState.Brightness, timeInEffect, effectLength);

            return currentState;
        }

        public override Effect Clone()
        {
            return this;
        }
    }
}
