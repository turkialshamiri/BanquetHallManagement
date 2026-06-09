/* Dev Login Helper — development only; no credentials are embedded. */
(() => {
  const run = () => {
    if (!/\/Account\/Login\/?$/i.test(window.location.pathname)) {
      return;
    }

    const userInput = document.querySelector(
      'input[name$="UserNameOrEmailAddress"], input[id$="UserNameOrEmailAddress"], input[name$="UserName"], input[id$="UserName"]'
    );
    const passwordInput = document.querySelector(
      'input[type="password"][name$="Password"], input[type="password"][id$="Password"]'
    );

    const addHint = (input, text) => {
      if (!input || input.dataset.defaultHintAdded === "true") {
        return;
      }

      const hint = document.createElement("span");
      hint.className = "text-muted small";
      hint.textContent = text;

      const floatingContainer = input.closest(".form-floating.mb-2");
      if (floatingContainer) {
        floatingContainer.appendChild(hint);
        input.dataset.defaultHintAdded = "true";
        return;
      }

      const container =
        input.closest(".input-group") ||
        input.closest(".form-group") ||
        input.closest(".mb-3") ||
        input.closest(".form-floating") ||
        input;

      if (container === input || container.classList.contains("input-group")) {
        container.insertAdjacentElement("afterend", hint);
      } else {
        container.appendChild(hint);
      }
      input.dataset.defaultHintAdded = "true";
    };

    addHint(
      userInput,
      "Use Seed:AbpAdmin credentials from appsettings.secrets.json or user secrets."
    );
    addHint(
      passwordInput,
      "Password is not stored in source control. Check your local secrets file."
    );
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", run);
  } else {
    run();
  }
})();
