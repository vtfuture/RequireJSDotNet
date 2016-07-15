(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-mixedButtonGroup', ['jquery', 'bootstrap'], factory);
    } else {
        factory(window.jQuery);
    }
})(function ($) {

    var mixedButtonGroup = function ($elem, opts) {

        this.$select = $elem;

        if (this.$select.hasClass('has-bsMixedButtonGroup')) {
            return this.$select;
        }

        this.options = opts;

        this._init();

        return this.$select;
    };

    //#region init
    mixedButtonGroup.prototype._init = function () {

        this._initSelectors();

        this._initDropdown();

        this._delegateEvents();

        this._monitorSource();

        this.$select.addClass('has-bsMixedButtonGroup');

    };

    mixedButtonGroup.prototype._initDropdown = function () {
        this.$element.dropdown();

        this.$buttonsContainer.on('show.bs.dropdown', $.proxy(this._onShowDropdown, this));
        this.$buttonsContainer.on('hide.bs.dropdown', $.proxy(this._onHideDropdown, this));
    };

    mixedButtonGroup.prototype._initSelectors = function () {
        this.$btnGroup = this.$select.siblings(this.options.containerSelector);
        this.$buttonsContainer = this.$btnGroup.find(this.options.buttonsContainerSelector);
        this.$element = this.$select.parent().find(this.options.toggleSelector + '[data-dropdown-for="' + this.$select.attr('id') + '"]');
    };

    mixedButtonGroup.prototype._delegateEvents = function () {

        this.$btnGroup.on('click', this.options.buttonSelector, $.proxy(this._onButtonClick, this));

        this.$btnGroup.on('click', this.options.optionSelector, $.proxy(this._onOptionClick, this));

        this.$select.on('bs.internalSelectChange', $.proxy(this._onInternalSelectChange, this));

    };

    mixedButtonGroup.prototype._monitorSource = function () {

    };
    //#endregion

    //#region events
    mixedButtonGroup.prototype._onShowDropdown = function(e) {
        this.$btnGroup.addClass('open');
    };

    mixedButtonGroup.prototype._onHideDropdown = function (e) {
        this.$btnGroup.removeClass('open');
    };

    mixedButtonGroup.prototype._onButtonClick = function (e) {
        e.preventDefault();

        var $btn = $(e.currentTarget),
            value = $btn.data('value');

        if (value == this._getCurrentValue()) {
            this._updateValue();
        } else {
            this._updateValue(value);
        };

    };

    mixedButtonGroup.prototype._onOptionClick = function (e) {

        e.preventDefault();

        var $option = $(e.currentTarget),
            value = $option.data('value');


        if (value == this._getCurrentValue()) {
            this._updateValue();
        } else {
            this._updateValue(value);
        };

    };

    mixedButtonGroup.prototype._onInternalSelectChange = function (e) {

        var value = this._getCurrentValue();

        var $previousMarked = this.$btnGroup.find('.' + this.options.selectedClass);

        $previousMarked.removeClass(this.options.selectedClass);
        $previousMarked.removeClass(this.options.markClass);


        if (value == '') {
            if ($previousMarked.parents(this.options.optionsListSelector)) {
                this.$element.removeClass(this.options.selectedClass);
                this._updateButtonText();
            }
        } else {
            var $toBeMarked = this.$btnGroup.find('*[data-value="' + value + '"]');

            if ($previousMarked.length) {

                if ($previousMarked.parents(this.options.optionsListSelector).length) {
                    if ($toBeMarked.parents(this.options.optionsListSelector).length) {
                        this._updateButtonText($toBeMarked);
                    } else {
                        this.$element.removeClass(this.options.selectedClass);
                        this._updateButtonText();
                    }
                }
            }

            $toBeMarked.addClass(this.options.selectedClass);

            if ($toBeMarked.parents(this.options.optionsListSelector).length) {
                this.$element.addClass(this.options.selectedClass);
                this._updateButtonText($toBeMarked);
                $toBeMarked.addClass(this.options.markClass);
            }
        }

        this.$select.trigger('customchange');
    };
    //#endregion

    //#region helpers
    mixedButtonGroup.prototype._updateValue = function (value) {

        if (typeof value === "undefined" || value == '') {
            if (this.$select.find('option[value=""]').length) {
                this.$select.val('');
                this.$select.trigger('bs.internalSelectChange', 0, {});
            }
        } else {
            this.$select.val(value);
            this.$select.trigger('bs.internalSelectChange', 0, {});
        }

    };

    mixedButtonGroup.prototype._getCurrentValue = function () {
        return this.$select.val();
    }

    mixedButtonGroup.prototype._updateButtonText = function ($selectedItem) {

        if ($selectedItem instanceof $) {

            this.$element.html(this._buildButtonText($selectedItem.text()));

            //unmark previous marked option
            this.$btnGroup.find(this.options.optionSelector + '[data-selected="true"]').removeClass(this.options.selectedClass).attr('data-selected', false);

            $selectedItem.addClass(this.options.selectedClass).attr('data-selected', true);
        } else {
            var $selectedOption = this.$select.find('option:selected');

            if ($selectedOption.val() !== '') {
                var $toBeMarked = this.$btnGroup.find(this.options.optionSelector + '[data-value="' + $selectedOption.val() + '"]');

                if ($toBeMarked.length) {
                    this._updateButtonText($toBeMarked);
                } else {
                    this.$element.html(this._buildButtonText(this.$element.data('placeholder')));
                }

            } else {
                this.$btnGroup.find(this.options.optionSelector + '[data-selected="true"]').removeClass(this.options.selectedClass).attr('data-selected', false);
                this.$element.html(this._buildButtonText(this.$element.data('placeholder')));
            }
        }

    };

    mixedButtonGroup.prototype._buildButtonText = function (label) {
        var $caretSpan = $('<span>', { 'class': 'caret' });
        return label + ' ' + $caretSpan[0].outerHTML;
    }
    //#endregion

    $.fn.bsMixedButtonGroupDefaults = {
        optionsListSelector: '.bs-dropdownList',
        buttonSelector: '.bs-buttonGroupItem',
        optionSelector: '.bs-buttonGroupDropdownOption',
        containerSelector: '.bs-mixedButtonGroupDropdownContainer',
        toggleSelector: '.bs-buttonGroupDropdownToggle',
        buttonsContainerSelector : '.bs-buttonsContainer',


        selectedClass : 'selected',
        markClass: 'mark',
        syncClasses: ['input-validation-error', 'valid']
    };

    $.fn.bsMixedButtonGroup = function (opts) {
        if ($(this).length === 0) {
            console.warn('bsMixedButtonGroup must be applied on an element');
            return $(this);
        }

        return new mixedButtonGroup($(this), $.extend(true, {}, $.fn.bsMixedButtonGroupDefaults));
    };

});