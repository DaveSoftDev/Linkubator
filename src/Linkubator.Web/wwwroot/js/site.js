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

    const appLayout = document.querySelector('.app-layout');
    const mobileToggle = document.querySelector('.mobile-nav-toggle');
    const sidebar = document.querySelector('.app-sidebar');
    const sidebarCollapse = document.querySelector('.sidebar-collapse');

    if (appLayout && sidebar) {
        const closeMobileSidebar = () => {
            appLayout.classList.remove('is-open');
            if (mobileToggle) {
                mobileToggle.setAttribute('aria-expanded', 'false');
                mobileToggle.setAttribute('aria-label', 'Abrir menú');
            }
        };

        if (mobileToggle) {
            mobileToggle.addEventListener('click', () => {
                const isOpen = appLayout.classList.toggle('is-open');
                mobileToggle.setAttribute('aria-expanded', String(isOpen));
                mobileToggle.setAttribute('aria-label', isOpen ? 'Cerrar menú' : 'Abrir menú');
            });
        }

        if (sidebarCollapse) {
            sidebarCollapse.addEventListener('click', () => {
                const isCollapsed = appLayout.classList.toggle('sidebar-collapsed');
                sidebarCollapse.setAttribute('aria-label', isCollapsed ? 'Expandir menú lateral' : 'Colapsar menú lateral');
                sidebarCollapse.setAttribute('aria-expanded', String(!isCollapsed));
            });
        }

        sidebar.addEventListener('click', (event) => {
            const target = event.target;
            if (mobileToggle && target instanceof HTMLElement && target.closest('button')) {
                closeMobileSidebar();
            }
        });

        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                closeMobileSidebar();
            }
        });
    }

    const quickAddInput = document.querySelector('[data-quick-add-input]');
    const quickAddTrigger = document.querySelector('[data-quick-add-trigger]');
    const quickAddToggle = document.querySelector('[data-quick-add-toggle]');
    const quickAddMenu = document.querySelector('#quick-add-menu');
    const quickAddAdvanced = document.querySelector('[data-quick-add-advanced]');
    const quickAddStatus = document.querySelector('.quick-add__status');

    if (quickAddTrigger && quickAddInput) {
        const submitQuickAdd = () => {
            const url = quickAddInput.value.trim();

            if (!url) {
                if (quickAddStatus) {
                    quickAddStatus.textContent = 'Introduce una URL para continuar.';
                }
                quickAddInput.focus();
                return;
            }

            if (quickAddStatus) {
                quickAddStatus.textContent = 'Enlace preparado para guardarse.';
            }
            quickAddInput.value = '';
            quickAddInput.focus();
        };

        quickAddTrigger.addEventListener('click', submitQuickAdd);
        quickAddInput.addEventListener('keydown', (event) => {
            if (event.key === 'Enter') {
                event.preventDefault();
                submitQuickAdd();
            }
        });
    }

    if (quickAddToggle && quickAddMenu) {
        const closeQuickAddMenu = () => {
            quickAddMenu.hidden = true;
            quickAddToggle.setAttribute('aria-expanded', 'false');
        };

        quickAddToggle.addEventListener('click', (event) => {
            event.stopPropagation();
            const shouldOpen = quickAddMenu.hidden;
            quickAddMenu.hidden = !shouldOpen;
            quickAddToggle.setAttribute('aria-expanded', String(shouldOpen));
        });

        document.addEventListener('click', (event) => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            if (!quickAddMenu.contains(target) && !quickAddToggle.contains(target)) {
                closeQuickAddMenu();
            }
        });
    }

    if (quickAddAdvanced && quickAddStatus) {
        quickAddAdvanced.addEventListener('click', () => {
            quickAddStatus.textContent = 'Abriendo creación avanzada…';
            if (quickAddToggle && quickAddMenu) {
                quickAddMenu.hidden = true;
                quickAddToggle.setAttribute('aria-expanded', 'false');
            }
        });
    }
});
