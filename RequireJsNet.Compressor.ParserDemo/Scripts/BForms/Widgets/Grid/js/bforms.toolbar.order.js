define('bforms-toolbar-order', [
        'jquery',
        'bforms-toolbar',
        'bforms-form',
        'bforms-sortable'
], function () {

    var Order = function ($toolbar, opts) {

        this.options = $.extend(true, {}, this._defaultOptions, opts);

        this.name = 'order';
        this.type = 'tab';

        this.$toolbar = $toolbar;
        this.widget = $toolbar.data('bformsBsToolbar');
        this._controls = {};
        this.updated = false;
        this.currentConnection = {};

        this._addDefaultControls();
    };

    Order.prototype.init = function () {

        var $elem = this.$container.find(this.options.selector);

        var controls = [];

        for (var k in this._controls) {
            if (this._controls.hasOwnProperty(k)) {
                controls.push(this._controls[k]);
            }
        }

        this.$orderForm = this.$container.bsForm({
            container: $elem.attr('id'),
            actions: controls
        });

        this.$sortable = $(this.options.containerSelector).find(this.options.listSelector + ':first');

        this.bsSortableOptions = {

            itemSelector: this.options.itemSelector,
            listSelector: this.options.listSelector,
            containerSelector: this.options.containerSelector,
            sortUpdate: $.proxy(this._evOnUpdate, this)
        };

        this.$sortable.bsSortable(this.bsSortableOptions);

        this.previousConfigurationHtml = this.$sortable.html();

        this._addDelegates();
    };

    Order.prototype._defaultOptions = {
        selector: '.bs-show_order',
        itemSelector: '.bs-sortable-item',
        listSelector: '.bs-sortable',
        containerSelector: '.sortable-container',
        serialize: 'hierarchy'
    };

    Order.prototype._addDefaultControls = function () {

        this._controls.reset = {
            name: 'reset',
            selector: '.js-btn-reset',
            validate: false,
            parse: false,
            handler: $.proxy(this._evOnReset, this)
        };

        this._controls.save = {
            name: 'save',
            selector: '.js-btn-save_order',
            validate: false,
            parse: false,
            handler: $.proxy(this._evOnSave, this)
        };
    };

    Order.prototype.setControl = function (controlName, options) {

        var control = this._controls[controlName];

        if (control) {
            control = $.extend(true, {}, control, options);
        }

        this._controls[controlName] = control;

    };


    Order.prototype._addDelegates = function () {

        $(this.options.containerSelector).on('update', $.proxy(this._evOnUpdate, this));
    };

    Order.prototype._evOnUpdate = function (e, data) {

        this.updated = true;
    };

    Order.prototype._evOnReset = function () {

        this.$sortable.html(this.previousConfigurationHtml);

        $('.placeholder').remove();

        this.$sortable.bsSortable(this.bsSortableOptions);
    };

    Order.prototype._evOnSave = function () {

        if (this.updated || true) {

            event.preventDefault();

            $(this._controls.save.selector).attr('disabled', true);
            this.$orderForm.attr('disabled', true);

            var data = this.$sortable.bsSortable('serialize', this.options.serialize);
            var url = $(this._controls.save.selector).data('url');

            this.widget._trigger('beforeOrderFormSubmit', event, { data: data });

            $('.loading-global').show();

            $.bforms.ajax({
                data: {
                    model: data
                },
                url: url,
                success: this._reorderSuccess,
                error: this._reorderError,
                context: this,
                loadingClass: 'loading'
            });
        }

    };

    Order.prototype._reorderError = function (response) {

        $('.loading-global').hide();
        this.$orderForm.removeAttr('disabled');
        $(this._controls.save.selector).attr('disabled', false);

        this.updated = false;
        
        this.widget._trigger('afterOrderFormSubmit', event, { response: response });
    };

    Order.prototype._reorderSuccess = function (response) {

        this.updated = false;

        $('.loading-global').hide();
        this.$orderForm.removeAttr('disabled');
        $(this._controls.save.selector).attr('disabled', false);

        this.previousConfigurationHtml = this.$sortable.html();
        
        this.widget._trigger('afterOrderFormSubmit', event, { response: response });
    };

    return Order;
});