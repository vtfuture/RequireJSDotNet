define('bforms-toolbar-quickSearch', [
    'jquery'
], function () {

    // plugin constructor
    var QuickSearch = function ($toolbar, options) {

        // set an unique name
        this.name = 'quickSearch';

        // set the type of the plugin: tab or custom
        this.type = 'custom';

        // set $toolbar container
        // required if your plugin has to communicate with toolbar
        // subscribers or other toolbar controls
        this.$toolbar = $toolbar;

        // merge options
        this.options = $.extend(true, {}, this._defaultOptions, options);

    };

    // plugin default options
    // will be extended by user options
    QuickSearch.prototype._defaultOptions = {
        // control selector, all plugins must have this as an option
        selector: '.bs-quick_search',
        // wheather the search is triggered while you type or on enter
        instant: true,
        // search timeout interval if it is set to instant
        timeout: 250
    };

    // plugin init
    // it will automatically be called
    QuickSearch.prototype.init = function () {

        // keep toolbar widget refrence as we need it later when 
        this.widget = this.$toolbar.data('bformsBsToolbar');

        // add handlers
        this.$toolbar.on('keyup',
                    this.options.selector + ' .bs-text',
                    $.proxy(this._evOnQuickSearchKeyup, this));

    };

    // event handler
    QuickSearch.prototype._evOnQuickSearchKeyup = function (e) {

        var $me = $(e.currentTarget);
        var val = $me.val().trim();

        if (val.length == 0 && $me.data('empty')) {
            return;
        }

        var advancedSearch = this.widget.getControl('advancedSearch');
        if (advancedSearch != null && advancedSearch.$element.hasClass('selected')) {
            advancedSearch.$element.trigger('click');
        }

        if (val.length == 0) {
            $me.data('empty', true);
        } else {
            $me.data('empty', false);
        }

        if (this.options.instant) {
            window.clearTimeout(this.quickSearchTimeout);
            this.quickSearchTimeout = window.setTimeout($.proxy(function () {
                this._search(val);
            }, this), this.options.timeout);
        } else if (e.which == 13 || e.keyCode == 13) {
            this._search(val);
        }

    };

    // search trigger
    QuickSearch.prototype._search = function (quickSearch) {

        // notify grid subscribers that a search was made
        for (var i = 0; i < this.widget.subscribers.length; i++) {
            this.widget.subscribers[i].bsGrid('search', quickSearch, true);
        }

    };

    // export module
    return QuickSearch;

});