using System.Collections.Concurrent;
using System.Reflection;
using Castle.DynamicProxy;

namespace Hangfire.Polly.Example;

public static class AutoFactory
{
    public static IServiceCollection AddAutoFactory<TFactory>(this IServiceCollection services)
        where TFactory : class
    {
        services.AddSingleton(CreateFactory<TFactory>);
        return services;
    }

    public static IServiceCollection AddAutoFactorySingleton<TFactory>(this IServiceCollection services)
        where TFactory : class
        => services.AddSingleton(CreateFactory<TFactory>);

    public static IServiceCollection AddAutoFactoryScoped<TFactory>(this IServiceCollection services)
        where TFactory : class
        => services.AddScoped(CreateFactory<TFactory>);

    public static IServiceCollection AddAutoFactoryTransient<TFactory>(this IServiceCollection services)
        where TFactory : class
        => services.AddTransient(CreateFactory<TFactory>);

    private static TFactory CreateFactory<TFactory>(IServiceProvider serviceProvider)
        where TFactory : class
        => new ProxyGenerator()
            .CreateInterfaceProxyWithoutTarget<TFactory>(new FactoryInterceptor(serviceProvider));
    // {
    //     var generator = new ProxyGenerator();
    //     return generator
    //         .CreateInterfaceProxyWithoutTarget<TFactory>(new FactoryInterceptor(serviceProvider));
    // }

    private class FactoryInterceptor : IInterceptor
    {
        private readonly ConcurrentDictionary<MethodInfo, ObjectFactory> _factories = new();
        private readonly IServiceProvider _serviceProvider;

        public FactoryInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Intercept(IInvocation invocation)
        {
            var factory = _factories.GetOrAdd(invocation.Method, CreateFactory);
            invocation.ReturnValue = factory(_serviceProvider, invocation.Arguments);
        }

        private ObjectFactory CreateFactory(MethodInfo method)
        {
            return ActivatorUtilities.CreateFactory(
                method.ReturnType,
                method.GetParameters().Select(p => p.ParameterType).ToArray());
        }
    }
}