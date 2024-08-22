class PWP {
    async load({url, id}) {
        const elm = document.getElementById(`Partial-${id}`);
        const resp = await fetch(url);
        if (resp.ok) {
            elm.innerHTML = await resp.text();
            document.dispatchEvent(new CustomEvent("pwp-load", {
                detail: {
                    partialElement: elm
                }
            }));
        } else {
            console.error('PWP error retrieving page content at {url}');
        }
    }

}

window.PWP = new PWP();