document.querySelectorAll("form").forEach(form => {
  form.addEventListener("submit", () => {
    const button = form.querySelector("button[type='submit'], button:not([type])");
    if (!button) return;
    button.disabled = true;
    button.dataset.originalText = button.textContent;
    button.textContent = button.dataset.loadingText || "Please wait...";
  });
});
