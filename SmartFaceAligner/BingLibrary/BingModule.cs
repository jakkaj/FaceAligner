using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BingLibrary.BingSdkFromSwaggerClient;
using Contracts;
using Contracts.Interfaces;
using Microsoft.Rest;

namespace BingLibrary
{
    public class BingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(
                (context =>
                    new BingSdkFromSwaggerClient.BingSdkFromSwaggerClient(
                        new TokenCredentials(context.Resolve<IConfigurationService>().BingSearchSubscriptionKey))))
                .As<IBingSdkFromSwaggerClient>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
