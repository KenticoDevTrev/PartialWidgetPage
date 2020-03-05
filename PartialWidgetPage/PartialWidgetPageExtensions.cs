using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using PartialWidgetPage;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// Helpers to allow the rendering of partial views of widget pages.
/// </summary>
public static class PartialWidgetPageExtensions
{
    /// <summary>
    /// Pulls in the given path's content, rendering the widget content as well. Target's view must have a Layout = Html.LayoutIfEditMode() or Layout = Html.LayoutIfEditMode("RenderAsPartialUrlParameter").
    /// Access to ASP.NET session state is exclusive per session, which means that if two different users make concurrent requests, access to each separate session is granted concurrently. 
    /// However, if two concurrent requests are made for the same session (by using the same SessionID value), the first request gets exclusive access to the session information. 
    /// The second request executes only after the first request is finished. (The second session can also get access if the exclusive lock on the information is freed because the first request exceeds the lock time-out.) 
    /// If the EnableSessionState value in the @ page directive is set to ReadOnly, a request for the read-only session information does not result in an exclusive lock on the session data. 
    /// However, read-only requests for session data might still have to wait for a lock set by a read-write request for session data to clear.  
    /// Here is the MVC readonly attribute. [SessionState(SessionStateBehavior.ReadOnly)]
    /// See https://docs.microsoft.com/en-us/previous-versions/ms178581(v=vs.140)?redirectedfrom=MSDN#concurrent-requests-and-session-state for more info.
    /// </summary>
    /// <param name="helper">The HTML helper.</param>
    /// <param name="path">The path to render (relative).</param>
    /// <param name="renderAsPartialUrlParameter">If needed, the URL parameter that indicates the view should be rendered as a partial see (Html.LayoutIfEditMode(string SharedLayoutPath, string RenderAsPartialUrlParameter))</param>
    /// <param name="pathIsNodeAliasPath">If true, then the relative URL will be derived from the NodeAliasPath give.</param>
    /// <param name="stripSession">If false the session will not be stripped from the request allowing you override the session locking bypass.</param>
    /// <returns>The rendered content.</returns>
    public static HtmlString PartialWidgetPage(this HtmlHelper helper, string path, string renderAsPartialUrlParameter = null, bool pathIsNodeAliasPath = false, bool stripSession = true)
    {
        using (var client = new CookieAwareWebClient(HttpContext.Current.Request))
        {
            var url = GetRequestUrl(path, renderAsPartialUrlParameter, pathIsNodeAliasPath);
            try
            {
                var content = client.DownloadString(url);
                return new HtmlString(content);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("PartialWidgetPage", "RenderFailure", ex, additionalMessage: "Error occurred while trying to render content at " + url);
                return new HtmlString("<!-- Error retrieving page content at " + url + " -->");
            }
        }
    }

    /// <summary>
    /// Renders out a DIV and AJAX call script to load in the content client side. Target's view must have a Layout = Html.LayoutIfEditMode() or Layout = Html.LayoutIfEditMode("RenderAsPartialUrlParameter").
    /// </summary>
    /// <param name="helper">The HTML helper.</param>
    /// <param name="path">The path to render (relative).</param>
    /// <param name="renderAsPartialUrlParameter">If needed, the URL parameter that indicates the view should be rendered as a partial see (Html.LayoutIfEditMode(string SharedLayoutPath, string RenderAsPartialUrlParameter))</param>
    /// <param name="pathIsNodeAliasPath">If true, then the relative URL will be derived from the NodeAliasPath give.</param>
    /// <returns>The DIV and AJAX logic to render.</returns>
    public static HtmlString PartialWidgetPageAjax(this HtmlHelper helper, string path, string renderAsPartialUrlParameter = null, bool pathIsNodeAliasPath = false)
    {
        var url = GetRequestUrl(path, renderAsPartialUrlParameter, pathIsNodeAliasPath);
        var divId = Guid.NewGuid().ToString().Replace("-", "");
        var content =
          $"<div id=\"Partial-{divId}\"></div>" +
          $"<script type=\"text/javascript\">" +
          $"(function() {{ var PartialContainer_{divId} = document.getElementById('Partial-{divId}'); " +
          $"var RequestObj = (XMLHttpRequest) ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');" +
          $"RequestObj.open('GET', '{url}', true);" +
          $"RequestObj.send();" +
          $"RequestObj.onreadystatechange = function() {{" +
          $"  if(RequestObj.readyState == 4) {{" +
          $"     PartialContainer_{divId}.innerHTML = (RequestObj.status == 200) ? RequestObj.responseText : '<!-- Error retrieving page content at {url} -->';" +
          $"  }}" +
          $"}};}})();" +
          $"</script>";
        return new HtmlString(content);
    }

