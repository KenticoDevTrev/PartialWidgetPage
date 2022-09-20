using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;


namespace PartialWidgetPage
{
    public class ClonePageBuilderFeature : IPageBuilderFeature
    {

        public ClonePageBuilderFeature(IPageDataContextInitializer pageDataContextInitializer)
        {
            PageDataContextInitializer = pageDataContextInitializer;
        }

        public PageBuilderOptions Options { get; set; }

        public bool EditMode { get; set; }

        public int PageIdentifier { get; set; }

        public IPageDataContextInitializer PageDataContextInitializer { get; }

        public void Initialize(int pageIdentifier)
        {
            PageDataContextInitializer.Initialize(pageIdentifier);
        }
    }
}
