namespace PartialWidgetPage.Internal
{
    public class DefaultPartialWidgetRenderingRetriever : IPartialWidgetRenderingRetriever
    {
        public PartialWidgetRendering GetRenderingViewComponent(string ClassName, int DocumentID = 0)
        {
            // Just here to provide a default

            return null;
        }
    }

}
