using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PartialWidgetPage
{
    public class PartialWidgetPageModel : IWidgetProperties
    {
        [EditingComponent(PageSelector.IDENTIFIER, Label = "Page", Order = 0, Tooltip = "The page to render, the URL will be determined by the page type's URL pattern")]
        public IList<PageSelectorItem> Page { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 1, Label = "Custom URL", Tooltip = "The relative URL to render")]
        public string CustomUrl { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 2, Label = "Render as partial URL parameter", Tooltip = "If used, layout should use Layout = Html.LayoutIfEditMode(\"~/Views/Shared/_Layout.cshtml\", \"UrlParameter\")", ExplanationText = "Optional")]
        public string RenderAsPartialUrlParameter { get; set; }

        [EditingComponent(CheckBoxComponent.IDENTIFIER, Order = 3, Label = "Load via AJAX", Tooltip = "If checked, the content will be loaded on the client side via AJAX call")]
        public bool UseAjax { get; set; }

        /// <summary>
        /// Helper
        /// </summary>
        public string ActualUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CustomUrl))
                    return CustomUrl;
                if (Page != null && Page.Count > 0 && Page.First().NodeGuid != Guid.Empty)
                    return PartialWidgetPageExtensions.NodeGuidToUrl(Page.First().NodeGuid);
                return null;
            }
        }
    }
}
