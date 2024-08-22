#nullable enable
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage;

[HtmlTargetElement("ajaxwidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PartialWidgetPageAjaxTagHelper(
    IWebPageUrlRetriever pageUrlRetriever,
    IPreferredLanguageRetriever contentLanguageRetriever,
    IWebsiteChannelContext websiteChannelContext)
    : TagHelper
{
    [ViewContext] [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;

    /// <summary>
    ///     If provided, this will be the relative URL the ajax call will make.  If not provided, will get the Relative Url
    ///     from the Page or DocumentID
    /// </summary>
    public string RelativeUrl { get; set; } = string.Empty;

    /// <summary>
    ///     The language of the Web Page you wish to retrieve
    ///     If language is not passed it will default to <see cref="IPreferredLanguageRetriever.Get()"/>
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    ///     The Document ID you wish to get the Relative url from.
    /// </summary>
    public int WebPageId { get; set; }

    protected string AjaxUrl { get; set; } = string.Empty;
    private bool Render { get; set; }

    public override void Init(TagHelperContext context)
    {
        if (string.IsNullOrWhiteSpace(Language))
            Language = contentLanguageRetriever.Get();

        base.Init(context);
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(ViewContext);
        
        if (WebPageId > 0)
        {
            var webPageUrl = await pageUrlRetriever
                .Retrieve(WebPageId, Language, websiteChannelContext.IsPreview, ViewContext.HttpContext.RequestAborted);

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

            var html = $@"
                <div id=""Partial-{uniqueId}""></div>
                <script type=""module"">await window.PWP.load({{url: '{url}', id: '{uniqueId}'}});</script>       
            ";
            return html;
        }

        return "<!-- Could not determine Ajax Url, please provide valid Page or Document ID -->";
    }
}