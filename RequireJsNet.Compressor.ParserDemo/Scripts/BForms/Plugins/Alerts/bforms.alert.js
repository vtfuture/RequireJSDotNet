(function (factory) {

    if (typeof define === "function" && define.amd) {
        define('bforms-alert', ['jquery', 'singleton-ich', 'bootstrap'], function ($, ichSingleton) {

            var ichInstance = ichSingleton.getInstance();

            factory($, ichInstance);

        });
    } else {
        factory(window.jQuery, ich);
    }

})(function ($, ich) {

    var bsAlert = function ($elem, opts) {

        this.$element = $elem;

        if (this.$element.data('bs-alert') != null) {
            var oldAlert = this.$element.data('bs-alert');
            oldAlert._removeAlert();
        }

        $.extend(true, this.options, opts);

        this._init();
        this._prepareOptions();
        this._render();
        this._addAlert();

        this.$element.data('bs-alert', this);

        return this.$element;
    };

    bsAlert.prototype.options = {
        allowedTypes: ['success', 'info', 'warning', 'danger']
    };

    bsAlert.prototype._init = function () {
        ich.addTemplate('bsRenderAlert', this.options.template);
    };

    bsAlert.prototype._prepareOptions = function () {

        if (this.options.allowedTypes.indexOf(this.options.type) == -1) {
            this.options.type = 'danger';//default alert type
        }
    };

    bsAlert.prototype._render = function () {
        this.$alert = $(ich.bsRenderAlert({
            dismissable: this.options.dismissable,
            message: this.options.message,
            type: this.options.type
        }));
    };

    bsAlert.prototype._addAlert = function () {
        switch (this.options.placement) {
            case 'after':
                this.$element.after(this.$alert);
                break;
            case 'before':
                this.$element.before(this.$alert);
                break;
            case 'inside-before':
                this.$element.prepend(this.$alert);
                break;
            default:
                this.$element.append(this.$alert);
                break;
        }

        if (this.options.dismissAfter !== false && typeof this.options.dismissAfter === "number") {
            this._timeoutHandler = window.setTimeout($.proxy(this._removeAlert, this), this.options.dismissAfter * 1000);
        }
    };

    bsAlert.prototype._removeAlert = function () {
        this.$alert.remove();
        this.$element.removeData('bs-alert');
        window.clearTimeout(this._timeoutHandler);
    };

    bsAlert.prototype.remove = function () {
        this._removeAlert();
    }

    $.fn.bsAlertDefaults = {
        dismissAfter: 5,
        dismissable: true,
        type: 'danger',
        template: '<div class="alert alert-{{type}}" role="alert">' +
					  '{{#dismissable}}<button type="button" class="close" data-dismiss="alert">' +
						'<span aria-hidden="true">&times;</span>' +
					  '</button>{{/dismissable}}' +
				  '{{{message}}}' +
				  '</div>',
        placement: 'inside-after', //supported values : inside-after,inside-before, after, before
        throwExceptions: false
    };

    $.fn.bsAlert = function (opts) {

        if ($(this).length == 0) {
            console.warn('bsAlert must be applied on an element');
            return $(this);
        }

        var args = Array.prototype.slice.call(arguments, 0),
            options = args[0],
            methodParams = args.splice(1);

        if (typeof options === "undefined" || typeof options === "object") {
            return new bsAlert($(this), $.extend(true, {}, $.fn.bsAlertDefaults, opts));
        } else if (typeof options === "string") {
            var instance = (this).data('bs-alert');
            if (typeof instance === "undefined") {
                if ($.fn.bsAlertDefaults.throwExceptions === true) {
                    throw 'Cannot call method ' + options + ' before initializing plugin';
                }
            } else {
                return instance[options].apply(instance, methodParams);
            }
        }
    };
});