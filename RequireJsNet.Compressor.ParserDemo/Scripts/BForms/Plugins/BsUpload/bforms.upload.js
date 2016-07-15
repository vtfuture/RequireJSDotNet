(function (factory) {
	if (typeof define === "function" && define.amd) {
		define('bforms-upload', ['jquery'], factory);
	} else {
		factory(window.jQuery);
	}
})(function ($) {

	var bsUpload = function ($element, options) {
		this.$input = $element;

		this.options = options;

		this._init();
	};

	bsUpload.prototype._init = function () {
		this._initSelectors();
		this._delegateEvents();
	};

	bsUpload.prototype._delegateEvents = function () {
		this.$wrapper.on('click', this.options.removeBtn, $.proxy(this._onRemoveClick, this));
		this.$wrapper.on('change', this.options.fileInput, $.proxy(this._onFileChange, this));
	};

	bsUpload.prototype._initSelectors = function () {
		this.$wrapper = this.$input.parents(this.options.parentCssClass).first();
	};

	bsUpload.prototype._onRemoveClick = function (e) {
		e.preventDefault();

		this.$input.val('');

		this.$input.trigger('focusout').trigger('change');
	};

	bsUpload.prototype._onFileChange = function () {

		var filePath = this.$input.val();

		var $formControl = this.$wrapper.find(this.options.formControl);

		if (filePath) {

			var parts = filePath.split('\\'),
		        fileName = parts[parts.length - 1];

			if (fileName.length > 20) {
			    var fileExtensionParts = fileName.split('.'),
			        fileExtension = fileExtensionParts[fileExtensionParts.length - 1];

			    fileName = fileName.substr(0, 20) + '... .' + fileExtension;
			}

			$formControl.attr('data-title', fileName);
			this.$wrapper.find(this.options.removeBtn).show();
		} else {
			$formControl.attr('data-title', $formControl.attr('data-choosefile'));
			this.$wrapper.find(this.options.removeBtn).hide();
		}

	    this.$input.trigger('focusout');

	};

	$.fn.bsUploadDefaults = {
		formControl: '.bs-uploadFormControl',
		removeBtn: '.bs-removeUploadBtn',
		fileInput: '.bs-file',
		parentCssClass: '.bs-uploadWrapper'
	};

	$.fn.bsUpload = function (opts) {
		if ($(this).length === 0) {
			console.warn('bsUpload must be applied on an element');
			return $(this);
		}

		return new bsUpload($(this), $.extend(true, {}, $.fn.bsUploadDefaults));
	};

});