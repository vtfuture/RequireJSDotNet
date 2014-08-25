define('bforms-form', [
    'jquery',
    'bforms-extensions',
    'jquery-ui-core',
    'bforms-validate-unobtrusive',
    'bforms-initUI',
    'bforms-ajax',
    'amplify'
], function () {

    var Form = function (opt) {
        this.options = opt;
        this._create();
    };


    Form.prototype.options = {
        uniqueName: null,
        hasGroupToggle: true,
        style: {},
        focusFirst: true,
        // save opened group containers in localstorage and retreive it later
        saveGroupContainerState: true,
        appendUrlToUniqueName: true,
        groupToggleSelector: '.bs-group_toggle',
        groupToggleContainerSelector: '.bs-group_toggle_container',
        groupToggleUp: 'glyphicon-chevron-up',
        groupToggleDown: 'glyphicon-chevron-down',
        glyphClass: '.glyphicon',
        actions: [],
        refreshUrl: null
    };

    //#region init
    Form.prototype._init = function () {

        if (!this.options.uniqueName) {
            this.options.uniqueName = this.element.attr('id');
        }

        if (!this.options.uniqueName) {
            throw 'form needs a unique name or the element on which it is aplied has to have an id attr';
        }

        this._buttons = new Array();

        this._initSelectors();

        this._addDelegates();

        this._initAmplifyStore();

        this._addActions(this.options.actions);

        var initUIPromise = this.$form.bsInitUI(this.options.style);
        initUIPromise.done($.proxy(function () {
            this._trigger('afterInitUI', 0, {
                name: this.options.uniqueName,
                form: this.$form
            });
        }, this));


    };

    Form.prototype._initSelectors = function () {

        this.$form = this.element.is('form') ? this.element : this.element.find('form');

    };

    Form.prototype._addDelegates = function () {

        if (this.options.hasGroupToggle) {
            this.element.on('click', this.options.groupToggleSelector, $.proxy(function (e) {
                var $elem = $(e.currentTarget);
                this.toggleGroup($elem);
            }, this));
        }
    };

    Form.prototype._initAmplifyStore = function () {

        if (this.options.saveGroupContainerState) {
            var amplifyKey = this._getKeyForGroupContainer();

            if (amplify.store(amplifyKey)) {
                try {
                    var savedGroupContainerValues = amplify.store(amplifyKey);
                    var context = this;
                    this.$form.find(this.options.groupToggleSelector).each(function (index) {

                        var keepOpen = $(this).closest(context.options.groupToggleContainerSelector).data("keepopen");

                        if (savedGroupContainerValues[index] || keepOpen) {
                            context.showGroup($(this));
                        } else {
                            context.hideGroup($(this));
                        }
                    });
                }
                catch (err) {
                    console.warn('Error on saving on local storage:' + err);
                }
            }
        }
    };

    Form.prototype._addActions = function (actions) {

        if (actions) {
            for (var i = 0; i < actions.length; i++) {
                this._addAction(actions[i]);
            }
        }

    };

    Form.prototype._addAction = function (buttonOpt) {

        var $elem = this.element.find(buttonOpt.selector);

        if ($elem.length == 0) {
            throw 'element with selector ' + buttonOpt.selector + ' is not found in form container ' + this.options.uniqueName;
        }

        $elem.on('click', {
            buttonOpt: buttonOpt,
            handlerContext: this.options.handlerContext
        }, $.proxy(this._evBtnClick, this));

        this._buttons.push({
            name: buttonOpt.name,
            elem: $elem
        });

    };
    //#endregion

    //#region events
    Form.prototype._evBtnClick = function (e) {

        e.preventDefault();

        var $me = $(e.currentTarget);
        var buttonOpt = e.data.buttonOpt;
        var handlerContext = e.data.handlerContext;

        if (buttonOpt.validate && !this._validate()) {
            return;
        }

        var validatedForm = this.$form.validate();

        var data;
        if (buttonOpt.parse) {
            data = this._parse();
            if (typeof buttonOpt.getExtraData === 'function') {
                buttonOpt.getExtraData.call(this, data);
            }

            this._trigger('beforeFormSubmit', 0, {
                $button: $me,
                buttonOpt: buttonOpt,
                data: data
            });
        }

        var action = $me.data('action') || buttonOpt.actionUrl;
        if (action) {
            $.bforms.ajax({
                name: this.options.uniqueName,
                url: action,
                data: data,
                callbackData: {
                    handler: buttonOpt.handler,
                    handlerContext: handlerContext,
                    sent: data
                },
                context: this,
                success: $.proxy(this._btnClickAjaxSuccess, this),
                error: $.proxy(this._btnClickAjaxError, this),
                loadingElement: this.element,
                loadingClass: 'loading',
                validationError: function (response) {

                    if (response != null && response.Errors != null) {

                        if (typeof validatedForm === "undefined") {
                            $.validator.unobtrusive.parse(this.$form);
                            validatedForm = this.$form.validate();
                        }

                        validatedForm.showErrors(response.Errors, true);

                        this._trigger("validationError", 0, response);
                    }
                }
            });
        } else {
            if (typeof buttonOpt.handler !== 'function') {
                throw 'action ' + buttonOpt.name + ' must implement an event handler or have an url option';
            }
            buttonOpt.handler.call(this, data);
        }

    };
    //#endregion

    //#region private methods
    Form.prototype._parse = function () {
        return this.element.parseForm(this.options.prefix ? this.options.prefix : '');
    };

    Form.prototype._saveGroupContainer = function () {
        if (this.options.saveGroupContainerState) {
            try {
                var arrayGroupContainers = [];
                var amplifyKey = this._getKeyForGroupContainer();

                this.$form.find(this.options.groupToggleContainerSelector).each(function () {
                    arrayGroupContainers.push($(this).next().is(":visible"));
                });

                amplify.store(amplifyKey, arrayGroupContainers);
            }
            catch (err) {
                console.warn('Error on saving on local storage:' + err);
            }
        }
    };

    Form.prototype._validate = function () {

        this.$form.removeData('validator').removeData('unobtrusiveValidation');

        $.validator.unobtrusive.parse(this.$form);

        var validatedForm = this.$form.validate();

        this._trigger('beforeFormValidation', 0, {
            validator: validatedForm,
            form: this.$form,
            name: this.options.uniqueName
        });
        return this.$form.valid();
    };

    Form.prototype._getAction = function (action) {

        return $.grep(this._buttons, function (elem, idx) {
            return elem.name == action;
        })[0];

    };

    Form.prototype._refresh = function (data) {
        var deferred = $.Deferred();

        return $.bforms.ajax({
            name: 'BsForm|Refresh|' + this.options.uniqueName,
            url: this.options.refreshUrl,
            data: data,
            callbackData: {
                deferred: deferred
            },
            context: this,
            success: this._onRefreshSuccess,
            error: this._onRefreshError,
            loadingElement: this.$form,
            loadingClass: 'loading'
        });
    };

    Form.prototype._getKeyForGroupContainer = function () {
        return this.options.appendUrlToUniqueName ? 'groupContainer|' + this.options.uniqueName + "|" + location.host + location.pathname : 'groupContainer|' + this.options.uniqueName;
    };
    //#endregion

    //#region ajax
    Form.prototype._btnClickAjaxSuccess = function (data, callbackData) {
        if (typeof callbackData.handler === 'function') {
            callbackData.handler.call(this, callbackData.sent, data, this);
        }
    };

    Form.prototype._btnClickAjaxError = function (data) {
        if (data.Message) {
            var validatedForm = this.$form.data('validator');
            validatedForm.showSummaryError(data.Message);
        }

        this._trigger("ajaxError", 0, data);
    };

    Form.prototype._onRefreshSuccess = function (response, callbackData) {

        var $html = $(response.Html);
        this.$form.html($html.html());

        this._initSelectors();
        this._initAmplifyStore();
        this._addActions(this.options.actions);

        this.element.bsResetForm(false);

        var initUIPromise = this.$form.bsInitUI(this.options.style);
        initUIPromise.done($.proxy(function () {
            this._trigger('afterInitUI', 0, {
                name: this.options.uniqueName,
                form: this.$form
            });
            callbackData.deferred.resolve();
        }, this));
    };

    Form.prototype._onRefreshError = function (data) {

        if (data.Message) {
            var validatedForm = this.$form.data('validator');
            validatedForm.showSummaryError(data.Message);
        }
    };
    //#endregion

    //#region public methods
    Form.prototype.getFormData = function (data) {

        var form = this.element.find('form');

        form.removeData("validator");
        form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);

        var validatedForm = form.validate();
        this._trigger('beforeFormValidation', 0, {
            validator: validatedForm,
            form: form,
            name: this.options.uniqueName
        });

        if (form.valid()) {
            data = this._parse();
        }

    };

    Form.prototype.parse = function (e) {
        return this._parse();
    };

    Form.prototype.triggerAction = function (actionName) {

        var action = this._getAction(actionName);

        if (action != null) {
            action.elem.trigger('click');
        }

    };

    Form.prototype.refresh = function (data) {
        return this._refresh(data);
    };

    Form.prototype.reset = function (e) {

        this.removeSummarySuccess();
        this.element.bsResetForm(this.options.focusFirst);
    };

    Form.prototype.toggleGroup = function ($elem) {
        if (this.options.hasGroupToggle) {
            $elem.find(this.options.glyphClass).toggleClass(this.options.groupToggleUp).toggleClass(this.options.groupToggleDown);
            $elem.toggleClass('open').closest(this.options.groupToggleContainerSelector).next().stop().slideToggle($.proxy(this._saveGroupContainer, this));
        }
    };

    Form.prototype.showGroup = function ($elem) {
        if (this.options.hasGroupToggle) {
            $elem.find(this.options.glyphClass).removeClass(this.options.groupToggleUp).addClass(this.options.groupToggleDown);
            $elem.addClass('open').closest(this.options.groupToggleContainerSelector).next().stop().slideDown($.proxy(this._saveGroupContainer, this));
        }
    };

    Form.prototype.hideGroup = function ($elem) {
        if (this.options.hasGroupToggle) {
            $elem.find(this.options.glyphClass).addClass(this.options.groupToggleUp).removeClass(this.options.groupToggleDown);
            $elem.removeClass('open').closest(this.options.groupToggleContainerSelector).next().stop().slideUp($.proxy(this._saveGroupContainer, this));
        }
    };

    Form.prototype.showSummarySuccess = function (message) {
        var $successContainer = this.$form.find('.bs-validation_summary');

        if ($successContainer.length == 0) {
            $successContainer = $('<div class="col-sm-12 col-lg-12 bs-validation_summary"></div>');
            this.$form.prepend($successContainer);
        }

        var $success = $('<div class="bs-form-success alert alert-success">' +
                                '<button class="close" data-dismiss="alert" type="button">×</button>' +
                                 message +
                            '</div>'
                        );

        $successContainer.html($success);
        return {
            element: $success
        };
    };
    Form.prototype.removeSummarySuccess = function () {
        var $successContainer = this.$form.find('.bs-validation_summary');
        if ($successContainer.length) {
            $successContainer.html('');
        }
    };
    //#endregion

    $.widget('bforms.bsForm', Form.prototype);

    return Form;
});
