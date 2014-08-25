/**
 * Select2 Romanian translation.
 */
(function(factory) {
    if (typeof define === "function" && define.amd) {
        define('select2-ro', ['jquery', 'select2'], factory);
    } else {
        factory(window.jQuery);
    }
}(function($) {
    $.extend($.fn.select2.defaults, {
        formatNoMatches: function () { return "Nu a fost găsit nimic"; },
        formatInputTooShort: function (input, min) { var n = min - input.length; return "Vă rugăm să introduceți incă " + n + " caracter" + (n == 1 ? "" : "e"); },
        formatInputTooLong: function (input, max) { var n = input.length - max; return "Vă rugăm să introduceți mai puțin de " + n + " caracter" + (n == 1 ? "" : "e"); },
        formatSelectionTooBig: function (limit) { return "Aveți voie să selectați cel mult " + limit + " element" + (limit == 1 ? "" : "e"); },
        formatLoadMore: function (pageNumber) { return "Se încarcă..."; },
        formatSearching: function () { return "Căutare..."; },
        formatPlaceholder: function () {
            return "Alegeți";
        },
        formatSaveItem: function (val) {
            return "Salvează " + val;
        },
    });
}));
