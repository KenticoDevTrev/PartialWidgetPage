## Installation Xperience By Kentico [.Net Core] 28.0.0
1. Install the [XperienceCommunity.PartialWidgetPage](https://www.nuget.org/packages/XperienceCommunity.PartialWidgetPage) Nuget Package on your Xperience by Kentico site
2. In your Startup, add `services.AddPartialWidgetPage();` or `services.AddPartialWidgetPage<CustomPartialWidgetRenderingRetriever>();`
3. Add this TagHelper to your View or _ViewImport.cshtml: `@addTagHelper *, XperienceCommunity.PartialWidgetPage.Core`
4. You may also want to add a Using for the PartialWidgetPage namespace: `@using PartialWidgetPage`

To install the Partial Widget Page Widget as well...
1. Install the [`XperienceCommunity.PartialWidgetPage.Widget] Nuget Package on your Xperience by Kentico.`
2.   Allow the widget with code name `PartialWidgetPage.PartialWidget` into any zones with a specified widget list.
3. Optionally, if you wish to use Server Side rendering with custom view component logic, implement your own  `IPartialWidgetRenderingRetriever`  (see examples below) and add it to your Startup file: `services.AddSingleton(typeof(IPartialWidgetRenderingRetriever), typeof(CustomPartialWidgetRenderingRetriever));`	

## WARNING: INFINITE LOOPS
When using this tool, be very careful not to render a widget page that render itself or the parent, thus causing an infinite loop of rendering.  If you editing a page that will be used in the Header and Footer, for example, please make a different Layout view that does not render the Header or Footer on it.

## Edit Mode Partial Widget Rendering
When in edit mode, and partial widget pages are rendered, please be aware that widgets may not show, only on Preview / Live mode do partial widget pages actually render their widget content.

## Tag Helpers
The Partial Widget Page system comes with the following tag helpers that allow you to leverage this in your code.

To leverage, add this TagHelper to your View or _ViewImport.cshtml
`@addTagHelper *, XperienceCommunity.PartialWidgetPage`

You may also want to add a Using for the PartialWidgetPage namespace
`@using PartialWidgetPage`

## Usage
### Render-Page View Component (Recommended)
There are two render-page view components that switch the page context and call whatever default routing exists for the page (Basic Routing or Page Templates).

You can call the below to render the page via DocumentID (will handle context switching and retrieving the typed page)
`<render-page web-page-id=@documentID />`

### PartialWidgetPageTagHelper [inlinewidgetpage]
This tag helper helps preserve and switch the Page Builder Context, and contains 3 parameters:
**initialize-document-prior**: [Default = true], if the Page Builder Context should be initialized before calling the inner content.  You would set this to false if the View Component you call within it calls the `IPageDataContextInitializer.Initialize` function.  *If **false** you do not need `page` or `documentid`.*
**page**: This will initialize the context with this `TreeNode` before rendering.  *If provided, you do **not** need the `documentid`.*
**documentid**: This will initialize the context with the `TreeNode` belonging to this document id before rendering.  *If provided, you do **not** need the `page`.*

``` csharp
<inlinewidgetpage initialize-document-prior="true" web-page-id="123" >
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

### PartialWidgetPageAjaxTagHelper [ajaxwidgetpage]
This tag helper wires up the Client Side Ajax request to pull in page content.  It contains 4 parameters:
**relative-url**: The url that will be called to pull in the request.  Can be any valid route, including custom ones, or just the relative url to the Page.  *If provided, you do **not** need page or documentid*
**page**: The `TreeNode` that you wish to render, this will be used to retrieve it's relative url.
**documentid** The `TreeNode`'s document id that you wish to render, this will be used to retrieve it's relative url.

``` csharp
        <ajaxwidgetpage relative-url="/Custom/CustomResponse" />
        <ajaxwidgetpage web-page-id="123" />
```

#### XbyK PartialWidgetPageAjaxTagHelper ####
When ajax content is processed and loaded a custom event will fire 
```javascript
    document.addEventListener('pwp-load', (e) => {
        //The partial container element that was loaded
        const element = e.detail.partialElement; 
        const loadMore = element.querySelector("button[data-load-more");
        //Reinitialze custom javascript in the ajax loaded element
        if(loadMore) {
            loadMore.addEventListener("click", (e) => {
                alert('Load More Clicked')
            })
        }
    });
```
---

## Partial Widget Page - Widget
The module comes with the `Partial Widget Page` widget [`PartialWidgetPage.PartialWidget`].  This tool allows you to render another page on your current page through this widget.   Let's look at the widget properties.

### Render Mode
You can render your content in one of two ways.  
**Server Render (Default)** will render the content inline with the request, sever side, and will generate the appropriate `ComponentViewModel` / `ComponentViewModel<TemplateType>` model.  This is the recommended approach.
**Server Render (Custom)** will render the content inline with the request, server side, but requires you to implement the `IPartialWidgetRenderingRetriever` interface to determine what `ViewComponent` each class should be rendered with (see the section *`IPartialWidgetRenderingRetriever`* below).
**AJAX** will generate the script to make a client-side ajax call to retrieve the content and inject it on the page.  Your View should use `IPartialWidgetPageHelper.LayoutIfNotAjax("_Layout")` in it's rendering so only the content and not the surround layout is rendered when retrieving via ajax.

### Page Selection Mode
**By Node Guid** allows you to select your page via the Node Guid, since a NodeGuid is unique to a node, only the Culture can be overwritten if you wish to deviate from the visitor culture.
### AJAX Additional Options
**Custom URL**  This allows you to enter a relative or full URL of the page you wish to retrieve, you can enter any valid Url that your site will render, even if it's a custom route.  This takes priority over the Page Selection if provided.

### IPartialWidgetPageHelper.LayoutIfNotAjax
In your rendering View, if you wish to toggle the Layout off during either server side or ajax calls, please use the 
`Layout = IPartialWidgetPageHelper.LayoutIfEditMode("_Layout")` or `@Context.PartialWidgetPage().LayoutIfEditMode("_Layout")"` in your view for server side, and `Layout = IPartialWidgetPageHelper.LayoutIfNotAjax("_Layout")` or `@Context.PartialWidgetPage().LayoutIfNotAjax("_Layou")"` if ajax.  This will return a Null for the layout if it's either not in Edit Mode or if it's being called from the Partial Widget Page's Ajax rendering.  

Be aware you may have to add a custom View or logic if you need to do Server Rendering with a partial, but also want it to render with a normal layout on non-edit mode.  However usually this is can be accomplished through your ViewComponent/Action Controller logic, or through a separate view.

---
## Setup Partial-Viewable Content for AJAX
Server side rendering will automatically use the prescribed Layout if you are accessing (editing) your page directly.  However, if you are using AJAX to pull in the page content, then you will need to instruct the View to not have a layout if it's an AJAX request to it. To do this, use  `IPartialWidgetPageHelper.LayoutIfNotAjax`
``` csharp
@inject IPartialWidgetPage PWPHelper
Layout = PWPHelper.LayoutIfNotAjax("~/Views/Shared/_layout.cshtml");
```

``` csharp
Layout = Context.PartialWidgetPage().LayoutIfNotAjax("~/Views/Shared/_layout.cshtml");
```

## IPartialWidgetRenderingRetriever
In order for the Partial Widget Page Widget to render a given page, it must know *how* to render it.

As of `27.0.0`, the Partial Widget Page is able to parse the given page's Template Configuration (or lack there of) and determine the proper view to render (along with compiling the `ComponentViewModel` / `ComponentViewModel<TemplatePropertyType>` and passing it to the view).  

However, if you wish to put in custom logic and render out a specific View-Component, or you use Custom Routing, you can implement your own `IPartialWidgetRenderingRetriever` to determine which View-Component to render and what data should be passed to it.

Here is my implementation where I'm registering my Tab's to be usable as an Partial Widget Page, as well as my ShareableContent class.   Now the widget will know what View Component and what data it needs, and that it should switch the Page Builder Context prior to calling these (as these View Components do not call the `pageDataContextInitializer.Initialize` method within them.
``` csharp
using CMS.DocumentEngine.Types.Generic;
using PartialWidgetPage;

namespace BlankSite.MyRepositories
{
    public class CustomPartialWidgetRenderingRetriever : IPartialWidgetRenderingRetriever
    {
        public PartialWidgetRendering GetRenderingViewComponent(string ClassName, int DocumentID = 0)
        {
            if (ClassName.Equals(Tab.CLASS_NAME))
            {
                return new PartialWidgetRendering()
                {
                    ViewComponentData = new { },
                    ViewComponentName = "TabComponent",
                    SetContextPriorToCall = true
                };
            }
            if (ClassName.Equals(ShareableContent.CLASS_NAME))
           {
               return new PartialWidgetRendering()
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

# Contributions, bug fixes and License
Feel free to Fork and submit pull requests to contribute.

You can submit bugs through the issue list and I will get to them as soon as i can, unless you want to fix it yourself and submit a pull request!

This is free to use and modify!

Thanks to Josh Kerbaugh on his port to Xperience by Kentico!

# Compatability
28.0.0 Can be used on any Xperience By Kentico site.  Version 28.0.0 is compatible MVC (.net > 6.0).