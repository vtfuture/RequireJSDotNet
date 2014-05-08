(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-select2', ['jquery', 'select2'], function ($) {
            factory($, window.jQuery.fn.select2);
        });
    } else {
        factory(window.jQuery, window.jQuery.fn.select2);
    }
})(function ($, select2) {

    var selectInput2 = function ($elem, opts) {
        this.$elem = $elem;

        //if (this.$elem.hasClass('bs-hasBformsSelect')) return;

        this.options = opts;
        this.init();
    };

    jQuery.fn.bsSelectInputDefaults = {
        textTag: true,
        select2TagsOpts: {
            tokenSeparators: [",", " "],
            tags: [],
            selectedValues: []
        },
        tags: true
    };

    selectInput2.prototype.init = function () {
        if (this.options.tags === true) {
            this._selectSettings = this._getSelectTagsSettings();
            this._initElement();
            this._applySelect2(this.$input);
        } else {
            this._selectSettings = this._getMultiSelectSettings(this.$elem);
            this._applySelect2(this.$elem);
        }

        this.$elem.addClass('bs-hasBformsSelect');
    };

    selectInput2.prototype._initElement = function () {
        var $input = $('<input></input>');

        $input.prop('type', 'hidden');

        $input.prop('id', this.$elem.prop('id'));
        $input.prop('name', this.$elem.prop('name'));
        $input.prop('class', this.$elem.prop('class'));
        $input.prop('value', this._selectSettings.selectedValues);

        $input.data(this.$elem.data());

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

        this.$elem.prop('id', 'tag_' + $input.prop('id'));
        this.$elem.prop('name', 'tag_' + $input.prop('name'));
        
        this.$input = $input;

        this.$input.on('change', function () {
            if (typeof $(this).valid === "function") {
                $(this).valid();
            }
        });

        this.$elem.before(this.$input);
        this.$elem.hide();
    };

    selectInput2.prototype._getSelectTagsSettings = function () {
        var settings = $.extend(true, {}, this.options.select2TagsOpts);

        if (this.$elem.is('select')) {
            this.$elem.find('option').each($.proxy(function (idx, opt) {
                var $opt = $(opt),
                    val = $opt.val();

                if (typeof val !== "undefined" && val !== '') {

                    var tagText = this.options.textTag === true ? $opt.text() : val;
                    settings.tags.push(tagText);

                    if ($opt.prop('selected') == true) {
                        settings.selectedValues.push(tagText);
                    }

                } else {
                    settings.placeholder = $opt.text();
                }

            }, this));
        }

        return settings;
    };

    selectInput2.prototype._getMultiSelectSettings = function () {
        var settings = {
        };

        var $firstOpt = this.$elem.find('option:first'),
            val = $firstOpt.val();

        if (typeof val === "undefined" || val == '') {
            settings.placeholder = $firstOpt.text();
            $firstOpt.remove();
        }

        return settings;
    };


    selectInput2.prototype._applySelect2 = function ($target) {
        $target.select2(this._selectSettings);
    };

    $.fn.bsSelectInput = function (opts) {

        if ($(this).length === 0) {
            console.warn('bsSelectInput must be applied on an element');
            return $(this);
        }

        return new selectInput2($(this), $.extend(true, {}, $.fn.bsSelectInputDefaults, opts));
    };

});