// Mobile-specific enhancements
(function () {
    'use strict';

    // Prevent iOS zoom on input focus
    if (/iPhone|iPad|iPod/.test(navigator.userAgent)) {
        document.addEventListener('DOMContentLoaded', function () {
            const inputs = document.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (!input.style.fontSize) {
                    input.style.fontSize = '16px';
                }
            });
        });
    }

    // Auto-collapse navbar on mobile after clicking link
    document.addEventListener('DOMContentLoaded', function () {
        if (window.innerWidth < 768) {
            const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
            const navbarCollapse = document.querySelector('.navbar-collapse');

            navLinks.forEach(link => {
                link.addEventListener('click', () => {
                    if (navbarCollapse && navbarCollapse.classList.contains('show')) {
                        const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                        if (bsCollapse) {
                            bsCollapse.hide();
                        }
                    }
                });
            });
        }
    });

    // Smooth scroll for better mobile experience
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Add touch feedback class
    if ('ontouchstart' in window) {
        document.body.classList.add('touch-device');
    }
})();