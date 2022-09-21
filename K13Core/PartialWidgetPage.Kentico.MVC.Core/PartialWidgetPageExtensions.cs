using Microsoft.Extensions.DependencyInjection;
using PartialWidgetPage.Internal;

namespace PartialWidgetPage
{
    public static  class PartialWidgetPageExtensions
    {
        public static IServiceCollection AddPartialWidgetPage(this IServiceCollection services)
        {
            services.AddSingleton<IPartialWidgetPageHelper, PartialWidgetPageHelper>()
                .AddSingleton<IPartialWidgetRenderingRetriever, DefaultPartialWidgetRenderingRetriever>()
                .AddSingleton<IComponentViewModelGenerator, ComponentViewModelGenerator>();
            return services;
        }
    }
}
