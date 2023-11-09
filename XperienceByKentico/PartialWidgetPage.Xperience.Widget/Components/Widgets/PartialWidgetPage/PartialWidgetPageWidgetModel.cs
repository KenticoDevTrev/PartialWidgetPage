using CMS.ContentEngine;

namespace PartialWidgetPage;

public class PartialWidgetPageWidgetModel : IWidgetProperties
{
    public const string IDENTITY = "PartialWidgetPage.PartialWidget";

    public const string RENDER_MODE_TIP = @"
            Server Render (Default Routing) will render the content server side using whatever Template or default configuration is on the Document.
            Server Render (Custom) will render the content server side, but requires IPartialWidgetRenderingRetreiver implementation for the selected class.
            Ajax loads the content client-side [automatic routing, cache dependency separate]
        ";

    [EditingComponent(DropDownComponent.IDENTIFIER, DefaultValue = RENDER_MODE_SERVER_PAGE_BUILDER_LOGIC,
        Label = "Render Mode",
        Tooltip = RENDER_MODE_TIP,
        ExplanationText = "Hover for more info", Order = 0)]
    [EditingComponentProperty(nameof(DropDownProperties.DataSource), RENDER_MODE_SOURCE)]
    public string RenderMode { get; set; } = RENDER_MODE_AJAX;

    [EditingComponent(PageSelector.IDENTIFIER, Order = 3, Label = "Page", Tooltip = "The Page to Render")]
    public IEnumerable<PageSelectorItem> Page { get; set; } = Enumerable.Empty<PageSelectorItem>();

    [EditingComponent(TextInputComponent.IDENTIFIER, Order = 4, Label = "Custom Url",
        Tooltip = "The relative Url to render, will overwrite the Path if provided",
        ExplanationText = "Overwrites the path if provided")]
    [VisibilityCondition(nameof(RenderMode), ComparisonTypeEnum.IsEqualTo, "Ajax")]
    public string? CustomUrl { get; set; }

    [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "Use preferred language?", Order = 90)]
    public bool UsePreferredLanguage { get; set; }

    [EditingComponent(ObjectSelector.IDENTIFIER, Label = "Language to use?", Order = 91)]
    [EditingComponentProperty(nameof(ObjectSelectorProperties.ObjectType), ContentLanguageInfo.OBJECT_TYPE)]
    [EditingComponentProperty(nameof(ObjectSelectorProperties.MaxItemsLimit), 1)]
    [VisibilityCondition(nameof(UsePreferredLanguage), ComparisonTypeEnum.IsFalse)]
    public IEnumerable<ObjectSelectorItem> Language { get; set; } = Enumerable.Empty<ObjectSelectorItem>();
}