using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Easing functions and constants reference from 
/// https://github.com/ai/easings.net
/// </summary>
namespace RGBFusion390SetColor.Effects.Channels
{
    public enum EasingType
    {
        LINEAR = 1,
		EASE_IN_QUAD,
		EASE_OUT_QUAD,
		EASE_IN_OUT_QUAD,
		EASE_IN_CUBIC,
		EASE_OUT_CUBIC,
		EASE_IN_OUT_CUBIC,
		EASE_IN_QUART,
		EASE_OUT_QUART,
		EASE_IN_OUT_QUART,
		EASE_IN_QUINT,
		EASE_OUT_QUINT,
		EASE_IN_OUT_QUINT,
		EASE_IN_SINE,
		EASE_OUT_SINE,
		EASE_IN_OUT_SINE,
		EASE_IN_EXPO,
		EASE_OUT_EXPO,
		EASE_IN_OUT_EXPO,
		EASE_IN_CIRC,
		EASE_OUT_CIRC,
		EASE_IN_OUT_CIRC,
		EASE_IN_BACK,
		EASE_OUT_BACK,
		EASE_IN_OUT_BACK,
		EASE_IN_ELASTIC,
		EASE_OUT_ELASTIC,
		EASE_IN_OUT_ELASTIC,
		EASE_IN_BOUNCE,
		EAST_OUT_BOUNCE,
		EASE_IN_OUT_BOUNCE
	}
    public class EasingChannel : IChannel
    {
        private readonly EasingType EasingType;
        private readonly Func<double, double> fn;
        private readonly double targetValue;

        private readonly Dictionary<EasingType, Func<double, double>> EasingMap = new Dictionary<EasingType, Func<double, double>>
        {
            { EasingType.LINEAR, EasingChannel.LinearEasing },
			{ EasingType.EASE_IN_QUAD, EasingChannel.easeInQuad },
			{ EasingType.EASE_OUT_QUAD, EasingChannel.easeOutQuad },
			{ EasingType.EASE_IN_OUT_QUAD, EasingChannel.easeInOutQuad },
			{ EasingType.EASE_IN_CUBIC, EasingChannel.easeInCubic },
			{ EasingType.EASE_OUT_CUBIC, EasingChannel.easeOutCubic },
			{ EasingType.EASE_IN_OUT_CUBIC, EasingChannel.easeInOutCubic },
			{ EasingType.EASE_IN_QUART, EasingChannel.easeInQuart },
			{ EasingType.EASE_OUT_QUART, EasingChannel.easeOutQuart },
			{ EasingType.EASE_IN_OUT_QUART, EasingChannel.easeInOutQuart },
			{ EasingType.EASE_IN_QUINT, EasingChannel.easeInQuint },
			{ EasingType.EASE_OUT_QUINT, EasingChannel.easeOutQuint },
			{ EasingType.EASE_IN_OUT_QUINT, EasingChannel.easeInOutQuint },
			{ EasingType.EASE_IN_SINE, EasingChannel.easeInSine },
			{ EasingType.EASE_OUT_SINE, EasingChannel.easeOutSine },
			{ EasingType.EASE_IN_OUT_SINE, EasingChannel.easeInOutSine },
			{ EasingType.EASE_IN_EXPO, EasingChannel.easeInExpo },
			{ EasingType.EASE_OUT_EXPO, EasingChannel.easeOutExpo },
			{ EasingType.EASE_IN_OUT_EXPO, EasingChannel.easeInOutExpo },
			{ EasingType.EASE_IN_CIRC, EasingChannel.easeInCirc },
			{ EasingType.EASE_OUT_CIRC, EasingChannel.easeOutCirc },
			{ EasingType.EASE_IN_OUT_CIRC, EasingChannel.easeInOutCirc },
			{ EasingType.EASE_IN_BACK, EasingChannel.easeInBack },
			{ EasingType.EASE_OUT_BACK, EasingChannel.easeOutBack },
			{ EasingType.EASE_IN_OUT_BACK, EasingChannel.easeInOutBack },
			{ EasingType.EASE_IN_ELASTIC, EasingChannel.easeInElastic },
			{ EasingType.EASE_OUT_ELASTIC, EasingChannel.easeOutElastic },
			{ EasingType.EASE_IN_OUT_ELASTIC, EasingChannel.easeInOutElastic },
			{ EasingType.EASE_IN_BOUNCE, EasingChannel.easeInBounce },
			{ EasingType.EAST_OUT_BOUNCE, EasingChannel.bounceOut },
			{ EasingType.EASE_IN_OUT_BOUNCE, EasingChannel.easeInOutBounce },
		};

        public EasingChannel(EasingType easingType, double targetValue)
        {
            this.EasingType = easingType;
            this.fn = EasingMap[this.EasingType];
            this.targetValue = targetValue;
        }

        public double Step(int clock, double currentValue, double startingValue, double previousValue, int timeInEffect, int effectLength)
        {
            var span = targetValue - startingValue;
            var percentageComplete = GetPercentageComplete(timeInEffect, effectLength);
            return startingValue + (fn(percentageComplete) * span);
        }

        private double GetPercentageComplete(int timeInEffect, int effectLength)
        {
            return (double)timeInEffect / (double)effectLength;
        }

		private const double PI = Math.PI;
		private const double c1 = 1.70158;
		private const double c2 = c1 + 1.525;
		private const double c3 = c1 + 1;
		private const double c4 = (2.0 * PI) / 3.0;
		private const double c5 = (2.0 * PI) / 4.5;
		private const double n1 = 7.5625;
		private const double d1 = 2.75;

		private static double LinearEasing(double value)
        {
            return value;
        }

