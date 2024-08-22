using CMS.ContentEngine;
using CMS.Core;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Routing;

namespace PartialWidgetPage;

public class PartialWidgetPageHelper(IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
    IWebPageDataContextRetriever webPageDataContextRetriever,
    IHttpContextRetriever httpContextRetriever,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    IInfoProvider<ContentItemInfo> contentItemInfoProvider,
    IInfoProvider<ContentLanguageInfo> languageInfoProvider,
    IHttpContextAccessor httpContextAccessor,
    IWebPageDataContextInitializer dataContextInitializer,
    IContentQueryExecutor executor,
    IProgressiveCache cache)
    : IPartialWidgetPageHelper
{
    private HttpContext Context => httpContextAccessor.HttpContext ?? 
                                   throw new ArgumentNullException(nameof(httpContextAccessor));
    private IFeatureSet Kentico => Context.Kentico();
    
    public PreservedPageBuilderContext GetCurrentContext()
    {
        var pageBuilderContext = pageBuilderDataContextRetriever.Retrieve();

        RoutedWebPage? page = null;
        
        if (webPageDataContextRetriever.TryRetrieve(out var webPageContext))
        {
            page = webPageContext.WebPage;
        }

        var featureSet = Kentico.GetFeature<IPageBuilderFeature>();
        return new PreservedPageBuilderContext(featureSet, pageBuilderContext, page);
    }

    public void ChangeContext() => 
        ChangeContextInternal();
    
    public void ChangeContext(int identifier, string language, string channel) => 
        ChangeContextAsync(identifier, language, channel).GetAwaiter().GetResult();

    public async Task ChangeContextAsync(int identifier, string languageName, string channel, CancellationToken token = default)
    {
        async Task<RoutedWebPage?> GetCachedDataInternal(CacheSettings cs)
        {
            if (cs.Cached)
            {
                cs.CacheDependency = CacheHelper.GetCacheDependency([$"webpageitem|byid|{identifier}", $"webpageitem|byid|{identifier}|{languageName}"]);
            }

            var contentName = GetContentTypeName(identifier);

            var builder = new ContentItemQueryBuilder().ForContentType(contentName, q =>
                {
                    q
                        .Where(w => w.Where(where =>
                            where.WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemID), identifier)))
                        .ForWebsite(channel)
                        .TopN(1);
                })
                .InLanguage(languageName);

            var options = new ContentQueryExecutionOptions()
            {
                ForPreview = Kentico.Preview().Enabled, 
                IncludeSecuredItems = Kentico.Preview().Enabled
            };

            var result = (await executor.GetWebPageResult(builder, async (container) =>
            {
                var languageInfo =
                    await languageInfoProvider.GetAsync(container.ContentItemCommonDataContentLanguageID, token);

                return new RoutedWebPage()
                {
                    WebPageItemID = container.WebPageItemID, 
                    ContentTypeName = contentName,
                    LanguageName = languageInfo.ContentLanguageName
                };
            }, options, token));
            
            return result.FirstOrDefault();
        }

        var cacheSettings = new CacheSettings(CacheHelper.CacheMinutes(),
            "PWP", nameof(ChangeContext), identifier, languageName, channel);

        var webPage = await cache.LoadAsync(GetCachedDataInternal, cacheSettings);
        
        if (webPage is not null)
        {
            ChangeContextInternal(webPage);
        }
    }

    public void RestoreContext(PreservedPageBuilderContext previousContext)
    {
        // Restore
        var context = httpContextRetriever.GetContext();
        
        Kentico.SetFeature(previousContext.PageBuilderFeature);

        context.Items[PAGE_BUILDER_DATA_CONTEXT_KEY] = previousContext.PageBuilderContext;
        context.Items[WEB_PAGE_DATA_CONTEXT_KEY] = previousContext.Page;
    }

    public string? LayoutIfEditMode(string layout)
    {
        return Context.PartialWidgetPage().LayoutIfEditMode(layout);
    }

    public string? LayoutIfNotAjax(string layout)
    {
        return Context.PartialWidgetPage().LayoutIfNotAjax(layout);
    }

    private void ChangeContextInternal(RoutedWebPage? webPage = null)
    {
        var context = httpContextRetriever.GetContext();
        
        var feature = Kentico.GetFeature<IPageBuilderFeature>();
        
        var pageBuilderContext = new PageBuilderDataContext
        {
            Options = feature.Options,
            EditMode = false,
        };

        context.Items[PAGE_BUILDER_DATA_CONTEXT_KEY] = pageBuilderContext;

        dataContextInitializer.Initialize(webPage);
        
        //Store new cloned context to change edit mode for rendered page builder interface
        Kentico.SetFeature<IPageBuilderFeature>(new ClonedPageBuilderFeature(context, pageBuilderContext));
    }
    
    private string GetContentTypeName(int webPageItemId)
    {
        return DataClassInfoProvider.GetClassName(contentItemInfoProvider
            .Get(webPageItemInfoProvider
                .Get(webPageItemId).WebPageItemContentItemID).ContentItemContentTypeID);
    }
}