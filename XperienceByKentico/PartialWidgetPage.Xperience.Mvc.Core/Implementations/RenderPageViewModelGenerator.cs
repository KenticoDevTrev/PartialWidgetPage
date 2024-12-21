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
    private const string _VIEW_DEFAULT_PATH = "~/Views/Shared/ContentTypes/{0}.cshtml";
    private readonly ComponentDefinitionProvider<PageTemplateDefinition> _componentDefinitionProvider = new();
    private readonly IPageBuilderDataContextRetriever _pageBuilderDataContextRetriever = pageBuilderDataContextRetriever;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever = webPageDataContextRetriever;
    private readonly IInfoProvider<WebPageItemInfo> _webPageItemInfoProvider = webPageItemInfoProvider;
    private readonly ICompositeViewEngine _compositeViewEngine = compositeViewEngine;

    public async Task<RenderPageViewModel> GeneratePageViewModel(int pageId,
        PreservedPageBuilderContext preservedPageBuilderContext, CancellationToken token = default)
    {
        var webPageContext = _webPageDataContextRetriever.Retrieve();
        var pageBuilderContext = _pageBuilderDataContextRetriever.Retrieve();


        var path = await GetViewPath(pageId, token);
        var component = ComponentViewModel.Create(webPageContext.WebPage);
        var model = new RenderPageViewModel(path, component);

        //page uses page template
        if (pageBuilderContext.Configuration.PageTemplate != null)
        {
            var definition = _componentDefinitionProvider.GetAll().SingleOrDefault(x =>
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
            ViewExists = _compositeViewEngine.GetView(null, model.ViewPath, false).Success
        };

        return model;
    }

    private async Task<string> GetViewPath(int pageId, CancellationToken token = default)
    {
        return string.Format(_VIEW_DEFAULT_PATH, (await _webPageItemInfoProvider.RetrieveClassName(pageId, token: token)).Replace('.', '_'));
    }
}