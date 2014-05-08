(function (factory) {

    if (typeof define === "function" && define.amd) {
        define('bforms-datepicker-range', ['jquery', 'bforms-datepicker', 'bforms-datepicker-tmpl'], factory);
    } else {
        factory(window.jQuery, window.bsDatepicker, window.bsDatepickerRenderer);
    }

}(function ($, bDatepicker, bDatepickerRenderer) {

    var bRangePicker = function ($elem, options) {

        this.$element = $elem;
        this.options = $.extend(true, {}, options);
        this.init();
    };

    bRangePicker.prototype.init = function () {

        this.renderer = new bDatepickerRenderer();

        if (this.options.checkForMobileDevice == true) {
            this.options.inlineMobile = $.browser != null && $.browser.mobile == true;
        }

        if (this.options.inlineMobile && this.options.inline != true) {

            this._initInlineMobile();

            if (this.$element.is('input')) {
                if (this.options.readonlyInput || ($.browser != null && $.browser.mobile == true)) {
                    this.$element.prop('readonly', true);
                }
            }

        } else {

            if (this.$element.is('input')) {
                this.$input = this.$element;

                if (this.options.readonlyInput || ($.browser != null && $.browser.mobile == true)) {
                    this.$input.prop('readonly', true);
                }
            }

            this._initLang(this.options.language);

            this.isInline = this.options.inline || false;

            this._visible = this.options.inline ? true : false;
            if (this.options.visible == false) {
                this._visible = false;
            }

            if (typeof this.options.placeholderValue === "undefined") {
                this.options.placeholderValue = this.options.placeholder;
            }

            this._buildElement();
            this._addHandlers();

            this.$element.data('bRangepicker', this);
            this.$element.addClass('hasRangepicker');

            this._timeoutHandler = null;
        }
    };

    bRangePicker.prototype._buildElement = function () {

        if (this.options.allowDeselect) {
            this.options.allowDeselectStart = this.options.allowDeselectEnd = true;
        }

        this.$container = this.renderer.renderRangeContainer({
            applyText: this.options.applyText,
            cancelText: this.options.cancelText,
            fromText: this.options.fromText,
            presetRangesText: this.options.presetRangesText,
            presetRangesPlaceholderText : this.options.presetRangesPlaceholderText,
            toText: this.options.toText,
            allowDeselectStart: this.options.allowDeselectStart,
            allowDeselectEnd: this.options.allowDeselectEnd,
            theme: this.options.theme,
            hideRanges: this.options.hideRanges,
            hasPresetRanges: this.options.usePresetRanges,
            presetRanges: this.options.usePresetRanges ? this._getPresetRanges() : null
        });

        this.$start = this.$container.find('.bs-start-replace');
        this.$end = this.$container.find('.bs-end-replace');

        this.$startLabel = this.$container.find('.bs-rangeStartLabel');
        this.$endLabel = this.$container.find('.bs-rangeEndLabel');

        if (typeof this.options.language !== "undefined") {
            if (typeof this.options.startOptions !== "undefined" && typeof this.options.startOptions.language === "undefined") {
                this.options.startOptions.language = this.options.language;
            }

            if (typeof this.options.endOptions !== "undefined" && typeof this.options.endOptions.language === "undefined") {
                this.options.endOptions.language = this.options.language;
            }
        }

        if (typeof this.options.theme !== "undefined") {
            this.options.startOptions.theme = this.options.endOptions.theme = 'blue';
        }

        var startOptions = this.options.startOptions;

        startOptions.maxDate = this.options.endOptions.maxDate || null;

        this.options.allowInvalidMinMax = this.options.allowInvalidMinMax && !this.options.startOptions.defaultDateValue && !this.options.endOptions.defaultDate;

        this.$start.bsDatepicker($.extend(true, {}, {
            onChange: $.proxy(this.onStartChange, this),
            onDayMouseOver: $.proxy(this.onStartDaysMouseOver, this),
            onDaysMouseOut: $.proxy(this.onStartDaysMouseOut, this),
            allowSame: this.options.allowSame,
            allowDeselect: this.options.allowDeselectStart,
            deferredRender: this.options.deferredRender
        }, startOptions, {
            inline: true,
            ShowClose: false,
        }));

        var endOptions = this.options.endOptions;

        endOptions.minDate = startOptions.minDate || null;

        this.$end.bsDatepicker($.extend(true, {}, {
            defaultDateValue: this.$start.bsDatepicker('getUnformattedValue'),
            defaultDate: endOptions.type == 'timepicker' ? '+1h' : '+1d',
            onChange: $.proxy(this.onEndChange, this),
            onDayMouseOver: $.proxy(this.onEndDaysMouseOver, this),
            onDaysMouseOut: $.proxy(this.onEndDaysMouseOut, this),
            allowSame: this.options.allowSame,
            allowDeselect: this.options.allowDeselectEnd,
            deferredRender: this.options.deferredRender
        }, endOptions, {
            inline: true,
            ShowClose: false,
        }));

        if (!this.options.allowInvalidMinMax) {
            this.$start.bsDatepicker('option', 'maxDate', this.$end.bsDatepicker('getUnformattedValue'));
            this.$end.bsDatepicker('option', 'minDate', this.$start.bsDatepicker('getUnformattedValue'));
        }

        this.$start.bsDatepicker('option', 'beforeShowDay', $.proxy(this.beforeShowDay, this));
        this.$end.bsDatepicker('option', 'beforeShowDay', $.proxy(this.beforeShowDay, this));

        this._setStartLabel(this.$start.bsDatepicker('getValue'));
        this._setEndLabel(this.$end.bsDatepicker('getValue'));

        if (this.isInline) {

            this.$element.after(this.$container.show());
            this.$element.hide();

            if (this.isInline) {
                this.$container.addClass('bs-inline-picker');
            }

        } else {
            $('body').append(this.$container);
            this._positionRange();
        }

        if (this._visible == false) {
            this.$container.hide();
        }

        var allowApplyRange = this.options.startOptions.initialValue && this.options.endOptions.initialValue ||
                         this.options.startOptions.initialValue && this.options.allowDeselectEnd ||
                         this.options.endOptions.initialValue && this.options.allowDeselectStart;

        var startValue = this.$start.bsDatepicker('getValue'),
            endValue = this.$end.bsDatepicker('getValue');

        if (startValue != null && startValue != '') {
            this._valueSettedForFirst = true;
        }

        if (endValue != null && endValue != '') {
            this._valueSettedForSecond = true;
        }

        if (allowApplyRange) {
            this.applyRange();
        }
    };

    bRangePicker.prototype._addHandlers = function () {

        if (!this.isInline || this.inlineMobile) {
            $(document).on('scroll', $.proxy(function () {
                if (this._visible) {
                    window.clearTimeout(this._timeoutHandler);
                    this._timeoutHandler = window.setTimeout($.proxy(this._positionRange, this), 20);
                }
            }, this));

            $(window).on('resize', $.proxy(function () {
                this._positionRange();
            }, this));

            if (this.options.openOnFocus === true) {
                this.$element.on('focus', $.proxy(function (e) {
                    this.show();
                }, this));
            }

            if (this.options.closeOnBlur === true) {
                $(document).on('mouseup', $.proxy(function (e) {

                    var $target = $(e.target);

                    if ($target[0] != this.$element[0] && $target.closest('.bs-range-picker').length === 0) {
                        if (!$target.hasClass('glyphicon') || $target.parent()[0] != this.$input.parent()[0]) {

                            var allowHide = true;

                            if (typeof this.options.toggleButtons !== "undefined") {
                                for (var toggle in this.options.toggleButtons) {
                                    var $toggleElement = $(this.options.toggleButtons[toggle].selector);
                                    if ($target.closest($toggleElement).length > 0) {
                                        allowHide = false;
                                    }
                                }
                            }

                            if (allowHide) {
                                this.hide();
                            }
                        }
                    }

                }, this));
            }

            if (typeof this.$input !== "undefined" && this.$input.length) {
                this.$input.on('change', $.proxy(this.onInputChange, this));
            }

            if (typeof this.options.toggleButtons !== "undefined" && $.isArray(this.options.toggleButtons)) {
                for (var idxT in this.options.toggleButtons) {

                    var currentTogle = this.options.toggleButtons[idxT];
                    this.unbindEvent(currentTogle.selector, currentTogle.event);

                    $('body').on(currentTogle.event, currentTogle.selector, $.proxy(function (e) {
                        if (this._visible) {
                            this.hide();
                        } else {
                            this.show();
                        }
                    }, this));
                }
            }

            if (typeof this.$input !== "undefined") {
                this.$input.on('focusout', $.proxy(function (e) {

                    if (e.relatedTarget && $(e.relatedTarget).parents(this.$element).length == 0) {
                        this.hide();
                    }

                }, this));
            }
        }

        this.$container.on('click', '.bs-applyRange', $.proxy(this.applyRangeClick, this));
        this.$container.on('click', '.bs-cancelRange', $.proxy(this.cancelRange, this));

        this.$container.on('click', '.bs-resetDateRange', $.proxy(this.cancelDate, this));

        this.$container.on('change', '.bs-preset-ranges', $.proxy(this._onPresetRangeChange, this));
    };

    bRangePicker.prototype._initInlineMobile = function () {

        var modalPickerOptions = $.extend(true, {}, this.options, {
            inline: true,
            altFields: [{
                selector: this.$element
            }],
            visible: false,
            closeOnBlur: false
        });

        var $pickerReplace = $('<div class="bs-picker-replace"></div>');
        this.$element.parent().after($pickerReplace);
        $pickerReplace.bsDateRange(modalPickerOptions);

        this.$element.on('click', function () {
            $pickerReplace.bsDateRange('show');
        });
    };

    bRangePicker.prototype._initLang = function (lang) {
        $.extend(true, this.options, $.fn.bsDateRangeLang[lang]);
    };

    //#region events
    bRangePicker.prototype.onInputChange = function (e) {
        var $target = $(e.currentTarget);

        var values = $target.val().split(' ' + this.options.delimiter + ' ');

        if (values.length == 2) {
            var startVal = values[0],
                endVal = values[1];

            var startDate = moment(startVal, this.$start.bsDatepicker('getFormat'), this.$start.bsDatepicker('option', 'language')),
                endDate = moment(endVal, this.$end.bsDatepicker('getFormat'), this.$end.bsDatepicker('option', 'language'));

            if (startDate != null && startDate.isValid() && this.$start.bsDatepicker('isValidDate', startDate)) {
                this.$start.bsDatepicker('setValue', startDate);
            }

            if (endDate != null && endDate.isValid() && this.$end.bsDatepicker('isValidDate', endDate)) {
                this.$end.bsDatepicker('setValue', endDate);
            }

            this.applyRange();

        } else {
            if (values[0] == '') {
                this.resetRange('');
                this._updateAltFields('', '');
            } else {
                this.applyRange();
            }
        }

    };

    bRangePicker.prototype.onStartChange = function (data) {

        if (data.date == null && typeof this.options.startOptions.minDate !== "undefined" && this.options.startOptions.minDate != '') {
            this.$end.bsDatepicker('option', 'minDate', this.options.startOptions.minDate);
        } else {
            this.$end.bsDatepicker('option', 'minDate', data.date);
        }

        this._setStartLabel(data.formattedDate);
        this.$startLabel.data('value', data.date);

        if (data.date != null) {
            this._valueSettedForFirst = true;
        } else {
            this._valueSettedForFirst = false;
        }

        var endValue = this.$end.data('bDatepicker').currentValue;

        if ((data.date != null) && endValue == null || !this.$end.bsDatepicker('isValidDate', endValue)) {
            this.$end.bsDatepicker('setValue', data.date.clone().add('days', 1));
        }

        this._trigger('onStartChange', data);

        if (this._resetRangeOnChange) {
            this.$container.find('.bs-preset-ranges').val('');

            this._resetRangeOnChange = false;
        }
    };

    bRangePicker.prototype.onEndChange = function (data) {

        if (data.date == null && typeof this.options.endOptions.maxDate !== "undefined" && this.options.endOptions.maxDate != '') {
            this.$start.bsDatepicker('option', 'maxDate', this.options.endOptions.maxDate);
        } else {
            this.$start.bsDatepicker('option', 'maxDate', data.date);
        }

        this._setEndLabel(data.formattedDate);
        this.$endLabel.data('value', data.date);

        if (data.date != null) {
            this._valueSettedForSecond = true;
        } else {
            this._valueSettedForSecond = false;
        }

        var startValue = this.$start.data('bDatepicker').currentValue;

        if ((data.date != null) && startValue != null && !this.$start.bsDatepicker('isValidDate', startValue)) {
            this.$start.bsDatepicker('setValue', data.date.clone().subtract('days', 1));
        }

        this._trigger('onEndChange', data);

        if (this._resetRangeOnChange) {
            this.$container.find('.bs-preset-ranges').val('');

            this._resetRangeOnChange = false;
        }
    };

    bRangePicker.prototype.applyRange = function (value) {
        this._startValue = this.$startLabel.data('value');

        if (typeof this._startValue === "undefined") {
            this._startValue = this.$start.bsDatepicker('getUnformattedValue');
            this.$startLabel.data(this._startValue);
        }

        this._endValue = this.$endLabel.data('value');
        if (typeof this._endValue === "undefined") {
            this._endValue = this.$end.bsDatepicker('getUnformattedValue');
            this.$endLabel.data(this._endValue);
        }

        var val = typeof value === "string" ? value : this.getValue();

        if (typeof this.$input !== "undefined") {
            this.$input.val(val);
            if (typeof this.$input.valid === "function" && this.$input.parents('form').length) {
                this.$input.valid();
            }
        }

        this._updateAltFields();

        this._trigger('onRangeChange', {
            $container: this.$container,
            startValue: this._startValue,
            endValue: this._endValue,
            $start: this.$start,
            $end: this.$end,
            value: this.getValue()
        });
    };

    bRangePicker.prototype.applyRangeClick = function (e) {
        if (e != null && typeof e.preventDefault === "function") {
            e.preventDefault();
            e.stopPropagation();
        }

        this.applyRange();
        if (this.options.allowHideOnApply) {
            this.hide();
        }
    };

    bRangePicker.prototype.cancelRange = function (e) {

        if (e != null && typeof e.preventDefault === "function") {
            e.preventDefault();
            e.stopPropagation();
        }

        this.hide();

        if (typeof this._startValue !== "undefined") {
            this.$start.bsDatepicker('setValue', this._startValue);
        }

        if (typeof this._endValaue !== "undefined") {
            this.$end.bsDatepicker('setValue', this._endValue);
        }
    };

    bRangePicker.prototype.cancelDate = function (e) {

        e.preventDefault();
        e.stopPropagation();
        var $target = $(e.currentTarget);

        if ($target.prev('.bs-rangeStartLabel').length) {
            this.$start.bsDatepicker('clearValue');
        } else {
            this.$end.bsDatepicker('clearValue');
        }
    };

    bRangePicker.prototype.onStartDaysMouseOver = function (momentDate, formattedDate, isValid) {
        if (isValid && this.$startLabel.val() != '') {
            this._setStartLabel(formattedDate);
        }
    };

    bRangePicker.prototype.onStartDaysMouseOut = function (momentDate, formattedDate) {

        if (this.$startLabel.val() != '') {
            this._setStartLabel(formattedDate);
        }
    };

    bRangePicker.prototype.onEndDaysMouseOver = function (momentDate, formattedDate, isValid) {
        if (isValid && this.$endLabel.val() != '') {
            this._setEndLabel(formattedDate);
        }
    };

    bRangePicker.prototype.onEndDaysMouseOut = function (momentDate, formattedDate) {
        if (this.$endLabel.val() != '') {
            this._setEndLabel(formattedDate);
        }
    };

    bRangePicker.prototype.beforeShowDay = function (val, dayModel) {
        var endValue = this.$end.bsDatepicker('getUnformattedValue'),
            startValue = this.$start.bsDatepicker('getUnformattedValue'),
            date = moment(val),
            returnModel = {};

        if ((this._valueSettedForSecond == false || endValue == null || date.isSame(endValue, 'day') || date.isBefore(endValue)) &&
            (startValue == null || date.isAfter(startValue) || date.isSame(startValue, 'day') || this._valueSettedForFirst == false)) {

            if (dayModel.selectable === true) {
                returnModel.cssClass = 'in-range';
            }
        }

        if (typeof this.options.beforeShowDay === "function") {
            $.extend(true, returnModel, this.options.beforeShowDay(val, dayModel));
        }

        if (returnModel.selectable === false && typeof returnModel.cssClass !== "undefined" && returnModel.cssClass.indexOf('in-range') !== -1) {
            returnModel.cssClass = returnModel.cssClass.replace('in-range', '');
        }

        return returnModel;
    };

    bRangePicker.prototype._onPresetRangeChange = function (e) {

        e.preventDefault();

        var $selectedOption = $(e.currentTarget).find('option:selected'),
            data = $selectedOption.data();

        if ($selectedOption.attr('value')) {

            this._resetRangeOnChange = false;

            var expressionFrom = data.expressionfrom,
                expressionTo = data.expressionto,
                priority = data.priority;

            if (data.priority == "to") {

                var toValue = this.$end.bsDatepicker('setValueFromExpression', data.expressionto, data.source == "now" ? moment() : null);
                this.$start.bsDatepicker('setValueFromExpression', data.expressionfrom, toValue);

            } else {
                var fromValue = this.$start.bsDatepicker('setValueFromExpression', data.expressionfrom, data.source == "now" ? moment() : null);
                this.$end.bsDatepicker('setValueFromExpression', data.expressionto, fromValue);
            }

            this._resetRangeOnChange = true;
            this.applyRangeClick();

        } else {

            this._resetRangeOnChange = false;
        }

    };
    //#endregion

    //#region private
    bRangePicker.prototype._setStartLabel = function (date) {
        this.$startLabel.val(date);

        if (this.options.allowDeselectStart) {
            if (date == '') {
                this.$startLabel.next('.bs-resetDateRange').hide();
            } else {
                this.$startLabel.next('.bs-resetDateRange').show();
            }
        }
    };

    bRangePicker.prototype._setEndLabel = function (date) {
        this.$endLabel.val(date);

        if (this.options.allowDeselectEnd) {
            if (date == '') {
                this.$endLabel.next('.bs-resetDateRange').hide();
            } else {
                this.$endLabel.next('.bs-resetDateRange').show();
            }
        }
    };

    bRangePicker.prototype._positionRange = function () {
        if (this.isInline) return;

        var xOrient = this.options.xOrient,
            yOrient = this.options.yOrient,
            rangeHeight = this.$container.outerHeight(true),
            rangeWidth = this.$container.outerWidth(true),
            elemOffset = this.$element.offset();

        if (yOrient != 'below' && yOrient != 'above') {

            var windowHeight = $(window).innerHeight(),
                scrollTop = $(document).scrollTop(),
                elemHeight = this.$element.outerHeight(true);

            var topOverflow = -scrollTop + elemOffset.top - rangeHeight,
                bottomOverflow = scrollTop + windowHeight - (elemOffset.top + elemHeight + rangeHeight);

            if (Math.max(topOverflow, bottomOverflow) === bottomOverflow) {
                yOrient = 'below';
            } else {
                yOrient = 'above';
            }
        }

        if (xOrient != 'right' && xOrient != 'left') {

            var windowWidth = $(window).innerWidth(),
                elemWidth = this.$element.outerWidth(true);

            var rightOverflow = elemOffset.left - (elemWidth > rangeWidth ? elemWidth - rangeWidth : rangeWidth - elemWidth),
                leftOverflow = windowWidth - (elemOffset.left + rangeWidth);

            if (rightOverflow > 0 && leftOverflow > 0) {
                xOrient = "left";
            } else {
                if (Math.max(rightOverflow, leftOverflow) === rightOverflow) {
                    xOrient = 'right';
                } else {
                    xOrient = 'left';
                }
            }
        }


        if (yOrient == 'below') {
            this.$container.css({
                top: elemOffset.top + this.$element.height() + 20
            });

            this.$container.removeClass('open-above');
            this.$container.addClass('open-below');

        } else if (yOrient == 'above') {
            this.$container.css({
                top: elemOffset.top - this.$element.height() - rangeHeight + 16
            });

            this.$container.removeClass('open-below');
            this.$container.addClass('open-above');
        }

        if (xOrient != 'right' && xOrient != 'left') {
            xOrient = 'left';
        }

        if (xOrient == 'left') {

            this.$container.css('left', elemOffset.left);
            this.$container.removeClass('open-right');
            this.$container.addClass('open-left');

        } else if (xOrient == 'right') {
            this.$container.css('left', elemOffset.left + this.$element.outerWidth() - this.$container.outerWidth());
            this.$container.removeClass('open-left');
            this.$container.addClass('open-right');
        }
    };

    bRangePicker.prototype._trigger = function (name, data, preventElementTrigger) {

        if (typeof this.options[name] === "function") {
            this.options[name](data);
        }

        if (preventElementTrigger !== true) {
            this.$element.trigger(name, data);
        }

    };

    bRangePicker.prototype._updateAltFields = function (startValue, endValue) {

        if (typeof this.options.startAltFields !== "undefined" && $.isArray(this.options.startAltFields)) {
            for (var idxS in this.options.startAltFields) {
                var currentS = this.options.startAltFields[idxS];
                var $toUpdateS = $(currentS.selector);

                if ($toUpdateS.length > 0) {
                    if ($toUpdateS.is('input')) {
                        $toUpdateS.val(typeof startValue === "undefined" ? moment.isMoment(this._startValue) ? this._startValue.format() : '' : startValue);
                    } else {
                        $toUpdateS.text(moment.isMoment(this._startValue) ? this._startValue.format() : '');
                    }
                }
            }
        }

        if (typeof this.options.endAltFields !== "undefined" && $.isArray(this.options.endAltFields)) {
            for (var idxE in this.options.endAltFields) {
                var currentE = this.options.endAltFields[idxE];

                var $toUpdateE = $(currentE.selector);
                if ($toUpdateE.length > 0) {
                    if ($toUpdateE.is('input')) {
                        $toUpdateE.val(typeof endValue === "undefined" ? moment.isMoment(this._endValue) ? this._endValue.format() : '' : '');
                    } else {
                        $toUpdateE.text(moment.isMoment(this._endValue) ? this._endValue.format() : '');
                    }
                }
            }
        }

        if (typeof this.options.altFields !== "undefined" && $.isArray(this.options.altFields)) {
            for (var idx in this.options.altFields) {
                var current = this.options.altFields[idx];

                var $toUpdate = $(current.selector);
                if ($toUpdate.length > 0) {
                    if ($toUpdate.is('input')) {
                        $toUpdate.val(this.getValue());
                    } else {
                        $toUpdate.text(this.getValue());
                    }
                }
            }
        }
    };

    bRangePicker.prototype._getPresetRanges = function () {

        if (typeof this.options.getPresetRanges == "function") return this.options.getPresetRanges();

        return [
            {
                text: 'Today',
                value: 1,
                source: 'now',
                expressionFrom: '_0h _0m _0s',
                expressionTo: '_23h _59m _59s',
                priority: 'to'
            }, {
                text: 'Last 12 hours',
                expressionFrom: '-12h',
                expressionTo: 'now',
                value: 2,
                priority: 'to'
            }, {
                text: "Last 7 days",
                expressionFrom: '-7d',
                expressionTo: 'now',
                value: 3,
                priority: 'to'
            }, {
                text: 'Last month',
                expressionFrom: '-1M',
                expressionTo: 'now',
                value: 4,
                priority: 'to'
            }
        ];
    };
    //#endregion

    //#region public methods
    bRangePicker.prototype.show = function () {
        if (this._visible !== true) {

            this._positionRange();

            var showData = {
                preventShow: false
            };

            this._trigger('beforeShow', showData);

            if (showData.preventShow == false) {

                if (this.options.deferredRender) {
                    this.$start.bsDatepicker('render', true);
                    this.$end.bsDatepicker('render', true);
                }

                this.$container.show();
                this._visible = true;

                this._trigger('afterShow', {
                    datepicker: this.$container,
                    element: this.$element,
                    datepickerType: this._type
                });
            }
        }

        return this;
    };

    bRangePicker.prototype.hide = function () {
        if (this._visible !== false) {

            var hideData = {
                preventHide: false
            };

            this._trigger('beforeHide', hideData);

            if (hideData.preventHide == false) {

                this.$container.hide();
                this._visible = false;

                if (typeof this.$input !== "undefined") {
                    this.$input.trigger('blur');
                }

                this._trigger('afterHide', {
                    datepicker: this.$container,
                    element: this.$element,
                    datepickerType: this._type
                });
            }
        }

        return this;
    };

    bRangePicker.prototype.getStartValue = function () {
        return this._valueSettedForFirst !== false ? this.$startLabel.val() : this.options.placeholderValue;
    };

    bRangePicker.prototype.getEndValue = function () {
        return this._valueSettedForSecond !== false ? this.$endLabel.val() : this.options.placeholderValue;
    };

    bRangePicker.prototype.resetRange = function (val) {
        this.$startLabel.data('value', this._startValue);
        this.$startLabel.val(this.$start.bsDatepicker('format', this._startValue));
        this.$start.bsDatepicker('setValue', this._startValue);

        this.$endLabel.data('value', this._endValue);
        this.$endLabel.val(this.$end.bsDatepicker('format', this._endValue));
        this.$end.bsDatepicker('setValue', this._endValue);
        this.applyRange(val);
    };

    bRangePicker.prototype.resetValue = function () {

        if (this.options.startOptions.initialValue) {
            this.$startLabel.data('value', this._startValue);
            this.$startLabel.val(this.$start.bsDatepicker('format', this._startValue));
            this.$start.bsDatepicker('setValue', this._startValue);
        } else {
            this.$startLabel.removeData('value');
            this.$start.bsDatepicker('resetValue');
            this._setStartLabel(this.$start.bsDatepicker('getValue'));
            this.$end.bsDatepicker('option', 'minDate', null)
        }

        if (this.options.endOptions.initialValue) {
            this.$endLabel.data('value', this._endValue);
            this.$endLabel.val(this.$end.bsDatepicker('format', this._endValue));
            this.$end.bsDatepicker('setValue', this._endValue);
        } else {
            this.$endLabel.removeData('value');
            this.$end.bsDatepicker('resetValue');
            this._setEndLabel(this.$end.bsDatepicker('getValue'));
            this.$start.bsDatepicker('option', 'maxDate', null);
        }

        this.applyRange('');

        if (typeof this.$input !== "undefined") {
            this.$input.val('');
        }

        if (this.options.startOptions.initialValue && this.options.endOptions.initialValue) {
            this.applyRange();
        } else {
            this._updateAltFields('', '');
        }
    };

    bRangePicker.prototype.destroy = function () {
        this.$element.removeData('bRangepicker');
        this.$element.removeClass('hasRangepicker');
        this.$container.remove();
    };

    bRangePicker.prototype.unbindEvent = function (selector, event, $context) {
        if (typeof selector !== "undefined" && typeof event !== "undefined") {

            if (typeof $context === "undefined") {
                $context = $('body');
            }
            $context.off(event, selector);
        }
    };

    bRangePicker.prototype.getValue = function () {

        if ((typeof this._valueSettedForFirst !== "undefined" && this._valueSettedForFirst !== false) || (typeof this._valueSettedForSecond !== "undefined" && this._valueSettedForSecond !== false)) {
            var startValue = this.getStartValue(),
                endValue = this.getEndValue();

            if (startValue === '' && endValue === '') {
                return '';
            } else {
                if (startValue === '') {
                    startValue = this.options.placeholderValue;
                }

                if (endValue === '') {
                    endValue = this.options.placeholderValue;
                }

                return startValue + ' ' + this.options.delimiter + ' ' + endValue;
            }

        } else {
            return '';
        }
    };

    bRangePicker.prototype.option = function (name, value) {
        if (typeof value === "undefined") {
            return this.options[name];
        } else {

            this.options[name] = value;

            if (typeof this["option_" + name] === "function") {
                this["option_" + name].apply(this, [value]);
            }
        }
    };

    bRangePicker.prototype.setStartValue = function (value) {

        var momentValue;

        if (!moment.isMoment(value)) {
            momentValue = moment(value, this.$start.bsDatepicker('getFormat'), this.$start.bsDatepicker('option', 'language'));
        } else {
            momentValue = value.clone();
        }

        if (momentValue != null && momentValue.isValid() && this.$start.bsDatepicker('isValidDate', momentValue)) {
            this.$start.bsDatepicker('setValue', momentValue);
        } else {
            this.$start.bsDatepicker('clearValue');
        }

        this.applyRange();


    };

    bRangePicker.prototype.setEndValue = function (value) {
        var momentValue;

        if (!moment.isMoment(value)) {
            momentValue = moment(value, this.$end.bsDatepicker('getFormat'), this.$end.bsDatepicker('option', 'language'));
        } else {
            momentValue = value.clone();
        }
        if (momentValue != null && momentValue.isValid() && this.$end.bsDatepicker('isValidDate', momentValue)) {
            this.$end.bsDatepicker('setValue', momentValue);
        }
        else {
            this.$end.bsDatepicker('clearValue');
        }

        this.applyRange();
    };
    //#endregion

    //#region options update
    bRangePicker.prototype.option_beforeShowDay = function () {
        this.$start.bsDatepicker('render');
        this.$end.bsDatepicker('render');
    };
    //#endregion

    $.fn.bsDateRangeDefaults = {
        openOnFocus: true,
        closeOnBlur: true,
        time: false,
        delimiter: '-',
        allowHideOnApply: true,
        startOptions: {
            inline: true,
            ShowClose: false
        },
        endOptions: {
            inline: true,
            ShowClose: false
        },
        language: 'en',
        allowInvalidMinMax: true,
        checkForMobileDevice: true,
        allowSame: true,
        deferredRender: true,
        usePresetRanges: false
    };

    $.fn.bsDateRangeLang = {
        'en': {
            applyText: 'Apply',
            cancelText: 'Cancel',
            fromText: 'From',
            toText: 'To',
            placeholder: 'not specified',
            presetRangesPlaceholderText: 'Choose preset',
            presetRangesText : 'Preset ranges'
        },
        'ro': {
            applyText: 'Setează',
            cancelText: 'Anulare',
            fromText: 'De la',
            toText: 'Până la',
            placeholder: 'nespecificat',
            presetRangesPlaceholderText: 'Alege intervalul',
            presetRangesText: 'Intervale predefinite'
        }
    };

    $.fn.bsDateRange = function () {
        var args = Array.prototype.slice.call(arguments, 0),
           options = args[0],
           methodParams = args.splice(1);

        if (typeof options === "undefined" || typeof options === "object") {
            return new bRangePicker($(this), $.extend(true, {}, $.fn.bsDateRangeDefaults, options));
        } else if (typeof options === "string") {
            var instance = (this).data('bRangepicker');
            if (typeof instance === "undefined") throw 'Cannot call method ' + options + ' before initializing plugin';
            else {
                return instance[options].apply(instance, methodParams);
            }
        }
    };
}));