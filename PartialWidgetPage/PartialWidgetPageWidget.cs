using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using PartialWidgetPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

[assembly: RegisterWidget("PartialWidgetPage.PartialWidget", "Partial Widget Page", typeof(PartialWidgetPageModel), customViewName: "Widgets/PartialWidgetPage/_PartialWidgetPage", Description = "Render a page's content with it's widgets, layout should use Html.LayoutIfEditMode() so will render as a partial view if pulling in.", IconClass = "icon-doc-torn")]
