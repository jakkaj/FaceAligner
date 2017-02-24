using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using XamlingCore.Portable.Messages.XamlingMessenger;

namespace SmartFaceAligner.Processor
{
    public class TaskProgressMessage : XMessage
    {
        public int Total { get; }

        public int Count { get; }

        public TaskProgressMessage(int count, int total)
        {
            Total = total;
            Count = count;
        }
    }

    public static class TaskHelper
    {
        public static async Task Parallel(this Queue<Func<Task>> queue, int parallelCount, CancellationToken token = default(CancellationToken))
        {
            var processors = new List<Task>();

            for(var i = 0; i < parallelCount; i++)
            {
                processors.Add(_process(queue, queue.Count, token));
            }

            try
            {
                await Task.WhenAll(processors);
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine("Cancelled tasks operation");
            }
           
        }

        static async Task _process(Queue<Func<Task>> queue, int queueLength, CancellationToken token)
        {
            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                var q = queue.Dequeue();
                //await q();
                await Task.Run(() => q(), token);
                new TaskProgressMessage(queue.Count, queueLength).Send();
            }
        }
    }
}
