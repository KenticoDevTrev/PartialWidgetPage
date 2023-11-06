using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage;

[HtmlTargetElement("ajaxwidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PartialWidgetPageAjaxTagHelper : TagHelper
{
    private readonly IContentLanguageRetriever mMContentLanguageRetriever;
    private readonly IWebPageUrlRetriever mMPageUrlRetriever;

    public PartialWidgetPageAjaxTagHelper(IWebPageUrlRetriever pageUrlRetriever,
        IContentLanguageRetriever contentLanguageRetriever)
    {
        mMPageUrlRetriever = pageUrlRetriever;
        mMContentLanguageRetriever = contentLanguageRetriever;
    }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    /// <summary>
    ///     If provided, this will be the relative URL the ajax call will make.  If not provided, will get the Relative Url
    ///     from the Page or DocumentID
    /// </summary>
    public string RelativeUrl { get; set; }

    /// <summary>
    ///     The Document ID you wish to get the Relative url from.
    /// </summary>
    public int WebPageId { get; set; }

    protected string AjaxUrl { get; set; }
    private bool Render { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (WebPageId > 0)
        {
            var language = await mMContentLanguageRetriever.GetDefaultContentLanguageOrThrow();
            var webPageUrl = await mMPageUrlRetriever.Retrieve(WebPageId, language.ContentLanguageName);

            if (webPageUrl != null)
                AjaxUrl = webPageUrl.RelativePath;
        }

        if (!string.IsNullOrWhiteSpace(RelativeUrl))
            AjaxUrl = RelativeUrl;

        if (!string.IsNullOrWhiteSpace(AjaxUrl))
        {
            Render = true;
            // Append Special Url parameter
            AjaxUrl += $"{(AjaxUrl.IndexOf('?') == -1 ? '?' : '&')}{PARTIAL_WIDGET_AJAX_ID}=true";
        }
        else
        {
            Render = false;
        }

        output.TagName = null;
        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(content.GetContent() + GetAjaxHtml());
    }

    private string GetAjaxHtml()
    {
        if (Render)
        {
            var url = AjaxUrl;

            // resolve Virtual Urls
            if (ViewContext.HttpContext.Request.PathBase.HasValue)
                url = url.Replace("~", ViewContext.HttpContext.Request.PathBase.Value);
            else
                url = url.Replace("~", "");

            var uniqueId = Guid.NewGuid().ToString().Replace("-", "");
            var html = $"<div id=\"Partial-{uniqueId}\"></div>" +
                       "<script type=\"text/javascript\">" +
                       $"(function() {{ var PartialContainer_{uniqueId} = document.getElementById('Partial-{uniqueId}'); " +
                       "var RequestObj = (XMLHttpRequest) ? new XMLHttpRequest() : new ActiveXObject('Microsoft.XMLHTTP');" +
                       $"RequestObj.open('GET', '{url}', true);" +
                       "RequestObj.send();" +
                       "RequestObj.onreadystatechange = function() {" +
                       "  if(RequestObj.readyState == 4) {" +
                       $"     PartialContainer_{uniqueId}.innerHTML = (RequestObj.status == 200) ? RequestObj.responseText : '<!-- Error retrieving page content at {url} -->';" +
                       "  }" +
                       "};})();" +
                       "</script>";

            return html;
        }

        return "<!-- Could not determine Ajax Url, please provide valid Page or Document ID -->";
    }
}