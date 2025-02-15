﻿using CMS.Websites.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PartialWidgetPage
{
    public class PartialWidgetPageTagHelperBase(
        IPartialWidgetPageHelper partialWidgetPageHelper,
        IWebsiteChannelContext channelContext,
        IPreferredLanguageRetriever preferredLanguageRetriever
    ) : TagHelper
    {
        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; } = null!;

        public string Language { get; set; } = "";
        public int WebPageId { get; set; } = 0;
        public string Channel { get; set; } = channelContext.WebsiteChannelName;

        
        protected readonly IPartialWidgetPageHelper PartialWidgetPageHelper = partialWidgetPageHelper;
        
        public override void Init(TagHelperContext context)
        {
            var (_, _, page) = PartialWidgetPageHelper.GetCurrentContext();

            if (string.IsNullOrWhiteSpace(Language) && page is not null) {
                Language = page.LanguageName;
            }
            if(string.IsNullOrWhiteSpace(Language)) {
                Language = preferredLanguageRetriever.Get();
            }

            base.Init(context);
        }
    }
}