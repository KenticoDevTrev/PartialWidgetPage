using CMS.ContentEngine;
using CMS.Helpers;
using System.Data.Common;
using System.Data;

namespace PartialWidgetPage;

public class PartialWidgetPageHelper(IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
    IWebPageDataContextRetriever webPageDataContextRetriever,
    IHttpContextRetriever httpContextRetriever,
    IInfoProvider<ContentLanguageInfo> languageInfoProvider,
    IHttpContextAccessor httpContextAccessor,
    IWebPageDataContextInitializer dataContextInitializer,
    IContentQueryExecutor executor,
    IPreferredLanguageRetriever preferredLanguageRetriever,
    IProgressiveCache progressiveCache)
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
        var contentName = await GetContentTypeName(identifier, channel);
        var language = !string.IsNullOrWhiteSpace(languageName) ? languageName : preferredLanguageRetriever.Get();
        var preview = Kentico.Preview().Enabled;
        
        var webPage = await progressiveCache.LoadAsync(async (cs, cancellationToken) => {

            if(cs.Cached) {
                cs.CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|byid|{identifier}");
            }
            var builder = new ContentItemQueryBuilder().ForContentType(contentName, q => {
                q
                    .Where(w => w.Where(where =>
                        where.WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemID), identifier)))
                    .ForWebsite(channel)
                    .TopN(1);
            })
            .InLanguage(language);

            var options = new ContentQueryExecutionOptions() {
                ForPreview = preview,
                IncludeSecuredItems = true
            };

            return (await executor.GetWebPageResult(builder, async (container) => {
                var languageInfo = await languageInfoProvider.GetAsync(container.ContentItemCommonDataContentLanguageID, cancellationToken);

                return new RoutedWebPage() {
                    WebPageItemID = container.WebPageItemID,
                    ContentTypeName = contentName,
                    LanguageName = languageInfo.ContentLanguageName
                };
            }, options, cancellationToken))
            .FirstOrDefault();
        }, new CacheSettings(1440, "PartialWidgetPages_GetWebPageForContextSwitch", identifier, languageName, channel, preview), token);
        
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
    
    private async Task<string> GetContentTypeName(int webPageItemId, string channel)
    {
        var dictionaryLookup = await progressiveCache.LoadAsync(async cs => {
            if (cs.Cached) {
                cs.CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|bychannel|{channel}|all");
            }

            var query = @"select WebPageItemID, ContentItemTypeID from CMS_WebPageItem
inner join CMS_ContentItem on ContentItemID = WebPageItemContentItemID
inner join CMS_Channel on ChannelID = ContentItemChannelID
where ChannelName = @ChannelName and ContentItemTypeID is not null";
            return DbDataReaderToDataSet(await ConnectionHelper.ExecuteReaderAsync(query, new QueryDataParameters() { { "@ChannelName", channel } }, QueryTypeEnum.SQLQuery, System.Data.CommandBehavior.Default, CancellationToken.None))
            .Tables[0].Rows.Cast<DataRow>().ToDictionary(key => (int)key["WebPageItemID"], value => (int)value["ContentItemTypeID"]);

        }, new CacheSettings(360, "PartialWidgetPage_GetWebPageItemIDToContentTypeID", channel));

        var classDictionary = await progressiveCache.LoadAsync(async cs => {
            if (cs.Cached) {
                cs.CacheDependency = CacheHelper.GetCacheDependency($"{DataClassInfo.OBJECT_TYPE}|all");
            }
            return (await DataClassInfoProvider.GetClasses()
            .Columns(nameof(DataClassInfo.ClassID), nameof(DataClassInfo.ClassName))
            .GetEnumerableTypedResultAsync())
            .ToDictionary(key => key.ClassID, value => value.ClassName);
        }, new CacheSettings(1440, "PartialWidgetPage_ClassIDToClassName"));
        return dictionaryLookup.TryGetValue(webPageItemId, out var contentItemID)
            && classDictionary.TryGetValue(contentItemID, out var className) ? className : "unknown";
    }

    private static DataSet DbDataReaderToDataSet(DbDataReader reader)
    {
        if (reader is null) {
            var emptyDs = new DataSet();
            emptyDs.Tables.Add(new DataTable());
            return emptyDs;
        }

        var ds = new DataSet();
        // read each data result into a datatable
        do {
            var table = new DataTable();
            table.Load(reader);
            ds.Tables.Add(table);
        } while (!reader.IsClosed);

        return ds;
    }
}