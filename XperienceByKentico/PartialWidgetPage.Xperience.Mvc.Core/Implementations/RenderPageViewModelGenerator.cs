using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace PartialWidgetPage;

internal class RenderPageViewModelGenerator : IRenderPageViewModelGenerator
{
    private const string VIEW_DEFAULT_PATH = "~/Views/Shared/ContentTypes/{0}.cshtml";
    private readonly IComponentDefinitionProvider<PageTemplateDefinition> mComponentDefinitionProvider;


    private readonly ICompositeViewEngine mCompositeViewEngine;
    private readonly IPageBuilderDataContextRetriever mPageBuilderDataContextRetriever;

    private readonly IWebPageDataContextRetriever mWebPageDataContextRetriever;

    private readonly IInfoProvider<WebPageItemInfo> mWebPageItemInfoProvider;

    public RenderPageViewModelGenerator(
        IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
        IWebPageDataContextRetriever webPageDataContextRetriever,
        IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
        ICompositeViewEngine compositeViewEngine)
    {
        mPageBuilderDataContextRetriever = pageBuilderDataContextRetriever;
        mWebPageDataContextRetriever = webPageDataContextRetriever;
        mWebPageItemInfoProvider = webPageItemInfoProvider;
        mCompositeViewEngine = compositeViewEngine;

        mComponentDefinitionProvider = new ComponentDefinitionProvider<PageTemplateDefinition>();
    }

    public async Task<RenderPageViewModel> GeneratePageViewModel(int pageId,
        PreservedPageBuilderContext preservedPageBuilderContext, CancellationToken token = default)
    {
        var webPageContext = mWebPageDataContextRetriever.Retrieve();
        var pageBuilderContext = mPageBuilderDataContextRetriever.Retrieve();


        var model = new RenderPageViewModel
        {
            ViewPath = await GetViewPath(pageId, token),
            ComponentViewModel = ComponentViewModel.Create(webPageContext.WebPage)
        };

        //page uses page template
        if (pageBuilderContext.Configuration.PageTemplate != null)
        {
            var definition = mComponentDefinitionProvider.GetAll().SingleOrDefault(x =>
                x.Identifier.Equals(pageBuilderContext.Configuration.PageTemplate.Identifier,
                    StringComparison.OrdinalIgnoreCase));

            if (definition != null)
            {
                model.ViewPath = definition.IsCustom
                    ? definition.ViewPath
                    : $"PageTemplates/_{definition.Identifier}";

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

                        if (result != null) model.ComponentViewModel = result;
                    }
                }
            }
        }

        model.ViewExists = mCompositeViewEngine.GetView(null, model.ViewPath, false).Success;

        return model;
    }

    private async Task<string> GetViewPath(int pageId, CancellationToken token = default)
    {
        return string.Format(VIEW_DEFAULT_PATH, (await mWebPageItemInfoProvider.RetrieveClassName(pageId, token: token)).Replace('.', '_'));
    }
}