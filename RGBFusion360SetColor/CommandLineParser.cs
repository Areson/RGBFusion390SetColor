using RGBFusion390SetColor.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RGBFusion390SetColor
{
    public static class CommandLineParser
    {
        private static HashSet<string> gradientMap = new HashSet<string>
        {
            "sin",
            "cos",
            "saw",
            "bsaw",
            "const"
        };

        private static GradientInfo ParseGradientInfo(string data)
        {
            var parts = data.Split(',');
            double center;
            double width = 0;
            double frequency = 0;
            double phase = 0;
            GradientType gradientType = GradientType.CONST;

            if (parts.Length > 1)
            {
                gradientType = (GradientType)Enum.Parse(typeof(GradientType), parts[0].ToUpper());
                
                center = double.Parse(parts[1]);

                if (gradientType != GradientType.CONST)
                {
                    width = double.Parse(parts[2]);
                    frequency = double.Parse(parts[3]);
                    phase = double.Parse(parts[4]);
                }
            }
            else
            {
                center = int.Parse(parts[0]);
            }            

            return new GradientInfo(frequency, phase, center, width, gradientType);
        }

        public static List<LedCommand> GetLedCommands(string[] args)
        {
            List<LedCommand> ledCommands = new List<LedCommand>();
            Dictionary<sbyte, Animation.Animation> lookup = new Dictionary<sbyte, Animation.Animation>();

            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("--setarea:") || arg.ToLower().Contains("--sa:"))
                {
                    try
                    {
                        var commandParts = arg.Split(':');
                        var command = new LedCommand();
                        if (commandParts.Length < 6)
                        {
                            throw new Exception( "Wrong value count in " + arg);
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
                    catch (Exception)
                    {
                        MessageBox.Show(messageBoxText: "Wrong --setarea: command in GetLedCommands: {ex.Message}");
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
                            if (animationParts.Length < 5)
                            {
                                throw new Exception("Wrong value count in " + arg);
                            }

                            sbyte areaId = sbyte.Parse(animationParts[1]);
                            int length = int.Parse(animationParts[2]);
                            var mode = ParseGradientInfo(animationParts[3]);
                            var red = ParseGradientInfo(animationParts[4]);
                            var green = ParseGradientInfo(animationParts[5]);
                            var blue = ParseGradientInfo(animationParts[6]);
                            var brightness = ParseGradientInfo(animationParts[7]);

                            if (!lookup.ContainsKey(areaId))
                            {
                                lookup[areaId] = new Animation.Animation(areaId);
                            }

                            lookup[areaId].AddPhase(new AnimationPhase(length, mode, red, green, blue, brightness));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(messageBoxText: $"Wrong --setarea: command in GetLedCommands: {ex.Message}");
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
