using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;


namespace RGBFusion390SetColor
{
    public class ArgsPipeInterOp
    {
        private CancellationToken cancellationToken;

        public ArgsPipeInterOp(CancellationToken cancellationToken)
        {            
            this.cancellationToken = cancellationToken;
        }

        public void StartArgsPipeServer(int serverInstances)
        {
            var s = new NamedPipeServerStream("RGBFusion390SetColor", PipeDirection.In, serverInstances);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(s, callback: ar => { }, @object: this);
        }

        private void GetArgsCallBack(NamedPipeServerStream pipe)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                pipe.WaitForConnectionAsync(cancellationToken).Wait();

                if (!cancellationToken.IsCancellationRequested)
                {
                    using (var sr = new StreamReader(pipe, System.Text.Encoding.ASCII, false, 1024, true))
                    {
                        var args = sr.ReadToEnd().Split(' ');
                        Task.Run(() => Program.Run(args));
                    }

                    pipe.Disconnect();
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public void SendArgs(string[] args)
        {
            using (var pipe = new NamedPipeClientStream(".", "RGBFusion390SetColor", PipeDirection.Out))
            using (var stream = new StreamWriter(pipe))
            {
                pipe.Connect(timeout: 1000);
                stream.Write(string.Join(" ", args));
            }
        }
    }
}
