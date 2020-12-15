
## Installation [.Net Core]
1. Install the [`PartialWidgetPage.Kentico.MVC.Core`](https://www.nuget.org/packages/PartialWidgetPage.Kentico.MVC.Core/) Nuget Package on your .net Core site.
2. In your Startup, add the implementation of the IPartialWidgetPageHelper: `services.AddSingleton(typeof(IPartialWidgetPageHelper), typeof(PartialWidgetPageHelper));`
3. Add this TagHelper to your View or _ViewImport.cshtml: `@addTagHelper *, PartialWidgetPage.Kentico.MVC.Core`
4. You may also want to add a Using for the PartialWidgetPage namespace: `@using PartialWidgetPage`

To install the Partial Widget Page Widget as well...
1. Install the [`PartialWidgetPage.Kentico.MVC.Core.Widget] Nuget Package on your .net Core Site.
2. Implement your own IPartialWidgetRenderingRetriever (see examples below) and add it to your Startup file: `services.AddSingleton(typeof(IPartialWidgetRenderingRetriever), typeof(CustomPartialWidgetRenderingRetriever));`	
3.   Allow the widget with code name `PartialWidgetPage.PartialWidget` into any zones with a specified widget list.

## Installation [.Net Full Framework]
1. Install the [`PartialWidgetPage.Kentico.MVC`] Nuget Package on your .net 4.8 MVC Site.
2. Register your dependency injections for the `IPartialWidgetPageHelper` and `IPartialWidgetRenderingRetriever` (you'll have to supply your own implementation of  `IPartialWidgetRenderingRetriever`), make sure that these are registered for both the main project assembly **and** the Partial Widget Assembly.  Here's what was in my `ApplicationConfig.RegisterFeatures`:
``` csharp
            // Register AutoFac Items
            var autofacBuilder = new ContainerBuilder();

            autofacBuilder.RegisterType<PartialWidgetPageHelper>().As<IPartialWidgetPageHelper>();
            autofacBuilder.RegisterType<CustomPartialWidgetRenderingRetriever>().As<IPartialWidgetRenderingRetriever>();

            // Register Dependencies for Cache, pass in any assemblies you wish to hook up
            DependencyResolverConfig.Register(autofacBuilder, new Assembly[] { typeof(ApplicationConfig).Assembly, typeof(PartialWidgetPageHelper).Assembly });

            // Set Autofac Dependency resolver to the builder
            DependencyResolver.SetResolver(new AutofacDependencyResolver(autofacBuilder.Build()));

```
3. Consider adding `<add namespace="PartialWidgetPage"/>` to the web.config in your Views to get the Html helper extensions
4.   Allow the widget with code name `PartialWidgetPage.PartialWidget` into any zones with a specified widget list.

## WARNING: INFINITE LOOPS
When using this tool, be very careful not to render a widget page that render itself or the parent, thus causing an infinite loop of rendering.  If you editing a page that will be used in the Header and Footer, for example, please make a different Layout view that does not render the Header or Footer on it.

## [.Net Core] Tag Helpers
The Partial Widget Page system comes with the following tag helpers that allow you to leverage this in your code.

To leverage, add this TagHelper to your View or _ViewImport.cshtml
`@addTagHelper *, PartialWidgetPage.Kentico.MVC.Core`

You may also want to add a Using for the PartialWidgetPage namespace
`@using PartialWidgetPage`

**PartialWidgetPageTagHelper [inlinewidgetpage]**
This tag helper helps preserve and switch the Page Builder Context, and contains 3 parameters:
**initialize-document-prior**: [Default = true], if the Page Builder Context should be initialized before calling the inner content.  You would set this to false if the View Component you call within it calls the `IPageDataContextInitializer.Initialize` function.  *If **false** you do not need `page` or `documentid`.*
**page**: This will initialize the context with this `TreeNode` before rendering.  *If provided, you do **not** need the `documentid`.*
**documentid**: This will initialize the context with the `TreeNode` belonging to this document id before rendering.  *If provided, you do **not** need the `page`.*

``` csharp
<inlinewidgetpage initialize-document-prior="true" documentid="123" >
    @await Component.InvokeAsync("ShareableContentComponent", new { Testing = "Hello" })
</inlinewidgetpage>
<inlinewidgetpage initialize-document-prior="true" page=@Model.Page >
    @await Component.InvokeAsync("ShareableContentComponent", new { Testing = "Hello" })
</inlinewidgetpage>
<inlinewidgetpage initialize-document-prior="false">
    @await Component.InvokeAsync("SomeComponentThatInitializesDoc", new { DocumentID = 123 })
</inlinewidgetpage>
```
The first two would map to this View Component:
``` csharp
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Generic.Models;
namespace Generic
{
    [ViewComponent(Name = "ShareableContentComponent")]
    public class ShareableContentComponent : ViewComponent
    {
        public ShareableContentComponent(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public IHttpContextAccessor HttpContextAccessor { get; }

        public async Task<IViewComponentResult> InvokeAsync(string Testing = null)
        {
            return View(new ShareableContentComponentViewModel() { EditMode = HttpContextAccessor.HttpContext.Kentico().PageBuilder().EditMode });
        }
    }
}
```

** PartialWidgetPageAjaxTagHelper [ajaxwidgetpage]**
This tag helper wires up the Client Side Ajax request to pull in page content.  It contains 4 parameters:
**relative-url**: The url that will be called to pull in the request.  Can be any valid route, including custom ones, or just the relative url to the Page.  *If provided, you do **not** need page or documentid*
**page**: The `TreeNode` that you wish to render, this will be used to retrieve it's relative url.
**documentid** The `TreeNode`'s document id that you wish to render, this will be used to retrieve it's relative url.

``` csharp
        <ajaxwidgetpage relative-url="/Custom/CustomResponse" />
        <ajaxwidgetpage documentid="123" />
        <ajaxwidgetpage page=@Model.MyTreeNode />
```

## [.Net 4.8] Html Helpers
For the .Net Full Framework, the IPartialWidgetPageHelper methods have been exposed in an HtmlHelper extension.  If you are `@using PartialWidgetPage` then you should see new `@Html` methods.

**LayoutIfEditMode**: Used on the Layout assignment, will return a NULL layout (rendering as partial) if not EditMode or if it's being called as an Ajax call using the Partial Widget Page's Ajax methods

**GetCurrentContext**: Retrieves the current Page Builder / Page Context, use this before calling another widget page inline so you can restore the context after.
**ChangeContext**: Change the Page Builder context to not be Edit Mode, and if a `TreeNode` or `DocumentID` is passed, it also changes the Page Builder Context to the given page.
**RestoreContext**: Pass the `PreservedPageBuilderContext` model you got from `GetCurrentContext` to this function to restore the Page Builder.  You call this after you have finished rendering a nested widget page.

Here's a sample of rendering a nested Tab Page
``` csharp
var Context = Html.GetCurrentContext();
 Html.ChangeContext(Element.DocumentID);
 <div class="tab-pane fade @(T == 0 ? "show active" : "")" id="Tab-@Element.DocumentID" role="tabpanel" aria-labelledby="Tab-@Element.DocumentID">
     @{Html.RenderAction("TabPartial", new { });}
 </div>
 Html.RestoreContext(Context);
```
This would map to this Partial:
``` csharp
 public class TestController : Controller
    {
public ActionResult TabPartial()
        {
            TabFields model = new TabFields(DataRetriever.Retrieve<Tab>().Page);
            return View(model);
        }
}
```
and this View:
``` csharp
@model CMS.DocumentEngine.Types.Generic.Tab.TabFields
<div class="FromViewComponent">
    <h2>@Model.Name</h2>
    <p>Content below</p>
    @Html.Kentico().EditableArea("tabContent")
    <editable-area area-identifier="tabContent" />
    <p>Content above</p>
</div>
```

**GetDocumentIDByNode/GetDocumentIDByDocument**: Helper functions to retrieve the DocumentID given the Path, Node Guid, NodeID, or Document Guid.

**PartialWidgetPageAjax**: Generates the Div/Javascript to render the given request/page client side.
``` csharp
@Html.PartialWidgetPageAjax(@Model.AjaxUrl)
@Html.PartialWidgetPageAjax(@Model.DocumentID
```

---
## Partial Widget Page - Widget
The module comes with the `Partial Widget Page` widget [`PartialWidgetPage.PartialWidget`].  This tool allows you to render another page on your current page through this widget.   Let's look at the widget properties.

### Render Mode
You can render your content in one of two ways.  

**Server Render** will render the content inline with the request, server side.   This requires you to implement the IPartialWidgetRenderingRetriever interface to determine what `ViewComponent` [.net Core] or `Controller/Action` [.net Full] each class should be rendered with (see the section *`IPartialWidgetRenderingRetriever`* below).

**AJAX** will generate the script to make a client-side ajax call to retrieve the content and inject it on the page.

### Page Selection Mode
**By NodeAliasPath** allows you to select your page via the Node Alias Path, you can also configure the Culture and Site Name if they vary from the visitor culture and current site.
**By Node Guid** allows you to select your page via the Node Guid, since a NodeGuid is unique to a node, only the Culture can be overwritten if you wish to deviate from the visitor culture.
### AJAX Additional Options
**Custom URL**  This allows you to enter a relative or full URL of the page you wish to retrieve, you can enter any valid Url that your site will render, even if it's a custom route.  This takes priority over the Page Selection if provided.

### IPartialWidgetPageHelper.LayoutIfEditMode
In your rendering View, if you wish to toggle the Layout off during either server side or ajax calls, please use the 
`Layout = IPartialWidgetPageHelper.LayoutIfEditMode("_Layout")` in your view.  This will return a Null for the layout if it's either not in Edit Mode or if it's being called from the Partial Widget Page's Ajax rendering.  

Be aware you may have to add a custom View or logic if you need to do Server Rendering with a partial, but also want it to render with a normal layout on non-edit mode.  However usually this is can be accomplished through your ViewComponent/Action Controller logic, or through a separate view.

---
## Setup Partial-Viewable Content
If you wish to use the same View for editing, but then render it as a partial view when pulled in as a partial, you can use `Html.LayoutIfEditMode` (.Net 4.8) or `IPartialWidgetPageHelper.LayoutIfEditMode` (.Net Core)

``` csharp
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml");
```
``` csharp
@inject IPartialWidgetPage PWPHelper
Layout = PWPHelper.LayoutIfEditMode("~/Views/Shared/_layout.cshtml");
```

If you instead are using Ajax to pull in a partial page, you can use the `Html.LayoutIfNotAjax` (.Net 4.8) or `IPartialWidgetPageHelper.LayoutIfNotAjax` (.Net Core)

``` csharp
Layout = Html.LayoutIfNotAjax("~/Views/Shared/_layout.cshtml");
```
``` csharp
@inject IPartialWidgetPage PWPHelper
Layout = PWPHelper.LayoutIfNotAjax("~/Views/Shared/_layout.cshtml");
```

### IPartialWidgetRenderingRetriever
In order for the Partial Widget Page Widget to render a given page, it must know *how* to render it.  If you wish to leverage this, you need to implement your own IPartialWidgetRenderingRetriever and hook it up with your dependency injection system.

#### .Net Core Example
Here is my implementation where I'm registering my Tab's to be usable as an Partial Widget Page, as well as my ShareableContent class.   Now the widget will know what View Component and what data it needs, and that it should switch the Page Builder Context prior to calling these (as these View Components do not call the `pageDataContextInitializer.Initialize` method within them.
``` csharp
using CMS.DocumentEngine.Types.Generic;
using PartialWidgetPage;

namespace BlankSite.MyRepositories
{
    public class CustomPartialWidgetRenderingRetriever : IPartialWidgetRenderingRetriever
    {
        public ParitalWidgetRendering GetRenderingViewComponent(string ClassName, int DocumentID = 0)
        {
            if (ClassName.Equals(Tab.CLASS_NAME))
            {
                return new ParitalWidgetRendering()
                {
                    ViewComponentData = new { },
                    ViewComponentName = "TabComponent",
                    SetContextPriorToCall = true
                };
            }
            if (ClassName.Equals(ShareableContent.CLASS_NAME))
           {
               return new ParitalWidgetRendering()
               {
                   ViewComponentData = new { Testing = "Hello" },
                   ViewComponentName = "ShareableContentComponent",
                   SetContextPriorToCall = true
               };
           }
           
            return null;
        }
    }
}
```

#### .Net 4.8 Example
Here is my implementation for the `IPartialWidgetRenderingRetriever`
``` csharp
namespace BlankSite.MyRepositories
{
    public class CustomPartialWidgetRenderingRetriever : IPartialWidgetRenderingRetriever
    {
        public ParitalWidgetRendering GetRenderingControllerAction(string ClassName, int DocumentID = 0)
        {
            if (ClassName.Equals(Tab.CLASS_NAME))
            {
                return new ParitalWidgetRendering()
                {
                    RouteValues = new { },
                    ActionName = "TabPartial",
                    ControllerName="Test",
                    SetContextPriorToCall = true
                };
            }
            if (ClassName.Equals(ShareableContent.CLASS_NAME))
            {
                return new ParitalWidgetRendering()
                {
                    RouteValues = new { Testing = "Hello" },
                    ActionName = "Partial",
                    ControllerName = "ShareableContent",
                    SetContextPriorToCall = true
                };
            }

            return null;
        }
    }
}
```

# Contributions, bug fixes and License
Feel free to Fork and submit pull requests to contribute.

You can submit bugs through the issue list and i will get to them as soon as i can, unless you want to fix it yourself and submit a pull request!

This is free to use and modify!

# Compatability
Can be used on any Kentico 13 MVC site, either .Net 4.8 or .Net Core