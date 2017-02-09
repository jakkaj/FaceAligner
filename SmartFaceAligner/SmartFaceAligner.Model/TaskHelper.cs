using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFaceAligner.Processor
{
    public static class TaskHelper
    {
        public static async Task Parallel(this Queue<Func<Task>> queue, int parallelCount, CancellationToken token = default(CancellationToken))
        {
            var processors = new List<Task>();

            for(var i = 0; i < parallelCount; i++)
            {
                processors.Add(_process(queue, token));
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

        static async Task _process(Queue<Func<Task>> queue, CancellationToken token)
        {
            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                var q = queue.Dequeue();
                await Task.Run(()=>q());
            }
        }
    }
}
