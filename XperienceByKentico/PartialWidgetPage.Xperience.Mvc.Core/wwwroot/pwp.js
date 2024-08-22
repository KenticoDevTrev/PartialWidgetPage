class o{async load({url:n,id:a}){const e=document.getElementById(`Partial-${a}`),t=await fetch(n);t.ok?(e.innerHTML=await t.text(),document.dispatchEvent(new CustomEvent("pwp-load",{detail:{partialElement:e}}))):console.error("PWP error retrieving page content at {url}")}}window.PWP=new o;
//# sourceMappingURL=pwp.js.map
