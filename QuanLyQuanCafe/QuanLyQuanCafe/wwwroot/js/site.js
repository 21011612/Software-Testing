(function () {
  if (document.body.classList.contains('admin-body')) return;

  var nav = document.querySelector('.navbar-tooru');
  if (nav) {
    window.addEventListener('scroll', function () {
      nav.classList.toggle('scrolled', window.scrollY > 20);
    }, { passive: true });
  }

  var path = (window.location.pathname || '').toLowerCase().replace(/\/$/, '') || '/';

  document.querySelectorAll('.navbar-tooru .nav-link[href]').forEach(function (link) {
    var href = (link.getAttribute('href') || '').toLowerCase().split('?')[0].replace(/\/$/, '') || '/';
    var active = false;

    if (href === '/' || href === '/home' || href === '/home/index') {
      active = path === '/' || path === '/home' || path === '/home/index';
    } else if (path === href) {
      active = true;
    } else if (href.length > 1 && path.indexOf(href + '/') === 0) {
      active = true;
    }

    if (active) link.classList.add('active');
  });
})();