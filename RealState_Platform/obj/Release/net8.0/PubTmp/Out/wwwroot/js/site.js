// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// ——————— ANIMATIONS & SCROLL EFFECTS ———————

// Intersection Observer for scroll animations
const observerOptions = {
  threshold: 0.1,
  rootMargin: '0px 0px -100px 0px'
};

const observer = new IntersectionObserver(function(entries) {
  entries.forEach(entry => {
    if (entry.isIntersecting) {
      entry.target.style.animation = 'fadeIn 0.6s ease-out forwards';
      entry.target.style.opacity = '1';
      observer.unobserve(entry.target);
    }
  });
}, observerOptions);

// Apply scroll animations to cards and elements
document.addEventListener('DOMContentLoaded', function() {
  // Animate cards on scroll
  const cards = document.querySelectorAll('.card, .card-item, [data-animate]');
  cards.forEach((card, index) => {
    card.style.opacity = '0';
    card.style.animation = 'none';
    setTimeout(() => {
      observer.observe(card);
    }, index * 100);
  });

  // Animate buttons on hover
  const buttons = document.querySelectorAll('button, a.btn, .btn');
  buttons.forEach(btn => {
    btn.addEventListener('mouseenter', function() {
      this.style.transform = 'translateY(-2px)';
    });
    btn.addEventListener('mouseleave', function() {
      this.style.transform = 'translateY(0)';
    });
  });

  // Smooth scroll to tops
  const scrollLinks = document.querySelectorAll('a[href^="#"]');
  scrollLinks.forEach(link => {
    link.addEventListener('click', function(e) {
      e.preventDefault();
      const target = document.querySelector(this.getAttribute('href'));
      if (target) {
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });
});

// Navbar animation on scroll
let lastScroll = 0;
const navbar = document.querySelector('.site-navbar');

window.addEventListener('scroll', () => {
  const currentScroll = window.pageYOffset;
  
  if (navbar) {
    // Add shadow on scroll
    if (currentScroll > 10) {
      navbar.style.boxShadow = '0 8px 32px rgba(0, 0, 0, 0.2)';
    } else {
      navbar.style.boxShadow = '0 4px 24px rgba(0, 0, 0, 0.15)';
    }
  }
  
  lastScroll = currentScroll;
});

// Loading animation for forms
const forms = document.querySelectorAll('form');
forms.forEach(form => {
  form.addEventListener('submit', function(e) {
    const submitBtn = this.querySelector('[type="submit"]');
    if (submitBtn) {
      submitBtn.style.opacity = '0.7';
      submitBtn.disabled = true;
    }
  });
});

// Animate counter numbers (if any)
function animateValue(element, start, end, duration) {
  let startTimestamp = null;
  const step = (timestamp) => {
    if (!startTimestamp) startTimestamp = timestamp;
    const progress = Math.min((timestamp - startTimestamp) / duration, 1);
    element.textContent = Math.floor(progress * (end - start) + start);
    if (progress < 1) {
      window.requestAnimationFrame(step);
    }
  };
  window.requestAnimationFrame(step);
}