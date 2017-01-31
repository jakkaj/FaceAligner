using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using XCoreLite.Navigation;

namespace XCoreLite.Glue
{
    public class XCoreLiteModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WindowsNativeViewResolver>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<XNavigator>().AsImplementedInterfaces().SingleInstance();

            base.Load(builder);
        }
    }
}
