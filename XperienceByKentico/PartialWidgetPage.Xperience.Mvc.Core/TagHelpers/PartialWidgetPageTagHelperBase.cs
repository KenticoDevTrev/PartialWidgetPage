using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage
{
    public class PartialWidgetPageTagHelperBase : TagHelper
    {
        [ViewContext][HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }
        public string Language { get; set; }

        public int WebPageId { get; set; }

        protected PreservedPageBuilderContext PreservedContext { get; set; }


        protected readonly IPartialWidgetPageHelper PartialWidgetPageHelper;

        public PartialWidgetPageTagHelperBase(IPartialWidgetPageHelper partialWidgetPageHelper)
        {
            PartialWidgetPageHelper = partialWidgetPageHelper;
        }

        public override void Init(TagHelperContext context)
        {
            PreservedContext = PartialWidgetPageHelper.GetCurrentContext();

            if (string.IsNullOrWhiteSpace(Language))
                Language = PreservedContext.Page.LanguageName;

            base.Init(context);
        }
    }
}
