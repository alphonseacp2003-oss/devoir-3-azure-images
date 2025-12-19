// =========================
// CONFIG (à modifier)
// =========================
const FUNCTION_BASE = "https://alphonsefonction-hzeee3h3beada9gw.canadacentral-01.azurewebsites.net"; // <-- change ça
const UPLOAD_URL = `${FUNCTION_BASE}/api/Televerserimages`;
const LIST_URL = `${FUNCTION_BASE}/api/ireduites`;

// =========================
// UI
// =========================
const fileInput = document.getElementById("fileInput");
const fileName = document.getElementById("fileName");
const uploadBtn = document.getElementById("uploadBtn");
const refreshBtn = document.getElementById("refreshBtn");
const grid = document.getElementById("grid");
const emptyState = document.getElementById("emptyState");
const countPill = document.getElementById("countPill");
const statusText = document.getElementById("statusText");
const dot = document.getElementById("dot");
const preview = document.getElementById("preview");

function setStatus(type, text) {
  statusText.textContent = text;

  // couleurs simples via styles inline (sans définir de variables CSS)
  if (type === "ok") {
    dot.style.background = "rgba(80, 255, 170, .9)";
    dot.style.boxShadow = "0 0 0 4px rgba(80, 255, 170, .15)";
  } else if (type === "loading") {
    dot.style.background = "rgba(255, 210, 80, .9)";
    dot.style.boxShadow = "0 0 0 4px rgba(255, 210, 80, .15)";
  } else if (type === "error") {
    dot.style.background = "rgba(255, 90, 90, .95)";
    dot.style.boxShadow = "0 0 0 4px rgba(255, 90, 90, .15)";
  } else {
    dot.style.background = "rgba(255,255,255,.35)";
    dot.style.boxShadow = "0 0 0 4px rgba(255,255,255,.08)";
  }
}

function escapeHtml(str) {
  return String(str).replace(/[&<>"']/g, (m) => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#039;",
  }[m]));
}

function setCount(n) {
  countPill.textContent = n <= 1 ? `${n} image` : `${n} images`;
}

function render(urls) {
  grid.innerHTML = "";

  if (!urls || urls.length === 0) {
    emptyState.classList.add("show");
    setCount(0);
    return;
  }

  emptyState.classList.remove("show");
  setCount(urls.length);

  // tri (optionnel) pour une grille plus stable
  const sorted = [...urls].sort((a, b) => a.localeCompare(b));

  for (const url of sorted) {
    const name = decodeURIComponent(url.split("/").pop() || "image");

    const tile = document.createElement("div");
    tile.className = "tile";
    tile.innerHTML = `
      <img class="thumb" src="${escapeHtml(url)}" alt="${escapeHtml(name)}" loading="lazy">
      <div class="meta">
        <div class="name" title="${escapeHtml(name)}">${escapeHtml(name)}</div>
        <div class="actions">
          <a class="link" href="${escapeHtml(url)}" target="_blank" rel="noopener">Ouvrir</a>
          <a class="link" href="${escapeHtml(url)}" download>Télécharger</a>
        </div>
      </div>
    `;
    grid.appendChild(tile);
  }
}

// =========================
// API calls
// =========================
async function listThumbs() {
  setStatus("loading", "Chargement des miniatures…");
  refreshBtn.disabled = true;

  try {
    const res = await fetch(LIST_URL, { method: "GET" });
    if (!res.ok) throw new Error(`Erreur liste: ${res.status}`);

    const urls = await res.json();
    render(urls);

    setStatus("ok", "Miniatures chargées");
  } catch (err) {
    console.error(err);
    setStatus("error", "Erreur lors du chargement (CORS / URL / container)");
    render([]);
  } finally {
    refreshBtn.disabled = false;
  }
}

async function uploadImage(file) {
  setStatus("loading", "Téléversement…");
  uploadBtn.disabled = true;
  refreshBtn.disabled = true;

  try {
    const fd = new FormData();
    // ton backend détecte un fichier dans form-data (le nom du champ n’est pas bloquant)
    fd.append("file", file, file.name);

    const res = await fetch(UPLOAD_URL, {
      method: "POST",
      body: fd
    });

    if (!res.ok) {
      const txt = await res.text().catch(() => "");
      throw new Error(`Erreur upload: ${res.status} ${txt}`);
    }

    await res.json().catch(() => null);

    setStatus("ok", "Upload OK. Génération miniature…");

    // attendre un peu que le BlobTrigger crée la miniature
    await new Promise(r => setTimeout(r, 1200));

    await listThumbs();
  } catch (err) {
    console.error(err);
    setStatus("error", "Upload échoué (CORS / endpoint / AzureWebJobsStorage)");
  } finally {
    uploadBtn.disabled = false;
    refreshBtn.disabled = false;
  }
}

// =========================
// Events
// =========================
fileInput.addEventListener("change", () => {
  const file = fileInput.files?.[0];
  if (!file) {
    fileName.textContent = "Aucun fichier sélectionné";
    preview.classList.remove("show");
    preview.innerHTML = "";
    return;
  }

  fileName.textContent = file.name;

  // aperçu local
  const url = URL.createObjectURL(file);
  preview.innerHTML = `<img src="${url}" alt="Aperçu">`;
  preview.classList.add("show");
});

uploadBtn.addEventListener("click", () => {
  const file = fileInput.files?.[0];
  if (!file) {
    setStatus("error", "Choisis une image d’abord");
    return;
  }
  uploadImage(file);
});

refreshBtn.addEventListener("click", () => listThumbs());

// Auto-load au démarrage
listThumbs();
