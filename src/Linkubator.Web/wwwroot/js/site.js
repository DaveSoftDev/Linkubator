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
        const mobileViewport = window.matchMedia('(max-width: 900px)');
        const main = appLayout.querySelector('.app-main');
        const rememberSidebarState = () => {
            const isCollapsed = appLayout.classList.contains('sidebar-collapsed');
            const secure = window.location.protocol === 'https:' ? '; Secure' : '';
            document.cookie = `sidebar-collapsed=${isCollapsed}; Path=/app; SameSite=Lax${secure}`;
        };

        const syncSidebar = () => {
            const isOpen = mobileViewport.matches && appLayout.classList.contains('is-open');
            const isCollapsed = appLayout.classList.contains('sidebar-collapsed');
            const hideSidebar = mobileViewport.matches && !isOpen && !isCollapsed;

            if ((hideSidebar && sidebar.contains(document.activeElement))
                || (isOpen && main?.contains(document.activeElement))) {
                mobileToggle?.focus();
            }
            sidebar.inert = hideSidebar;
            if (main) {
                main.inert = isOpen;
            }
            sidebarCollapse?.setAttribute('aria-expanded', String(!isCollapsed));
            sidebarCollapse?.setAttribute('aria-label', isCollapsed ? 'Expandir menú lateral' : 'Colapsar menú lateral');
        };

        const closeMobileSidebar = () => {
            appLayout.classList.remove('is-open');
            syncSidebar();
            if (mobileToggle) {
                mobileToggle.setAttribute('aria-expanded', 'false');
                mobileToggle.setAttribute('aria-label', 'Abrir menú');
            }
        };

        if (mobileToggle) {
            mobileToggle.addEventListener('click', () => {
                const isPortrait = window.matchMedia('(max-width: 900px)').matches;

                if (isPortrait) {
                    const isOpen = appLayout.classList.toggle('is-open');
                    if (isOpen) {
                        appLayout.classList.remove('sidebar-collapsed');
                        rememberSidebarState();
                    }
                    mobileToggle.setAttribute('aria-expanded', String(isOpen));
                    mobileToggle.setAttribute('aria-label', isOpen ? 'Cerrar menú' : 'Abrir menú');

                    if (sidebarCollapse) {
                        sidebarCollapse.setAttribute('aria-label', isOpen ? 'Colapsar menú lateral' : 'Expandir menú lateral');
                        sidebarCollapse.setAttribute('aria-expanded', String(isOpen));
                    }

                    syncSidebar();
                    return;
                }

                const isOpen = appLayout.classList.toggle('is-open');
                mobileToggle.setAttribute('aria-expanded', String(isOpen));
                mobileToggle.setAttribute('aria-label', isOpen ? 'Cerrar menú' : 'Abrir menú');
            });
        }

        if (sidebarCollapse) {
            sidebarCollapse.addEventListener('click', (event) => {
                event.stopPropagation();

                const isPortrait = window.matchMedia('(max-width: 900px)').matches;

                if (isPortrait) {
                    const isCollapsed = appLayout.classList.contains('sidebar-collapsed');

                    if (isCollapsed) {
                        appLayout.classList.remove('sidebar-collapsed');
                        appLayout.classList.add('is-open');
                    } else {
                        appLayout.classList.add('sidebar-collapsed');
                        appLayout.classList.remove('is-open');
                    }
                } else {
                    appLayout.classList.toggle('sidebar-collapsed');
                    appLayout.classList.remove('is-open');
                }

                const nextCollapsed = appLayout.classList.contains('sidebar-collapsed');
                rememberSidebarState();
                sidebarCollapse.setAttribute('aria-label', nextCollapsed ? 'Expandir menú lateral' : 'Colapsar menú lateral');
                sidebarCollapse.setAttribute('aria-expanded', String(!nextCollapsed));

                if (mobileToggle) {
                    const openState = appLayout.classList.contains('is-open');
                    mobileToggle.setAttribute('aria-expanded', String(openState));
                    mobileToggle.setAttribute('aria-label', openState ? 'Cerrar menú' : 'Abrir menú');
                }
                syncSidebar();
            });
        }

        mobileViewport.addEventListener('change', closeMobileSidebar);
        window.addEventListener('pageshow', (event) => {
            if (event.persisted) {
                const isCollapsed = document.cookie.split(';').some(cookie => cookie.trim() === 'sidebar-collapsed=true');
                appLayout.classList.toggle('sidebar-collapsed', isCollapsed);
                closeMobileSidebar();
            }
        });
        window.addEventListener('resize', () => {
            if (!mobileViewport.matches) {
                closeMobileSidebar();
            } else {
                syncSidebar();
            }
        });
        syncSidebar();

        sidebar.addEventListener('click', (event) => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            const clickedButton = target.closest('button');
            if (mobileToggle && clickedButton && !clickedButton.classList.contains('sidebar-collapse')) {
                closeMobileSidebar();
            }
        });

        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                closeMobileSidebar();
            }
        });
    }

    const linkViewSwitch = document.querySelector('[data-link-view-switch]');
    const linkViewContainer = document.querySelector('[data-link-view-container]');

    if (linkViewSwitch && linkViewContainer) {
        const cookiePath = linkViewSwitch.dataset.viewPreferencePath || '/';
        const viewButtons = linkViewSwitch.querySelectorAll('[data-link-view]');
        const readListPreference = () => document.cookie.split(';').some(cookie => cookie.trim() === 'links-view=list');
        const ensureDefaultLinkViewCookie = () => {
            const hasPreference = document.cookie.split(';').some(cookie => cookie.trim().startsWith('links-view='));
            if (!hasPreference) {
                const secure = window.location.protocol === 'https:' ? '; Secure' : '';
                document.cookie = `links-view=cards; Path=${cookiePath}; SameSite=Lax${secure}`;
            }
        };
        const setListView = (isList) => {
            linkViewContainer.classList.toggle('links-grid--list', isList);
            viewButtons.forEach(button => {
                const isSelected = (button.dataset.linkView === 'list') === isList;
                button.setAttribute('aria-pressed', String(isSelected));
                button.classList.toggle('view-switch__button--active', isSelected);
            });
        };

        viewButtons.forEach(button => {
            button.addEventListener('click', () => {
                const isList = button.dataset.linkView === 'list';
                setListView(isList);
                const secure = window.location.protocol === 'https:' ? '; Secure' : '';
                document.cookie = `links-view=${isList ? 'list' : 'cards'}; Path=${cookiePath}; SameSite=Lax${secure}`;
            });
        });

        window.addEventListener('pageshow', event => {
            if (event.persisted) {
                ensureDefaultLinkViewCookie();
                setListView(readListPreference());
            }
        });
        ensureDefaultLinkViewCookie();
        setListView(readListPreference());
        linkViewSwitch.hidden = false;
    }

    const quickAddInput = document.querySelector('[data-quick-add-input]');
    const quickAddToggle = document.querySelector('[data-quick-add-toggle]');
    const quickAddMenu = document.querySelector('#quick-add-menu');
    const quickAddAdvanced = document.querySelector('[data-quick-add-advanced]');

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

        const navigateQuickAddMenu = (event) => {
            const fromToggle = event.currentTarget === quickAddToggle;
            const keys = fromToggle ? ['ArrowDown', 'ArrowUp'] : ['ArrowDown', 'ArrowUp', 'Home', 'End'];
            if (!keys.includes(event.key)) {
                return;
            }

            const items = [...quickAddMenu.querySelectorAll('.quick-add__menu-item:not(:disabled)')]
                .filter((item) => !item.hidden);
            if (items.length === 0) {
                return;
            }

            event.preventDefault();
            quickAddMenu.hidden = false;
            quickAddToggle.setAttribute('aria-expanded', 'true');

            const currentIndex = items.indexOf(document.activeElement);
            let nextIndex;
            if (event.key === 'Home' || (fromToggle && event.key === 'ArrowDown')) {
                nextIndex = 0;
            } else if (event.key === 'End' || (fromToggle && event.key === 'ArrowUp')) {
                nextIndex = items.length - 1;
            } else {
                const step = event.key === 'ArrowDown' ? 1 : -1;
                nextIndex = (currentIndex + step + items.length) % items.length;
            }
            items[nextIndex].focus();
        };

        quickAddToggle.addEventListener('keydown', navigateQuickAddMenu);
        quickAddMenu.addEventListener('keydown', navigateQuickAddMenu);

        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape' && !quickAddMenu.hidden) {
                const restoreFocus = quickAddMenu.contains(document.activeElement);
                closeQuickAddMenu();
                if (restoreFocus) {
                    quickAddToggle.focus();
                }
            }
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

    if (quickAddAdvanced instanceof HTMLAnchorElement && quickAddInput instanceof HTMLInputElement) {
        const updateAdvancedLink = () => {
            const destination = new URL(quickAddAdvanced.href);
            if (quickAddInput.value) {
                destination.searchParams.set('url', quickAddInput.value);
            } else {
                destination.searchParams.delete('url');
            }
            quickAddAdvanced.href = destination.href;
        };

        quickAddInput.addEventListener('input', updateAdvancedLink);
        quickAddInput.addEventListener('change', updateAdvancedLink);
        quickAddAdvanced.addEventListener('click', updateAdvancedLink);
        updateAdvancedLink();
    }
});
