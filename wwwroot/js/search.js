// Sugerencias de búsqueda al escribir (estilo Amazon).
// Pide términos a /Home/Suggest (coincidencias por texto + relacionados con pgvector)
// y al elegir uno abre la página de resultados.
(function () {
  const input = document.getElementById("search-input");
  const list = document.getElementById("search-suggestions");
  if (!input || !list) return;
  const form = input.form;
  let timer = null, active = -1, lastQuery = "", controller = null;

  function close() { list.hidden = true; list.innerHTML = ""; active = -1; input.setAttribute("aria-expanded", "false"); }

  function choose(term) {
    input.value = term;
    close();
    form.submit();
  }

  function render(terms, query) {
    list.innerHTML = "";
    active = -1;
    if (!terms.length) { close(); return; }
    const q = query.toLowerCase();
    terms.forEach((term, i) => {
      const li = document.createElement("li");
      li.id = "sugg-" + i;
      li.setAttribute("role", "option");
      const pos = term.indexOf(q);
      if (pos >= 0) {
        li.append(term.slice(0, pos));
        const b = document.createElement("b");
        b.textContent = term.slice(pos, pos + q.length);
        li.append(b, term.slice(pos + q.length));
      } else {
        li.textContent = term; // relacionado por significado (pgvector)
      }
      li.addEventListener("mousedown", e => { e.preventDefault(); choose(term); });
      list.append(li);
    });
    list.hidden = false;
    input.setAttribute("aria-expanded", "true");
  }

  function highlight(i) {
    const items = list.querySelectorAll("li");
    if (!items.length) return;
    active = (i + items.length) % items.length;
    items.forEach((li, n) => li.classList.toggle("active", n === active));
    input.setAttribute("aria-activedescendant", items[active].id);
  }

  async function fetchSuggestions(query) {
    if (controller) controller.abort();
    controller = new AbortController();
    try {
      const res = await fetch("/Home/Suggest?q=" + encodeURIComponent(query), { signal: controller.signal });
      if (!res.ok) return close();
      if (input.value.trim() === query) render(await res.json(), query);
    } catch (e) {
      if (e.name !== "AbortError") close();
    }
  }

  input.addEventListener("input", () => {
    const query = input.value.trim();
    clearTimeout(timer);
    if (query.length < 2) { close(); return; }
    if (query === lastQuery && !list.hidden) return;
    lastQuery = query;
    timer = setTimeout(() => fetchSuggestions(query), 200);
  });

  input.addEventListener("keydown", e => {
    if (list.hidden) return;
    if (e.key === "ArrowDown") { e.preventDefault(); highlight(active + 1); }
    else if (e.key === "ArrowUp") { e.preventDefault(); highlight(active - 1); }
    else if (e.key === "Enter" && active >= 0) { e.preventDefault(); choose(list.querySelectorAll("li")[active].textContent); }
    else if (e.key === "Escape") { close(); }
  });

  input.addEventListener("blur", () => setTimeout(close, 100));
})();
