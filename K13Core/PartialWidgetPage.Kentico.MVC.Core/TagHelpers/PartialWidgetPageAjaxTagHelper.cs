using CMS.DocumentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PartialWidgetPage.TagHelpers
{
    /// <summary>
    /// Tag Helper to render the AJAX Script and div to pull a partial html chunk client side.
    /// </summary>
    [HtmlTargetElement("ajaxwidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PartialWidgetPageAjaxTagHelper : TagHelper
    {
        /// <summary>
        /// If provided, this will be the relative URL the ajax call will make.  If not provided, will get the Relative Url from the Page or DocumentID
        /// </summary>
        public string RelativeUrl { get; set; }
        
        /// <summary>
        /// The Tree Node you wish to get the Relative Url from.
        /// </summary>
        public TreeNode Page { get; set; }

        /// <summary>
        /// The Document ID you wish to get the Relative url from.
        /// </summary>
        public int Documentid { get; set; }

        private string AjaxUrl { get; set; }
        private bool Render { get; set; }
        
        public IPageRetriever PageRetriever { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public IPartialWidgetPageHelper PartialWidgetPageHelper { get; }

        public PartialWidgetPageAjaxTagHelper(IPageRetriever pageRetriever, 
            IHttpContextAccessor httpContextAccessor,
            IPartialWidgetPageHelper partialWidgetPageHelper)
        {
            PageRetriever = pageRetriever;
            HttpContextAccessor = httpContextAccessor;
            PartialWidgetPageHelper = partialWidgetPageHelper;
        }

        public override void Init(TagHelperContext context)
        {
            if (!string.IsNullOrWhiteSpace(RelativeUrl))
            {
                AjaxUrl = RelativeUrl;
            }
            else if (Page != null)
            {
                AjaxUrl = DocumentURLProvider.GetUrl(Page);
            }
            else if (Documentid > 0)
            {
                // Get Document
                Page = PageRetriever.RetrieveMultiple(query =>
                    query.WhereEquals(nameof(TreeNode.DocumentID), Documentid)
                    .Published(false)
                    , cache =>
                    cache.Key($"PartialWidgetPageAjaxTagHelperGetDocument|{Documentid}")
                    .Dependencies((pages, deps) => deps.Pages(pages))
                    ).FirstOrDefault();
                if (Page != null)
                {
                    AjaxUrl = DocumentURLProvider.GetUrl(Page);
                }
            }

            if (!string.IsNullOrWhiteSpace(AjaxUrl))
            {
                Render = true;
                // Append Special Url parameter
                AjaxUrl += $"{(AjaxUrl.IndexOf('?') == -1 ? '?' : '&')}{PartialWidgetPageHelper.GetPartialUrlParameter()}=true";
            }
            else
            {
                Render = false;
            }
            base.Init(context);
        }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            var content = output.Content.GetContent();
            output.Content.SetHtmlContent(content + GetAjaxHtml());
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            var content = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(content.GetContent() + GetAjaxHtml());
        }

        private string GetAjaxHtml()
        {
            if (Render)
            {
                string Url = AjaxUrl;

                // resolve Virtual Urls
                if(HttpContextAccessor.HttpContext.Request.PathBase.HasValue)
                {
                    Url = Url.Replace("~", HttpContextAccessor.HttpContext.Request.PathBase.Value);
                } else
                {
                    Url = Url.Replace("~", "");
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

                return Html;
            }
            else
            {
                return "<!-- Could not determine Ajax Url, please provide valid Page or Document ID -->";
            }
        }
    }
}

