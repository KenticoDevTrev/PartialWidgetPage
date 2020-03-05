using Kentico.PageBuilder.Web.Mvc;
using PartialWidgetPage;

[
    assembly: RegisterWidget
    (
        "PartialWidgetPage.PartialWidget",
        "Partial Widget Page",
        typeof(PartialWidgetPageModel),
        "Widgets/PartialWidgetPage/_PartialWidgetPage",
        Description = "Render a page's content with its widgets, layout should use Html.LayoutIfEditMode() so will render as a partial view if pulling in.",
        IconClass = "icon-doc-torn"
    )
]
