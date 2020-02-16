using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace RGBFusion390SetColor
{
    public class AsyncQueue<T>
    {
        private ConcurrentQueue<T> queue;
        private ManualResetEvent messagesAvailable;
        private CancellationToken cancellationToken;

        public AsyncQueue(CancellationToken cancellationToken)
        {
            queue = new ConcurrentQueue<T>();
            messagesAvailable = new ManualResetEvent(false);
            this.cancellationToken = cancellationToken;
        }

        public void Enqueue(T message)
        {
            queue.Enqueue(message);
            messagesAvailable.Set();
        }

        public IEnumerable<T> Messages
        {
            get
            {
                var waitHandles = new WaitHandle[] { cancellationToken.WaitHandle, messagesAvailable };
                var done = false;

                while (!done)
                {
                    switch (WaitHandle.WaitAny(waitHandles))
                    {
                        case 0:
                            // The cancellation token was fired
                            done = true;
                            break;

                        case 1:
                            // Reset the wait handle
                            messagesAvailable.Reset();

                            // Process messages as long as we have them
                            while (!queue.IsEmpty)
                            {
                                while (queue.TryDequeue(out T value))
                                {
                                    yield return value;
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
