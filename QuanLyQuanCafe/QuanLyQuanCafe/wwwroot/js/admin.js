(function () {
  var toggle = document.getElementById('adminSidebarToggle');
  var sidebar = document.getElementById('adminSidebar');
  var backdrop = document.getElementById('adminSidebarBackdrop');

  function closeSidebar() {
    sidebar?.classList.remove('show');
    backdrop?.classList.remove('show');
    document.body.style.overflow = '';
  }

  function openSidebar() {
    sidebar?.classList.add('show');
    backdrop?.classList.add('show');
    document.body.style.overflow = 'hidden';
  }

  toggle?.addEventListener('click', function () {
    if (sidebar?.classList.contains('show')) closeSidebar();
    else openSidebar();
  });

  backdrop?.addEventListener('click', closeSidebar);

  closeSidebar();

  window.addEventListener('resize', function () {
    if (window.innerWidth >= 992) closeSidebar();
  });

  var path = (window.location.pathname || '').toLowerCase().replace(/\/$/, '') || '/';
  document.querySelectorAll('.admin-nav-link[data-nav]').forEach(function (link) {
    var href = (link.getAttribute('href') || '').toLowerCase().split('?')[0].replace(/\/$/, '') || '';
    if (!href) return;
    var active = path === href;
    if (!active && (path === href + '/index' || href === path + '/index')) {
      active = true;
    }
    if (active) link.classList.add('active');
  });
})();