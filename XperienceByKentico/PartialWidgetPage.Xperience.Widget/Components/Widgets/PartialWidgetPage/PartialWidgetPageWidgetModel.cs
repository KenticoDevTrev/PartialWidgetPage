using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;

namespace PartialWidgetPage;

public class PartialWidgetPageWidgetModel : IWidgetProperties
{
    public const string IDENTITY = "PartialWidgetPage.PartialWidget";

    public const string RENDER_MODE_TIP = @"
            Server Render (Default Routing) will render the content server side using whatever Template or default configuration is on the Document.
            Server Render (Custom) will render the content server side, but requires IPartialWidgetRenderingRetreiver implementation for the selected class.
            Ajax loads the content client-side [automatic routing, cache dependency separate]
        ";

    [DropDownComponent(
        Label = "Render Mode",
        Tooltip = RENDER_MODE_TIP,
        ExplanationText = "Hover for more info", Order = 0,
        Options = RENDER_MODE_SOURCE)]
    public string RenderMode { get; set; } = RENDER_MODE_SERVER_PAGE_BUILDER_LOGIC;

    [WebPageSelectorComponent(Label = "Page", Tooltip = "The Page to Render", Order = 3)]
    [VisibleIfNotEqualTo(nameof(RenderMode), "Ajax")]
    public IEnumerable<WebPageRelatedItem> Page { get; set; } = [];

    [VisibleIfEqualTo(nameof(RenderMode), "Ajax")]
    [UrlSelectorComponent(Label = "Custom Url",
        Tooltip = "The relative Url to render, will overwrite the Path if provided",
        ExplanationText = "Overwrites the path if provided",
        Order = 4)]
    public string? CustomUrl { get; set; }

    [CheckBoxComponent(Label = "Use preferred language?", Order = 90)]
    public bool UsePreferredLanguage { get; set; } = false;

    [VisibleIfFalse(nameof(UsePreferredLanguage))]
    [ObjectSelectorComponent(ContentLanguageInfo.OBJECT_TYPE, Label = "Language to use?", Order = 91, MaximumItems = 1)]
    public IEnumerable<ObjectRelatedItem> Language { get; set; } = [];
}