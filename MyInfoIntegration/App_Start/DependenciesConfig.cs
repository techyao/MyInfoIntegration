using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using MyInfoIntegration.Services.Contract;
using MyInfoIntegration.Services.Implementation;

namespace MyInfoIntegration
{
    public class DependenciesConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterApiControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            // OPTIONAL: Register the Autofac model binder provider.
            builder.RegisterWebApiModelBinderProvider();

            //IMyInfoService
            builder.RegisterType<MyInfoService>().As<IMyInfoService>().InstancePerDependency();

            //builder.RegisterType<APIClientHelper>().AsSelf().InstancePerRequest();
            //builder.Register(ctx =>
            //{
            //    return ctx.Resolve<IMyInfoService>().CreateRepository<MyInfoService>();
            //}).As<MyInfoService>().InstancePerRequest();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}