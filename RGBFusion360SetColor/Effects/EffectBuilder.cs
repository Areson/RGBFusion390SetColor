using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RGBFusion390SetColor.Effects.Channels;
using System.Text.RegularExpressions;
using RGBFusion390SetColor.Animations;

namespace RGBFusion390SetColor.Effects
{    
    internal class EffectLibrary
    {
        private static EffectLibrary _library;
        public static EffectLibrary Library
        {
            get
            {
                if (_library == null)
                {
                    _library = new EffectLibrary();
                }

                return _library;
            }
        }

        private Dictionary<string, Effect> lookup = new Dictionary<string, Effect>();
        public readonly PassThroughChannel PassThroughChannel;
        public readonly Effect PassThroughEffect;        

        private EffectLibrary()
        {
            PassThroughChannel = new PassThroughChannel();
            PassThroughEffect = new ChannelEffect(null, PassThroughChannel, PassThroughChannel, PassThroughChannel, PassThroughChannel, PassThroughChannel, PassThroughChannel);
        }

        public bool EffectExists(string name)
        {
            var cleanName = EffectNameParser.CleanEffectName(name);

            return lookup.ContainsKey(cleanName);
        }

        public Effect GetEffect(string name)
        {
            var cleanName = EffectNameParser.CleanEffectName(name);

            if (lookup.ContainsKey(cleanName))
            {
                return lookup[cleanName].Clone();
            }
            else
            {
                throw new Exception($"The referenced effect {name} does not exist.");
            }
        }

        public void AddEffect(Effect effect)
        {
            if (!string.IsNullOrEmpty(effect.Name))
            {
                lookup[effect.Name] = effect;
            }
        }
    }

