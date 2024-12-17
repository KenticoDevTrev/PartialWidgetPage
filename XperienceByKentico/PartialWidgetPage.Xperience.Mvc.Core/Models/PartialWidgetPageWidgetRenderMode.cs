namespace PartialWidgetPage;

public enum PartialWidgetPageWidgetRenderMode
{
    ServerSide,
    Ajax,

    /// <summary>
    ///     This will ignore the PartialWidgetRendering and render whatever View/Template that this page has.
    /// </summary>
    ServerSidePageBuilderLogic
}