define('bforms-toolbar-advancedSearch', [
	'jquery',
	'bforms-toolbar',
	'bforms-form'
], function () {

    var AdvancedSearch = function ($toolbar, options) {

        this.name = 'advancedSearch';

        this.type = 'tab';

        this.$toolbar = $toolbar;

        this.options = $.extend(true, {}, this._defaultOptions, options);

        this.widget = this.$toolbar.data('bformsBsToolbar');

        this._controls = {};

        this._addDefaultOptions();

    };

    AdvancedSearch.prototype._defaultOptions = {
        selector: '.bs-show_advanced_search'
    };

    AdvancedSearch.prototype.init = function () {

        var controls = [];
        for (var k in this._controls) {
            if (k in this._controls) {
                controls.push(this._controls[k]);
            }
        }

        var $elem = this.$container.find(this._defaultOptions.selector);

        var opts = $.extend(true, this.options, {
            container: $elem.attr('id'),
            actions: controls
        });

        this.$searchForm = this.$container.bsForm(opts);

        this._initSearchModel(this.$searchForm.bsForm('parse'));
    };

    AdvancedSearch.prototype._addDefaultOptions = function () {

        var searchOptions = {
            // button name. When you want to customize a form button
            // functionality the name is the key based on which the 
            // options will be merged
            name: 'search',
            // button selector that the handler will attach to
            selector: '.js-btn-search',
            // validate form. We don't validate data on search
            validate: false,
            // parse form and send parsed data to handler
            parse: true,
            // button handler
            handler: $.proxy(this._evOnSearch, this)
        };
        this._controls['search'] = searchOptions;

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
            // parse form and send parsed data to handler. We parse the data
            // in case some fields have default values
            parse: true,
            // button handler
            handler: $.proxy(this._evOnReset, this)
        };
        this._controls['reset'] = resetOptions;
    };

    AdvancedSearch.prototype.setControl = function (controlName, options) {

        var control = this._controls[controlName];

        if (control) {
            control = $.extend(true, {}, control, options);
        }

        this._controls[controlName] = control;

    };

    AdvancedSearch.prototype._evOnSearch = function (data) {
        for (var i = 0; i < this.widget.subscribers.length; i++) {
            this.widget.subscribers[i].bsGrid('search', data);
        }
        
        this.widget._trigger('afterAdvancedSearch', 0, {
            data: data
        });
    };

    AdvancedSearch.prototype._initSearchModel = function (data) {
        for (var i = 0; i < this.widget.subscribers.length; i++) {
            this.widget.subscribers[i].bsGrid('initSearch', data);
        }
    };

    AdvancedSearch.prototype._evOnReset = function () {
      
        this.$searchForm.bsForm('reset');
        var data = {};
        this.$searchForm.bsForm('getFormData', data);
        for (var i = 0; i < this.widget.subscribers.length; i++) {
            this.widget.subscribers[i].bsGrid('reset', data);
        }
        
        this.widget._trigger('afterAdvancedSearchReset', 0, {
            data: data
        });

    };

    return AdvancedSearch;

});