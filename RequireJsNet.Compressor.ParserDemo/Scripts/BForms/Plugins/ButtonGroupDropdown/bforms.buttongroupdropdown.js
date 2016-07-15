(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-buttonGroupDropdown', ['jquery', 'bootstrap'], function ($) {
            factory($);
        });
    } else {
        factory(window.jQuery);
    }
})(function ($) {

    var buttonGroupDropdown = function ($elem, opts) {

        this.$select = $elem;

        if (this.$select.hasClass('has-bsButtonGroupDropdown')) {
            return this.$select;
        }

        this.options = opts;
        this._init();

        return this.$select;
    };

    buttonGroupDropdown.prototype._init = function () {

        this._initSelectors();

        this._initDropdown();

        this._delegateEvents();

        this._monitorSource();

        this.$select.addClass('has-bsButtonGroupDropdown');
    };

    buttonGroupDropdown.prototype._initDropdown = function () {
        this.$element.dropdown();
    };

    buttonGroupDropdown.prototype._initSelectors = function () {

        this.$btnGroup = this.$select.siblings(this.options.containerSelector);
        this.$element = this.$select.parent().find(this.options.toggleSelector + '[data-dropdown-for="' + this.$select.attr('id') + '"]');

    };

    buttonGroupDropdown.prototype._delegateEvents = function () {
        this.$btnGroup.on('click', this.options.optionSelector, $.proxy(this._onOptionClick, this));
    };

    buttonGroupDropdown.prototype._monitorSource = function () {

        //for mozilla and ie
        this.$select.on('propertychange', $.proxy(this._sync,this));

        //webkit
        if (typeof window.MutationObserver !== "undefined") {
            if (this._propertyObserver) {
                delete this._propertyObserver;
                this._propertyObserver = null;
            }

            this._propertyObserver = new window.MutationObserver($.proxy(this._mutationCallback, this));
            this._propertyObserver.observe(this.$select.get(0), { attributes: true, subtree: false });
        }

    };

    //#region events
    buttonGroupDropdown.prototype._onOptionClick = function (e) {
        e.preventDefault();

        var $target = $(e.currentTarget),
            val = $target.data('value');

        if ($target.attr('data-selected') !== "true") {
            this.$select.val(val).trigger('customchange');

            this._updateButtonText($target);
        } else {

            if (this.$select.find('option[value=""]').length) {
                this.$select.val('').trigger('customchange');
                this._updateButtonText();
            }
        }

    }

    buttonGroupDropdown.prototype._mutationCallback = function (mutations) {

        for (var key in mutations) {
            this._sync();
        }
    };
    //#endregion

    //#region helpers
    buttonGroupDropdown.prototype._updateButtonText = function ($selectedItem) {

        if ($selectedItem instanceof $) {

            this.$element.html(this._buildButtonText($selectedItem.text()));

            //unmark previous marked option
            this.$btnGroup.find(this.options.optionSelector + '[data-selected="true"]').removeClass(this.options.markClass).attr('data-selected', false);

            $selectedItem.addClass(this.options.markClass).attr('data-selected', true);
        } else {
            var $selectedOption = this.$select.find('option:selected');

            if ($selectedOption.val() !== '') {
                var $toBeMarked = this.$btnGroup.find(this.options.optionSelector + '[data-value="' + $selectedOption.val() + '"]');

                if ($toBeMarked.length) {
                    this._updateButtonText($toBeMarked);
                }

            } else {
                this.$btnGroup.find(this.options.optionSelector + '[data-selected="true"]').removeClass(this.options.markClass).attr('data-selected', false);
                this.$element.html(this._buildButtonText(this.$element.data('placeholder')));
            }
        }

    };

    buttonGroupDropdown.prototype._buildButtonText = function (label) {
        var $caretSpan = $('<span>', { 'class': 'caret' });
        return label + ' ' + $caretSpan[0].outerHTML;
    }

    buttonGroupDropdown.prototype._sync = function () {
        this._syncCssClasses(this.$element, this.$select);
    };

    buttonGroupDropdown.prototype._syncCssClasses = function ($dest, $src) {

        var toBeSynced = this.options.syncClasses;

        $.each(toBeSynced, function () {
            if ($src.hasClass(this)) {
                $dest.addClass(this.toString());
            } else {
                $dest.removeClass(this.toString());
            }
        });

    }
    //#endregion

    $.fn.bsButtonGroupDropdownDefaults = {
        optionsListSelector: '.bs-dropdownList',
        optionSelector: '.bs-buttonGroupDropdownOption',
        containerSelector: '.bs-buttonGroupDropdownContainer',
        toggleSelector: '.bs-buttonGroupDropdownToggle',


        markClass: 'mark',
        syncClasses: ['input-validation-error', 'valid']
    };

    $.fn.bsButtonGroupDropdown = function (opts) {
        if ($(this).length === 0) {
            console.warn('bsButtonGroupDropdown must be applied on an element');
            return $(this);
        }

        return new buttonGroupDropdown($(this), $.extend(true, {}, $.fn.bsButtonGroupDropdownDefaults));
    };

});;