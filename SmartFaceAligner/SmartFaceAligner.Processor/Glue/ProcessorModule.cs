using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.ProjectOxford.Face;
using SmartFaceAligner.Processor.Contract;
using SmartFaceAligner.Processor.Services;

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

            builder.Register((c, r)=>new FaceServiceClient(c.Resolve<IConfig>()[""]))

            base.Load(builder);
        }
    }
}
