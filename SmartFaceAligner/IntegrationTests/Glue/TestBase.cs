using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartFaceAligner.Processor.Glue;

namespace IntegrationTests.Glue
{
    public class TestBase
    {
        protected IContainer Container;

        public TestBase()
        {
            var g = new ProjectGlue();
            g.Init();
            Container = g.Container;
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}
