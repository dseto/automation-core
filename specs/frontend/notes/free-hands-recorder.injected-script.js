
// FREE-HANDS Recorder injected script (MVP reference)
(() => {
  if (window.__fhRecorder?.version) return;
  const buffer = [];
  const version = "mvp-1";
  const now = () => Date.now();

  const shortText = (el) => (el && (el.innerText || el.value || el.getAttribute?.("aria-label") || "") || "").toString().trim().slice(0, 80);

  const attrs = (el) => {
    if (!el || !el.getAttribute) return {};
    const pick = ["id","name","type","role","aria-label","data-testid"];
    const out = {};
    for (const k of pick) {
      const v = el.getAttribute(k);
      if (v) out[k] = v;
    }
    return out;
  };

  const cssPath = (el) => {
    try {
      if (!el || !el.tagName) return null;
      if (el.id) return `#${el.id}`;
      const tag = el.tagName.toLowerCase();
      const name = el.getAttribute?.("name");
      if (name) return `${tag}[name="${name}"]`;
      return tag;
    } catch { return null; }
  };

  const push = (e) => buffer.push(e);

  const nav = () => push({ kind:"navigate", ts: now(), route: location.pathname + location.search + location.hash });
  const _ps = history.pushState;
  history.pushState = function(...args){ const r=_ps.apply(this,args); nav(); return r; };
  const _rs = history.replaceState;
  history.replaceState = function(...args){ const r=_rs.apply(this,args); nav(); return r; };
  window.addEventListener("popstate", nav);

  document.addEventListener("click", (ev) => {
    const el = ev.target?.closest?.("button,a,input,select,textarea,[role]") || ev.target;
    push({ kind:"click", ts: now(), target:{ tag: el?.tagName?.toLowerCase() || null, text: shortText(el), css: cssPath(el), attributes: attrs(el) } });
  }, true);

  const timers = new WeakMap();
  const pending = new WeakMap();
  const scheduleFill = (el, value) => {
    pending.set(el, value);
    const prev = timers.get(el);
    if (prev) clearTimeout(prev);
    timers.set(el, setTimeout(() => {
      const v = pending.get(el);
      push({ kind:"fill", ts: now(), target:{ tag: el?.tagName?.toLowerCase() || null, text: shortText(el), css: cssPath(el), attributes: attrs(el) }, value:{ literal: v } });
    }, 400));
  };
  document.addEventListener("input", (ev) => {
    const el = ev.target;
    if (!el) return;
    if (el.tagName && ["INPUT","TEXTAREA"].includes(el.tagName)) scheduleFill(el, el.value);
  }, true);
  document.addEventListener("change", (ev) => {
    const el = ev.target;
    if (!el) return;
    if (el.tagName && ["SELECT"].includes(el.tagName)) push({ kind:"select", ts: now(), target:{ tag:"select", css: cssPath(el), attributes: attrs(el) }, value:{ literal: el.value } });
  }, true);

  document.addEventListener("submit", (ev) => {
    push({ kind:"submit", ts: now(), target:{ tag:"form", css: "form", attributes: {} } });
  }, true);

  window.__fhRecorder = { version, drain: () => buffer.splice(0, buffer.length) };
  nav();
})();
