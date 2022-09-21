using CMS.DocumentEngine;
using System;

namespace PartialWidgetPage.Internal
{
    public interface IComponentViewModelGenerator
    {
        /// <summary>
        /// Parses out the Typed ComponentViewModel given the page and it's template properties/property type
        /// </summary>
        /// <param name="page">The Tree Node</param>
        /// <param name="properties">the PageTemplateConfiguration's Properties</param>
        /// <param name="propertyType">the PageTemplateDefinition PropertiesType</param>
        /// <returns>The ComponentViewModel and if it was successful or not.</returns>
        Tuple<object, bool> GetComponentViewModel(TreeNode page, object? properties, Type propertyType);
    }
}
