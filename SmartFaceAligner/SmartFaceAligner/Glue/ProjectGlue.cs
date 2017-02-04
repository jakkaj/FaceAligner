using System.Windows.Navigation;
using System.Windows.Threading;
using Autofac;
using SmartFaceAligner.Processor.Glue;
using SmartFaceAligner.Util;
using XamlingCore.NET.Glue;
using XamlingCore.Portable.Contract.UI;
using XamlingCore.Portable.Data.Glue;
using XCoreLite.Glue;

namespace SmartFaceAligner.Glue
{
    public class ProjectGlue : NETGlue
    {
        private readonly NavigationService _navigationService;

        public ProjectGlue(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        public override void Init()
        {
            base.Init();

            Builder.RegisterModule<XCoreLiteModule>();
            Builder.RegisterModule<ProcessorModule>();

            Builder.Register((c) => _navigationService);
            Builder.Register((c) => new XDispatcher(Dispatcher.CurrentDispatcher)).As<IDispatcher>();

            Builder.RegisterAssemblyTypes(typeof(ProjectGlue).Assembly)
                .Where(t => t.Name.EndsWith("View") || t.Name.EndsWith("ViewModel"))
                .AsSelf()
                .PropertiesAutowired()
                .InstancePerDependency();


            Container = Builder.Build();
            ContainerHost.Container = Container;
        }

     }
}
