namespace PartialWidgetPage;

public interface IPartialWidgetRenderingRetriever
{
    /// <summary>
    ///     Get the View Component and other necessary rendering logic for the given class.  Should be implemented by each
    ///     project.
    /// </summary>
    /// <param name="className">The Class Name</param>
    /// <param name="webPageId">The webPageId in case page-specific context is needed</param>
    /// <returns>The Partial Widget rendering</returns>
    public PartialWidgetRendering? GetRenderingViewComponent(string className, int webPageId = 0);
}