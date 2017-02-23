using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartFaceAligner.Processor;


namespace IntegrationTests.Tests
{
    [TestClass]
    public class ParallelTests
    {
        [TestMethod]
        public async Task TestParallelThings()
        {
            var tasks = new Queue<Func<Task>>();

            for (var i = 0; i < 200; i++)
            {
                int c = i;
                tasks.Enqueue(()=>_somFucn(c));
            }
            await Task.Delay(5000);

            await tasks.Parallel(20);
        }

        async Task _somFucn(int i)
        {
            await Task.Delay(1000);
            Debug.WriteLine("Finished" + i);
        }
    }
}
