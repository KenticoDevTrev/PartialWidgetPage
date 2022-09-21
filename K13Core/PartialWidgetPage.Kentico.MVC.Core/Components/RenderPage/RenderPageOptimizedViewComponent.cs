using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Microsoft.AspNetCore.Mvc;
using PartialWidgetPage.Internal;
using System;
using System.Linq;

namespace PartialWidgetPage
{
    /// <summary>
    /// Only difference is the model passed should contain the full typed tree node, this will prevent queries done to render.
    /// </summary>
    public class RenderPageOptimizedViewComponent : ViewComponent
    {
        private readonly IPartialWidgetPageHelper _partialWidgetPageHelper;
        private readonly IPageDataContextRetriever _pageDataContextRetriever;
        private readonly IEventLogService _eventLogService;
        private readonly IComponentViewModelGenerator _componentViewModelGenerator;
        private readonly ComponentDefinitionProvider<PageTemplateDefinition> _componentDefinitionProvider;

        public RenderPageOptimizedViewComponent(IPartialWidgetPageHelper partialWidgetPageHelper,
            IPageDataContextRetriever pageDataContextRetriever,
            IEventLogService eventLogService,
            IComponentViewModelGenerator componentViewModelGenerator
            )
        {
            _partialWidgetPageHelper = partialWidgetPageHelper;
            _pageDataContextRetriever = pageDataContextRetriever;
            _eventLogService = eventLogService;
            _componentViewModelGenerator = componentViewModelGenerator;
            _componentDefinitionProvider = new ComponentDefinitionProvider<PageTemplateDefinition>();
        }

        public IViewComponentResult Invoke(TreeNode typedPage)
        {
            // Save current context
            var currentContext = _partialWidgetPageHelper.GetCurrentContext();
            try
            {
                if(typedPage == null)
                {
                    throw new NullReferenceException("Typed Page is null");
                }

                // Set to new page's context
                _partialWidgetPageHelper.ChangeContext(typedPage);

                // retrieve the page (which should include typed info)
                var page = _pageDataContextRetriever.Retrieve<TreeNode>().Page;

                // Default model
                var model = new RenderPageViewModel()
                {
                    PreservedContext = currentContext,
                    ViewPath = $"PageTypes/{page.ClassName.Replace(".", "_")}.cshtml",
                    ComponentViewModel = ComponentViewModel.Create(page)
                };

                // see if it has a page template
                string templateConfiguration = page.GetValue("DocumentPageTemplateConfiguration", string.Empty);
                if (!string.IsNullOrWhiteSpace(templateConfiguration))
                {
                    var deserializer = new PageTemplateConfigurationSerializer();

                    var templateInfo = deserializer.Deserialize(templateConfiguration);
                    if (templateInfo != null)
                    {
                        var template = _componentDefinitionProvider.GetAll().FirstOrDefault(x => x.Identifier.Equals(templateInfo.Identifier, StringComparison.OrdinalIgnoreCase));
                        if (template != null)
                        {
                            model.ViewPath = template.IsCustom ? template.ViewPath : $"PageTemplates/_{template.Identifier}";

                            // Try to parse Template properties if there
                            Type modelType = template.PropertiesType;
                            if (modelType != null)
                            {
                                var typedModel = _componentViewModelGenerator.GetComponentViewModel(page, templateInfo.Properties, template.PropertiesType);
                                if (typedModel.Item2)
                                {
                                    model.ComponentViewModel = typedModel.Item1;
                                }
                            }
                        }
                    }
                }

                return View("/Components/RenderPage/RenderPage.cshtml", model);
            } catch (Exception ex)
            {
                _eventLogService.LogException("RenderPageViewComponent", "ErrorRendering", ex, additionalMessage: $"For document id {typedPage?.DocumentID ?? 0}.");
                _partialWidgetPageHelper.RestoreContext(currentContext);
                return Content(string.Empty);
            }
        }
    }
}