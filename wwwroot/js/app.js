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
