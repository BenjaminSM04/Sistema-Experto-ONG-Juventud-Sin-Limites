// Funciones adicionales para la página de inicio
window.downloadFileFromBase64 = function (base64, fileName, contentType) {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${base64}`;
    link.download = fileName;
    link.click();
};

// Animaciones suaves al cargar
document.addEventListener('DOMContentLoaded', function() {
    // Agregar clase de animación a elementos cuando entran en el viewport
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in-up');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observar elementos con animación
    document.querySelectorAll('.stat-card, .action-card').forEach(el => {
        observer.observe(el);
    });
});

// Agregar estilos CSS para animaciones
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .fade-in-up {
        animation: fadeInUp 0.6s ease-out forwards;
    }
`;
document.head.appendChild(style);
