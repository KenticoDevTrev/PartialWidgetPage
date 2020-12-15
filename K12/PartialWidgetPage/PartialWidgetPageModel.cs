using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PartialWidgetPage
{
    public class PartialWidgetPageModel : IWidgetProperties
    {
        
        [EditingComponent(DropDownComponent.IDENTIFIER, DefaultValue = "ServerRequest", Label = "Render Mode", Tooltip = "Server Request [SR] will make a server side web request to gather the content [automatic routing but no output cache dependency sharing between this page and the partial page]\n\nAjax [AJ] loads the content client-side [automatic routing, cache dependency separate]", ExplanationText = "Hover for more info", Order = 0)]
        [EditingComponentProperty(nameof(DropDownProperties.DataSource), "ServerRequest;Server Request [SR]\r\nAjax;Ajax [AJ]")]

        public string RenderMode { get; set; }

        [EditingComponent(PathSelector.IDENTIFIER, Order = 1, Label = "Path", Tooltip = "The Page to Render, the url will be determined by the Page Type's URL pattern", ExplanationText = "Required")]
        public IList<PathSelectorItem> Path { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 2, Label = "Custom Url", Tooltip = "The relative Url to render", ExplanationText = "Optional")]
        public string CustomUrl { get; set; }

        [EditingComponent(TextInputComponent.IDENTIFIER, Order = 3, Label = "Render as Parital Url Parameter",Tooltip = "if used layout should use Layout = Html.LayoutIfEditMode(\"~/Views/Shared/_layout.cshtml\", \"UrlParameter\")", ExplanationText = "Optional")]
        public string RenderAsPartialUrlParameter { get; set; }

        public bool IsEditMode
        {
            get
            {
                return HttpContext.Current.Kentico().PageBuilder().EditMode;
            }
        }

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

        public string SinglePath
        {
            get
            {
                return Path != null && Path.Count > 0 ? Path.FirstOrDefault().NodeAliasPath : null;
            }
        }

    }
}
