using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace PartialWidgetPage;

internal class RenderPageViewModelGenerator(
    IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
    IWebPageDataContextRetriever webPageDataContextRetriever,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    ICompositeViewEngine compositeViewEngine)
    : IRenderPageViewModelGenerator
{
    private const string VIEW_DEFAULT_PATH = "~/Views/Shared/ContentTypes/{0}.cshtml";
    private readonly IComponentDefinitionProvider<PageTemplateDefinition> mComponentDefinitionProvider = new ComponentDefinitionProvider<PageTemplateDefinition>();


    public async Task<RenderPageViewModel> GeneratePageViewModel(int pageId,
        PreservedPageBuilderContext preservedPageBuilderContext, CancellationToken token = default)
    {
        var webPageContext = webPageDataContextRetriever.Retrieve();
        var pageBuilderContext = pageBuilderDataContextRetriever.Retrieve();


        var path = await GetViewPath(pageId, token);
        var component = ComponentViewModel.Create(webPageContext.WebPage);
        var model = new RenderPageViewModel(path, component);

        //page uses page template
        if (pageBuilderContext.Configuration.PageTemplate != null)
        {
            var definition = mComponentDefinitionProvider.GetAll().SingleOrDefault(x =>
                x.Identifier.Equals(pageBuilderContext.Configuration.PageTemplate.Identifier,
                    StringComparison.OrdinalIgnoreCase));

            if (definition != null)
            {
                model = model with
                {
                    ViewPath = definition.IsCustom
                        ? definition.ViewPath
                        : $"PageTemplates/_{definition.Identifier}"
                };

                //Page Template Definition has properties
                if (definition.PropertiesType != null)
                {
                    var componentViewModelOfTProperties =
                        typeof(ComponentViewModel<>).MakeGenericType(definition.PropertiesType);
                    var method = componentViewModelOfTProperties.GetMethod(nameof(ComponentViewModel.Create));

                    if (method != null)
                    {
                        var result = method.Invoke(componentViewModelOfTProperties,
                            [webPageContext.WebPage, pageBuilderContext.Configuration.PageTemplate.Properties]);

                        if (result != null)
                        {
                            model = model with
                            {
                                ComponentViewModel = result
                            };
                        }
                    }
                }
            }
        }

        model = model with
        {
            ViewExists = compositeViewEngine.GetView(null, model.ViewPath, false).Success
        };

        return model;
    }

    private async Task<string> GetViewPath(int pageId, CancellationToken token = default)
    {
        return string.Format(VIEW_DEFAULT_PATH, (await webPageItemInfoProvider.RetrieveClassName(pageId, token: token)).Replace('.', '_'));
    }
}