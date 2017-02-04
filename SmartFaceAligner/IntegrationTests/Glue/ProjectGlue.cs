using Autofac;
using BingLibrary;
using SmartFaceAligner.Processor.Glue;
using XamlingCore.NET.Glue;
using XamlingCore.Portable.Data.Glue;
using XCoreLite.Glue;

namespace IntegrationTests.Glue
{
    public class ProjectGlue : NETGlue
    {
        public override void Init()
        {
            base.Init();

            Builder.RegisterModule<XCoreLiteModule>();
            Builder.RegisterModule<ProcessorModule>();
            Builder.RegisterModule<BingModule>();

            Container = Builder.Build();
            ContainerHost.Container = Container;
        }

     }
}
