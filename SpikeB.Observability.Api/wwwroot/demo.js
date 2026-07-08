const buttons = document.querySelectorAll("button[data-url]");
const output = document.getElementById("output");
const statusText = document.getElementById("status");

buttons.forEach(button => {
    button.addEventListener("click", async () => {
        const url = button.dataset.url;
        const label = button.textContent.trim();
        const started = performance.now();

        buttons.forEach(x => x.disabled = true);
        button.classList.add("running");

        statusText.textContent = `Running ${label}...`;
        output.textContent = "{}";

        try {
            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            const elapsedMs = Math.round(performance.now() - started);
            const text = await response.text();

            let body;

            try {
                body = JSON.parse(text);
            } catch {
                body = text;
            }

            statusText.textContent = `${label} completed with HTTP ${response.status} in ${elapsedMs}ms.`;

            output.textContent = JSON.stringify({
                scenario: label,
                requestUrl: url,
                status: response.status,
                ok: response.ok,
                elapsedMs,
                body
            }, null, 2);
        } catch (error) {
            const elapsedMs = Math.round(performance.now() - started);

            statusText.textContent = `${label} failed after ${elapsedMs}ms.`;

            output.textContent = JSON.stringify({
                scenario: label,
                requestUrl: url,
                elapsedMs,
                error: error.message
            }, null, 2);
        } finally {
            button.classList.remove("running");
            buttons.forEach(x => x.disabled = false);
        }
    });
});