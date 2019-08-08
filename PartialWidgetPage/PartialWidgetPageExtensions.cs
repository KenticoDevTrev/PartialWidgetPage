using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


/// <summary>
/// Helpers to allow the rendering of Partial Views of Widget Pages
/// </summary>
public static class PartialWidgetPageExtensions
{

    /// <summary>
    /// Pulls in the given path's content, rendering the widget content as well.  Target's View must have a Layout = Html.LayoutIfEditMode() or Layout = Html.LayoutIfEditMode("RenderAsPartialUrlParameter")
    /// </summary>
    /// <param name="helper">The HTML Helper</param>
    /// <param name="Path">The Path to render (relative)</param>
    /// <param name="RenderAsPartialUrlParameter">If needed, the Url Parameter that indicates the view should be rendered as a partial see (Html.LayoutIfEditMode(string SharedLayoutPath, string RenderAsPartialUrlParameter))</param>
    /// <param name="PathIsNodeAliasPath">If true, then the Relative Url will be derived from the NodeAliasPath Give.</param>
    /// <returns>The rendered content</returns>
    public static HtmlString PartialWidgetPage(this HtmlHelper helper, string Path, string RenderAsPartialUrlParameter = null, bool PathIsNodeAliasPath = false)
    {
        using (WebClient client = new WebClient())
        {
            string url = GetRequestUrl(Path, RenderAsPartialUrlParameter, PathIsNodeAliasPath);
            try
            {
                string Content = client.DownloadString(url);
                return new HtmlString(Content);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("PartialWidgetPage", "RenderFailure", ex, additionalMessage: "Error occurred while trying to render content at " + url);
                return new HtmlString("<!-- Error retrieving page content at " + url + " -->");
            }
        }
    }

    /// <summary>
    /// Renders out a Div and AJAX Call script to load in the content client side.  Target's View must have a Layout = Html.LayoutIfEditMode() or Layout = Html.LayoutIfEditMode("RenderAsPartialUrlParameter")
    /// </summary>
    /// <param name="helper">The HTML Helper</param>
    /// <param name="Path">The Path to render (relative)</param>
    /// <param name="RenderAsPartialUrlParameter">If needed, the Url Parameter that indicates the view should be rendered as a partial see (Html.LayoutIfEditMode(string SharedLayoutPath, string RenderAsPartialUrlParameter))</param>
    /// <param name="PathIsNodeAliasPath">If true, then the Relative Url will be derived from the NodeAliasPath Give.</param>
    /// <returns>The Div and Ajax logic to render</returns>
    public static HtmlString PartialWidgetPageAjax(this HtmlHelper helper, string Path, string RenderAsPartialUrlParameter = null, bool PathIsNodeAliasPath = false)
    {
        string url = GetRequestUrl(Path, RenderAsPartialUrlParameter, PathIsNodeAliasPath);
        string DivID = Guid.NewGuid().ToString().Replace("-", "");
        string Content = $"<div id=\"Partial-{DivID}\"></div>" +
            $"<script type=\"text/javascript\">" +
            $"(function() { var PartialContainer_{DivID} = document.getElementById('Partial-{DivID}'); " +
            $"var RequestObj = (XMLHttpRequest) ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');" +
            $"RequestObj.open('GET', '{url}', true);" +
            $"RequestObj.send();" +
            $"RequestObj.onreadystatechange = function() {{" +
            $"  if(RequestObj.readyState == 4) {{" +
            $"     PartialContainer_{DivID}.innerHTML = (RequestObj.status == 200) ? RequestObj.responseText : '<!-- Error retrieving page content at {url} -->';" +
            $"  }}" +
            $"}};})();" +
            $"</script>";
        return new HtmlString(Content);
    }

