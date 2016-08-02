using System;
using Microsoft.Practices.Unity;
using System.Web.Http;
using UrlShortener.Business;
using Unity.WebApi;
using UrlShortener.Data;

namespace UrlShortener.Web
{
    public static class UnityConfig
    {
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IShortenerContext, ShortenerContext>();
            container.RegisterType<IUrlManager, UrlManager>();
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}