		private static double bounceOut(double x) {		
			if (x < 1.0 / d1) {
				return n1* x * x;
			} else if (x < 2.0 / d1) {
				return n1* (x -= 1.5 / d1) * x + 0.75;
			} else if (x < 2.5 / d1) {
				return n1* (x -= 2.25 / d1) * x + 0.9375;
			} else {
				return n1* (x -= 2.625 / d1) * x + 0.984375;
			}
		}

		private static double easeInQuad(double value)
		{
			return value * value;
		}

		private static double easeOutQuad(double value)
		{
			return 1.0 - (1.0 - value) * (1.0 - value);
		}

		private static double easeInOutQuad(double value)
		{
			return value < .5 ? 2.0 * value * value : 1.0 - Math.Pow(-2.0 * value + 2.0, 2.0) / 2.0;
		}

		private static double easeInCubic(double value)
		{
			return value * value * value;
		}

		private static double easeOutCubic(double value)
		{
			return 1.0 - Math.Pow(1.0 - value, 3.0);
		}

		private static double easeInOutCubic(double value)
		{
			return value < .5 ? 4.0 * value * value * value : 1.0 - Math.Pow(-2.0 * value + 2.0, 3.0) / 2.0;
		}

		private static double easeInQuart(double value)
		{
			return value * value * value * value;
		}

		private static double easeOutQuart(double value)
		{
			return 1.0 - Math.Pow(1.0 - value, 4.0);
		}

		private static double easeInOutQuart(double value)
		{
			return value < .5 ? 8.0 * value * value * value * value : 1.0 - Math.Pow(-2.0 * value + 2.0, 4.0) / 2.0;
		}

		private static double easeInQuint(double value)
		{
			return value * value * value * value * value;
		}

		private static double easeOutQuint(double value)
		{
			return 1.0 - Math.Pow(1.0 - value, 5.0);
		}

		private static double easeInOutQuint(double value)
		{
			return value < .5 ? 16.0 * value * value * value * value * value : 1.0 - Math.Pow(-2.0 * value + 2.0, 5.0) / 2.0;
		}

		private static double easeInSine(double value)
		{
			return 1.0 - Math.Cos((value * PI) / 2.0);
		}

		private static double easeOutSine(double value)
		{
			return Math.Sin((value * PI) / 2.0);
		}

		private static double easeInOutSine(double value)
		{
			return -(Math.Cos(PI * value) - 1.0) / 2.0;
		}

		private static double easeInExpo(double value)
		{
			return value == 0.0 ? 0.0 : Math.Pow(2.0, 10.0 * value - 10.0);
		}

		private static double easeOutExpo(double value)
		{
			return value == 1.0 ? 1.0 : 1.0 - Math.Pow(2.0, -10.0 * value);
		}

		private static double easeInOutExpo(double value)
		{
			return value == 0.0
				? 0.0
				: value == 1.0
				? 1.0
				: value < .5
				? Math.Pow(2.0, 20.0 * value - 10.0) / 2.0
				: (2.0 - Math.Pow(2.0, -20.0 * value + 10.0)) / 2.0;
		}

		private static double easeInCirc(double value)
		{
			return 1.0 - Math.Sqrt(1.0 - Math.Pow(value, 2.0));
		}

		private static double easeOutCirc(double value)
		{
			return Math.Sqrt(1.0 - Math.Pow(value - 1.0, 2.0));
		}

		private static double easeInOutCirc(double value)
		{
			return value < .5
				? (1.0 - Math.Sqrt(1.0 - Math.Pow(2.0 * value, 2.0))) / 2.0
				: (Math.Sqrt(1.0 - Math.Pow(-2.0 * value + 2.0, 2.0)) + 1.0) / 2.0;
		}

		private static double easeInBack(double value)
		{
			return c3 * value * value * value - c1 * value * value;
		}

		private static double easeOutBack(double value)
		{
			return 1.0 + c3 * Math.Pow(value - 1.0, 3.0) + c1 * Math.Pow(value - 1.0, 2.0);
		}

		private static double easeInOutBack(double value)
		{
			return value < .5
				? (Math.Pow(2.0 * value, 2.0) * ((c2 + 1.0) * 2.0 * value - c2)) / 2.0
				: (Math.Pow(2.0 * value - 2.0, 2.0) * ((c2 + 1.0) *(value * 2.0 - 2.0) + c2) +2.0) / 2.0;
		}

		private static double easeInElastic(double value)
		{
			return value == 0.0
				? 0.0
				: value == 1.0
				? 1.0
				: -Math.Pow(2.0, 10.0 * value - 10.0) * Math.Sin((value * 10.0 - 10.75) * c4);
		}

		private static double easeOutElastic(double value)
		{
			return value == 0.0
				? 0.0
				: value == 1.0
				? 1.0
				: Math.Pow(2.0, -10.0 * value) * Math.Sin((value * 10.0 - 10.75) * c4) +1.0;
		}

		private static double easeInOutElastic(double value)
		{
			return value == 0.0
				? 0.0
				: value == 1.0
				? 1.0
				: value < .5
				? -(Math.Pow(2.0, 20.0 * value - 10.0) * Math.Sin((20.0 * value - 11.125) * c5)) / 2.0
				: (Math.Pow(2.0, -20.0 * value + 10.0) * Math.Sin((20.0 * value - 11.125) * c5)) / 2.0 + 1.0;
		}

		private static double easeInBounce(double value)
		{
			return 1.0 - bounceOut(1.0 - value);
		}

		private static double easeInOutBounce(double value)
		{
			return value < .5
				? (1.0 - bounceOut(1.0 - 2.0 * value)) / 2.0
				: (1.0 + bounceOut(2.0 * value - 1.0)) / 2.0;
		}

	}
}