    /// <summary>
    /// Gets the correct path, handling the RenderAsPartialUrlParameter if provided
    /// </summary>
    /// <param name="path">The relative path</param>
    /// <param name="renderAsPartialUrlParameter">The URL parameter that must be true in order to render as a partial method.</param>
    /// <param name="pathIsNodeAliasPath">If true, then the relative URL will be derived from the NodeAliasPath give.</param>
    /// <returns>The proper full URL.</returns>
    private static string GetRequestUrl(string path, string renderAsPartialUrlParameter = null, bool pathIsNodeAliasPath = false)
    {
        if (pathIsNodeAliasPath)
            path = NodeAliasPathToUrl(path);
        var url = new Uri(HttpContext.Current.Request.Url, path.Trim('~')).AbsoluteUri;
        if (!string.IsNullOrWhiteSpace(renderAsPartialUrlParameter))
        {
            var urlSeparator = url.Contains("?") ? "&" : "?";
            url = $"{url}{urlSeparator}{renderAsPartialUrlParameter}=true";
        }
        return url;
    }

    /// <summary>
    /// Will return the shared layout path if it's edit mode (so widgets can be edited), otherwise null if it's being pulled in from a partial view.
    /// </summary>
    /// <param name="sharedLayoutPath">The shared layout, must NOT contain a Html.PartialWidgetPage of the page that will be rendered or infinite loop will occur.</param>
    /// <returns>The proper layout value.</returns>
    public static string LayoutIfEditMode(this HtmlHelper helper, string sharedLayoutPath)
    {
        return HttpContext.Current.Kentico().PageBuilder().EditMode ? sharedLayoutPath : null;
    }

    /// <summary>
    /// Will return the shared layout path if the context is EditMode, or the given RenderAsPartialUrlParameter is true ('true' or 1 or empty but present). Use this if the rendering of the page outside of EditMode also needs to be a full view.
    /// </summary>
    /// <param name="sharedLayoutPath">The shared layout, must NOT contain a Html.PartialWidgetPage of the page that will be rendered or infinite loop will occur.</param>
    /// <param name="renderAsPartialUrlParameter">The URL parameter that should be present to render as a partial</param>
    /// <returns>The proper layout value.</returns>
    public static string LayoutIfEditMode(this HtmlHelper helper, string sharedLayoutPath, string renderAsPartialUrlParameter)
    {
        var url = HttpContext.Current.Request.RawUrl;
        var renderAsPartial = !HttpContext.Current.Kentico().PageBuilder().EditMode;
        if (url.ToLower().Contains(renderAsPartialUrlParameter.ToLower()))
        {
            var paramVal = URLHelper.GetUrlParameter(url, renderAsPartialUrlParameter);
            renderAsPartial = string.IsNullOrWhiteSpace(paramVal) || ValidationHelper.GetBoolean(paramVal, false);
        }
        return renderAsPartial ? null : sharedLayoutPath;
    }

    /// <summary>
    /// Gets the relative URL from the node alias path.
    /// </summary>
    /// <param name="nodeAliasPath">The node alias path.</param>
    /// <returns></returns>
    public static string NodeAliasPathToUrl(string nodeAliasPath)
    {
        return CacheHelper.Cache
        (
            cs =>
            {
                // get proper URL path for the given document
                var docQuery = DocumentHelper
                    .GetDocuments()
                    .Path(nodeAliasPath)
                    .CombineWithDefaultCulture()
                    .OnCurrentSite()
                    .TopN(1);
                if (LocalizationContext.CurrentCulture != null)
                    docQuery.Culture(LocalizationContext.CurrentCulture.CultureCode);

                var treeNode = docQuery.FirstOrDefault();
                if (treeNode != null)
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency
                        (
                            new[]
                            {
                                "cms.document|byid|"+treeNode.DocumentID,
                                "cms.class|byname|"+treeNode.ClassName
                            }
                        );
                    }
                    return treeNode.RelativeURL;
                }
                return null;
            },
            new CacheSettings
            (
                CacheHelper.CacheMinutes(SiteContext.CurrentSiteName),
                "PartialWidgetGetUrlFromPath",
                nodeAliasPath,
                SiteContext.CurrentSiteName,
                LocalizationContext.CurrentCulture?.CultureCode
            )
        );
    }
}
