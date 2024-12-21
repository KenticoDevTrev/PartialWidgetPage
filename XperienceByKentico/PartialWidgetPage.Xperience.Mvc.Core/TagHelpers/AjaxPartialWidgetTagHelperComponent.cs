namespace PartialWidgetPage;

public class AjaxPartialWidgetTagHelperComponent : TagHelperComponent
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
        {
            var loader = @"<script type=""module"" src=""~/_content/XperienceCommunity.PartialWidgetPage/pwp.js""></script>";
            output.PostContent.AppendHtml(loader);
        }
    }
}