using CMS.Helpers;

namespace PartialWidgetPage
{
    internal static class WebPageItemInfoProviderExtensions
    {
        public static async Task<string> RetrieveClassName(this IInfoProvider<WebPageItemInfo> provider, int pageId)
        {
            return await CacheHelper.CacheAsync(async cs =>
            {
                return await provider.Get().Source(source =>
                    {
                        source.LeftJoin<ContentItemInfo>(nameof(WebPageItemInfo.WebPageItemContentItemID),
                            nameof(ContentItemInfo.ContentItemID));
                        source.LeftJoin<DataClassInfo>(nameof(ContentItemInfo.ContentItemContentTypeID),
                            nameof(DataClassInfo.ClassID));
                    }).WhereEquals(nameof(WebPageItemInfo.WebPageItemID), pageId)
                    .Column(nameof(DataClassInfo.ClassName))
                    .GetScalarResultAsync("");
            }, new CacheSettings(CacheHelper.CacheMinutes(), $"PartialWidgetPageWidget_GetPageClassName|{pageId}")
            {
                CacheDependency = CacheHelper.GetCacheDependency(
                    $"{WebPageItemInfo.OBJECT_TYPE}|byid|{pageId}")
            });
        }
    }
}
