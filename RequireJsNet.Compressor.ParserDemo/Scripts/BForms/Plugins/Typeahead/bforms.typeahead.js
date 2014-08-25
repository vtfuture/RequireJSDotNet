(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-typeahead', ['jquery', 'typeahead'], function ($) {
            factory($, window.jQuery.fn.typeahead);
        });
    } else {
        factory(window.jQuery, window.jQuery.fn.typeahead);
    }
})(function ($, typeahead) {

    var typeaheadSelect = function ($elem, opts) {
        this.$elem = $elem;
        this.options = opts;
        this.init();

        this.$elem.data('typeaheadSelect', this);
        this.$input.data('typeaheadSelect', this);

        return $elem;
    };

    typeaheadSelect.prototype.init = function () {
        this._settings = this._getSettings();
        this._initElement();
        this._applyTypeahead();
    };

    typeaheadSelect.prototype._getSettings = function () {
        var settings = {
            value: ''
        };

        if (this.options.ajaxQuery === false) {
            settings.local = [];

            if (this.$elem.is('select')) {

                this.$elem.find('option').each($.proxy(function (idx, opt) {
                    var $opt = $(opt),
                        val = $opt.val();

                    if (typeof val !== "undefined" && val !== '') {

                        var tagText = this.options.textTag === true ? $opt.text() : val;
                        settings.local.push(tagText);

                        if ($opt.attr('selected')) {
                            settings.value = tagText;
                        }

                    } else {
                        settings.placeholder = $opt.text();
                    }

                }, this));
            }
        }

        return settings;

    };

    typeaheadSelect.prototype._initElement = function () {
        var $input = $('<input></input>');

        $input.prop('type', 'text');
        $input.prop('id', this.$elem.prop('id'));
        $input.prop('name', this.$elem.prop('name'));
        $input.prop('class', this.$elem.prop('class'));

        this._settings.name = this.$elem.prop('name');

        var attrs = this.$elem[0].attributes,
		    i = 0,
		    l = attrs.length,
		    toRemove = [];

        for (; i < l; i++) {
            var attr = attrs[i];
            if (typeof attr.nodeName !== 'undefined' && attr.nodeName.indexOf('data-') === 0) {
                $input.attr(attr.nodeName, attr.nodeValue);
                toRemove.push(attr.nodeName);
            }
        }

        for (var index = 0; index < toRemove.length; index++) {
            this.$elem.removeAttr(toRemove[index]);
        }

        $input.data(this.$elem.data());

        this.$elem.prop('id', 'autocomplete_' + $input.prop('id'));
        this.$elem.prop('name', 'autocomplete_' + $input.prop('name'));

        this.$input = $input;

        this.$elem.hide()
            .before(this.$input);

    };

    typeaheadSelect.prototype._applyTypeahead = function () {

        if (typeof this._settings.value !== "undefined") {
            this.$input.val(this._settings.value);
        }

        this.$input.typeahead(this._settings);

        if (typeof this._settings.placeholder !== "undefined") {
            this.$input.prop('placeholder', this._settings.placeholder);
        }
    };

    typeaheadSelect.prototype._updateTypeahead = function (value, name, preserveInput) {

        if (preserveInput !== true) {
            this.$input.val('');
        }

        this.$input.typeahead("destroy");

        this.$input.typeahead({
            name: name,
            local: value
        });

    };

    jQuery.fn.typeaheadSelectDefaults = {
        ajaxQuery: false,
        textTag: true
    };

    $.fn.bsTypeahead = function (opts) {
        return new typeaheadSelect($(this), $.extend(true, {}, $.fn.typeaheadSelectDefaults, opts));
    };

    $.fn.bsTypeaheadUpdate = function () {
        var instance = $(this).data('typeaheadSelect');

        if (typeof instance !== "undefined") {

            instance._updateTypeahead.apply(instance, arguments);

        }
    };

});