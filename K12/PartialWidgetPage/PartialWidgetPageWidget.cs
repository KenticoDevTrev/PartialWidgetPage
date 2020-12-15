using Kentico.PageBuilder.Web.Mvc;
using PartialWidgetPage;

[assembly: RegisterWidget("PartialWidgetPage.PartialWidget", "Partial Widget Page", typeof(PartialWidgetPageModel), customViewName: "Widgets/PartialWidgetPage/_PartialWidgetPage", Description = "Render a page's content with it's widgets, layout should use Html.LayoutIfEditMode() so will render as a partial view if pulling in.", IconClass = "icon-doc-torn")]
