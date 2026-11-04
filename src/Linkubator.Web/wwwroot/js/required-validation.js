document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('form[data-required-validation]').forEach((form) => {
        const summaryContainer = form.querySelector('[data-validation-summary]');
        const fields = Array.from(form.querySelectorAll('[required][data-required-message]')).map((field) => {
            const error = form.querySelector(`[data-valmsg-for="${field.name}"]`);
            return {
                field,
                error,
                serverMessage: error.textContent,
                serverErrorClass: error.className,
                serverInvalid: field.classList.contains('input-validation-error'),
                serverAriaInvalid: field.getAttribute('aria-invalid'),
            };
        });
        let attempted = false;
        let summary = summaryContainer?.querySelector('.form-summary');

        const updateSummary = (invalidFields) => {
            if (!summaryContainer) {
                return;
            }
            if (!summary && invalidFields.length > 0) {
                summary = document.createElement('div');
                summary.className = 'form-summary';
                summary.setAttribute('role', 'alert');
                summary.setAttribute('aria-live', 'assertive');
                const title = document.createElement('h2');
                title.id = `${form.id}-error-summary-title`;
                title.textContent = summaryContainer.dataset.validationSummaryTitle;
                summary.setAttribute('aria-labelledby', title.id);
                const list = document.createElement('ul');
                list.className = 'form-summary__list';
                summary.append(title, list);
                summaryContainer.prepend(summary);
            }
            if (!summary) {
                return;
            }

            const list = summary.querySelector('.form-summary__list');
            list.querySelectorAll('[data-client-validation]').forEach((item) => item.remove());
            Array.from(list.children).forEach((item) => {
                const target = item.querySelector('a')?.getAttribute('href');
                item.hidden = invalidFields.some(({ field }) => target === `#${field.id}`);
            });
            invalidFields.forEach(({ field }) => {
                const item = document.createElement('li');
                item.dataset.clientValidation = 'true';
                const link = document.createElement('a');
                link.className = 'form-summary__link';
                link.href = `#${field.id}`;
                link.textContent = field.dataset.requiredMessage;
                item.append(link);
                list.append(item);
            });
            summary.hidden = !Array.from(list.children).some((item) => !item.hidden);
        };

        const validate = () => {
            const invalidFields = fields.filter(({ field }) => !field.disabled && field.value.trim().length === 0);
            fields.forEach((state) => {
                const { field, error } = state;
                const invalid = invalidFields.includes(state);
                error.textContent = invalid ? field.dataset.requiredMessage : state.serverMessage;
                error.className = state.serverErrorClass;
                field.classList.toggle('input-validation-error', invalid || state.serverInvalid);
                if (invalid) {
                    error.classList.remove('field-validation-valid');
                    error.classList.add('field-validation-error');
                    field.setAttribute('aria-invalid', 'true');
                } else if (state.serverAriaInvalid === null) {
                    field.removeAttribute('aria-invalid');
                } else {
                    field.setAttribute('aria-invalid', state.serverAriaInvalid);
                }
            });
            updateSummary(invalidFields);
            return invalidFields;
        };

        form.addEventListener('submit', (event) => {
            if (event.submitter?.formNoValidate) {
                return;
            }
            attempted = true;
            const invalidFields = validate();
            if (invalidFields.length > 0) {
                event.preventDefault();
                invalidFields[0].field.focus();
            }
        });
        fields.forEach(({ field }) => {
            const revalidate = () => {
                if (attempted) {
                    validate();
                }
            };
            field.addEventListener('input', revalidate);
            field.addEventListener('change', revalidate);
        });
        form.noValidate = true;
    });
});
