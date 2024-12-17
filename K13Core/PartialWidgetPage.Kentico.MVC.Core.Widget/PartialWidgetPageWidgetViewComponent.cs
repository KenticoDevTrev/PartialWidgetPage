using CMS.Core;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using PartialWidgetPage;
using System;
using System.Linq;
using System.Threading.Tasks;

[assembly: RegisterWidget(PartialWidgetPageWidgetModel.IDENTITY, typeof(PartialWidgetPageWidgetViewComponent), "Partial Widget Page", typeof(PartialWidgetPageWidgetModel), Description ="Displays the given Page/Url with Widget Context", IconClass = "icon-doc-torn")]

namespace PartialWidgetPage
{
    public class PartialWidgetPageWidgetViewComponent : ViewComponent
    {

        public const string _VIEWPATH = "~/Components/PartialWidgetPageWidget/default.cshtml";

        public PartialWidgetPageWidgetViewComponent(IPageRetriever pageRetriever,
            IPartialWidgetPageHelper partialWidgetPageHelper,
            IPartialWidgetRenderingRetriever partialWidgetRenderingRetriever,
            IEventLogWriter eventLogWriter)
        {
            PageRetriever = pageRetriever;
            PartialWidgetPageHelper = partialWidgetPageHelper;
            PartialWidgetRenderingRetriever = partialWidgetRenderingRetriever;
            EventLogWriter = eventLogWriter;
        }

        public IPageRetriever PageRetriever { get; }
        public IPartialWidgetPageHelper PartialWidgetPageHelper { get; }
        public IPartialWidgetRenderingRetriever PartialWidgetRenderingRetriever { get; }
        public IEventLogWriter EventLogWriter { get; }
        public object RenderAsPartialUrlParameter { get; private set; }

        public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<PartialWidgetPageWidgetModel> widgetProperties)
        {
            PartialWidgetPageWidgetViewComponentModel model = new PartialWidgetPageWidgetViewComponentModel();
            if (widgetProperties == null || widgetProperties.Properties == null || (widgetProperties.Properties.Path == null && widgetProperties.Properties.Page == null && widgetProperties.Properties.CustomUrl == null))
            {
                model.Render = false;
            }
            else
            {
                model.Render = true;
                var Properties = widgetProperties.Properties;
                if (Properties.RenderMode.Equals(PartialWidgetPageWidgetModel._RenderMode_Ajax))
                {
                    model.RenderMode = PartialWidgetPageWidgetRenderMode.Ajax;
                    // Get path
                    if (!string.IsNullOrWhiteSpace(Properties.CustomUrl))
                    {
                        model.AjaxUrl = Properties.CustomUrl;
                    }
                    else
                    {
                        TreeNode Page = GetPage(Properties, false);
                        if (Page == null)
                        {
                            model.Render = false;
                            model.Error = "Could not locate Page, please check configuration";
                            EventLogWriter.WriteLog(new EventLogData(EventTypeEnum.Warning, "PartialWidgetPageWidget", "PAGENOTFOUND")
                            {
                                EventDescription = "Could not find Page from the configuration of the Partial Widget Page Widget, located on page: " + widgetProperties.Page.NodeAliasPath
                            });
                        }
                        else
                        {
                            // get Relative Url
                            model.AjaxUrl = DocumentURLProvider.GetUrl(Page);
                        }
                    }
                }
                else if (Properties.RenderMode.Equals(PartialWidgetPageWidgetModel._RenderMode_ServerPageBuilderLogic))
                {
                    model.RenderMode = PartialWidgetPageWidgetRenderMode.ServerSidePageBuilderLogic;
                    TreeNode Page = GetPage(Properties, false);
                    if (Page == null)
                    {
                        model.Render = false;
                        model.Error = "Could not locate Page, please check configuration";
                        EventLogWriter.WriteLog(new EventLogData(EventTypeEnum.Warning, "PartialWidgetPageWidget", "PAGENOTFOUND")
                        {
                            EventDescription = "Could not find Page from the configuration of the Partial Widget Page Widget, located on page: " + widgetProperties.Page.NodeAliasPath
                        });
                    }
                    else
                    {
                        // Only need DocumentID
                        model.DocumentID = Page.DocumentID;
                    }
                }
                else if (Properties.RenderMode.Equals(PartialWidgetPageWidgetModel._RenderMode_Server))
                {
                    model.RenderMode = PartialWidgetPageWidgetRenderMode.ServerSide;
                    TreeNode Page = GetPage(Properties, false);
                    if (Page == null)
                    {
                        model.Render = false;
                        model.Error = "Could not locate Page, please check configuration";
                        EventLogWriter.WriteLog(new EventLogData(EventTypeEnum.Warning, "PartialWidgetPageWidget", "PAGENOTFOUND")
                        {
                            EventDescription = "Could not find Page from the configuration of the Partial Widget Page Widget, located on page: " + widgetProperties.Page.NodeAliasPath
                        });
                    }
                    else
                    {
                        // get DocumentID and RenderClass
                        model.Renderer = PartialWidgetRenderingRetriever.GetRenderingViewComponent(Page.ClassName);
                        model.DocumentID = Page.DocumentID;
                        if(model.Renderer == null)
                        {
                            model.Render = false;
                        }
                    }
                } else
                {
                    model.Render = false;
                }
            }

            return View(_VIEWPATH, model);

        }

        private TreeNode GetPage(PartialWidgetPageWidgetModel Properties, bool DocumentIDAndClassOnly = false)
        {
            string Culture = !string.IsNullOrWhiteSpace(Properties.Culture) ? Properties.Culture : System.Globalization.CultureInfo.CurrentCulture.Name;
            if (Properties.PageSelectionMode.Equals(PartialWidgetPageWidgetModel._PageSelectionMode_Path) && !string.IsNullOrWhiteSpace(Properties.Path.FirstOrDefault()?.NodeAliasPath))
            {
                string Path = Properties.Path.FirstOrDefault().NodeAliasPath;
                string SiteName = !string.IsNullOrWhiteSpace(Properties.SiteName) ? Properties.SiteName : SiteContext.CurrentSiteName;

                // Convert path / page to url
                return PageRetriever.RetrieveMultiple(query =>
                {
                    query.Path(Properties.Path.FirstOrDefault().NodeAliasPath, PathTypeEnum.Single)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .OnSite(SiteName)
                    .Published(false);
                    if (DocumentIDAndClassOnly)
                    {
                        query.Columns(nameof(TreeNode.DocumentID), nameof(TreeNode.ClassName));
                    }
                }
                    , cache =>
                    cache.Key($"PartialWidgetPageWidget_GetPage|{Path}|{Culture}|{SiteName}|{DocumentIDAndClassOnly}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            }
            else if (Properties.PageSelectionMode.Equals(PartialWidgetPageWidgetModel._PageSelectionMode_ByNodeGuid) && Properties.Page.Count() > 0 && Properties.Page.First().NodeGuid != Guid.Empty)
            {
                // Convert path / page to url
                return PageRetriever.RetrieveMultiple(query =>
                {
                    query.WhereEquals(nameof(TreeNode.NodeGUID), Properties.Page.First().NodeGuid)
                    .Culture(Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .Published(false);
                    if (DocumentIDAndClassOnly)
                    {
                        query.Columns(nameof(TreeNode.DocumentID), nameof(TreeNode.ClassName));
                    }
                }
                    , cache =>
                    cache.Key($"PartialWidgetPageWidget_GetPage|{Properties.Page.First().NodeGuid}|{Culture}|{DocumentIDAndClassOnly}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

    }
}