    /// <summary>
    /// Gets the Correct path, handling the RenderAsPartialUrlParameter if provided
    /// </summary>
    /// <param name="Path">The Relative path</param>
    /// <param name="RenderAsPartialUrlParameter">The URL Parameter that must be true in order to render as a partial method</param>
    /// <param name="PathIsNodeAliasPath">If true, then the Relative Url will be derived from the NodeAliasPath Give.</param>
    /// <returns>The proper full Url</returns>
    private static string GetRequestUrl(string Path, string RenderAsPartialUrlParameter = null, bool PathIsNodeAliasPath = false)
    {
        if (PathIsNodeAliasPath)
        {
            Path = NodeAliasPathToUrl(Path);
        }
        string url = new Uri(HttpContext.Current.Request.Url, Path.Trim('~')).AbsoluteUri;
        if (!string.IsNullOrWhiteSpace(RenderAsPartialUrlParameter))
        {
            string UrlSeparator = url.Contains("?") ? "&" : "?";
            url = $"{url}{UrlSeparator}{RenderAsPartialUrlParameter}=true";
        }
        return url;
    }

    /// <summary>
    /// Will return the Shared Layout Path if it's edit mode (so widgets can be edited), otherwise null if it's being pulled in from a partial view.
    /// </summary>
    /// <param name="SharedLayoutPath">The Shared Layout, must NOT contain a Html.PartialWidgetPage of the page that will be rendered or infinite loop will occur.</param>
    /// <returns>The proper Layout value.</returns>
    public static string LayoutIfEditMode(this HtmlHelper helper, string SharedLayoutPath)
    {
        return (HttpContext.Current.Kentico().PageBuilder().EditMode ? SharedLayoutPath : null);
    }

    /// <summary>
    /// Will return the Shared Layout Path if the context is EditMode, or the given RenderAsPartialUrlParameter is true ('true' or 1 or empty but present).  Use this if the rendering of the page outside of EditMode also needs to be a full view.
    /// </summary>
    /// <param name="SharedLayoutPath">The Shared Layout, must NOT contain a Html.PartialWidgetPage of the page that will be rendered or infinite loop will occur.</param>
    /// <param name="RenderAsPartialUrlParameter">The URL Parameter that should be present to render as a Partial</param>
    /// <returns>The proper Layout value.</returns>
    public static string LayoutIfEditMode(this HtmlHelper helper, string SharedLayoutPath, string RenderAsPartialUrlParameter)
    {
        string Url = HttpContext.Current.Request.RawUrl;
        bool RenderPartial = !HttpContext.Current.Kentico().PageBuilder().EditMode;
        if (Url.ToLower().Contains(RenderAsPartialUrlParameter.ToLower()))
        {
            string ParamVal = URLHelper.GetUrlParameter(Url, RenderAsPartialUrlParameter);
            RenderPartial = string.IsNullOrWhiteSpace(ParamVal) || (ValidationHelper.GetBoolean(ParamVal, false) == true);
        }
        return (RenderPartial ? null : SharedLayoutPath);
    }

    /// <summary>
    /// Gets the RelativeURL from the Node Alias Path
    /// </summary>
    /// <param name="NodeAliasPath">The Node Alias Path</param>
    /// <returns></returns>
    public static string NodeAliasPathToUrl(string NodeAliasPath)
    {
        return CacheHelper.Cache<string>(cs =>
        {
            // get proper Url path for the given document
            var DocQuery = DocumentHelper.GetDocuments()
                .Path(NodeAliasPath)
                .CombineWithDefaultCulture(true)
                .OnCurrentSite()
                .TopN(1);
            if (LocalizationContext.CurrentCulture != null)
            {
                DocQuery.Culture(LocalizationContext.CurrentCulture.CultureCode);
            }

            var TreeNode = DocQuery.FirstOrDefault();
            if (TreeNode != null)
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(new string[]
                    {
                        "cms.document|byid|"+TreeNode.DocumentID,
                        "cms.class|byname|"+TreeNode.ClassName
                    });
                }
                return TreeNode.RelativeURL;
            }
            else
            {
                return null;
            }
        }, new CacheSettings(CacheHelper.CacheMinutes(SiteContext.CurrentSiteName), "PartialWidgetGetUrlFromPath", NodeAliasPath, SiteContext.CurrentSiteName, LocalizationContext.CurrentCulture?.CultureCode));
    }

}
