define('bforms-toolbar', [
    'bforms-toolbar-advancedSearch',
    'bforms-toolbar-add',
    'bforms-toolbar-quickSearch',
    'bforms-toolbar-order',
    'jquery',
    'jquery-ui-core',
    'amplify',
    'bforms-form'
], function (AdvancedSearch, Add, QuickSearch, Order) {

    jQuery.nsx('bforms.toolbar.defaults');
    jQuery.nsx('bforms.toolbar.controls');

    // attach default controls to namespace
    $.bforms.toolbar.defaults.AdvancedSearch = AdvancedSearch;
    $.bforms.toolbar.defaults.Add = Add;
    $.bforms.toolbar.defaults.QuickSearch = QuickSearch;
    $.bforms.toolbar.defaults.Order = Order;

    //#region Toolbar
    var Toolbar = function (opt) {
        this.options = opt;
        this._create();
    };

    Toolbar.prototype.options = {
        uniqueName: null,
        // save opened tab in localstorage and retreive it later
        saveTabState: true,
        // tab container selector
        tabContainerSelector: '.grid_toolbar_form',
        // auto initialize controls that were attached on bforms.toolbar.defaults and bforms.toolbar.controls namespaces
        autoInitControls: true,
        // reset handler
        reset: null,
        // controls to be added to toolbar widget
        controls: null,

        // common options for all toolbar controls
        controlsOptions: null
    };

    Toolbar.prototype._init = function () {

        if (!this.options.uniqueName) {
            this.options.uniqueName = this.element.attr('id');
        }

        if (!this.options.uniqueName) {
            throw 'toolbar needs a unique name or the element on which it is aplied has to have an id attr';
        }

        this._controls = [];

        // add 
        this.subscribers = new Array();

        if (this.options.subscribers) {
            this._addSubscribers(this.options.subscribers);
        }

        if (this.options.autoInitControls) {
            //init default controls if any
            for (var k in $.bforms.toolbar.defaults) {
                if (k in $.bforms.toolbar.defaults) {

                    var opts = $.extend(true, {}, this.options.controlsOptions);

                    if (typeof this.options.customControlsOptions !== "undefined" &&  typeof this.options.customControlsOptions[k] !== "undefined") {
                        $.extend(true, opts, this.options.customControlsOptions[k]);
                    }

                    var control = new $.bforms.toolbar.defaults[k](this.element, opts);                  

                    var $btn = this.element.find(control._defaultOptions.selector);
                    if ($btn.length > 0) {
                        control.$element = $btn;
                        this._controls.push(control);
                    }
                }
            }

            //init custom controls requested outside of toolbar
            for (var k in $.bforms.toolbar.controls) {
                if (k in $.bforms.toolbar.controls) {
                    var control = new $.bforms.toolbar.controls[k](this.element);
                    var $btn = this.element.find(control._defaultOptions.selector);
                    if ($btn.length > 0) {
                        control.$element = $btn;
                        this._controls.push(control);
                    }
                }
            }
        }

        // init controls passed as options
        if (this.options.controls instanceof Array) {
            for (var i = 0; i < this.options.controls.length; i++) {
                if (typeof (this.options.controls[i]) != 'undefined') {
                    var control = new this.options.controls[i](this.element);
                    var $btn = this.element.find(control._defaultOptions.selector);
                    if ($btn.length > 0) {
                        control.$element = $btn;
                        this._controls.push(control);
                    }
                }
            }
        }

        this._addControls(this._controls);

        this._expandSavedTab();
    };

    Toolbar.prototype.reset = function () {
        debugger;
        this._reset(arguments);
        if (typeof this.options.reset === "function") {
            this.options.reset.apply(this, arguments);
        }
    };

    Toolbar.prototype._reset = function (options) {
        
        var preventPagination = true,
            quickSearch = this.getControl('quickSearch');

        if (typeof options !== "undefined") {
            preventPagination = options.preventPagination;
        }

        // reset quick search if any
        if (quickSearch != null) {
            quickSearch.$element.find('input').val('');
        }

        // reset advanced search if any
        var advancedSearch = this.getControl('advancedSearch'),
            data;

        if (advancedSearch != null) {
            advancedSearch.$container.bsForm('reset');
            data = advancedSearch.$container.bsForm('parse');
        }

        // reset grid
        var widget = this.element.data('bformsBsToolbar');
        for (var i = 0; i < widget.subscribers.length; i++) {
            widget.subscribers[i].bsGrid('reset', data, preventPagination);
        }

    };

    Toolbar.prototype._addDelegates = function () {



    };

    Toolbar.prototype.controls = function (controls) {

        for (var i = 0; i < controls.length; i++) {
            var control = controls[i];
            var $btn = this.element.find(control._defaultOptions.selector);
            if ($btn.length > 0) {
                control.$element = $btn;
                this._controls.push(control);
            }
        }

        this._addControls(controls);

    };

    Toolbar.prototype._addControls = function (controls) {

        if (!controls) {
            return;
        }

        for (var i = 0; i < controls.length; i++) {

            var control = controls[i];

            if (control.$element) {

                switch (control.type) {
                    case "action": {
                        this._addAction(control);
                        break;
                    }
                    case "tab": {

                        this._addTab(control);

                        control.init();

                        break;
                    }
                    default: {

                        this._addCustomControl(control);

                        control.init();

                        break;
                    }
                }
            }
        }

    };

    Toolbar.prototype._addTab = function (tab) {

        tab.$container = $('#' + tab.$element.data('tabid'));

        tab.$element.on('click', { tab: tab }, $.proxy(this._evBtnTabClick, this));
        //control.options.init.call(this, tab.$container, control.options);
    };

    Toolbar.prototype._expandSavedTab = function () {

        if (this.options.saveTabState) {
            var tabs = this._getTabs();

            for (var i = 0; i < tabs.length; i++) {
                var tab = tabs[i];
                if (amplify.store('slide|' + this.options.uniqueName + '|' + tab.options.selector)) {
                    tab.$element.trigger('click');
                }
            }
        }

    };

    Toolbar.prototype._addCustomControl = function (control) {

        if (typeof control.name !== 'string' || control.name.trim().length == 0) {
            throw 'all controls must have a name';
        }

        if (typeof control.options.selector !== 'string' || control.options.selector.trim().length == 0) {
            throw 'all controls must have a name';
        }

        //control.options.init.call(this, $(control.options.selector), control.options);

    };

    Toolbar.prototype._addAction = function (buttonOpt) {

        if (typeof buttonOpt.handler !== 'function') {
            throw 'action ' + buttonOpt.name + ' must implement an event handler';
        }

        var $elem = this.element.find(buttonOpt.selector);

        $elem.on('click', { buttonOpt: buttonOpt }, $.proxy(this._evBtnClick, this));

        var control = {
            name: buttonOpt.name,
            type: 'action',
            $element: $elem,
            options: buttonOpt
        };

        this._controls.push(control);
    };

    Toolbar.prototype._addSubscribers = function (subscribers) {

        for (var i = 0; i < subscribers.length; i++) {
            this._addSubscriber(subscribers[i]);
        }

    };

    Toolbar.prototype._addSubscriber = function (subscriber) {
        this.subscribers.push(subscriber);
    };

    Toolbar.prototype._evBtnClick = function (e) {

        e.preventDefault();

        var buttonOpt = e.data.buttonOpt;

        buttonOpt.handler.call(this, e);
    };

    Toolbar.prototype._evBtnTabClick = function (e, triggeredTab) {

        e.preventDefault();

        var clickedTab = e.data.tab;

        if (this._selectedTab != null && this._selectedTab.name != clickedTab.name) {
            this._toggleTab(this._selectedTab);
        }

        this._toggleTab(clickedTab);
    };

    Toolbar.prototype._toggleTab = function (tab) {

        if (tab == null) return;

        tab.$element.toggleClass('selected');
        if (tab.name == 'advancedSearch') {
            var quickSearch = this.getControl('quickSearch');
            if (quickSearch != null) {
                quickSearch.$element.toggleClass('selected');
            }
        }

        tab.$container.stop(true, false).slideToggle();

        var isSelected = tab.$element.hasClass('selected');

        if (this.options.saveTabState) {
            amplify.store('slide|' + this.options.uniqueName + '|' + tab.options.selector, isSelected);
        }

        if (isSelected) {
            this._selectedTab = tab;
        } else {
            this._selectedTab = null;
        }
    };

    //#region helpers
    Toolbar.prototype._getTabs = function () {

        return this._controls.filter(function (el) {
            return el.type == 'tab';
        });

    };

    Toolbar.prototype.getControl = function (name) {

        var control = $.grep(this._controls, function (el) {
            return el.name == name;
        });

        if (control != null && control[0] != null) return control[0];

        return null;
    };

    Toolbar.prototype.toggleControl = function (name, show) {
        var control = this.getControl(name);

        if (control != null && control.type == "tab") {

            if (typeof show !== "undefined") {

                if (show == true) {
                    if (this._selectedTab != null) {
                        if (this._selectedTab.name != control.name) {
                            this._toggleTab(this._selectedTab);
                            this._toggleTab(control);
                        }
                    } else {
                        this._toggleTab(control);
                    }
                } else {
                    if (this._selectedTab != null && this._selectedTab.name == control.name) {
                        this._toggleTab(control);
                    }
                }

            } else {
                if (this._selectedTab != null && this._selectedTab.name != control.name) {
                    this._toggleTab(this._selectedTab);
                }

                this._toggleTab(control);
            }
        }

        return control;
    };
    //#endregion

    //#endregion

    $.widget('bforms.bsToolbar', Toolbar.prototype);
});