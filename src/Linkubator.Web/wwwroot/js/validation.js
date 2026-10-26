// validation.js
(function () {
    const form = document.getElementById('collection-form');

    // Mapeo: name del input → span correspondiente
    function getErrorSpan(input) {
        const name = input.name;
        return form.querySelector(`[data-valmsg-for="${name}"]`);
    }

    function showError(input, message) {
        const span = getErrorSpan(input);
        if (!span) return;
        span.textContent = message;
        span.classList.remove('field-validation-valid');
        span.classList.add('field-validation-error');
    }

    function clearError(input) {
        const span = getErrorSpan(input);
        if (!span) return;
        span.textContent = '';
        span.classList.remove('field-validation-error');
        span.classList.add('field-validation-valid');
    }

    function validateField(input) {
        const value = input.value.trim();
        const d = input.dataset;

        // Required
        if (d.valRequired) {
            if (!value) { showError(input, d.valRequired); return false; }
        }

        // MaxLength / StringLength
        if (d.valMaxlength && value.length > parseInt(d.valMaxlength, 10)) {
            showError(input, d.valMaxlength); return false;
        }
        if (d.valStringlength && value.length > parseInt(d.valStringlength, 10)) {
            showError(input, d.valStringlength); return false;
        }

        // MinLength
        if (d.valMinlength && value.length < parseInt(d.valMinlength, 10)) {
            showError(input, d.valMinlength); return false;
        }

        // Range
        if (d.valRange) {
            const min = parseFloat(d.valRangeMin);
            const max = parseFloat(d.valRangeMax);
            const num = parseFloat(value);
            if (isNaN(num) || num < min || num > max) {
                showError(input, d.valRange); return false;
            }
        }

        // Email
        if (d.valEmail && value) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!re.test(value)) { showError(input, d.valEmail); return false; }
        }

        // Pattern / Regex
        if (d.valPattern && value) {
            const re = new RegExp(d.valPattern);
            if (!re.test(value)) { showError(input, d.valPattern); return false; }
        }
        if (d.valRegex && value) {
            const re = new RegExp(d.valRegexPattern);
            if (!re.test(value)) { showError(input, d.valRegex); return false; }
        }

        // EqualTo (Compare)
        if (d.valEqualToOther) {
            const prefix = input.name.split('.')[0];
            const otherName = prefix + '.' + d.valEqualToOther.split('.')[1];
            const other = form.querySelector(`[name="${otherName}"]`);
            if (other && value !== other.value) {
                showError(input, d.valEqualTo); return false;
            }
        }

        clearError(input);
        return true;
    }

    function validateForm() {
        let valid = true;
        const fields = form.querySelectorAll('[data-val="true"]');
        fields.forEach(input => {
            if (!validateField(input)) valid = false;
            valid = false;
        });
        return valid;
    }

    // Eventos
    form.addEventListener('submit', (e) => {
        if (!validateForm()) e.preventDefault();
    });

    // Validar al perder el foco (blur)
    form.querySelectorAll('[data-val="true"]').forEach(input => {
        input.addEventListener('blur', () => validateField(input));
    });
})();   