using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Internal;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    private readonly IPartialWidgetRenderingRetriever mPartialWidgetRenderingRetriever;
    private readonly IPreferredLanguageRetriever mPreferredLanguageRetriever;
    private readonly IWebsiteChannelContext mWebsiteChannelContext;
    private readonly IWebPageUrlRetriever mUrlRetriever;

    private readonly IInfoProvider<WebPageItemInfo> mWebPageInfoProvider;

    public PartialWidgetPageWidgetViewComponent(
        IPartialWidgetRenderingRetriever partialWidgetRenderingRetriever,
        IInfoProvider<WebPageItemInfo> webPageInfoProvider,
        IWebPageUrlRetriever urlRetriever,
        IPreferredLanguageRetriever preferredLanguageRetriever, 
        IWebsiteChannelContext websiteChannelContext)
    {
        mPartialWidgetRenderingRetriever = partialWidgetRenderingRetriever;
        mWebPageInfoProvider = webPageInfoProvider;
        mUrlRetriever = urlRetriever;
        mPreferredLanguageRetriever = preferredLanguageRetriever;
        mWebsiteChannelContext = websiteChannelContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        ComponentViewModel<PartialWidgetPageWidgetModel> widgetProperties)
    {
        if (widgetProperties == null)
            throw new ArgumentNullException(nameof(widgetProperties));

        var properties = widgetProperties.Properties;

        //Set language resolved as preferred language
        var model = new PartialWidgetPageViewComponentModel()
        {
            Language = mPreferredLanguageRetriever.Get()
        };

        //if language is selected from properties override the preferred language
        if (!properties.UsePreferredLanguage && properties.Language.Any())
            model.Language = properties.Language.First().ObjectCodeName;

        if (!string.IsNullOrWhiteSpace(properties.Identifier))
            model.Identifier = properties.Identifier;
        
        if (Enum.TryParse<PartialWidgetPageWidgetRenderMode>(properties.RenderMode, out var result))
        {
            var page = await GetPage(properties, ViewContext.HttpContext.RequestAborted);

            model.RenderMode = result;

            switch (result)
            {
                case PartialWidgetPageWidgetRenderMode.Ajax:
                    if (!string.IsNullOrWhiteSpace(properties.CustomUrl))
                    {
                        model.AjaxUrl = properties.CustomUrl;
                    }
                    else if (page is not null)
                    {
                        var url = await mUrlRetriever.Retrieve(page.WebPageItemGUID, model.Language,
                            mWebsiteChannelContext.IsPreview, ViewContext.HttpContext.RequestAborted);

                        model.AjaxUrl = url.RelativePath;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not locate Page, please check configuration");
                    }

                    break;
                case PartialWidgetPageWidgetRenderMode.ServerSidePageBuilderLogic:

                    if (page is not null)
                    {
                        model.WebPageId = page.WebPageItemID;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not locate Page, please check configuration");
                    }

                    break;
                case PartialWidgetPageWidgetRenderMode.ServerSide:

                    if (page is not null)
                    {
                        var info = await mWebPageInfoProvider.GetAsync(page.WebPageItemID,
                            ViewContext.HttpContext.RequestAborted);
                        var className = info.ClassName;
                        model.Renderer =
                            mPartialWidgetRenderingRetriever.GetRenderingViewComponent(className,
                                page.WebPageItemID);
                        model.WebPageId = page.WebPageItemID;

                        if (model.Renderer is null)
                        {
                            ModelState.AddModelError("",
                                "There was no Renderer defined in the IPartialWidgetRenderingRetreiver.GetRenderingViewComponent for this class. Please have a developer return a ParitalWidgetRendering for this class prior to usage or set the mode to ServerSide.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not locate Page, please check configuration");
                    }

                    break;
                default:
                {
                    ModelState.AddModelError("", "Unknown Render Mode");
                    break;
                }
            }
        }

        return View(!ModelState.IsValid ? 
            "~/Components/Widgets/PartialWidgetPage/ComponentError.cshtml" : 
            "~/Components/Widgets/PartialWidgetPage/Default.cshtml", model);
    }

    private async Task<WebPageItemInfo?> GetPage(PartialWidgetPageWidgetModel widgetModel, CancellationToken token = default)
    {
        if (widgetModel.Page.Any())
        {
            var webPageGuid = widgetModel.Page.First().WebPageGuid;

            return await CacheHelper.CacheAsync(
                async cs => await mWebPageInfoProvider.GetAsync(webPageGuid, token),
                new CacheSettings(CacheHelper.CacheMinutes(), $"PartialWidgetPageWidget_GetPage|{webPageGuid}")
                {
                    CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|byguid|{webPageGuid}")
                });
        }


        return null;
    }
}