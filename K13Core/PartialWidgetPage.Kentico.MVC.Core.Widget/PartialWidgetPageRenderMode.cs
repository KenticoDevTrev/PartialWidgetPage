namespace PartialWidgetPage
{
    public enum PartialWidgetPageWidgetRenderMode
    {
        ServerSide,
        Ajax,
        /// <summary>
        /// This will ignore the ParitalWidgetRendering and render whatever View/Template that this page has.
        /// </summary>
        ServerSidePageBuilderLogic
    }
}
