using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFaceAligner.Util
{
    public static class TaskHelper
    {
        public static async Task Parallel(this Queue<Func<Task>> queue, int parallelCount)
        {
            var processors = new List<Task>();

            for(var i = 0; i < parallelCount; i++)
            {
                processors.Add(_process(queue));
            }

            await Task.WhenAll(processors);
        }

        static async Task _process(Queue<Func<Task>> queue)
        {
            while (queue.Count > 0)
            {
                await Task.Run(async () =>
                {
                    var q = queue.Dequeue();
                    await q();
                });
            }
        }
    }
}
