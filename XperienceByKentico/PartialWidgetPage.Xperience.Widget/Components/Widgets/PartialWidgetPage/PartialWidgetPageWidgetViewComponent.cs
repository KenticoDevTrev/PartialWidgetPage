using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Internal;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using PartialWidgetPage;

[assembly: RegisterWidget(PartialWidgetPageWidgetModel.IDENTITY,
    typeof(PartialWidgetPageWidgetViewComponent),
    "Partial Widget Page",
    typeof(PartialWidgetPageWidgetModel),
    Description = "Displays the given Page/Url with Widget Context",
    IconClass = "icon-doc-torn")]


namespace PartialWidgetPage;

public class PartialWidgetPageWidgetViewComponent : ViewComponent
{
    private readonly IPreferredLanguageRetriever mContentLanguageRetriever;
    private readonly IPartialWidgetRenderingRetriever mPartialWidgetRenderingRetriever;

    private readonly IWebPageUrlRetriever mUrlRetriever;

    private readonly IInfoProvider<WebPageItemInfo> mWebPageInfoProvider;

    public PartialWidgetPageWidgetViewComponent(
        IPreferredLanguageRetriever contentLanguageRetriever,
        IPartialWidgetRenderingRetriever partialWidgetRenderingRetriever,
        IInfoProvider<WebPageItemInfo> webPageInfoProvider,
        IWebPageUrlRetriever urlRetriever)
    {
        mContentLanguageRetriever = contentLanguageRetriever;
        mPartialWidgetRenderingRetriever = partialWidgetRenderingRetriever;
        mWebPageInfoProvider = webPageInfoProvider;
        mUrlRetriever = urlRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        ComponentViewModel<PartialWidgetPageWidgetModel> widgetProperties)
    {
        if (widgetProperties == null)
            throw new ArgumentNullException(nameof(widgetProperties));

        var properties = widgetProperties.Properties;

        var model = new PartialWidgetPageViewComponentModel();

        var page = await GetPage(properties);

        if (page == null)
        {
            model.Render = false;
            model.Error = "Could not locate Page, please check configuration";
        }
        else
        {
            if (Enum.TryParse<PartialWidgetPageWidgetRenderMode>(properties.RenderMode, out var result))
            {
                model.RenderMode = result;

                switch (result)
                {
                    case PartialWidgetPageWidgetRenderMode.Ajax:
                        if (!string.IsNullOrWhiteSpace(properties.CustomUrl))
                        {
                            model.AjaxUrl = properties.CustomUrl;
                        }
                        else
                        {
                            var language = mContentLanguageRetriever.Get();
                            var url = await mUrlRetriever.Retrieve(page.WebPageItemGUID, language);
                            model.AjaxUrl = url.RelativePath;
                        }

                        break;
                    case PartialWidgetPageWidgetRenderMode.ServerSidePageBuilderLogic:

                        model.WebPageId = page.WebPageItemID;

                        break;
                    case PartialWidgetPageWidgetRenderMode.ServerSide:

                        var className = await mWebPageInfoProvider.RetrieveClassName(page.WebPageItemID);
                        model.Renderer = mPartialWidgetRenderingRetriever.GetRenderingViewComponent(className, page.WebPageItemID);
                        model.WebPageId = page.WebPageItemID;

                        break;
                    default:
                    {
                        model.Render = false;
                        break;
                    }
                }
            }
            else
            {
                model.Render = false;
            }
        }

        return View("~/Components/Widgets/PartialWidgetPage/Default.cshtml", model);
    }

    private async Task<WebPageItemInfo> GetPage(PartialWidgetPageWidgetModel widgetModel)
    {
        if (widgetModel.Page.Any())
        {
            var webPageGuid = widgetModel.Page.First().WebPageGuid;

            return await CacheHelper.CacheAsync(
                async cs => await mWebPageInfoProvider.GetAsync(webPageGuid),
                new CacheSettings(CacheHelper.CacheMinutes(), $"PartialWidgetPageWidget_GetPage|{webPageGuid}")
                {
                    CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|byguid|{webPageGuid}")
                });
        }


        return null;
    }
}