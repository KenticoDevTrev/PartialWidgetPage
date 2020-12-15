using CMS.DocumentEngine;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace PartialWidgetPage.TagHelpers
{
    /// <summary>
    /// Tag Helper for rendering Widget Pages from within another widget page
    /// </summary>
    [HtmlTargetElement("inlinewidgetpage", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PartialWidgetPageTagHelper : TagHelper
    {
        /// <summary>
        /// If False, will not set the Page Context to the Node/DocumentID (if the View Component sets this). Defaults to True where it sets it given the Page / DocumentID
        /// </summary>
        public bool InitializeDocumentPrior { get; set; } = true;

        /// <summary>
        /// The Node you wish to switch the context to
        /// </summary>
        public TreeNode Page { get; set; }

        /// <summary>
        /// The DocumentID you wish to switch the context to if Page isn't provided
        /// </summary>
        public int Documentid { get; set; }

        private IPartialWidgetPageHelper PartialWidgetPageHelper { get; }
        private PreservedPageBuilderContext PreservedContext { get; set; }

        public PartialWidgetPageTagHelper(IPartialWidgetPageHelper partialWidgetPageHelper)
        {
            PartialWidgetPageHelper = partialWidgetPageHelper;
        }

        public override void Init(TagHelperContext context)
        {
            // Preserve Context
            PreservedContext = PartialWidgetPageHelper.GetCurrentContext();
            // Change context
            if (InitializeDocumentPrior)
            {
                if (Page != null)
                {
                    PartialWidgetPageHelper.ChangeContext(Page);
                }
                else if (Documentid > 0)
                {
                    PartialWidgetPageHelper.ChangeContext(Documentid);
                }
            } else
            {
                PartialWidgetPageHelper.ChangeContext();
            }
            base.Init(context);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Render out inner content
            output.TagName = null;
            var content = output.Content.GetContent();
            output.Content.SetHtmlContent(content);
            // restore previous context
            PartialWidgetPageHelper.RestoreContext(PreservedContext);
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Render out inner content
            output.TagName = null;
            var content = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(content.GetContent());
            // restore previous context
            PartialWidgetPageHelper.RestoreContext(PreservedContext);
        }
    }
}
