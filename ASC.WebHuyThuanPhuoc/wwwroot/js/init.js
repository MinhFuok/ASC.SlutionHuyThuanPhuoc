(function ($) {
    $(function () {
        $('.sidenav').sidenav();
        $('.parallax').parallax();
        $('.collapsible').collapsible();
    });
})(jQuery);

window.history.forward();

function noBack() {
    window.history.forward();
}

window.onload = noBack;
window.onpageshow = function (evt) {
    if (evt.persisted) {
        noBack();
    }
};

window.onunload = function () { };

document.addEventListener('contextmenu', function (e) {
    e.preventDefault();
});