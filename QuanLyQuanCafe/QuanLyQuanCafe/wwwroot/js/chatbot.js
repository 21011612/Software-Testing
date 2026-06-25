(function () {
  if (document.body.classList.contains('admin-body')) return;

  var panel = document.getElementById('chatbotPanel');
  var fab = document.getElementById('chatbotFab');
  var closeBtn = document.getElementById('chatbotClose');
  var messages = document.getElementById('chatbotMessages');
  var form = document.getElementById('chatbotForm');
  var input = document.getElementById('chatbotInput');
  var quick = document.getElementById('chatbotQuick');
  if (!panel || !fab || !messages) return;

  var tuVan = loadTuVan();

  function loadTuVan() {
    var el = document.getElementById('tooruChatData');
    if (!el || !el.textContent) return null;
    try {
      return JSON.parse(el.textContent);
    } catch (e) {
      return null;
    }
  }

  var imgFallback = '/images/Ca_phe_sua_da.jpg';

  function fmtGia(n) {
    return Number(n).toLocaleString('vi-VN') + ' đ';
  }

  function normalizeMon(m, loai) {
    return {
      ten: m.ten,
      gia: m.gia,
      donVi: m.donVi || 'Ly',
      moTa: m.moTa,
      tenLoai: loai ? loai.tenLoai : (m.tenLoai || ''),
      hinhAnh: m.hinhAnh || imgFallback,
      maSp: m.maSp
    };
  }

  function allMon() {
    if (!tuVan || !tuVan.loaiMon) return [];
    var list = [];
    tuVan.loaiMon.forEach(function (loai) {
      (loai.mon || []).forEach(function (m) {
        list.push(normalizeMon(m, loai));
      });
    });
    return list;
  }

  function monImgTag(src, alt, size) {
    var cls = size === 'lg' ? 'chatbot-mon-img chatbot-mon-img--lg' : 'chatbot-mon-img';
    return '<img class="' + cls + '" src="' + escapeHtml(src || imgFallback) + '" alt="' + escapeHtml(alt) + '" loading="lazy" onerror="this.onerror=null;this.src=\'' + imgFallback + '\';" />';
  }

  function monDetailLink(maSp, inner) {
    if (!maSp) return inner;
    return '<a href="/ThucDon/ChiTiet/' + maSp + '" class="chatbot-mon-link">' + inner + '</a>';
  }

  function monRowHtml(m, showMoTa) {
    var html = '<li class="chatbot-mon-row">';
    html += '<div class="chatbot-mon-thumb">' + monDetailLink(m.maSp, monImgTag(m.hinhAnh, m.ten)) + '</div>';
    html += '<div class="chatbot-mon-body">';
    html += monDetailLink(m.maSp, '<span class="chatbot-mon-name">' + escapeHtml(m.ten) + '</span>');
    html += '<div class="chatbot-mon-meta"><span class="chatbot-mon-price">' + fmtGia(m.gia) + '</span>';
    html += ' <span class="chatbot-muted">/' + escapeHtml(m.donVi) + '</span></div>';
    if (showMoTa && m.moTa) html += '<div class="chatbot-muted chatbot-mon-desc">' + escapeHtml(m.moTa) + '</div>';
    html += '</div></li>';
    return html;
  }

  function monCardHtml(m) {
    var html = '<div class="chatbot-mon-card">';
    html += monDetailLink(m.maSp, monImgTag(m.hinhAnh, m.ten, 'lg'));
    html += '<div class="chatbot-mon-card-body">';
    html += monDetailLink(m.maSp, '<strong>' + escapeHtml(m.ten) + '</strong>');
    html += '<div class="chatbot-mon-price">' + fmtGia(m.gia) + ' <span class="chatbot-muted">/' + escapeHtml(m.donVi) + '</span></div>';
    if (m.moTa) html += '<p class="chatbot-muted small mb-0">' + escapeHtml(m.moTa) + '</p>';
    html += '</div></div>';
    return html;
  }

  function categoryBannerHtml(loai) {
    if (!loai.anhLoai) return '';
    return '<img class="chatbot-category-banner" src="' + escapeHtml(loai.anhLoai) + '" alt="' + escapeHtml(loai.tenLoai) + '" loading="lazy" onerror="this.style.display=\'none\'" />';
  }

  function shopBannerHtml() {
    var src = (tuVan && tuVan.anhQuan) ? tuVan.anhQuan : imgFallback;
    return '<div class="chatbot-banner"><img src="' + escapeHtml(src) + '" alt="Tooru Coffee" loading="lazy" onerror="this.onerror=null;this.src=\'' + imgFallback + '\';" /></div>';
  }

  function replyThucDonDayDu() {
    if (!tuVan || !tuVan.loaiMon || tuVan.loaiMon.length === 0) {
      return 'Hiện chưa tải được thực đơn. Bạn xem trực tiếp tại <a href="/ThucDon">Thực đơn</a> nhé!';
    }
    var html = shopBannerHtml();
    html += '<strong>Tooru Coffee</strong> — <strong>' + tuVan.tongSoMon + ' món</strong>, ' + tuVan.loaiMon.length + ' nhóm (ảnh từng món bên dưới):<br><br>';
    tuVan.loaiMon.forEach(function (loai) {
      html += '<div class="chatbot-menu-group">';
      html += categoryBannerHtml(loai);
      html += '<div class="chatbot-menu-group-title">☕ ' + escapeHtml(loai.tenLoai);
      if (loai.moTaLoai) html += ' <span class="chatbot-muted">— ' + escapeHtml(loai.moTaLoai) + '</span>';
      html += '</div><ul class="chatbot-menu-list">';
      (loai.mon || []).forEach(function (m) {
        html += monRowHtml(normalizeMon(m, loai), true);
      });
      html += '</ul></div>';
    });
    html += '<br>👉 <a href="/ThucDon">Mở thực đơn &amp; đặt món</a>';
    return html;
  }

  function replyTheoLoai(tenLoaiKey) {
    if (!tuVan || !tuVan.loaiMon) return null;
    var key = tenLoaiKey.toLowerCase();
    var loai = tuVan.loaiMon.find(function (l) {
      return (l.tenLoai || '').toLowerCase().indexOf(key) >= 0 || key.indexOf((l.tenLoai || '').toLowerCase()) >= 0;
    });
    if (!loai || !loai.mon || !loai.mon.length) return null;
    var html = categoryBannerHtml(loai);
    html += '<strong>' + escapeHtml(loai.tenLoai) + '</strong> (' + loai.mon.length + ' món):<ul class="chatbot-menu-list">';
    loai.mon.forEach(function (m) {
      html += monRowHtml(normalizeMon(m, loai), false);
    });
    html += '</ul><a href="/ThucDon?loai=' + loai.maLoai + '">Xem trên thực đơn</a>';
    return html;
  }

  function replyTimMon(text) {
    var mon = allMon();
    if (!mon.length) return null;
    var words = text.toLowerCase().split(/\s+/).filter(function (w) {
      return w.length > 1 && !stopWords[w];
    });
    if (!words.length) return null;
    var hits = mon.filter(function (m) {
      var blob = (m.ten + ' ' + m.tenLoai + ' ' + (m.moTa || '')).toLowerCase();
      return words.some(function (w) { return blob.indexOf(w) >= 0; });
    });
    if (!hits.length) return null;

    if (hits.length === 1) {
      return monCardHtml(hits[0]) + '<br><a href="/ThucDon/ChiTiet/' + hits[0].maSp + '">Xem chi tiết &amp; thêm giỏ</a>';
    }

    if (hits.length > 6) hits = hits.slice(0, 6);
    var html = 'Mình tìm thấy <strong>' + hits.length + '</strong> món:<ul class="chatbot-menu-list">';
    hits.forEach(function (m) {
      html += monRowHtml(m, false);
    });
    html += '</ul><a href="/ThucDon?q=' + encodeURIComponent(words[0]) + '">Xem thêm trên thực đơn</a>';
    return html;
  }

  function replyKhuyenMai() {
    if (tuVan && tuVan.khuyenMai && tuVan.khuyenMai.length) {
      var html = '<strong>Ưu đãi đang áp dụng:</strong><ul class="chatbot-menu-list">';
      tuVan.khuyenMai.forEach(function (k) {
        html += '<li><strong>' + escapeHtml(k.ten) + '</strong> — ' + escapeHtml(k.moTaGiam) + ' (đến ' + escapeHtml(k.hetHan) + ')</li>';
      });
      html += '</ul>Áp dụng khi thanh toán (đủ điều kiện). Thành viên hạng Bạc trở lên có thêm quyền lợi.';
      return html;
    }
    return 'Hiện không có chương trình khuyến mãi công bố trên web. Bạn theo dõi <a href="/">Trang chủ</a> hoặc hỏi nhân viên khi thanh toán.';
  }

  var stopWords = {
    'quán': 1, 'có': 1, 'món': 1, 'gì': 1, 'là': 1, 'của': 1, 'cho': 1, 'tôi': 1, 'mình': 1, 'bạn': 1, 'xin': 1, 'hỏi': 1, 'muốn': 1, 'biết': 1, 'về': 1, 'thế': 1, 'nào': 1, 'không': 1, 'được': 1, 'hay': 1, 'gì': 1
  };

  var matchers = [
    {
      test: function (t) {
        return /quán có món|co mon gi|bán gì|ban gi|món gì|mon gi|thực đơn|thuc don|menu đầy|danh sách món|co nhung mon|những món nào/.test(t);
      },
      reply: function () { return replyThucDonDayDu(); }
    },
    {
      test: function (t) { return /khuyến mãi|khuyen mai|giảm giá|giam gia|ưu đãi|uu dai|km\b/.test(t); },
      reply: function () { return replyKhuyenMai(); }
    },
    {
      test: function (t) { return /giá|gia\b|bao nhiêu|bao nhieu|cost/.test(t) && /cà phê|ca phe|coffee/.test(t); },
      reply: function () {
        var r = replyTheoLoai('cà phê') || replyTheoLoai('ca phe') || replyTheoLoai('coffee');
        return r || replyTimMon('cà phê') || replyThucDonDayDu();
      }
    },
    {
      test: function (t) { return /trà|tra\b|tea/.test(t) && !/đặt|dat|mua/.test(t); },
      reply: function () {
        return replyTheoLoai('trà') || replyTheoLoai('tra') || replyTimMon('trà') || 'Xem nhóm trà trên <a href="/ThucDon">Thực đơn</a>.';
      }
    },
    {
      test: function (t) { return /sinh tố|sinh to|smoothie/.test(t); },
      reply: function () { return replyTheoLoai('sinh') || replyTimMon('sinh tố') || replyThucDonDayDu(); }
    },
    {
      test: function (t) { return /đồ ăn|do an|ăn|banh|bánh|snack/.test(t); },
      reply: function () { return replyTheoLoai('ăn') || replyTheoLoai('đồ') || replyTimMon('ăn'); }
    },
    {
      test: function (t) { return /giá|gia\b|bao nhiêu/.test(t); },
      reply: function (t) { return replyTimMon(t) || replyThucDonDayDu(); }
    }
  ];

  var rules = [
    { keys: ['xin chào', 'hello', 'hi', 'chào', 'hey'], reply: function () {
      var intro = 'Xin chào! Mình là <strong>Trợ lý tư vấn Tooru Coffee</strong> ☕<br><br>';
      if (tuVan && tuVan.tongSoMon) {
        intro += 'Quán đang có <strong>' + tuVan.tongSoMon + ' món</strong>. Hỏi <em>“quán có món gì”</em> để xem đầy đủ, hoặc gõ tên món (vd: <em>cà phê sữa</em>) để tra giá.';
      } else {
        intro += 'Hỏi <em>quán có món gì</em>, <em>khuyến mãi</em>, <em>đặt món</em> hoặc <em>địa chỉ</em>.';
      }
      return intro;
    }},
    { keys: ['giờ', 'mở cửa', 'đóng cửa', 'mấy giờ', 'mo cua'], reply: function () {
      var g = (tuVan && tuVan.gioMoCua) ? tuVan.gioMoCua : '7:00 – 22:00';
      return 'Tooru Coffee phục vụ <strong>' + escapeHtml(g) + '</strong>. Ngày lễ có thể điều chỉnh — gọi quán để chắc chắn nhé!';
    }},
    { keys: ['địa chỉ', 'ở đâu', 'chỉ đường', 'map', 'dia chi'], reply: function () {
      var dc = (tuVan && tuVan.diaChi) ? tuVan.diaChi : '123 Nguyễn Tất Thành, Sơn Trà, Đà Nẵng';
      return shopBannerHtml() + '<strong>' + escapeHtml(dc) + '</strong><br><br>Có không gian tầng trệt, tầng 2 và sân vườn. <a href="/Home/GioiThieu">Giới thiệu quán</a>';
    }},
    { keys: ['điện thoại', 'sđt', 'sdt', 'gọi', 'liên hệ', 'lien he'], reply: function () {
      var sdt = (tuVan && tuVan.dienThoai) ? tuVan.dienThoai : '0236 3xxx xxx';
      return 'Liên hệ quán: <strong>' + escapeHtml(sdt) + '</strong> (đặt bàn, hỏi món đặc biệt).';
    }},
    { keys: ['đặt món', 'gọi món', 'đặt hàng', 'mua', 'dat mon', 'goi mon'], reply: 'Các bước đặt món online:<ol class="chatbot-steps"><li>Vào <a href="/ThucDon">Thực đơn</a> (hoặc tìm món trên thanh menu)</li><li>Chọn món → <strong>Thêm vào giỏ</strong></li><li><a href="/DonHang/GioHang">Giỏ hàng</a> → ghi chú, chọn mang về / tại quán</li><li>Thanh toán: tiền mặt, thẻ, Momo, ZaloPay...</li></ol>' },
    { keys: ['giỏ', 'cart', 'gio hang'], reply: 'Nhấn <strong>Giỏ hàng</strong> trên menu hoặc <a href="/DonHang/GioHang">vào đây</a> để sửa số lượng và đặt hàng.' },
    { keys: ['thanh toán', 'momo', 'zalopay', 'thẻ', 'tiền mặt', 'shopeepay', 'chuyển khoản'], reply: 'Hỗ trợ: <strong>tiền mặt, thẻ, chuyển khoản, Momo, ZaloPay, ShopeePay</strong> (tùy đơn). Chọn khi đặt hàng hoặc tại quầy.' },
    { keys: ['tích điểm', 'hạng', 'thành viên', 'bạc', 'vàng', 'kim cương', 'diem'], reply: '<strong>Hạng thành viên:</strong> Thường → Bạc → Vàng → Kim Cương.<br>Quy tắc quán: <strong>10.000đ = 1 điểm</strong> khi thanh toán. <a href="/TaiKhoan/DangKy">Đăng ký</a> để tích lũy.' },
    { keys: ['đăng ký', 'tài khoản', 'login', 'đăng nhập', 'dang ky', 'dang nhap'], reply: '<a href="/TaiKhoan/DangKy">Đăng ký</a> thành viên hoặc <a href="/TaiKhoan/DangNhap">Đăng nhập</a> để xem đơn, tích điểm và nhận ưu đãi hạng.' },
    { keys: ['đơn', 'lịch sử', 'don hang'], reply: 'Đăng nhập → menu <strong>Đơn của tôi</strong> để xem lịch sử và trạng thái đơn.' },
    { keys: ['đặt bàn', 'book bàn', 'bàn', 'dat ban'], reply: 'Đặt bàn: gọi <strong>' + escapeHtml((tuVan && tuVan.dienThoai) || '0236 3xxx xxx') + '</strong> hoặc nhắn fanpage. Web hỗ trợ <strong>gọi món &amp; thanh toán</strong>; có bàn VIP và sân vườn.' },
    { keys: ['wifi', 'ngồi', 'không gian', 'sân vườn', 'vip'], reply: 'Không gian: <strong>tầng trệt, tầng 2, sân vườn</strong>, có khu VIP. Phù hợp làm việc, học nhóm hoặc gặp gỡ.' },
    { keys: ['cảm ơn', 'thanks', 'cam on'], reply: 'Không có gì! Chúc bạn thưởng thức Tooru Coffee ngon lành ☕' },
    { keys: ['giới thiệu', 'gioi thieu', 'về quán', 've quan'], reply: function () {
      return shopBannerHtml() + '<a href="/Home/GioiThieu">Giới thiệu Tooru Coffee</a> — cà phê Việt, trà trái cây, sinh tố và món ăn nhẹ tại Đà Nẵng.';
    }}
  ];

  function replyFor(text) {
    var t = (text || '').toLowerCase().trim();
    if (!t) return 'Bạn gõ câu hỏi hoặc chọn gợi ý bên dưới nhé.';

    for (var i = 0; i < matchers.length; i++) {
      if (matchers[i].test(t)) {
        var out = matchers[i].reply(t);
        if (out) return typeof out === 'function' ? out(t) : out;
      }
    }

    var tim = replyTimMon(t);
    if (tim) return tim;

    for (var r = 0; r < rules.length; r++) {
      for (var k = 0; k < rules[r].keys.length; k++) {
        if (t.indexOf(rules[r].keys[k]) >= 0) {
          var rep = rules[r].reply;
          return typeof rep === 'function' ? rep() : rep;
        }
      }
    }

    return 'Mình chưa chắc ý bạn. Thử hỏi:<br>• <em>Quán có món gì</em><br>• <em>Giá cà phê</em> / tên món cụ thể<br>• <em>Khuyến mãi</em>, <em>đặt món</em>, <em>địa chỉ</em><br>Hoặc <a href="/ThucDon">mở thực đơn</a>.';
  }

  function addBubble(html, who) {
    var el = document.createElement('div');
    var hasMedia = who === 'bot' && (html.indexOf('<img') >= 0 || html.indexOf('chatbot-mon-row') >= 0);
    el.className = 'chatbot-bubble chatbot-bubble--' + who + (hasMedia ? ' chatbot-bubble--wide' : '');
    el.innerHTML = html;
    messages.appendChild(el);
    messages.scrollTop = messages.scrollHeight;
  }

  function openChat() {
    panel.hidden = false;
    fab.classList.add('is-open');
    if (!panel.dataset.ready) {
      var greet = shopBannerHtml();
      greet += 'Chào bạn! Mình tư vấn <strong>thực đơn có ảnh</strong>, giá &amp; cách đặt món tại Tooru Coffee.';
      if (tuVan && tuVan.tongSoMon) greet += '<br><br>Hiện có <strong>' + tuVan.tongSoMon + ' món</strong> — hỏi <em>“quán có món gì”</em> để xem ảnh từng món.';
      addBubble(greet, 'bot');
      panel.dataset.ready = '1';
    }
    input.focus();
  }

  function closeChat() {
    panel.hidden = true;
    fab.classList.remove('is-open');
  }

  fab.addEventListener('click', function () {
    if (panel.hidden) openChat();
    else closeChat();
  });
  closeBtn.addEventListener('click', closeChat);

  form.addEventListener('submit', function (e) {
    e.preventDefault();
    var text = input.value.trim();
    if (!text) return;
    addBubble(escapeHtml(text), 'user');
    input.value = '';
    setTimeout(function () {
      addBubble(replyFor(text), 'bot');
    }, 280);
  });

  if (quick) {
    quick.querySelectorAll('button[data-q]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var q = btn.getAttribute('data-q');
        input.value = q;
        form.dispatchEvent(new Event('submit', { cancelable: true }));
      });
    });
  }

  function escapeHtml(s) {
    return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }
})();