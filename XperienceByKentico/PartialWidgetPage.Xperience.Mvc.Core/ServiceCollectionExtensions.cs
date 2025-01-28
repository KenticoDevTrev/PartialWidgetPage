using Microsoft.Extensions.DependencyInjection;

namespace PartialWidgetPage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPartialWidgetPage<TRenderer>(this IServiceCollection services, bool addAjaxPWPJs = true)
        where TRenderer : class, IPartialWidgetRenderingRetriever
    {
        return services.AddPartialWidgetPageCore<TRenderer>(addAjaxPWPJs: addAjaxPWPJs);
    }

    public static IServiceCollection AddPartialWidgetPage(this IServiceCollection services, bool addAjaxPWPJs = true)
    {
        return services.AddPartialWidgetPageCore<DefaultPartialWidgetRenderingRetriever>(addAjaxPWPJs: addAjaxPWPJs);
    }

    private static IServiceCollection AddPartialWidgetPageCore<TRenderer>(this IServiceCollection services, bool addAjaxPWPJs = true)
        where TRenderer : class, IPartialWidgetRenderingRetriever
    {
        services
            .AddSingleton<IPartialWidgetRenderingRetriever, TRenderer>()
            .AddSingleton<IPartialWidgetPageHelper, PartialWidgetPageHelper>()
            .AddSingleton<IRenderPageViewModelGenerator, RenderPageViewModelGenerator>();
        if(addAjaxPWPJs) {
            services.AddSingleton<ITagHelperComponent, AjaxPartialWidgetTagHelperComponent>();
        }
        return services;
    }
}