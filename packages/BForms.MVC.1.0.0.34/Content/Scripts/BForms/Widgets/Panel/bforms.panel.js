(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-panel', ['jquery', 'bootstrap', 'amplify', 'bforms-ajax', 'bforms-form'], factory);
    } else {
        factory(window.jQuery);
    }
})(function ($) {

    var bsPanel = function () {

    };

    bsPanel.prototype.options = {
        collapse: true,
        loaded: false,
        editable: true,

        toggleSelector: '.bs-togglePanel',

        editSelector: '.bs-editPanel',
        cancelEditSelector: '.bs-cancelEdit',
        saveFormSelector: '.bs-savePanel',

        containerSelector: '.bs-containerPanel',
        contentSelector: '.bs-contentPanel',

        retrySelector: '.bs-retryBtn',

        headerToggle: false,
        headerSelector: '.bs-panelHeader',

        cacheReadonlyContent: true,
        additionalData: {
        },
        formOptions: {

            validateSave: true,
            parseSave: true,

        },
        expandable: true,
        formSelector: 'form',

        retryMessage: 'Reload'
    };

    //#region init
    bsPanel.prototype._init = function () {

        if (this.element.hasClass('bs-hasPanel')) return this.element;

        this.$element = this.element;

        this.$element.addClass('bs-hasPanel');

        this.$toggleEleemnt = this.$element.find(this.options.toggleSelector);
        this._loadOptions();

        this._initDefaultProperties();
        this._initSelectors();
        this._delegateEvents();

        if (this.options.loaded === true) {
            this._initControls();
            this._loadState();
            this._initCurrentContent();
        } else {
            var method = this._readonly ? '_loadReadonlyContent' : "_loadEditableContent";

            this[method]().then($.proxy(function () {
                this._initControls();
                this._loadState();
                this._initCurrentContent();
            }, this));
        }

        if (!this.$toggleEleemnt.data("expandable")) {
            this._loadState(true);
        }
    };

    bsPanel.prototype._loadOptions = function () {
        var settings = this.$element.data('settings');

        $.extend(true, this.options, settings);
    };

    bsPanel.prototype._initDefaultProperties = function () {
        this._name = this.options.name || this.$element.prop('id');

        if (typeof this._name === "undefined" || this._name == '') {
            throw "boxForm requires an unique name";
        }

        this._componentId = this.$element.data('component');

        if (typeof this._componentId === "undefined") {
            console.warn("No component id specified for " + this._name);
        }

        this._objId = this.$element.data('objid');

        this._key = window.location.pathname + '|BoxForm|' + this._name;

        if ((typeof this.options.initialReadonly === "undefined" || this.options.initialReadonly == true) && this.$element.data('readonly') !== false) {
            this.showReadonly();
        } else {
            this.showEditable();
        }

        this.options.readonlyUrl = this.options.readonlyUrl || this.$element.data('readonlyurl');
        this.options.editableUrl = this.options.editableUrl || this.$element.data('editableurl');
        this.options.saveUrl = this.options.saveUrl || this.$element.data('saveurl');

        this._allowExpand = this.options.loaded;
    };

    bsPanel.prototype._initSelectors = function () {
        this.$container = this.$element.find(this.options.containerSelector);
        this.$content = this.$element.find(this.options.contentSelector);
    };

    bsPanel.prototype._delegateEvents = function () {
        if (this.options.expandable) {
            this.$element.on('click', this.options.toggleSelector, $.proxy(this._onToggleClick, this));
        }

        if (this.options.editable !== false) {
            if (this.options.headerToggle) {
                this.$element.on('click', this.options.headerSelector, $.proxy(this._onHeaderClick, this));
            } else {
                this.$element.on('click', this.options.editSelector, $.proxy(this._onEditClick, this));
            }
        }


        this.$element.on('click', this.options.cancelEditSelector, $.proxy(this._onCancelEditClick, this));

        this.$element.on('click', this.options.retrySelector, $.proxy(this._onRetryClick, this));
    };

    bsPanel.prototype._initCurrentContent = function () {
        if (this._readonly) {

        } else {
            this._initEditable();
        }
    };
    //#endregion

    //#region events
    bsPanel.prototype._onToggleClick = function (e) {
        e.preventDefault();
        e.stopPropagation();
        var currentTarget = $(e.currentTarget);

        if (!currentTarget.data("expandable")) {
            return;
        }
        if (this._allowExpand) {
            if (this._state) {
                this.close();
            } else {
                this.open();
            }
        }
    };

    bsPanel.prototype._onHeaderClick = function (e) {
        if (this._readonly == true) {
            this._loadEditableContent().then($.proxy(function () {
                if (!this._state && this._allowExpand) {
                    this.open();
                }
            }, this));

            e.preventDefault();
            e.stopPropagation();
        }
    };

    bsPanel.prototype._onEditClick = function (e) {
        e.preventDefault();
        e.stopPropagation();

        this._loadEditableContent().then($.proxy(function () {
            if (!this._state && this._allowExpand) {
                this.open();
            }
        }, this));
    };

    bsPanel.prototype._onCancelEditClick = function (e) {
        e.preventDefault();
        e.stopPropagation();

        if (this.options.cacheReadonlyContent && this._cachedReadonlyContent) {
            this.$content.html(this._cachedReadonlyContent);
            this.showReadonly();
            this._toggleEditBtn(true);

            if (!this._state) {
                this.open();
            }
        } else {
            this._loadReadonlyContent().then($.proxy(function () {
                this._toggleEditBtn(true);

                if (!this._state) {
                    this.open();
                }
            }, this));
        }
    };

    bsPanel.prototype._onRetryClick = function (e) {
        var $target = $(e.currentTarget),
            method = $target.data('method');

        if (typeof this[method] === "function") {
            this[method]();
        }
    };
    //#endregion

    //#region private methods
    bsPanel.prototype._saveState = function () {
        amplify.store(this._key, this._state);
    };

    bsPanel.prototype._loadState = function (forceOpen) {
        if (forceOpen) {
            this.open();
            return;
        }

        var lastState = amplify.store(this._key);
        if (lastState != null) {

            if (lastState == true) {
                this.open();
            } else {
                this.close();
            }
        }

    };

    bsPanel.prototype._initControls = function () {

        if (this.options.editable) {
            this._toggleEditBtn(this._readonly ? true : false);
        }

    };

    bsPanel.prototype._toggleLoading = function (show) {
        if (show) {
            this.$element.find('.bs-panelLoader').show();
        } else {
            this.$element.find('.bs-panelLoader').hide();
        }
    };

    bsPanel.prototype._toggleCaret = function (show) {
        if (show) {
            this.$element.find('.bs-panelCaret').show();
        } else {
            this.$element.find('.bs-panelCaret').hide();
        }
    };

    bsPanel.prototype._toggleEditBtn = function (show) {
        if (show) {
            this.$element.find(this.options.cancelEditSelector).hide().end()
                .find(this.options.editSelector).show();
        } else {
            this.$element.find(this.options.editSelector).hide().end()
                .find(this.options.cancelEditSelector).show();
        }
    };

    bsPanel.prototype._getXhrData = function () {

        return $.extend(true, {}, {
            componentId: this._componentId,
            objId: this._objId
        }, this.options.additionalData);

    };

    bsPanel.prototype._showErrorMessage = function (message, $errorContainer, replace, method) {

        var $error = $('<div class="bs-form-error alert alert-danger">' +
                           '<button class="close" data-dismiss="alert" type="button">×</button>' +
                               message +
                            '<div>' +
                                '<a href="#" class="bs-retryBtn" data-method="' + method + '">' + this.options.retryMessage + ' <span class="glyphicon glyphicon-refresh"></span> </a>' +
                            '</div>' +
                       '</div>');
        if (replace) {

            $errorContainer.html($error);
        } else {
            $errorContainer.append($error);
        }
    };

    bsPanel.prototype._initEditable = function () {

        var $form = this.$content.find(this.options.formSelector).show(),
            $saveBtn = $form.find(this.options.saveFormSelector);

        this._allowExpand = true;

        if ($form.length == 0) {
            console.warn('No editable form found');
        }

        if ($saveBtn.length == 0) {
            console.warn("No save button found");
        }

        if (this.options.formOptions.parseSave != true) {
            debugger;
        }

        var formOptions = $.extend(true, {}, {
            prefix: this.options.prefix,
            actions: [{
                name: 'save',
                selector: this.options.saveFormSelector,
                validate: this.options.formOptions.validateSave,
                actionUrl: this.options.saveUrl,
                parse: true,
                getExtraData: $.proxy(function (data) {
                    data.componentId = this._componentId;
                    data.objId = this._objId;

                    $.extend(true, data, this.options.formAdditionalData);

                    this._trigger('beforeSaveAjax', 0, data);

                }, this),
                handler: $.proxy(function (sent, data) {

                    this._trigger('beforeEditSuccessHandler', 0, data);

                    this.$content.html(data.Html);

                    this._toggleLoading();
                    this._toggleEditBtn(true);

                    if (this.options.cacheReadonlyContent) {
                        this._cachedReadonlyContent = this.$content.html();
                    }

                    this.showReadonly();

                    this._trigger('editSuccessHandler', 0, data);
                }, this)
            }]
        }, this.options.formOptions);

        this.$content.find(this.options.formSelector).bsForm(formOptions);

        this._trigger('afterFormInit', 0);
    };
    //#endregion

    //#region ajax
    bsPanel.prototype._loadReadonlyContent = function (additionalData) {

        var data = $.extend(true, this._getXhrData(), additionalData);

        this._trigger('beforeReadonlyLoad', 0, data);

        this.$content.find(this.options.formSelector).addClass('loading');

        return $.bforms.ajax({
            name: 'BsPanel|LoadReadonly|' + this._name,
            url: this.options.readonlyUrl,

            data: data,

            context: this,
            success: this._onReadonlyLoadSuccess,
            error: this._onReadonlyLoadError
        });
    };

    bsPanel.prototype._onReadonlyLoadSuccess = function (response) {
        this.$content.html(response.Html);

        this._toggleLoading();
        this._toggleCaret(true);

        this._allowExpand = true;

        if (this.options.cacheReadonlyContent) {
            this._cachedReadonlyContent = this.$content.html();
        }

        this._trigger('onReadonlyLoadSuccess', 0, response);
        this.showReadonly();
    };

    bsPanel.prototype._onReadonlyLoadError = function (data) {

        this.$content.find(this.options.formSelector).removeClass('loading');

        if (data.Message) {

            var $errorContainer = this.$element.find('.bs-panel-error');

            if ($errorContainer.length == 0) {
                $errorContainer = $('<div class="col-sm-12 col-lg-12 bs-validation_row_control bs-panel-error"></div>');
                this.$container.before($errorContainer);
            }

            this._toggleLoading();

            this._showErrorMessage(data.Message, $errorContainer, true, '_loadReadonlyContent');
        }
    };

    bsPanel.prototype._loadEditableContent = function (additionalData) {

        var data = $.extend(true, this._getXhrData(), additionalData);

        this._trigger('beforeEditableLoad', 0, data);

        this.$content.find(this.options.formSelector).addClass('loading');

        return $.bforms.ajax({
            name: 'BsPanel|LoadEditable|' + this._name,
            url: this.options.editableUrl,
            context: this,

            data: data,

            success: this._onEditableLoadSuccess,
            error: this._onEditableLoadError
        });
    };

    bsPanel.prototype._onEditableLoadSuccess = function (response) {

        if (this.options.cacheReadonlyContent && this._readonly == true) {
            this._cachedReadonlyContent = this.$content.clone().find(this.options.formSelector).removeClass('loading').end()
                                                                .html();
        }

        this.$content.html(response.Html);

        this._initEditable();

        this._toggleEditBtn();

        this._toggleLoading();
        this._toggleCaret(true);

        this._trigger('onEditableLoadSuccess', 0, response);

        this.showEditable();
    };

    bsPanel.prototype._onEditableLoadError = function (data) {

        this.$content.find(this.options.formSelector).removeClass('loading');

        if (data.Message) {

            var $errorContainer = this.$element.find('.bs-panel-error');

            if ($errorContainer.length == 0) {
                $errorContainer = $('<div class="col-sm-12 col-lg-12 bs-validation_row_control bs-panel-error"></div>');
                this.$container.before($errorContainer);
            }

            this._toggleLoading();

            this._showErrorMessage(data.Message, $errorContainer, true, '_loadEditableContent');
        }
    };
    //#endregion

    //#region public methods
    bsPanel.prototype.open = function () {
        var openData = {
            allowOpen: true,
            $content: this.$content
        };

        this._trigger('beforeOpen', openData);

        if (openData.allowOpen === true) {
            this.$container.stop(true, true).slideDown(300);
            this.$element.find(this.options.toggleSelector).addClass('dropup');
        }

        this._state = true;
        this._saveState();

        this._trigger('afterOpen');
    };

    bsPanel.prototype.close = function () {
        var closeData = {
            allowClose: true,
            $content: this.$content
        };

        this._trigger('beforeClose', closeData);

        if (closeData.allowClose === true) {
            this.$container.stop(true, true).slideUp(300);
            this.$element.find(this.options.toggleSelector).removeClass('dropup');
        }

        this._state = false;

        this._saveState();

        this._trigger('afterClose');
    };

    bsPanel.prototype.showReadonly = function () {
        this._readonly = true;
        this.$element.data('readonly', true);
        this.$element.removeClass('bs-panelEditMode');

        this._trigger('onReadonlyShow', 0);
    };

    bsPanel.prototype.showEditable = function () {
        this._readonly = false;
        this.$element.data('readonly', false);
        this.$element.addClass('bs-panelEditMode');

        this._trigger('onEditableShow', 0);
    };

    bsPanel.prototype.save = function () {
        var $form = this.$content.find(this.options.formSelector);

        if (typeof $form.bsForm === "function") {
            $form.bsForm('triggerAction', 'save');
        }
    };

    bsPanel.prototype.refresh = function (additionalData) {

        var method = this._readonly ? '_loadReadonlyContent' : "_loadEditableContent";

        this[method](additionalData).then($.proxy(function () {
            this._initControls();
            this._loadState();

        }, this));

    };
    //#endregion

    $.widget('bforms.bsPanel', bsPanel.prototype);

    return bsPanel;
});