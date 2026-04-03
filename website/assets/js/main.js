// ═══════════════════════════════════════════════════════════════════════
// MISTBORN WEBSITE — Enhanced JavaScript
// ═══════════════════════════════════════════════════════════════════════

// ── Navbar scroll effect ─────────────────────────────────────────────
let lastScroll = 0;
window.addEventListener('scroll', () => {
    const navbar = document.getElementById('navbar');
    const scrollY = window.scrollY;
    navbar.classList.toggle('scrolled', scrollY > 50);
    lastScroll = scrollY;
});

// ── Mobile nav toggle ────────────────────────────────────────────────
document.getElementById('navToggle').addEventListener('click', () => {
    document.getElementById('navLinks').classList.toggle('active');
});

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

// ── Ash particles (enhanced) ─────────────────────────────────────────
function createAshParticles() {
    const container = document.getElementById('ashContainer');
    const count = 60;

    for (let i = 0; i < count; i++) {
        const particle = document.createElement('div');
        particle.className = 'ash-particle';

        const size = Math.random() * 3 + 1;
        particle.style.left = Math.random() * 100 + '%';
        particle.style.width = size + 'px';
        particle.style.height = size + 'px';
        particle.style.animationDuration = (Math.random() * 20 + 12) + 's';
        particle.style.animationDelay = (Math.random() * 25) + 's';

        // Vary opacity and color for depth
        const brightness = Math.random() * 0.3 + 0.1;
        particle.style.background = `rgba(180, 165, 140, ${brightness})`;

        container.appendChild(particle);
    }
}
createAshParticles();

// ── Smooth scroll reveal with stagger ────────────────────────────────
const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry, index) => {
        if (entry.isIntersecting) {
            // Stagger the reveal based on sibling index
            const siblings = entry.target.parentElement.children;
            let siblingIndex = Array.from(siblings).indexOf(entry.target);

            setTimeout(() => {
                entry.target.classList.add('revealed');
            }, siblingIndex * 100);
        }
    });
}, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

document.querySelectorAll('.feature-card, .world-card, .team-card, .faq-item, .metal-category, .about-text, .about-image').forEach(el => {
    el.style.opacity = '0';
    el.style.transform = 'translateY(30px)';
    el.style.transition = 'opacity 0.8s cubic-bezier(0.4, 0, 0.2, 1), transform 0.8s cubic-bezier(0.4, 0, 0.2, 1)';
    revealObserver.observe(el);
});

// Inject revealed class styles
const revealStyle = document.createElement('style');
revealStyle.textContent = '.revealed { opacity: 1 !important; transform: translateY(0) !important; }';
document.head.appendChild(revealStyle);

// ── Hero parallax on mouse move ──────────────────────────────────────
const heroContent = document.querySelector('.hero-content');
if (heroContent) {
    document.querySelector('.hero').addEventListener('mousemove', (e) => {
        const x = (e.clientX / window.innerWidth - 0.5) * 10;
        const y = (e.clientY / window.innerHeight - 0.5) * 10;
        heroContent.style.transform = `translate(${x}px, ${y}px)`;
    });
}

// ── Typed tagline effect ─────────────────────────────────────────────
function typeWriter(element, text, speed = 50) {
    element.textContent = '';
    let i = 0;
    function type() {
        if (i < text.length) {
            element.textContent += text.charAt(i);
            i++;
            setTimeout(type, speed);
        }
    }
    type();
}

// Start typing effect when hero is visible
const tagline = document.querySelector('.hero-tagline');
if (tagline) {
    const taglineText = tagline.textContent;
    tagline.textContent = '';

    setTimeout(() => {
        typeWriter(tagline, taglineText, 60);
    }, 1500);
}

// ── Contact form ─────────────────────────────────────────────────────
function handleSubmit(e) {
    e.preventDefault();
    const form = e.target;
    const btn = form.querySelector('button');
    const originalText = btn.textContent;

    btn.textContent = 'Sending...';
    btn.disabled = true;
    btn.style.opacity = '0.6';

    setTimeout(() => {
        btn.textContent = 'Message Sent!';
        btn.style.background = 'linear-gradient(135deg, #2a7a2a, #1a5a1a)';
        btn.style.opacity = '1';
        form.reset();

        setTimeout(() => {
            btn.textContent = originalText;
            btn.style.background = '';
            btn.disabled = false;
        }, 3000);
    }, 1500);

    return false;
}

// ── Active nav link highlighting ─────────────────────────────────────
const sections = document.querySelectorAll('section[id]');
const navLinks = document.querySelectorAll('.nav-links a');

window.addEventListener('scroll', () => {
    let current = '';
    sections.forEach(section => {
        const top = section.offsetTop - 100;
        if (window.scrollY >= top) {
            current = section.getAttribute('id');
        }
    });

    navLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') === '#' + current) {
            link.style.color = '#4488FF';
        } else {
            link.style.color = '';
        }
    });
});