    internal static class EffectNameParser
    {
        public static string CleanEffectName(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, "[^a-zA-Z0-9_]+", "", System.Text.RegularExpressions.RegexOptions.Singleline);
        }
    }    

    internal class EffectBuilder
    {        
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = EffectNameParser.CleanEffectName(value);
            }
        }

        [JsonProperty(PropertyName = "steps")]
        public List<EffectTemplate> effects = new List<EffectTemplate>();

        public Effect Generate(EffectLibrary library)
        {
            Effect result = null;

            try
            {
                if (effects.Any())
                {
                    if (effects.Count == 1)
                    {
                        result = effects[0].Generate(library, Name);
                    }
                    else
                    {
                        if (effects.Any(e => !e.length.HasValue))
                        {
                            throw new Exception($"A step is missing an effect length. Effect lengths are required when defining a multi-step effect.");
                        }
                        else
                        {
                            var steps = effects.Select(e => new TimedEffect(e.Generate(library), e.length.Value)).ToArray();
                            result = new PipelineEffect(Name, steps);
                        }
                    }
                }
                else
                {
                    return library.PassThroughEffect;
                }

                if (result != null)
                {
                    library.AddEffect(result);
                }

                return result;
            }
            catch(Exception ex)
            {
                throw new Exception($"Effect {Name}: {ex.Message}");
            }
        }
    }

    internal class EffectTemplate
    {
        public string[] Name;
        public int? length;
        public string Mode;
        public string Speed;
        public string Red;
        public string Green;
        public string Blue;
        public string Brightness;

        public Effect Generate(EffectLibrary library, string name = null)
        {
            if (Name != null && Name.Any())
            {
                // We are making a composite effect
                var compositeEffects = Name.Select(x => library.GetEffect(x)).ToArray();
                return new CompositeEffect(name, compositeEffects);
            }
            else
            {
                return new ChannelEffect(
                    name,
                    ParseChannel(Mode) ?? library.PassThroughChannel,
                    ParseChannel(Speed) ?? library.PassThroughChannel,
                    ParseChannel(Red) ?? library.PassThroughChannel,
                    ParseChannel(Green) ?? library.PassThroughChannel,
                    ParseChannel(Blue) ?? library.PassThroughChannel,
                    ParseChannel(Brightness) ?? library.PassThroughChannel
                );
            }
        }

        private IChannel ParseChannel(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return null;
            }
            else
            {
                var cleanArgs = args.Replace(" ", "");
                var match = Regex.Match(cleanArgs, "([^\\(]+)\\(([^\\)]+)\\)");

                if (match.Success)
                {
                    var funType = match.Groups[1].Value;
                    var contents = match.Groups[2].Value.Split(',');

                    switch (contents.Length)
                    {
                        case 1:
                            return ParseEasing(funType, contents);

                        case 4:
                            return ParseGradient(funType, contents);

                        default:
                            throw new Exception($"Function type {funType} does not have the correct number of arguments.");
                    }
                }
                else
                {
                    var numMatch = Regex.Match(cleanArgs, "([0-9]+(\\.[0-9]+)?|\\.[0-9]+)");

                    if (numMatch.Success)
                    {
                        return new ConstantChannel(double.Parse(cleanArgs));
                    }
                    else
                    {
                        throw new Exception($"Unknown effect parameter value {args}.");
                    }
                }
            }
        }

        private IChannel ParseGradient(string gradientType, string[] args)
        {
            if (!Enum.TryParse(gradientType.ToUpper(), out Channels.GradientType gtype))
            {
                throw new Exception($"Unknown gradient type {gradientType} in definition.");
            }

            return new GradientChannel(gtype, double.Parse(args[0]), double.Parse(args[1]), double.Parse(args[2]), double.Parse(args[3]));
        }

        private IChannel ParseEasing(string easingType, string[] args)
        {
            if (!Enum.TryParse(easingType.ToUpper(), out EasingType eType))
            {
                throw new Exception($"Unknown easing type {easingType} in definition.");
            }

            return new EasingChannel(eType, double.Parse(args[0]));
        }
    }

    internal class AnimationTemplate
    {
        public sbyte AreaId;

        [JsonProperty(PropertyName = "steps")]
        public List<AnimationStepTemplate> AnimationSteps = new List<AnimationStepTemplate>();

        public Animation Generate(EffectLibrary library)
        {
            return new Animation(AreaId, AnimationSteps.Select(a => a.Generate(library)).ToArray());
        }
    }

    internal class AnimationStepTemplate
    {
        public string[] Effects = new string[0];

        public int? StepLength;

        public int? totalEffectLength;

        public bool playOnce = false;

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public AnimationStepEndingType EndingType = AnimationStepEndingType.CONTINUOUS;

        public AnimationStep Generate(EffectLibrary library)
        {
            var effectItems = Effects.Select(e => ParseEffect(e, library)).ToArray();
            return new AnimationStep(StepLength ?? effectItems.Sum(e => e.Length), playOnce, EndingType, effectItems);
        }

        private TimedEffect ParseEffect(string arg, EffectLibrary library)
        {
            var match = Regex.Match(arg, "([^\\(]+)\\(([^\\)]+)\\)");

            if (match.Success)
            {
                var effectName = EffectNameParser.CleanEffectName(match.Groups[1].Value);
                var duration = int.Parse(match.Groups[2].Value);

                return new TimedEffect(library.GetEffect(effectName), duration);
            }
            else
            {
                throw new Exception($"Effect name is not formatted correctly: {arg}.");
            }
        }
    }

    internal class AnimationFileTemplate
    {
        public sbyte[] SimulateBrightness = new sbyte[0];

        public List<EffectBuilder> Effects = new List<EffectBuilder>();

        public List<AnimationTemplate> Animations = new List<AnimationTemplate>();

        public List<Animation> Generate(EffectLibrary library)
        {
            // Marke any items as needed to be simulated
            foreach(var simulate in SimulateBrightness)
            {
                LedCommand.SimulateBrightness(simulate);
            }

            // Determine the evaluation order for effects and then generate them
            var effectLookup = Effects.ToDictionary(x => x.Name, x => x);
            var evaluationOrder = GetEvaluationOrder();

            foreach(var name in evaluationOrder)
            {
                effectLookup[name].Generate(library);
            }

            // Now generate the animations
            // Condense multiple definitions for the same area down to a single definition.
            return Animations
                .GroupBy(x => x.AreaId)
                .Select(x =>
                {
                    if (x.Count() == 1)
                    {
                        return x.First();
                    }
                    else
                    {
                        return new AnimationTemplate
                        {
                            AreaId = x.Key,
                            AnimationSteps = x.SelectMany(s => s.AnimationSteps).ToList()
                        };
                    }
                })
                .Select(a => a.Generate(library))
                .ToList();
        }

        private IEnumerable<string> GetEvaluationOrder()
        {
            // Generate the effect graph            
            var depths = Effects.ToDictionary(x => x.Name, x => 0);
            var dependencyLookup = new Dictionary<string, HashSet<string>>();
            var parentLookup = new Dictionary<string, HashSet<string>>();

            foreach (var effect in Effects)
            {
                var dependencies = effect.effects
                    .Where(e => e.Name != null && e.Name.Length > 0)
                    .SelectMany(e => e.Name.Where(n => !string.IsNullOrEmpty(n)).Select(n => n))
                    .Distinct()
                    .ToList();

                if (dependencies.Any())
                {
                    if (!parentLookup.ContainsKey(effect.Name))
                    {
                        parentLookup.Add(effect.Name, new HashSet<string>(dependencies));
                    }

                    dependencies.ForEach(d =>
                    {
                        if (!dependencyLookup.ContainsKey(d))
                        {
                            dependencyLookup.Add(d, new HashSet<string>());
                        }

                        dependencyLookup[d].Add(effect.Name);
                    });

                    // Determine our new value
                    var depth = dependencies.Select(d => depths[d]).Max() + 1;
                    depths[effect.Name] = depth;

                    if (dependencyLookup.ContainsKey(effect.Name))
                    {
                        var toUpdate = dependencyLookup[effect.Name];
                        foreach (var u in toUpdate)
                        {
                            depths[u] = Math.Max(depth + 1, depths[u]);
                        }
                    }
                }
            }

            // Circular dependency check
            var circular = dependencyLookup.SelectMany(x => x.Value.Select(d =>
            {
                var comp = string.Compare(d, x.Key);

                if (comp >= 0)
                {
                    return $"{d} <-> {x.Key}";
                }
                else
                {
                    return $"{x.Key} <-> {d}";
                }
            }))
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .ToList();

            if (circular.Any())
            {
                throw new Exception($"Circular references between effects exist: {string.Join(", ", circular)}");
            }

            return depths
                .OrderBy(d => d.Value)
                .Select(x => x.Key)
                .ToList();
        }
    }
}
