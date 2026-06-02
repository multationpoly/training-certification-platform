window.reportingCharts = {
    renderBar: function (id, labels, data, title, axisLabel, horizontal) {
        const element = document.getElementById(id);
        if (!element || !window.Chart) return;
        new Chart(element, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: axisLabel,
                    data: data,
                    backgroundColor: '#2563eb'
                }]
            },
            options: {
                indexAxis: horizontal ? 'y' : 'x',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: { display: true, text: title },
                    tooltip: { enabled: true }
                },
                scales: {
                    x: { title: { display: true, text: horizontal ? axisLabel : 'Course' } },
                    y: { title: { display: true, text: horizontal ? 'Instructor' : axisLabel }, beginAtZero: true }
                }
            }
        });
    },
    renderStackedBar: function (id, labels, inProgress, eligible, issued) {
        const element = document.getElementById(id);
        if (!element || !window.Chart) return;
        new Chart(element, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    { label: 'In Progress', data: inProgress, backgroundColor: '#b7791f' },
                    { label: 'Eligible', data: eligible, backgroundColor: '#2563eb' },
                    { label: 'Issued', data: issued, backgroundColor: '#157347' }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: { display: true, text: 'Certification Status by Track' },
                    legend: { display: true },
                    tooltip: { enabled: true }
                },
                scales: {
                    x: { stacked: true, title: { display: true, text: 'Certification Track' } },
                    y: { stacked: true, beginAtZero: true, title: { display: true, text: 'Trainees' } }
                }
            }
        });
    },
    renderLine: function (id, labels, data) {
        const element = document.getElementById(id);
        if (!element || !window.Chart) return;
        new Chart(element, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Collected Revenue',
                    data: data,
                    borderColor: '#157347',
                    backgroundColor: 'rgba(21, 115, 71, .14)',
                    fill: true,
                    tension: .25
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: { display: true, text: 'Collected Revenue Over Time' },
                    tooltip: { enabled: true }
                },
                scales: {
                    x: { title: { display: true, text: 'Period' } },
                    y: { beginAtZero: true, title: { display: true, text: 'Amount' } }
                }
            }
        });
    }
};
