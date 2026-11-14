// Форма записи: загрузка свободных слотов на выбранную дату.
(function () {
    "use strict";

    document.querySelectorAll("[data-booking]").forEach(function (form) {
        var url = form.getAttribute("data-slots-url");
        var dateInput = form.querySelector("[data-date]");
        var timeInput = form.querySelector("[data-time]");
        var box = form.querySelector("[data-slots]");
        if (!dateInput || !timeInput || !box) return;

        var message = function (text) {
            box.innerHTML = "";
            var span = document.createElement("span");
            span.className = "muted small";
            span.textContent = text;
            box.appendChild(span);
        };

        var load = function () {
            var date = dateInput.value;
            if (!date) { message("Сначала выберите дату"); return; }
            message("Загрузка…");

            fetch(url + "?date=" + encodeURIComponent(date), { headers: { "Accept": "application/json" } })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.error) { message(data.error); timeInput.value = ""; return; }
                    var slots = data.slots || [];
                    if (!slots.some(function (s) { return s.available; })) {
                        message("На эту дату свободного времени нет — выберите другой день.");
                        timeInput.value = "";
                        return;
                    }
                    box.innerHTML = "";
                    var selectedStillFree = false;
                    slots.forEach(function (s) {
                        var b = document.createElement("button");
                        b.type = "button";
                        b.className = "slot";
                        b.textContent = s.time;
                        b.disabled = !s.available;
                        b.title = s.available ? "Свободно постов: " + s.free : "Занято";
                        if (s.available && s.time === timeInput.value) {
                            b.classList.add("is-selected");
                            selectedStillFree = true;
                        }
                        b.addEventListener("click", function () {
                            box.querySelectorAll(".slot").forEach(function (x) { x.classList.remove("is-selected"); });
                            b.classList.add("is-selected");
                            timeInput.value = s.time;
                        });
                        box.appendChild(b);
                    });
                    if (!selectedStillFree) timeInput.value = "";
                })
                .catch(function () { message("Не удалось загрузить расписание. Попробуйте ещё раз."); });
        };

        dateInput.addEventListener("change", load);
        if (dateInput.value) load();

        form.addEventListener("submit", function (e) {
            if (!timeInput.value) {
                e.preventDefault();
                message(dateInput.value ? "Выберите время визита" : "Выберите дату и время визита");
                box.scrollIntoView({ block: "center" });
            }
        });
    });
})();
