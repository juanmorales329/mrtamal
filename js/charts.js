var charts = {};

function renderBarChart(canvasId, labels, ingresos, egresos) {
    if (charts[canvasId]) charts[canvasId].destroy();
    var ctx = document.getElementById(canvasId);
    if (!ctx) return;
    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                { label: 'Ingresos', data: ingresos, backgroundColor: '#4CAF50' },
                { label: 'Egresos', data: egresos, backgroundColor: '#F44336' }
            ]
        },
        options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'top' } } }
    });
}

function renderDoughnutChart(canvasId, totalIngresos, totalEgresos) {
    if (charts[canvasId]) charts[canvasId].destroy();
    var ctx = document.getElementById(canvasId);
    if (!ctx) return;
    charts[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Ingresos', 'Egresos'],
            datasets: [{ data: [totalIngresos, totalEgresos], backgroundColor: ['#4CAF50', '#F44336'] }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });
}

function renderProyChart(canvasId, labels, metas, reales) {
    if (charts[canvasId]) charts[canvasId].destroy();
    var ctx = document.getElementById(canvasId);
    if (!ctx) return;
    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                { label: 'Meta', data: metas, backgroundColor: 'rgba(255,109,0,0.8)', borderColor: '#FF6D00', borderWidth: 1 },
                { label: 'Real', data: reales, backgroundColor: 'rgba(76,175,80,0.8)', borderColor: '#4CAF50', borderWidth: 1 }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'top' } },
            scales: { y: { beginAtZero: true } }
        }
    });
}

function downloadPdf(base64, filename) {
    var link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64;
    link.download = filename;
    link.click();
}

function downloadExcel(base64, filename) {
    var link = document.createElement('a');
    link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64;
    link.download = filename;
    link.click();
}

function downloadFile(base64, filename, mimeType) {
    var link = document.createElement('a');
    link.href = 'data:' + mimeType + ';base64,' + base64;
    link.download = filename;
    link.click();
}

async function captureAndDownload(elementId, filename) {
    const el = document.getElementById(elementId);
    if (!el) return;
    const canvas = await html2canvas(el, { backgroundColor: '#ffffff', scale: 2 });
    const link = document.createElement('a');
    link.href = canvas.toDataURL('image/png');
    link.download = filename;
    link.click();
}
