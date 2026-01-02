// Dashboard JavaScript

$(document).ready(function () {
    // Sidebar toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
    });

    // Close sidebar on mobile when clicking outside
    $(document).on('click', function (e) {
        if ($(window).width() <= 768) {
            if (!$(e.target).closest('#sidebar, #sidebarCollapse').length) {
                $('#sidebar').addClass('active');
            }
        }
    });

    // Auto-close alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);

    // Add ripple effect to buttons
    $('.btn').on('click', function (e) {
        var ripple = $('<span class="ripple"></span>');
        $(this).append(ripple);

        var x = e.pageX - $(this).offset().left;
        var y = e.pageY - $(this).offset().top;

        ripple.css({
            left: x + 'px',
            top: y + 'px'
        });

        setTimeout(function () {
            ripple.remove();
        }, 600);
    });

    // Responsive sidebar behavior
    function handleResize() {
        if ($(window).width() > 768) {
            $('#sidebar').removeClass('active');
        } else {
            $('#sidebar').addClass('active');
        }
    }

    // Initial check
    handleResize();

    // On window resize
    $(window).on('resize', handleResize);

    // Highlight active menu item
    var currentPath = window.location.pathname;
    $('.sidebar ul li a').each(function () {
        var href = $(this).attr('href');
        if (currentPath.indexOf(href) > -1 && href !== '/') {
            $(this).parent().addClass('active');
        }
    });

    // Smooth scroll for anchor links
    $('a[href^="#"]').on('click', function (e) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            e.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 100
            }, 1000);
        }
    });

    // Tooltip initialization
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Popover initialization
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});

// Add loading state to forms
$('form').on('submit', function () {
    var btn = $(this).find('[type="submit"]');
    btn.prop('disabled', true);
    btn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Processing...');
});

// Table row click handler
$('.table-clickable tbody tr').on('click', function () {
    var href = $(this).data('href');
    if (href) {
        window.location = href;
    }
});

// Search functionality
$('#search').on('input', function () {
    var searchTerm = $(this).val().toLowerCase();
    $('.searchable').each(function () {
        var text = $(this).text().toLowerCase();
        if (text.indexOf(searchTerm) > -1) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
});

// Auto-save form data to localStorage
function autoSaveForm(formId) {
    var form = $('#' + formId);
    form.find('input, textarea, select').on('change', function () {
        var data = form.serializeArray();
        localStorage.setItem(formId + '_data', JSON.stringify(data));
    });

    // Restore on page load
    var savedData = localStorage.getItem(formId + '_data');
    if (savedData) {
        var data = JSON.parse(savedData);
        $.each(data, function (i, field) {
            $('[name="' + field.name + '"]').val(field.value);
        });
    }
}

// Clear localStorage after successful form submission
function clearFormData(formId) {
    localStorage.removeItem(formId + '_data');
}

// Add ripple effect CSS
$('<style>')
    .prop('type', 'text/css')
    .html(`
        .ripple {
            position: absolute;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.6);
            transform: scale(0);
            animation: ripple-animation 0.6s ease-out;
            pointer-events: none;
        }
        
        @keyframes ripple-animation {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
    `)
    .appendTo('head');
