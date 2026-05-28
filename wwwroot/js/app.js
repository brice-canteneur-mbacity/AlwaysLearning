// Applique l'apparence demandée (auto | light | dark) sur l'élément racine.
window.alApplyAppearance = function (mode) {
    var root = document.documentElement;
    if (mode === 'auto') {
        var dark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        root.dataset.theme = dark ? 'dark' : 'light';
    } else {
        root.dataset.theme = mode;
    }
};

// Applique la palette (standard | cb pour daltonisme).
window.alApplyPalette = function (palette) {
    document.documentElement.dataset.palette = palette;
};

// Partage natif si disponible, sinon copie du lien dans le presse-papier.
// Renvoie "shared", "copied" ou "none".
window.alShare = async function (title, url) {
    try {
        if (navigator.share) {
            await navigator.share({ title: title, url: url });
            return "shared";
        }
    } catch (e) {
        // L'utilisateur a annulé le partage : on ne fait rien de plus.
        return "none";
    }
    try {
        if (navigator.clipboard) {
            await navigator.clipboard.writeText(url);
            return "copied";
        }
    } catch (e) {
        return "none";
    }
    return "none";
};
