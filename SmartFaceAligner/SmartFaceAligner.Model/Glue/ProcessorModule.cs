using Autofac;
using Microsoft.ProjectOxford.Face;
using SmartFaceAligner.Processor.Entity;
using SmartFaceAligner.Processor.Services;
using XamlingCore.Portable.Contract.Config;

namespace SmartFaceAligner.Processor.Glue
{
    public class ProcessorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(ProcessorModule).Assembly)
              .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Repo"))
              .AsImplementedInterfaces()
              .InstancePerLifetimeScope().PropertiesAutowired();

            builder.RegisterType<ConfigService>().AsImplementedInterfaces().SingleInstance();

            builder.Register(
                (c, r) => new FaceServiceClient(c.Resolve<IConfig>()[Constants.Settings.SubsKeys]));

            base.Load(builder);
        }
    }
}
