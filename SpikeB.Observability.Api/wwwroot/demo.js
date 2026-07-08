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

const chaosButton = document.getElementById("run-chaos");

chaosButton.addEventListener("click", async () => {
    const started = performance.now();

    buttons.forEach(x => x.disabled = true);
    chaosButton.disabled = true;
    chaosButton.classList.add("running");

    statusText.textContent = "Running chaos traffic...";
    output.textContent = "{}";

    try {
        const response = await fetch("/api/traffic/run", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify({
                totalRequests: 50,
                delayMs: 200
            })
        });

        const elapsedMs = Math.round(performance.now() - started);
        const body = await response.json();

        statusText.textContent =
            `Chaos run completed in ${elapsedMs}ms. Success: ${body.successCount}, Failed: ${body.failureCount}.`;

        output.textContent = JSON.stringify(body, null, 2);
    } catch (error) {
        const elapsedMs = Math.round(performance.now() - started);

        statusText.textContent = `Chaos run failed after ${elapsedMs}ms.`;

        output.textContent = JSON.stringify({
            elapsedMs,
            error: error.message
        }, null, 2);
    } finally {
        chaosButton.classList.remove("running");
        buttons.forEach(x => x.disabled = false);
        chaosButton.disabled = false;
    }
});