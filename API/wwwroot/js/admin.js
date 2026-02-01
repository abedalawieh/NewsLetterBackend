
const AdminApp = (() => {
    const init = () => {
        console.log('Admin Intelligence System Initialized');
        setupTooltips();
    };

    const setupTooltips = () => {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    };

    return {
        init
    };
})();

document.addEventListener('DOMContentLoaded', AdminApp.init);
