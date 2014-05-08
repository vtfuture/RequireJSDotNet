(function (factory) {

    if (typeof define === "function" && define.amd) {
        define('bforms-fileupload', ['jquery', 'jquery-fileupload', 'jquery.fileupload-validate', 'jquery-iframe-transport', 'bforms-ajax'], factory);
    } else {
        factory(window.jQuery);
    }

})(function ($) {

    var fileUpload = function () {
    };

    fileUpload.prototype.options = {
        loadingClass: 'loading',
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
        imageUrlParam: 'AvatarUrl',

        imageSelector: '.bs-uploadImage',
        deleteImageSelector: '.bs-deleteImage',
        
        hasInitialImage : false
    };

    //#region init
    fileUpload.prototype._init = function () {

        this.$element = this.element;

        this._prepareOptions();
        this._initSelectors();
        this._initUpload();
        this._delegateEvents();

        this._initElement();
    };

    fileUpload.prototype._delegateEvents = function () {
        this.$element.on('click', this.options.deleteImageSelector, $.proxy(this._onDeleteClick, this));
    };

    fileUpload.prototype._initSelectors = function () {
        this.$image = this.$element.find(this.options.imageSelector);
    };

    fileUpload.prototype._initElement = function() {

        if (this.options.hasInitialImage == true) {
            this._toggleBlockDelete(false);
        }

    };

    fileUpload.prototype._prepareOptions = function () {
        if (typeof this.$element.data('hasinitialimage') !== "undefined") {
            this.options.hasInitialImage = this.$element.data('hasinitialimage');
        }

    };

    fileUpload.prototype._initUpload = function () {

        this.$element.fileupload($.extend(true, {
            url: this.options.url,
            autoUpload: true,
            start: $.proxy(this._onStart, this),
            progress: $.proxy(this._onProgress, this),
            done: $.proxy(this._onDone, this),
            stop: $.proxy(this._onStop, this),
            maxFileSize: this.options.maxFileSize,
            acceptFileTypes: this.options.acceptFileTypes,
            dropZone: this.$element,
            formData: $.proxy(this._getFormData, this)
        }, this.options.fileUpload));
    };
    //#endregion

    //#region events
    fileUpload.prototype._onStart = function () {

        this._toggleLoading(true);

        this._trigger('start', 0, arguments);
    };

    fileUpload.prototype._onProgress = function () {
        this._trigger('progress', 0, arguments);
    };

    fileUpload.prototype._onDone = function (e, data) {

        var response = data.result.Data;

        var imageUrl = response[this.options.imageUrlParam];
        this._updateImage(imageUrl);
        this._toggleBlockDelete(false);

        this._trigger('done', 0, arguments);
    };

    fileUpload.prototype._onStop = function () {

        this._toggleLoading(false);

        this._trigger('stop', 0, arguments);
    };

    fileUpload.prototype._onDeleteClick = function (e) {
        e.preventDefault();
        e.stopPropagation();

        if (this.options.deleteUrl) {

            var data = this._getExtraData();

            $.bforms.ajax({
                url: this.options.deleteUrl,
                data: data,
                success: this._onDeleteSuccess,
                error: this._onDeleteError,
                context : this
            });
        } else if (typeof this.options.defaultImageUrl !== "undefined") {
            this._updateImage(this.options.defaultImageUrl);
        }
    };
    //#endregion

    //#region xhr
    fileUpload.prototype._onDeleteSuccess = function (response) {

        if (typeof response[this.options.imageUrlParam] !== "undefined") {
            this._updateImage(response[this.options.imageUrlParam]);
        } else if (typeof this.options.defaultImageUrl !== "undefined") {
            this._updateImage(this.options.defaultImageUrl);
        }

        this._toggleBlockDelete(true);
    };

    fileUpload.prototype._onDeleteError = function () {
    };
    //#endregion

    //#region private methods
    fileUpload.prototype._toggleLoading = function (show) {
        if (this.options.loadingClass) {
            if (show) {
                this.$element.addClass(this.options.loadingClass);
            } else {
                this.$element.removeClass(this.options.loadingClass);
            }
        }
    };

    fileUpload.prototype._toggleBlockDelete = function(block) {
        if (block) {
            this.$element.find(this.options.deleteImageSelector).addClass('disabled');
        } else {
            this.$element.find(this.options.deleteImageSelector).removeClass('disabled');
        }
    };

    fileUpload.prototype._updateImage = function (avatarUrl) {
        if (this.$image.length) {

            if (avatarUrl.indexOf('?') === -1) {
                avatarUrl += "?" + new Date();
            } else {
                avatarUrl += "&" + new Date();
            }

            this.$image.attr('src', avatarUrl);

        }
    };

    fileUpload.prototype._getExtraData = function (data) {

        if (typeof data !== "object" || data === null) {
            data = {};
        }

        $.extend(true, data, this.options.extraData);

        this._trigger('getExtraData', 0, data);

        return data;
    };

    fileUpload.prototype._getFormData = function () {
        var data = [];
        
        if (typeof this.options.getFormData === "function") {
            data = this.options.getFormData();
        }

        return data;
    };
    //#endregion

    //#region public methods

    //#endregion

    $.widget('bforms.bsFileUpload', fileUpload.prototype);
});