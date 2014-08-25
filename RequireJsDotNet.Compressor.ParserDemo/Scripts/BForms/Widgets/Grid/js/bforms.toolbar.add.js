define('bforms-toolbar-add', [
	'jquery',
	'bforms-toolbar',
	'bforms-form'
], function () {

    var Add = function ($toolbar, options) {

        this.name = 'add';

        this.type = 'tab';

        this.$toolbar = $toolbar;

        this.options = $.extend(true, {}, this._defaultOptions, options);

        this.widget = this.$toolbar.data('bformsBsToolbar');

        this._controls = {};

        this._addDefaultOptions();

    };

    Add.prototype._defaultOptions = {
        selector: '.bs-show_add'
    };

    Add.prototype.init = function () {

        var $elem = this.$container.find(this._defaultOptions.selector);
        var controls = [];
        for (var k in this._controls) {
            if (k in this._controls) {
                controls.push(this._controls[k]);
            }
        }

        var opts = $.extend(true, this.options, {
            container: $elem.attr('id'),
            actions: controls
        });

        this.$addForm = this.$container.bsForm(opts);
    };

    Add.prototype._addDefaultOptions = function () {

        var addOptions = {
            // button name. When you want to customize a form button
            // functionality the name is the key based on which the 
            // options will be merged
            name: 'save',
            // button selector that the handler will attach to
            selector: '.js-btn-save',
            // validate form. In this case we validate the form because
            // adding an entity might have some consitions to meet
            validate: true,
            // parse form and send parsed data to handler
            parse: true,
            // button handler
            handler: $.proxy(this._evOnAdd, this)
        };
        this._controls['save'] = addOptions;

        var resetOptions = {
            // button name. When you want to customize a form button
            // functionality the name is the key based on which the 
            // options will be merged
            name: 'reset',
            // button selector that the handler will attach to
            selector: '.js-btn-reset',
            // validate form. In this case we don't want to validate
            // the form because all user input will be reset
            validate: false,
            // parse form and send parsed data to handler. We don't parse
            // the form because we've just reseted it
            parse: false,
            // button handler
            handler: $.proxy(this._evOnReset, this)
        };
        this._controls['reset'] = resetOptions;
    };

    Add.prototype.setControl = function (controlName, options) {

        var control = this._controls[controlName];

        if (control) {
            control = $.extend(true, {}, control, options);
        }

        this._controls[controlName] = control;

    };

    Add.prototype._evOnAdd = function (data, response) {
        for (var i = 0; i < this.widget.subscribers.length; i++) {
            this.widget.subscribers[i].bsGrid('add', response.Row, data, response);
        }
        this.$addForm.bsForm('reset');

        this.widget._trigger('afterAdd', 0, {
            data: data,
            response: response
        });
    };

    Add.prototype._evOnReset = function (data) {
        this.$addForm.bsForm('reset');

        this.widget._trigger('afterReset', 0, {
            data: data
        });
    };

    return Add;

});