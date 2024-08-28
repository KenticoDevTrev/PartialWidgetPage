using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage
{
    public class PartialWidgetPageTagHelperBase(
        IPartialWidgetPageHelper partialWidgetPageHelper,
        IWebsiteChannelContext channelContext
    ) : TagHelper
    {
        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; } = null!;

        public string Language { get; set; } = "";
        public int WebPageId { get; set; } = 0;
        public string Channel { get; set; } = channelContext.WebsiteChannelName;

        
        protected readonly IPartialWidgetPageHelper PartialWidgetPageHelper = partialWidgetPageHelper;
        protected readonly PreservedPageBuilderContext PreservedContext = partialWidgetPageHelper.GetCurrentContext();

        public override void Init(TagHelperContext context)
        {
            var (_, _, page) = PreservedContext;

            if (string.IsNullOrWhiteSpace(Language) && page is not null)
                Language = page.LanguageName;

            base.Init(context);
        }
    }
}