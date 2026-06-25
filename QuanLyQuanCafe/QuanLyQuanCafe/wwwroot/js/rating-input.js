(function () {
  var root = document.getElementById('starInput');
  if (!root) return;

  var hidden = document.getElementById('soSaoHidden');
  var label = document.getElementById('starInputLabel');
  var buttons = root.querySelectorAll('.star-input-btn');

  function setStars(n) {
    n = Math.max(1, Math.min(5, n));
    hidden.value = n;
    root.dataset.value = n;
    if (label) label.textContent = n + '/5 sao';
    buttons.forEach(function (btn) {
      var s = parseInt(btn.getAttribute('data-star'), 10);
      btn.classList.toggle('is-on', s <= n);
    });
  }

  setStars(parseInt(hidden.value, 10) || 5);

  buttons.forEach(function (btn) {
    btn.addEventListener('click', function () {
      setStars(parseInt(btn.getAttribute('data-star'), 10));
    });
  });
})();