using Microsoft.Extensions.DependencyInjection;

namespace PartialWidgetPage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPartialWidgetPage<TRenderer>(this IServiceCollection services)
        where TRenderer : class, IPartialWidgetRenderingRetriever
    {
        return services.AddPartialWidgetPageCore<TRenderer>();
    }

    public static IServiceCollection AddPartialWidgetPage(this IServiceCollection services)
    {
        return services.AddPartialWidgetPageCore<DefaultPartialWidgetRenderingRetriever>();
    }

    public static IServiceCollection AddPartialWidgetPageCore<TRenderer>(this IServiceCollection services)
        where TRenderer : class, IPartialWidgetRenderingRetriever
    {
        return services
            .AddSingleton<IPartialWidgetRenderingRetriever, TRenderer>()
            .AddSingleton<IPartialWidgetPageHelper, PartialWidgetPageHelper>()
            .AddSingleton<IRenderPageViewModelGenerator, RenderPageViewModelGenerator>();
    }
}