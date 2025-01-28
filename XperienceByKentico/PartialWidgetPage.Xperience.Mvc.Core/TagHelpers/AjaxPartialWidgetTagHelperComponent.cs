namespace PartialWidgetPage;

public class AjaxPartialWidgetTagHelperComponent : TagHelperComponent
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
        {
            // the file was not always including, so this will inject the small bit of minified JS into the head directly
            var loader = @"<script type=""module"">class PWP{async load({url:e,id:t}){let a=document.getElementById(t),r=await fetch(e);r.ok?(a.innerHTML=await r.text(),a.dispatchEvent(new Event(""pwp.load"",{bubbles:!0,cancelable:!0})),document.dispatchEvent(this._hydrateObj(new Event(""pwp.load"",{bubbles:!0,cancelable:!0}),{elm:a}))):console.error(""PWP error retrieving page content at {url}"")}_hydrateObj=(e,t={})=>{for(let[a,r]of Object.entries(t))try{e[a]=r}catch{Object.defineProperty(e,a,{configurable:!0,get:()=>r})}return e}}window.PWP=new PWP;</script>";
            output.PostContent.AppendHtml(loader);
        }
    }
}