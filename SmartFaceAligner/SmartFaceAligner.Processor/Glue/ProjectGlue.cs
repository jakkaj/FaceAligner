using Autofac;
using XamlingCore.NET.Glue;
using XamlingCore.Portable.Data.Glue;


namespace SmartFaceAligner.Processor.Glue
{
    public class ProjectGlue : NETGlue
    {
        public override void Init()
        {
            base.Init();

            Builder.RegisterModule<ProcessorModule>();

            Container = Builder.Build();
            ContainerHost.Container = Container;
        }

     }
}
