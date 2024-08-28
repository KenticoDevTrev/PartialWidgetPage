class PWP {
    async load({url, id}) {
        const elm = document.getElementById(id);
        const resp = await fetch(url);
        if (resp.ok) {
            elm.innerHTML = await resp.text();
            elm.dispatchEvent(new Event('pwp.load', {bubbles: true, cancelable: true}));
            document.dispatchEvent(this._hydrateObj(new Event('pwp.load', {bubbles: true, cancelable: true}), {elm}));
        } else {
            console.error('PWP error retrieving page content at {url}');
        }
    }

    _hydrateObj = (obj, meta = {}) => {
        for (const [key, value] of Object.entries(meta)) {
            try {
                obj[key] = value
            } catch {
                Object.defineProperty(obj, key, {
                    configurable: true,
                    get() {
                        return value
                    }
                })
            }
        }
        return obj
    }
}

window.PWP = new PWP();