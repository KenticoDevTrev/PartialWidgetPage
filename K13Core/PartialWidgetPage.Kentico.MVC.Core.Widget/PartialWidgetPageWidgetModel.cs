using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using System.Collections.Generic;

namespace PartialWidgetPage
{
    public class PartialWidgetPageWidgetModel : IWidgetProperties
    {
        public const string IDENTITY = "PartialWidgetPage.PartialWidget";
        public const string _RenderMode_Server = "ServerRequest";
        public const string _RenderMode_Ajax = "Ajax";
        public const string _PageSelectionMode_Path = "ByPath";
        public const string _PageSelectionMode_ByNodeGuid = "ByNodeGuid";

        [EditingComponent(DropDownComponent.IDENTIFIER, DefaultValue = "ServerRequest", Label = "Render Mode", Tooltip = "Server Render will render the content server side (requires IPartialWidgetRenderingRetreiver implementation for the selected class).\n\nAjax loads the content client-side [automatic routing, cache dependency separate]", ExplanationText = "Hover for more info", Order = 0)]
        [EditingComponentProperty(nameof(DropDownProperties.DataSource), "ServerRequest;Server Render\r\nAjax;Ajax")]
        public string RenderMode { get; set; }

        [EditingComponent(DropDownComponent.IDENTIFIER, DefaultValue = "ByPath", Label = "Page Selection", Tooltip = "How you would like to select the page", ExplanationText = "", Order = 1)]
        [EditingComponentProperty(nameof(DropDownProperties.DataSource), "ByPath;By NodeAliaPath\r\nByNodeGuid;By Selected Page")]
        public string PageSelectionMode { get; set; }

        [EditingComponent(PathSelector.IDENTIFIER, Order = 2, Label = "Path", Tooltip = "The Page to Render, if using Ajax, the url will be determined by the Page Type's URL pattern")]
        [VisibilityCondition(nameof(PageSelectionMode), ComparisonTypeEnum.IsEqualTo, "ByPath")]

        public IEnumerable<PathSelectorItem> Path { get; set; }

        [EditingComponent(PageSelector.IDENTIFIER, Order = 3, Label = "Page", Tooltip = "The Page to Render")]
        [VisibilityCondition(nameof(PageSelectionMode), ComparisonTypeEnum.IsEqualTo, "ByNodeGuid")]
        public IEnumerable<PageSelectorItem> Page { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 4, Label = "Custom Url", Tooltip = "The relative Url to render, will overwrite the Path if provided", ExplanationText = "Overwrites the path if provided")]
        [VisibilityCondition(nameof(RenderMode), ComparisonTypeEnum.IsEqualTo, "Ajax")]
        public string CustomUrl { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 5, Label = "Culture", Tooltip = "The Culture you wish to render, if empty the current user's culture will be preferred.")]
        public string Culture { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 6, Label = "SiteName", Tooltip = "The Site code name the Path belongs to, if empty the current site will be used.")]
        [VisibilityCondition(nameof(PageSelectionMode), ComparisonTypeEnum.IsEqualTo, "ByPath")]
        public string SiteName { get; set; }
    }
}
