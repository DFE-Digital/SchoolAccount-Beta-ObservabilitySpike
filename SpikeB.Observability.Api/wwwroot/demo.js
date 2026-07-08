const startButton = document.getElementById("start");
const stopButton = document.getElementById("stop");

const trafficStatus = document.getElementById("traffic-status");
const scenarioLabel = document.getElementById("current-scenario");

const requestsLabel = document.getElementById("requests");
const successLabel = document.getElementById("success");
const failedLabel = document.getElementById("failed");
const latencyLabel = document.getElementById("latency");
const currentRpsLabel = document.getElementById("current-rps");

const activity = document.getElementById("activity");

const rpsSlider = document.getElementById("rps");
const rpsValue = document.getElementById("rps-value");

let timer = null;

let totalRequests = 0;
let successfulRequests = 0;
let failedRequests = 0;
let totalLatency = 0;

rpsSlider.addEventListener("input", () => {
    rpsValue.innerText = rpsSlider.value;

    if (timer !== null) {
        stopTraffic();
        startTraffic();
    }
});

document
    .querySelectorAll("input[name=scenario]")
    .forEach(radio => {
        radio.addEventListener("change", () => {
            scenarioLabel.textContent = `Scenario: ${formatScenario(radio.value)}`;
        });
    });

startButton.addEventListener("click", startTraffic);
stopButton.addEventListener("click", stopTraffic);

function startTraffic() {
    if (timer !== null) return;

    const rps = Number(rpsSlider.value);

    currentRpsLabel.innerText = rps;
    trafficStatus.textContent = "● Running";
    trafficStatus.className = "status running";

    timer = setInterval(sendRequest, 1000 / rps);
}

function stopTraffic() {
    clearInterval(timer);
    timer = null;

    currentRpsLabel.innerText = "0";
    trafficStatus.textContent = "● Stopped";
    trafficStatus.className = "status stopped";
}

async function sendRequest() {
    const scenario = getSelectedScenario();
    const started = performance.now();

    try {
        const response = await fetch("/api/traffic/run", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ scenario })
        });

        const result = await response.json();
        const duration = Math.round(performance.now() - started);

        totalRequests++;

        if (result.success) {
            successfulRequests++;
        } else {
            failedRequests++;
        }

        totalLatency += duration;

        refreshStats();

        addActivity(
            scenario,
            result.statusCode,
            duration,
            result.success);
    }
    catch {
        const duration = Math.round(performance.now() - started);

        totalRequests++;
        failedRequests++;
        totalLatency += duration;

        refreshStats();

        addActivity(
            scenario,
            "FAIL",
            duration,
            false);
    }
}

function refreshStats() {
    requestsLabel.innerText = totalRequests;
    successLabel.innerText = successfulRequests;
    failedLabel.innerText = failedRequests;

    latencyLabel.innerText =
        `${Math.round(totalLatency / totalRequests)} ms`;
}

function addActivity(scenario, status, duration, success) {
    const row = document.createElement("tr");

    const css = success
        ? "success"
        : "failure";

    row.innerHTML = `
        <td>${new Date().toLocaleTimeString()}</td>
        <td>${formatScenario(scenario)}</td>
        <td class="${css}">${status}</td>
        <td>${duration} ms</td>
    `;

    activity.prepend(row);

    while (activity.children.length > 100) {
        activity.removeChild(activity.lastChild);
    }
}

function getSelectedScenario() {
    return document.querySelector("input[name=scenario]:checked").value;
}

function formatScenario(value) {
    return value
        .replace("-", " ")
        .replace(/\b\w/g, l => l.toUpperCase());
}


rpsSlider.value = 2;
rpsValue.innerText = "2";
scenarioLabel.textContent = "Scenario: Normal";
startTraffic();