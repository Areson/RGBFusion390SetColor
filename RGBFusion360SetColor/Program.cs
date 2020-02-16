using System;
using System.Diagnostics;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace RGBFusion390SetColor
{
    internal static class Program
    {
        private static RgbFusion _fusion;
        private static CancellationTokenSource cancellationTokenSource;
        private static List<ArgsPipeInterOp> pipeServers;

        [STAThread]
        private static void Main(string[] args)
        {
            Util.SetPriorityProcessAndThreads(Process.GetCurrentProcess().ProcessName, ProcessPriorityClass.Idle, ThreadPriorityLevel.Lowest);

            cancellationTokenSource = new CancellationTokenSource();
            var pipeInterOp = new ArgsPipeInterOp(cancellationTokenSource.Token);
            var thisProcess = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
            if (thisProcess > 1)
            {
                pipeInterOp.SendArgs(args);
                return;
            }

            // Setup the server pipes
            InitializeServerPipes();            

            // Setup the service
            _fusion = new RgbFusion(cancellationTokenSource.Token);
            Util.MinimizeMemory();
            
            // Send any arguments that were called with this run
            if (args.Any())
            {
                pipeInterOp.SendArgs(args);
            }

            // Finish initilization
            Util.MinimizeMemory();
            _fusion.Init();
        }

        private static void InitializeServerPipes()
        {
            var numberOfServers = 4;
            pipeServers = new List<ArgsPipeInterOp>();

            for(var i = 0; i < numberOfServers; i++)
            {
                var server = new ArgsPipeInterOp(cancellationTokenSource.Token);
                server.StartArgsPipeServer(numberOfServers);
                pipeServers.Add(server);
            }
        }

        public static void Run(string[] args)
        {
            var isInitialized = _fusion?.IsInitialized() ?? false;
            
            if (CommandLineParser.LoadProfileCommand(args) > 0)
            {
                _fusion?.EnqueueCommands(new LedCommand[] {
                    new LedCommand
                    {
                        AreaId = -3,
                        NewMode = (sbyte)CommandLineParser.LoadProfileCommand(args),
                    }                    
                });
            }

            if (CommandLineParser.GetLedCommands(args).Count > 0)
            {
                _fusion?.EnqueueCommands(CommandLineParser.GetLedCommands(args).ToArray());
            }
            else if (isInitialized && CommandLineParser.GetAreasCommand(args))
                MessageBox.Show(_fusion?.GetAreasReport());
            else if (CommandLineParser.StartMusicMode(args))
            {
                _fusion?.EnqueueCommands(new LedCommand[] {
                    new LedCommand
                    {
                        AreaId = -2,
                        NewMode = 1,
                    }
                });
            }
            else if (CommandLineParser.StopMusicMode(args))
            {
                _fusion?.EnqueueCommands(new LedCommand[] {
                    new LedCommand
                    {
                        AreaId = -2,
                        NewMode = 0,
                    }
                });
            }

            if (CommandLineParser.HasExitCommand(args))
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}
