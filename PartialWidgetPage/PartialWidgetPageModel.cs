using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialWidgetPage
{
    public class PartialWidgetPageModel : IWidgetProperties
    {
        [EditingComponent(PathSelector.IDENTIFIER, Order = 0, Label = "Path", Tooltip ="The Page to Render, the url will be determined by the Page Type's URL pattern")]
        public IList<PathSelectorItem> Path { get; set; }
            
        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 1, Label = "Custom Url", Tooltip = "The relative Url to render")]
        public string CustomUrl { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 2, Label = "Render as Parital Url Parameter",Tooltip = "if used layout should use Layout = Html.LayoutIfEditMode(\"~/Views/Shared/_layout.cshtml\", \"UrlParameter\")", ExplanationText = "Optional")]
        public string RenderAsPartialUrlParameter { get; set; }

        [EditingComponent(CheckBoxComponent.IDENTIFIER, Order = 3, Label = "Load via Ajax", Tooltip = "If checked, the content will load client side via ajax call")]
        public bool UseAjax { get; set; }

        /// <summary>
        /// Helper
        /// </summary>
        public string ActualUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CustomUrl))
                {
                    return CustomUrl;
                }
                else if (Path != null && Path.Count > 0 && !string.IsNullOrWhiteSpace(Path.FirstOrDefault().NodeAliasPath))
                {
                    return PartialWidgetPageExtensions.NodeAliasPathToUrl(Path.FirstOrDefault().NodeAliasPath);
                } else
                {
                    return null;
                }
            }
        }


    }
}
