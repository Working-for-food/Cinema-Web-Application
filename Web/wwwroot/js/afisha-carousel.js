(function () {

    function updateHeaderHeightVar() {
        var header = document.querySelector('.afisha-header');
        if (!header) return;

        var h = Math.ceil(header.getBoundingClientRect().height);
        document.documentElement.style.setProperty('--afisha-header-h', h + 'px');
    }

    function setBlurBgFromCarousel(carouselRoot) {
        var bg = document.querySelector('.afisha-bg');
        if (!bg || !carouselRoot) return;

        var active = carouselRoot.querySelector('.afisha-car-item.is-active');
        if (!active) return;

        var img = active.querySelector('img.afisha-poster');
        if (!img) return;

        var src = img.getAttribute('src') || img.src;
        if (!src) return;

        bg.style.backgroundImage = 'url("' + src + '")';
    }

    function getActiveCarouselInActiveTab() {
        var panel = document.querySelector('.afisha-tab-panel.is-active');
        if (!panel) return null;

        return panel.querySelector('.afisha-carousel');
    }

    function setTab(tab) {
        var btnNow = document.querySelector('.afisha-tab-btn[data-tab="now"]');
        var btnSoon = document.querySelector('.afisha-tab-btn[data-tab="soon"]');
        var panelNow = document.getElementById('tab-now');
        var panelSoon = document.getElementById('tab-soon');

        var isNow = tab === 'now';

        btnNow.classList.toggle('is-active', isNow);
        btnSoon.classList.toggle('is-active', !isNow);

        btnNow.setAttribute('aria-selected', isNow ? 'true' : 'false');
        btnSoon.setAttribute('aria-selected', !isNow ? 'true' : 'false');

        panelNow.classList.toggle('is-active', isNow);
        panelSoon.classList.toggle('is-active', !isNow);

        try { localStorage.setItem('afishaTab', tab); } catch (e) { }

        var activeCarousel = getActiveCarouselInActiveTab();
        setBlurBgFromCarousel(activeCarousel);
    }

    function setupTabs() {
        var buttons = document.querySelectorAll('.afisha-tab-btn');
        buttons.forEach(function (b) {
            b.addEventListener('click', function () {
                setTab(b.getAttribute('data-tab'));
            });
        });

        var saved = null;
        try { saved = localStorage.getItem('afishaTab'); } catch (e) { }

        setTab(saved === 'soon' ? 'soon' : 'now');
    }

    function render(root, items, activeIndex) {
        var n = items.length;

        items.forEach(function (el, i) {
            el.classList.remove('is-prev', 'is-active', 'is-next', 'is-hidden');

            if (n <= 0) return;

            var prev = (activeIndex - 1 + n) % n;
            var next = (activeIndex + 1) % n;

            if (i === activeIndex) el.classList.add('is-active');
            else if (i === prev) el.classList.add('is-prev');
            else if (i === next) el.classList.add('is-next');
            else el.classList.add('is-hidden');
        });

        setBlurBgFromCarousel(root);
    }

    function setupCarousel(root) {
        var items = Array.prototype.slice.call(root.querySelectorAll('.afisha-car-item'));
        var btnPrev = root.querySelector('.afisha-car-btn.prev');
        var btnNext = root.querySelector('.afisha-car-btn.next');

        if (!items.length) {
            if (btnPrev) btnPrev.disabled = true;
            if (btnNext) btnNext.disabled = true;
            return;
        }

        var activeIndex = 0;
        render(root, items, activeIndex);

        function prev() {
            activeIndex = (activeIndex - 1 + items.length) % items.length;
            render(root, items, activeIndex);
        }

        function next() {
            activeIndex = (activeIndex + 1) % items.length;
            render(root, items, activeIndex);
        }

        btnPrev.addEventListener('click', prev);
        btnNext.addEventListener('click', next);

        items.forEach(function (el) {
            el.addEventListener('click', function () {
                var idx = parseInt(el.getAttribute('data-index') || '0', 10);
                if (!Number.isNaN(idx)) {
                    activeIndex = idx;
                    render(root, items, activeIndex);
                }
            });
        });

        root.setAttribute('tabindex', '0');
        root.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowLeft') prev();
            if (e.key === 'ArrowRight') next();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        updateHeaderHeightVar();
        window.addEventListener('resize', updateHeaderHeightVar);

        setupTabs();

        var carousels = document.querySelectorAll('.afisha-carousel');
        carousels.forEach(function (c) { setupCarousel(c); });

        // стартове підхоплення фону
        var activeCarousel = getActiveCarouselInActiveTab();
        setBlurBgFromCarousel(activeCarousel);
    });

})();
