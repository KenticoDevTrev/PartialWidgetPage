using CMS.Base.Internal;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Kentico.Web.Mvc;
using PartialWidgetPage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

public static class PartialWidgetPageAjaxExtensions
{
    public static string LayoutIfNotAjax(this HtmlHelper helper, string Layout)
    {
        IPartialWidgetPageHelper Helper = DependencyResolver.Current.GetService<IPartialWidgetPageHelper>();
        if (HttpContext.Current.Request.QueryString != null && HttpContext.Current.Request.QueryString.AllKeys.Contains(Helper.GetPartialUrlParameter()))
        {
            return null;
        }
        return Layout;
    }

    public static string GetPartialUrlParameter(this HtmlHelper helper)
    {
        IPartialWidgetPageHelper Helper = DependencyResolver.Current.GetService<IPartialWidgetPageHelper>();
        return Helper.GetPartialUrlParameter();
    }

    public static HtmlString PartialWidgetPageAjax(this HtmlHelper helper, string RelativeUrl)
    {
        string Url = RelativeUrl;
        return PartialWidgetPageAjax(Url);
    }

    public static HtmlString PartialWidgetPageAjax(this HtmlHelper helper, TreeNode Page)
    {
        string Url = DocumentURLProvider.GetUrl(Page);
        return PartialWidgetPageAjax(Url);
    }

    public static HtmlString PartialWidgetPageAjax(this HtmlHelper helper, int DocumentID)
    {
        IPageRetriever pageRetriever = DependencyResolver.Current.GetService<IPageRetriever>();
        string Url = string.Empty;
        var Page = pageRetriever.RetrieveMultiple(query =>
                query.WhereEquals(nameof(TreeNode.DocumentID), DocumentID)
                .Published(false)
                , cache =>
                cache.Key($"PartialWidgetPageAjaxTagHelperGetDocument|{DocumentID}")
                .Dependencies((pages, deps) => deps.Pages(pages))
                ).FirstOrDefault();
        if (Page != null)
        {
            Url = DocumentURLProvider.GetUrl(Page);
        }
        return PartialWidgetPageAjax(Url);
    }

    private static HtmlString PartialWidgetPageAjax(string Url)
    {

        if (!string.IsNullOrWhiteSpace(Url))
        {
            IPartialWidgetPageHelper Helper = DependencyResolver.Current.GetService<IPartialWidgetPageHelper>();
            Url += $"{(Url.IndexOf('?') == -1 ? '?' : '&')}{Helper.GetPartialUrlParameter()}=true";

            // resolve Virtual Urls
            if (Url.StartsWith("~/"))
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.Request.ApplicationPath.Trim('/')))
                {
                    Url = Url.Replace("~/", $"{HttpContext.Current.Request.ApplicationPath}/");
                }
                else
                {
                    Url = Url.Replace("~/", "/");
                }
            }

            string UniqueID = Guid.NewGuid().ToString().Replace("-", "");
            string Html = $"<div id=\"Partial-{UniqueID}\"></div>" +
            $"<script type=\"text/javascript\">" +
            $"(function() {{ var PartialContainer_{UniqueID} = document.getElementById('Partial-{UniqueID}'); " +
            $"var RequestObj = (XMLHttpRequest) ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');" +
            $"RequestObj.open('GET', '{Url}', true);" +
            $"RequestObj.send();" +
            $"RequestObj.onreadystatechange = function() {{" +
            $"  if(RequestObj.readyState == 4) {{" +
            $"     PartialContainer_{UniqueID}.innerHTML = (RequestObj.status == 200) ? RequestObj.responseText : '<!-- Error retrieving page content at {Url} -->';" +
            $"  }}" +
            $"}};}})();" +
            $"</script>";

            return new HtmlString(Html);
        }
        else
        {
            return new HtmlString("<!-- Could not determine Ajax Url, please provide valid Page or Document ID -->");
        }



    }
}
