document.addEventListener('DOMContentLoaded', () => {
    const forms = document.querySelectorAll('form[data-native-validation="true"]');

    forms.forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                form.reportValidity();
            }

            form.classList.add('was-validated');
        });
    });
});
