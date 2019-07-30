# Partial Widget Page
This tool allows you to render out a Widget (Page Builder) Page as a Partial.

## Installation
Install the Nuget Package on the MVC Site

## Usage

### Setup Partial-Viewable Content
On a Page with Kentico's Page Builder Enabled and an Editable Area existing, set the Layout from 
```
Layout = "~/Views/Shared/_Layout.cshtml";
```
 to 
```
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml");
```

This will enable the normal view to be resolved as a partial view, depending on the context.  When visiting through Kentico, EditMode is enabled and the full layout will render, allowing you to add Widgets.

#### WARNING: INFINITE LOOPS
Do not use a Layout that renders the page you are editing in itself.  This will cause an infinite loop when editing the page.  If you editing a page that will be used in the Header and Footer, for example, please make a different Layout view that does not have the PartialWidgetPage Html Helpers.

#### Allow page to render as normal page AND Partial View
If you wish for a page to render as a full page on Live, but also still want to pull this in as a Partial in certain cases, use the following in the View:

```
Layout = Html.LayoutIfEditMode("~/Views/Shared/_layout.cshtml", "RenderAsPartial");
``` 
and call it using 
```
@Html.PartialWidgetPage("/Route/To/Page", "RenderAsPartial")
```
(or add RenderAsPartial as the `Render as Parital Url Parameter` on the Partial Widget Page widget).  The system will now pass this Url Parameter in it's Render Partial requests.

### Showing Partial Widget Pages
On your other pages, you can either use the 
`@Html.PartialWidgetPage` / `@Html.PartialWidgetPageAjax` Html Helpers to pull in the content, or use the Partial Widget Page widget in a widget zone.

You can pass either a custom Path, or a NodeAliasPath. 

The normal method will perform a client side `DownloadString(Url)` on the page and return the content, this is done syncly so the content is there upon page load.

The ajax method is also available which will pull in the content through an ajax call client side.

# Contributions, but fixes and License
Feel free to Fork and submit pull requests to contribute.

You can submit bugs through the issue list and i will get to them as soon as i can, unless you want to fix it yourself and submit a pull request!

This is free to use and modify!

# Compatability
Can be used on any Kentico 12 SP site (hotfix 29 or above).

