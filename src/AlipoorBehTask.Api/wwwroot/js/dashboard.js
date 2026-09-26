(() => {
    const serviceNames = { Wheelchair: "ویلچر", HousingDepositLoan: "وام ودیعه مسکن", Pension: "مستمری" };
    const statusNames = { Pending: "در انتظار بررسی", Approved: "تأییدشده", Rejected: "ردشده", Completed: "تکمیل‌شده" };
    const statusClasses = { Pending: "status-pending", Approved: "status-approved", Rejected: "status-rejected", Completed: "status-completed" };
    const factorNames = {
        age_over_70: "سن بالای ۷۰ سال",
        severe_disability: "معلولیت شدید",
        below_poverty_threshold: "درآمد زیر آستانه حمایتی"
    };
    const statuses = ["Pending", "Approved", "Rejected", "Completed"];
    const services = ["Wheelchair", "HousingDepositLoan", "Pension"];
    const state = {
        page: 1, pageSize: 5, total: 0, items: [], summary: [], view: "queue",
        sortBy: "priority", sortDirection: "desc", search: "",
        reportStatus: null,
        beneficiaries: { page: 1, pageSize: 5, total: 0, items: [], sortBy: "nationalId", sortDirection: "asc", search: "" }
    };
    const queueBody = document.querySelector("#queue-body");
    const queueRegion = document.querySelector("#queue-region");
    const filters = document.querySelector("#queue-filters");
    const beneficiaryBody = document.querySelector("#beneficiary-body");
    const beneficiaryRegion = document.querySelector("#beneficiary-region");
    const beneficiaryFilters = document.querySelector("#beneficiary-filters");
    const notice = document.querySelector("#notice");
    const nf = new Intl.NumberFormat("fa-IR");

    document.querySelector("#today-date").textContent = new Intl.DateTimeFormat("fa-IR", { weekday: "long", day: "numeric", month: "long", year: "numeric" }).format(new Date());

    const escapeHtml = (value) => String(value ?? "").replace(/[&<>"']/g, (character) => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    })[character]);

    async function api(url, options = {}) {
        const response = await fetch(url, {
            ...options,
            headers: { Accept: "application/json", ...(options.body ? { "Content-Type": "application/json" } : {}), ...options.headers }
        });
        if (!response.ok) {
            let problem;
            try { problem = await response.json(); } catch { problem = null; }
            const validation = problem?.errors ? Object.values(problem.errors).flat().join(" ") : "";
            throw new Error(problem?.detail || validation || problem?.title || `خطا در ارتباط با سامانه (${response.status})`);
        }
        return response.status === 204 ? null : response.json();
    }

    function announce(message, isError = false) {
        notice.textContent = message;
        notice.classList.toggle("is-error", isError);
        notice.hidden = false;
        window.clearTimeout(announce.timeout);
        announce.timeout = window.setTimeout(() => { notice.hidden = true; }, 6500);
    }

    function renderReport() {
        const grouped = Object.fromEntries(services.map((service) => [service, Object.fromEntries(statuses.map((status) => [status, 0]))]));
        for (const row of state.summary) {
            if (grouped[row.serviceType] && statuses.includes(row.status)) grouped[row.serviceType][row.status] = row.count;
        }
        const totals = Object.fromEntries(statuses.map((status) => [status, 0]));
        for (const service of services) {
            for (const status of statuses) totals[status] += grouped[service][status];
        }
        const grandTotal = Object.values(totals).reduce((sum, count) => sum + count, 0);
        const selectedStatus = state.reportStatus;
        const reportTitle = document.querySelector("#report-breakdown-title");
        const clearDrilldown = document.querySelector("#clear-report-drilldown");
        if (selectedStatus) {
            const statusTotal = totals[selectedStatus];
            reportTitle.textContent = `توزیع «${statusNames[selectedStatus]}» بر اساس نوع خدمت`;
            clearDrilldown.hidden = false;
            document.querySelector(".report-table thead").innerHTML = "<tr><th scope=\"col\">نوع خدمت</th><th scope=\"col\">تعداد</th><th scope=\"col\">سهم از این وضعیت</th></tr>";
            document.querySelector("#report-body").innerHTML = services.map((service) => {
                const count = grouped[service][selectedStatus];
                const share = statusTotal === 0 ? 0 : Math.round(count / statusTotal * 100);
                return `<tr><td><strong>${serviceNames[service]}</strong></td><td>${nf.format(count)}</td><td>${nf.format(share)}٪</td></tr>`;
            }).join("");
            document.querySelector("#report-foot").innerHTML = `<tr><th scope=\"row\">مجموع ${statusNames[selectedStatus]}</th><th>${nf.format(statusTotal)}</th><th>${statusTotal ? "۱۰۰٪" : "۰٪"}</th></tr>`;
        } else {
            reportTitle.textContent = "خدمت به تفکیک وضعیت";
            clearDrilldown.hidden = true;
            document.querySelector(".report-table thead").innerHTML = "<tr><th scope=\"col\">نوع خدمت</th><th scope=\"col\">در انتظار</th><th scope=\"col\">تأییدشده</th><th scope=\"col\">ردشده</th><th scope=\"col\">تکمیل‌شده</th><th scope=\"col\">جمع خدمت</th></tr>";
            document.querySelector("#report-body").innerHTML = services.map((service) => {
                const cells = statuses.map((status) => `<td>${nf.format(grouped[service][status])}</td>`).join("");
                const total = statuses.reduce((sum, status) => sum + grouped[service][status], 0);
                return `<tr><td><strong>${serviceNames[service]}</strong></td>${cells}<td><strong>${nf.format(total)}</strong></td></tr>`;
            }).join("");
            document.querySelector("#report-foot").innerHTML = `<tr><th scope=\"row\">مجموع کل</th>${statuses.map((status) => `<th>${nf.format(totals[status])}</th>`).join("")}<th>${nf.format(grandTotal)}</th></tr>`;
        }
        const maxCount = Math.max(1, ...Object.values(totals));
        document.querySelector("#status-chart").innerHTML = statuses.map((status) => {
            const count = totals[status];
            const height = count === 0 ? 3 : Math.max(8, Math.round(count / maxCount * 100));
            const selected = selectedStatus === status;
            return `<button type="button" class="chart-column${selected ? " is-selected" : ""}" data-report-status="${status}" aria-pressed="${selected}" aria-label="${statusNames[status]}، ${nf.format(count)} پرونده. برای نمایش توزیع بر اساس نوع خدمت انتخاب کنید."><strong class="chart-value">${nf.format(count)}</strong><span class="chart-track"><span class="chart-bar chart-bar-${status.toLowerCase()}" style="height:${height}%" aria-hidden="true"></span></span><span class="chart-label">${statusNames[status]}</span></button>`;
        }).join("");
    }

    async function loadSummary() {
        try {
            const summary = await api("/api/reports/requests-summary");
            state.summary = summary.items;
            renderReport();
            document.querySelector("#connection-label").textContent = "متصل و آماده";
            document.querySelector(".connection-dot").classList.add("is-online");
        } catch (error) {
            state.summary = [];
            document.querySelector("#connection-label").textContent = "عدم دسترسی به سامانه";
            document.querySelector(".connection-dot").classList.add("is-offline");
            announce(`دریافت آمار ممکن نشد: ${error.message}`, true);
            document.querySelector("#report-body").innerHTML = `<tr><td colspan="6"><div class="table-state is-error">دریافت گزارش ناموفق بود. ${escapeHtml(error.message)} <button class="button button-quiet" type="button" id="retry-reports">تلاش دوباره</button></div></td></tr>`;
            document.querySelector("#report-foot").innerHTML = "";
            document.querySelector("#retry-reports").addEventListener("click", loadSummary);
        }
    }

    function formatDate(dateValue) {
        const date = new Date(`${dateValue}T12:00:00`);
        return Number.isNaN(date.getTime()) ? escapeHtml(dateValue) : new Intl.DateTimeFormat("fa-IR", { year: "numeric", month: "short", day: "numeric" }).format(date);
    }

    function validTransitions(status) {
        if (status === "Pending") return ["Approved", "Rejected"];
        if (status === "Approved") return ["Completed"];
        return [];
    }

    function factorLabel(factor) {
        if (factorNames[factor.code]) return factorNames[factor.code];
        const dependents = factor.code === "dependents" && factor.description.match(/^(\d+) dependent/);
        if (dependents) return `${nf.format(Number(dependents[1]))} فرد تحت تکفل`;
        const waitingMonths = factor.code === "waiting_months" && factor.description.match(/^(\d+) completed waiting month/);
        if (waitingMonths) return `${nf.format(Number(waitingMonths[1]))} ماه انتظار`;
        return escapeHtml(factor.description);
    }

    const maritalNames = { Single: "مجرد", Married: "متأهل", Divorced: "مطلقه", Widowed: "همسر فوت‌شده" };
    const disabilityNames = { None: "ندارد", Mild: "خفیف", Moderate: "متوسط", Severe: "شدید" };

    function updatePagination(prefix, page, pageSize, total) {
        const idPrefix = prefix ? `${prefix}-` : "";
        const lastPage = Math.max(1, Math.ceil(total / pageSize));
        document.querySelector(`#${idPrefix}page-caption`).textContent = `صفحه ${nf.format(page)} از ${nf.format(lastPage)}`;
        document.querySelector(`#${idPrefix}page-number`).textContent = nf.format(page);
        document.querySelector(`#${idPrefix}previous${prefix ? "" : "-page"}`).disabled = page <= 1;
        document.querySelector(`#${idPrefix}next${prefix ? "" : "-page"}`).disabled = page >= lastPage;
    }

    function renderBeneficiaries() {
        const table = state.beneficiaries;
        document.querySelector("#beneficiary-total").textContent = `${nf.format(table.total)} مددجو`;
        updatePagination("beneficiary", table.page, table.pageSize, table.total);
        if (!table.items.length) {
            beneficiaryBody.innerHTML = `<tr><td colspan="6"><div class="table-state is-empty"><strong>مددجویی در این فهرست نیست</strong><span>جست‌وجو را تغییر دهید یا مددجوی تازه‌ای ثبت کنید.</span></div></td></tr>`;
            return;
        }
        beneficiaryBody.innerHTML = table.items.map((item) => `<tr>
            <td dir="ltr"><strong>${escapeHtml(item.nationalId)}</strong></td>
            <td>${nf.format(item.age)}</td>
            <td>${maritalNames[item.maritalStatus] || escapeHtml(item.maritalStatus)}</td>
            <td>${nf.format(item.dependentCount)}</td>
            <td>${disabilityNames[item.disabilityType] || escapeHtml(item.disabilityType)}</td>
            <td>${nf.format(item.monthlyIncome)}</td>
        </tr>`).join("");
    }

    async function loadBeneficiaryPage() {
        const table = state.beneficiaries;
        beneficiaryRegion.setAttribute("aria-busy", "true");
        beneficiaryBody.innerHTML = `<tr><td colspan="6"><div class="table-state"><span class="spinner" aria-hidden="true"></span>در حال بارگذاری مددجویان…</div></td></tr>`;
        const query = new URLSearchParams({
            page: String(table.page),
            pageSize: String(table.pageSize),
            sortBy: table.sortBy,
            sortDirection: table.sortDirection
        });
        if (table.search) query.set("search", table.search);
        try {
            const result = await api(`/api/beneficiaries?${query}`);
            table.items = result.items;
            table.total = result.totalCount;
            renderBeneficiaries();
        } catch (error) {
            table.items = [];
            beneficiaryBody.innerHTML = `<tr><td colspan="6"><div class="table-state is-error">دریافت فهرست مددجویان ناموفق بود. ${escapeHtml(error.message)} <button class="button button-quiet" type="button" id="retry-beneficiaries">تلاش دوباره</button></div></td></tr>`;
            document.querySelector("#retry-beneficiaries").addEventListener("click", loadBeneficiaryPage);
        } finally {
            beneficiaryRegion.setAttribute("aria-busy", "false");
        }
    }

    function renderQueue() {
        document.querySelector("#queue-total").textContent = `${nf.format(state.total)} درخواست`;
        updatePagination("", state.page, state.pageSize, state.total);
        if (!state.items.length) {
            queueBody.innerHTML = `<tr><td colspan="7"><div class="table-state is-empty"><strong>درخواستی در این نما نیست</strong><span>فیلترها را تغییر دهید یا یک درخواست تازه ثبت کنید.</span></div></td></tr>`;
            return;
        }
        queueBody.innerHTML = state.items.map((item, index) => {
            const transitions = validTransitions(item.status);
            const options = transitions.map((status) => `<option value="${status}">${statusNames[status]}</option>`).join("");
            const factors = (item.priorityBreakdown || []).map((factor) => `<span class="factor-tag">${factorLabel(factor)} <strong dir="ltr">+${nf.format(factor.points)}</strong></span>`).join("");
            const scoreDetails = `<details class="factor-details"><summary>ریز امتیاز</summary><div class="factor-list">${factors}</div></details>`;
            const action = transitions.length
                ? `<div class="row-actions"><label class="visually-hidden" for="status-${item.id}">تغییر وضعیت برای مددجوی ${escapeHtml(item.beneficiaryNationalId)}</label><select id="status-${item.id}">${options}</select><button type="button" data-status-id="${item.id}" aria-label="ثبت وضعیت جدید برای مددجوی ${escapeHtml(item.beneficiaryNationalId)}">ثبت</button></div>`
                : `<span aria-label="اقدام دیگری وجود ندارد">—</span>`;
            return `<tr style="animation-delay:${Math.min(index * 24, 240)}ms"><td dir="ltr">${nf.format((state.page - 1) * state.pageSize + index + 1)}</td>
                <td><div class="score-cell"><strong class="score-value" dir="ltr">${nf.format(item.priorityScore)}</strong><span class="score-unit">امتیاز</span></div>${scoreDetails}</td>
                <td><div class="beneficiary-cell"><strong dir="ltr">${escapeHtml(item.beneficiaryNationalId)}</strong><small>شناسه پرونده: ${escapeHtml(String(item.id).slice(0, 8))}</small></div></td>
                <td><span class="service-name">${serviceNames[item.serviceType] || escapeHtml(item.serviceType)}</span><details class="description-detail"><summary>شرح درخواست</summary>${escapeHtml(item.description)}</details></td>
                <td class="date-cell">${formatDate(item.registrationDate)}</td>
                <td><span class="status-pill ${statusClasses[item.status] || ""}">${statusNames[item.status] || escapeHtml(item.status)}</span></td><td>${action}</td></tr>`;
        }).join("");
    }

    async function loadQueue() {
        queueRegion.setAttribute("aria-busy", "true");
        queueBody.innerHTML = `<tr><td colspan="7"><div class="table-state"><span class="spinner" aria-hidden="true"></span>در حال بارگذاری صف…</div></td></tr>`;
        const query = new URLSearchParams({
            page: String(state.page),
            pageSize: String(state.pageSize),
            sortBy: state.sortBy,
            sortDirection: state.sortDirection
        });
        const form = new FormData(filters);
        if (form.get("serviceType")) query.set("serviceType", form.get("serviceType"));
        if (form.get("status")) query.set("status", form.get("status"));
        if (state.search) query.set("search", state.search);
        try {
            const result = await api(`/api/service-requests/queue?${query}`);
            state.items = result.items;
            state.total = result.totalCount;
            renderQueue();
        } catch (error) {
            state.items = [];
            queueBody.innerHTML = `<tr><td colspan="7"><div class="table-state is-error">دریافت صف با خطا روبه‌رو شد. ${escapeHtml(error.message)} <button class="button button-quiet" type="button" id="retry-queue">تلاش دوباره</button></div></td></tr>`;
            document.querySelector("#retry-queue").addEventListener("click", loadQueue);
        } finally {
            queueRegion.setAttribute("aria-busy", "false");
        }
    }

    async function loadBeneficiaryOptions() {
        const select = document.querySelector("#request-form [name=beneficiaryId]");
        const hint = document.querySelector("[data-beneficiary-hint]");
        select.disabled = true;
        select.innerHTML = `<option value="">در حال دریافت فهرست…</option>`;
        try {
            const result = await api("/api/beneficiaries?page=1&pageSize=100&sortBy=nationalId&sortDirection=asc");
            select.innerHTML = `<option value="">انتخاب مددجو</option>` + result.items.map((beneficiary) => `<option value="${beneficiary.id}">${escapeHtml(beneficiary.nationalId)}</option>`).join("");
            hint.textContent = result.totalCount ? `${nf.format(result.totalCount)} مددجو ثبت شده` : "هنوز مددجویی ثبت نشده است؛ ابتدا مددجو را ثبت کنید.";
            select.disabled = result.items.length === 0;
        } catch (error) {
            select.innerHTML = `<option value="">دریافت فهرست ناموفق بود</option>`;
            hint.textContent = error.message;
        }
    }

    function setView(view) {
        state.view = view;
        document.querySelector("#queue-view").hidden = view !== "queue";
        document.querySelector("#reports-view").hidden = view !== "reports";
        document.querySelectorAll(".nav-link").forEach((button) => {
            const active = button.dataset.view === view;
            button.classList.toggle("is-active", active);
            if (active) button.setAttribute("aria-current", "page");
            else button.removeAttribute("aria-current");
        });
    }

    function clearValidation(form) {
        form.querySelectorAll("[aria-invalid]").forEach((field) => field.removeAttribute("aria-invalid"));
        const error = form.querySelector("[data-form-error]");
        error.textContent = "";
        error.hidden = true;
    }

    function validateForm(form) {
        let valid = true;
        for (const field of form.querySelectorAll("input, select, textarea")) {
            const invalid = !field.checkValidity();
            field.setAttribute("aria-invalid", String(invalid));
            if (invalid) valid = false;
        }
        if (!valid) {
            form.querySelector(":invalid")?.focus();
            const error = form.querySelector("[data-form-error]");
            error.textContent = "لطفاً موارد مشخص‌شده را بررسی و تکمیل کنید.";
            error.hidden = false;
        }
        return valid;
    }

    async function submitForm(form, path, getBody, successMessage, afterSuccess) {
        clearValidation(form);
        if (!validateForm(form)) return;
        const submit = form.querySelector("[type=submit]");
        submit.disabled = true;
        const originalText = submit.textContent;
        submit.textContent = "در حال ثبت…";
        try {
            await api(path, { method: "POST", body: JSON.stringify(getBody(new FormData(form))) });
            form.reset();
            form.closest("dialog").close();
            announce(successMessage);
            await afterSuccess();
        } catch (error) {
            const errorNode = form.querySelector("[data-form-error]");
            errorNode.textContent = error.message;
            errorNode.hidden = false;
        } finally {
            submit.disabled = false;
            submit.textContent = originalText;
        }
    }

    filters.addEventListener("change", (event) => {
        const form = new FormData(filters);
        state.pageSize = Number(form.get("pageSize"));
        state.sortBy = form.get("sortBy");
        state.sortDirection = form.get("sortDirection");
        state.page = 1;
        loadQueue();
    });
    filters.querySelector("[name=search]").addEventListener("input", () => {
        window.clearTimeout(filters.searchTimer);
        filters.searchTimer = window.setTimeout(() => {
            state.search = filters.querySelector("[name=search]").value.trim();
            state.page = 1;
            loadQueue();
        }, 250);
    });
    beneficiaryFilters.addEventListener("change", () => {
        const form = new FormData(beneficiaryFilters);
        state.beneficiaries.pageSize = Number(form.get("pageSize"));
        state.beneficiaries.page = 1;
        loadBeneficiaryPage();
    });
    beneficiaryFilters.querySelector("[name=search]").addEventListener("input", () => {
        window.clearTimeout(beneficiaryFilters.searchTimer);
        beneficiaryFilters.searchTimer = window.setTimeout(() => {
            state.beneficiaries.search = beneficiaryFilters.querySelector("[name=search]").value.trim();
            state.beneficiaries.page = 1;
            loadBeneficiaryPage();
        }, 250);
    });
    document.querySelector("#previous-page").addEventListener("click", () => { state.page--; loadQueue(); });
    document.querySelector("#next-page").addEventListener("click", () => { state.page++; loadQueue(); });
    document.querySelector("#beneficiary-previous").addEventListener("click", () => { state.beneficiaries.page--; loadBeneficiaryPage(); });
    document.querySelector("#beneficiary-next").addEventListener("click", () => { state.beneficiaries.page++; loadBeneficiaryPage(); });
    document.querySelectorAll("[data-table-sort]").forEach((button) => button.addEventListener("click", () => {
        const table = button.dataset.tableSort === "beneficiaries" ? state.beneficiaries : state;
        const key = button.dataset.sortKey;
        table.sortDirection = table.sortBy === key && table.sortDirection === "asc" ? "desc" : "asc";
        table.sortBy = key;
        if (button.dataset.tableSort === "beneficiaries") {
            table.page = 1;
            loadBeneficiaryPage();
        } else {
            filters.querySelector("[name=sortBy]").value = key;
            filters.querySelector("[name=sortDirection]").value = table.sortDirection;
            table.page = 1;
            loadQueue();
        }
        document.querySelectorAll(`[data-table-sort="${button.dataset.tableSort}"]`).forEach((item) => {
            const active = item.dataset.sortKey === key;
            item.classList.toggle("is-sorted", active);
            item.querySelector("span").textContent = active ? (table.sortDirection === "asc" ? "↑" : "↓") : "↕";
        });
    }));
    document.querySelector("#refresh-queue").addEventListener("click", loadQueue);
    document.querySelector("#refresh-reports").addEventListener("click", loadSummary);
    document.querySelector("#status-chart").addEventListener("click", (event) => {
        const bar = event.target.closest("[data-report-status]");
        if (!bar) return;
        state.reportStatus = bar.dataset.reportStatus;
        renderReport();
    });
    document.querySelector("#clear-report-drilldown").addEventListener("click", () => {
        state.reportStatus = null;
        renderReport();
    });
    document.querySelectorAll(".nav-link").forEach((button) => button.addEventListener("click", () => setView(button.dataset.view)));

    queueBody.addEventListener("click", async (event) => {
        const button = event.target.closest("[data-status-id]");
        if (!button) return;
        const select = document.querySelector(`#status-${CSS.escape(button.dataset.statusId)}`);
        button.disabled = true;
        try {
            await api(`/api/service-requests/${encodeURIComponent(button.dataset.statusId)}/status`, { method: "PATCH", body: JSON.stringify({ status: select.value }) });
            announce(`وضعیت پرونده به «${statusNames[select.value]}» تغییر کرد.`);
            await Promise.all([loadQueue(), loadSummary()]);
        } catch (error) {
            announce(`تغییر وضعیت انجام نشد: ${error.message}`, true);
            button.disabled = false;
        }
    });

    document.querySelectorAll("[data-open]").forEach((button) => button.addEventListener("click", async () => {
        const dialog = document.getElementById(button.dataset.open);
        if (dialog.id === "request-dialog") await loadBeneficiaryOptions();
        dialog.showModal();
        dialog.querySelector("input, select, textarea")?.focus();
    }));
    document.querySelectorAll("[data-close]").forEach((button) => button.addEventListener("click", () => button.closest("dialog").close()));
    document.querySelectorAll("dialog").forEach((dialog) => dialog.addEventListener("click", (event) => { if (event.target === dialog) dialog.close(); }));

    const beneficiaryForm = document.querySelector("#beneficiary-form");
    beneficiaryForm.addEventListener("input", () => clearValidation(beneficiaryForm));
    beneficiaryForm.addEventListener("change", () => clearValidation(beneficiaryForm));
    beneficiaryForm.addEventListener("submit", (event) => {
        event.preventDefault();
        submitForm(beneficiaryForm, "/api/beneficiaries", (form) => ({
            nationalId: form.get("nationalId"), age: Number(form.get("age")), maritalStatus: form.get("maritalStatus"),
            dependentCount: Number(form.get("dependentCount")), disabilityType: form.get("disabilityType"), monthlyIncome: Number(form.get("monthlyIncome"))
        }), "مددجو با موفقیت ثبت شد.", async () => { state.beneficiaries.page = 1; await Promise.all([loadBeneficiaryPage(), loadSummary()]); });
    });

    const requestForm = document.querySelector("#request-form");
    requestForm.addEventListener("input", () => clearValidation(requestForm));
    requestForm.addEventListener("change", () => clearValidation(requestForm));
    requestForm.addEventListener("submit", (event) => {
        event.preventDefault();
        submitForm(requestForm, "/api/service-requests", (form) => ({
            beneficiaryId: form.get("beneficiaryId"), serviceType: form.get("serviceType"), description: form.get("description")
        }), "درخواست ثبت شد و به صف اولویت افزوده شد.", async () => {
            state.page = 1;
            await Promise.all([loadQueue(), loadSummary()]);
        });
    });

    loadQueue();
    loadBeneficiaryPage();
    loadSummary();
})();