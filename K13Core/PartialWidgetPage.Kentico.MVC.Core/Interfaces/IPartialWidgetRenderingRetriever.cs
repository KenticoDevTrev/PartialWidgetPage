
namespace PartialWidgetPage
{
    public interface IPartialWidgetRenderingRetriever
    {
        /// <summary>
        /// Get the View Component and other necessary rendering logic for the given class.  Should be implemented by each project.
        /// </summary>
        /// <param name="ClassName">The Class Name</param>
        /// <param name="DocumentID">The Document ID in case page-specific context is needed</param>
        /// <returns>The Partial Widget rendering</returns>
        public ParitalWidgetRendering GetRenderingViewComponent(string ClassName, int DocumentID = 0);
    }
}
