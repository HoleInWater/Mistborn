// ═══════════════════════════════════════════════════════════════════════
// MISTBORN WEBSITE — Main JavaScript
// ═══════════════════════════════════════════════════════════════════════

// ── Navbar scroll effect ─────────────────────────────────────────────
window.addEventListener('scroll', () => {
    const navbar = document.getElementById('navbar');
    navbar.classList.toggle('scrolled', window.scrollY > 50);
});

// ── Mobile nav toggle ────────────────────────────────────────────────
document.getElementById('navToggle').addEventListener('click', () => {
    document.getElementById('navLinks').classList.toggle('active');
});

// Close mobile nav when a link is clicked
document.querySelectorAll('.nav-links a').forEach(link => {
    link.addEventListener('click', () => {
        document.getElementById('navLinks').classList.remove('active');
    });
});

// ── Media tabs ───────────────────────────────────────────────────────
document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
        btn.classList.add('active');
        document.getElementById(btn.dataset.tab).classList.add('active');
    });
});

// ── Ash particles ────────────────────────────────────────────────────
function createAshParticles() {
    const container = document.getElementById('ashContainer');
    const count = 40;

    for (let i = 0; i < count; i++) {
        const particle = document.createElement('div');
        particle.className = 'ash-particle';
        particle.style.left = Math.random() * 100 + '%';
        particle.style.width = (Math.random() * 3 + 1) + 'px';
        particle.style.height = particle.style.width;
        particle.style.animationDuration = (Math.random() * 15 + 10) + 's';
        particle.style.animationDelay = (Math.random() * 20) + 's';
        particle.style.opacity = Math.random() * 0.4 + 0.1;
        container.appendChild(particle);
    }
}
createAshParticles();

// ── Contact form ─────────────────────────────────────────────────────
function handleSubmit(e) {
    e.preventDefault();
    const form = e.target;
    const btn = form.querySelector('button');
    const originalText = btn.textContent;

    btn.textContent = 'Sending...';
    btn.disabled = true;

    // Simulate send (replace with actual form handler — Formspree, Netlify, etc.)
    setTimeout(() => {
        btn.textContent = 'Message Sent!';
        btn.style.background = '#2a7a2a';
        form.reset();

        setTimeout(() => {
            btn.textContent = originalText;
            btn.style.background = '';
            btn.disabled = false;
        }, 3000);
    }, 1500);

    return false;
}

// ── Smooth reveal on scroll ──────────────────────────────────────────
const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('revealed');
        }
    });
}, { threshold: 0.1 });

document.querySelectorAll('.feature-card, .world-card, .team-card, .faq-item').forEach(el => {
    el.style.opacity = '0';
    el.style.transform = 'translateY(20px)';
    el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
    observer.observe(el);
});

// Add revealed styles
const style = document.createElement('style');
style.textContent = '.revealed { opacity: 1 !important; transform: translateY(0) !important; }';
document.head.appendChild(style);
