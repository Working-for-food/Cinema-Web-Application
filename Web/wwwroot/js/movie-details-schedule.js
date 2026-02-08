
    (() => {
    const root = document;
    const dayButtons = Array.from(root.querySelectorAll('#scheduleDays [data-day]'));
    const panels = Array.from(root.querySelectorAll('.schedule-day-panel[data-day]'));

    if (dayButtons.length === 0 || panels.length === 0) return;

    const availableDays = new Set(panels.map(p => p.dataset.day));
    const todayIso = new Date().toISOString().slice(0, 10);

    const defaultDay = availableDays.has(todayIso)
        ? todayIso
        : (dayButtons[0].dataset.day || todayIso);

    function setActive(dayIso) {
        dayButtons.forEach(b => b.classList.toggle('active', b.dataset.day === dayIso));

        panels.forEach(p => {
            p.style.display = (p.dataset.day === dayIso) ? '' : 'none';
        });

        const cinemas = Array.from(root.querySelectorAll('.schedule-cinema'));
        cinemas.forEach(cinema => {
            const visiblePanels = Array.from(cinema.querySelectorAll('.schedule-day-panel'))
                .filter(p => p.dataset.day === dayIso);

            cinema.style.display = (visiblePanels.length > 0) ? '' : 'none';
        });
    }

    dayButtons.forEach(b => b.addEventListener('click', () => setActive(b.dataset.day)));

    setActive(defaultDay);
})();
