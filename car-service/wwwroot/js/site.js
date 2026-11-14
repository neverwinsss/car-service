// Общие скрипты сайта: мобильное меню и баннер-слайдер.
(function () {
    "use strict";

    // ---- Мобильное меню ----
    var toggle = document.querySelector("[data-nav-toggle]");
    var nav = document.querySelector("[data-nav]");
    if (toggle && nav) {
        toggle.addEventListener("click", function (e) {
            e.stopPropagation();
            var open = nav.classList.toggle("is-open");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
        });
        document.addEventListener("click", function (e) {
            if (nav.classList.contains("is-open") && !nav.contains(e.target)) {
                nav.classList.remove("is-open");
                toggle.setAttribute("aria-expanded", "false");
            }
        });
    }

    // ---- Слайдер ----
    var slider = document.querySelector("[data-slider]");
    if (slider) {
        var slides = slider.querySelectorAll("[data-slide]");
        var dots = slider.querySelectorAll("[data-dot]");
        var current = 0;
        var timer = null;
        var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        var show = function (i) {
            if (!slides.length) return;
            current = (i + slides.length) % slides.length;
            slides.forEach(function (s, k) { s.classList.toggle("is-active", k === current); });
            dots.forEach(function (d, k) { d.classList.toggle("is-active", k === current); });
        };
        var restart = function () {
            if (timer) clearInterval(timer);
            if (!reduceMotion && slides.length > 1) timer = setInterval(function () { show(current + 1); }, 7000);
        };

        var prev = slider.querySelector("[data-prev]");
        var next = slider.querySelector("[data-next]");
        if (prev) prev.addEventListener("click", function () { show(current - 1); restart(); });
        if (next) next.addEventListener("click", function () { show(current + 1); restart(); });
        dots.forEach(function (d) {
            d.addEventListener("click", function () { show(parseInt(d.getAttribute("data-dot"), 10)); restart(); });
        });
        slider.addEventListener("mouseenter", function () { if (timer) clearInterval(timer); });
        slider.addEventListener("mouseleave", restart);
        restart();
    }
})();
