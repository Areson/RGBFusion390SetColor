using RGBFusion390SetColor.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.IO;
using Newtonsoft.Json;
using RGBFusion390SetColor.Effects;
using RGBFusion390SetColor.Effects.Channels;

namespace RGBFusion390SetColor
{
    public static class CommandLineParser
    {        
        public static List<LedCommand> GetLedCommands(string[] args)
        {
            List<LedCommand> ledCommands = new List<LedCommand>();
            Dictionary<sbyte, Animation> lookup = new Dictionary<sbyte, Animation>();

            for (var i = 0; i < args.Length; i++)            
            {
                var arg = args[i];

                try
                {
                    if (arg.ToLower().Contains("--setarea:") || arg.ToLower().Contains("--sa:"))
                    {
                        try
                        {
                            var commandParts = arg.Split(':');
                            var command = new LedCommand();
                            if (commandParts.Length < 6)
                            {
                                throw new Exception("Wrong value count in " + arg);
                            }

                            if (commandParts.Length >= 6)
                            {

                                command.AreaId = sbyte.Parse(commandParts[1]);
                                command.NewMode = sbyte.Parse(commandParts[2]);
                                command.NewColor = Color.FromRgb(byte.Parse(commandParts[3]), byte.Parse(commandParts[4]), byte.Parse(commandParts[5]));
                                command.Bright = 9;
                                command.Speed = 2;
                            }
                            if (commandParts.Length >= 8)
                            {
                                command.Speed = sbyte.Parse(arg.Split(':')[6]);
                                command.Bright = sbyte.Parse(arg.Split(':')[7]);
                            }


                            ledCommands.Add(command);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Wrong --setarea: command in GetLedCommands: {ex.Message}");
                        }
                    }
                    else if (arg.ToLower().Contains("--animation-file"))
                    {
                        try
                        {
                            var fileName = args[i + 1].Trim();

                            if (!File.Exists(fileName))
                            {
                                throw new Exception($"Could not file animation file {fileName}.");
                            }
                            else
                            {
                                using (var sr = new StreamReader(fileName))
                                {
                                    var data = sr.ReadToEnd();
                                    var animationTemplate = JsonConvert.DeserializeObject<AnimationFileTemplate>(data);
                                    var animationData = animationTemplate.Generate(EffectLibrary.Library);

                                    ledCommands.AddRange(animationData.Select(x => new LedCommand { AreaId = -4, Animation = x }));
                                }

                                i++;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw new Exception($"Wrong --animation-file: {ex.Message}");
                        }
                    }
                    else if (arg.ToLower().Contains("--animation:"))
                    {
                        try
                        {
                            var animationParts = arg.Split(':');

                            if (animationParts.Length == 2)
                            {
                                switch (animationParts[1].ToLower())
                                {
                                    case "play":
                                        ledCommands.Add(new LedCommand { AreaId = -5 });
                                        break;

                                    case "pause":
                                        ledCommands.Add(new LedCommand { AreaId = -6 });
                                        break;

                                    case "stop":
                                        ledCommands.Add(new LedCommand { AreaId = -7 });
                                        break;
                                }
                            }
                            else
                            {
                                if (animationParts.Length < 6)
                                {
                                    throw new Exception("Wrong value count in " + arg);
                                }

                                sbyte areaId = sbyte.Parse(animationParts[1]);
                                int length = int.Parse(animationParts[2]);

                                var effectTemplate = new EffectTemplate()
                                {
                                    Mode = animationParts[3],
                                    Red = animationParts[4],
                                    Green = animationParts[5],
                                    Blue = animationParts[6],
                                };

                                if (animationParts.Length > 6)
                                {
                                    effectTemplate.Speed = animationParts[7];
                                }

                                if (animationParts.Length > 7)
                                {
                                    effectTemplate.Brightness = animationParts[8];
                                }

                                var effect = effectTemplate.Generate(EffectLibrary.Library);

                                if (!lookup.ContainsKey(areaId))
                                {
                                    lookup[areaId] = new Animation(areaId);
                                }

                                lookup[areaId].AddStep(new AnimationStep(length, false, AnimationStepEndingType.CONTINUOUS));
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Wrong --animation: command in GetLedCommands: {ex.Message}");
                        }
                    }                   
                    else if (arg.ToLower().Contains("--simulate-brightness:"))
                    {
                        var brightnessParts = arg.Split(':');
                        var areaId = sbyte.Parse(brightnessParts[1]);
                        var enabled = int.Parse(brightnessParts[2]) == 1;

                        LedCommand.SimulateBrightness(areaId, enabled);
                    }
                }
                catch(Exception oe)
                {
                    MessageBox.Show(messageBoxText: oe.Message);
                }
            }

            if (lookup.Any())
            {
                ledCommands.AddRange(lookup.Values.Select(a => new LedCommand { AreaId = -4, Animation = a }));
            }

            return ledCommands;
        }

        public static int LoadProfileCommand(string[] args)
        {
            var profileId = -1;
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("--loadprofile:"))
                {
                    try
                    {
                        profileId = sbyte.Parse(arg.Split(':')[1]);
                        break;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong --loadprofile: command in LoadProfileCommand: {ex.Message}");
                    }
                }
            }
            return profileId;
        }

        public static bool HasExitCommand(string[] args)
        {
            return args.Any(a => a.ToLower().Contains("--exit"));
        }

        public static bool GetAreasCommand(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--getareas") || s.ToLower().Contains("--areas"));
        }

        public static bool StartMusicMode(string[] args)
        {
            return args.Any(s => s.ToLower().Contains("--startmusicmode"));
        }
        public static bool StopMusicMode(string[] args)
        {
            return args.Any(s => s.ToLower().Contains("--startmusicmode"));
        }
    }
}
