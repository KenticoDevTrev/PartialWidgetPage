using CMS.DocumentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;

namespace PartialWidgetPage
{
    /// <summary>
    /// Stores the preserved Page Builder Context
    /// </summary>
    public class PreservedPageBuilderContext
    {
        public IPageBuilderDataContext PageBuilderContext { get; set; }
        public TreeNode Page { get; set; }
    }
}
