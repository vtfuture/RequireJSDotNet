var factory = function ($) {

    var ControlPanel = function (options) {

    };

    ControlPanel.prototype.options = {

        instantQuickSearch: true,

        panelActionSelector: '.control-panel-action',
        panelHeadingSelector: '.panel-heading',
        panelBodySelector: '.panel-body',
        tabButtonSelector: '.control-panel-nav-tab',
        tabSelector: '.control-panel-tab',
        quickSearchSelector: '.tab-search',
        titleSelector: '.control-panel-title',
        gridSelector: '.grid_view'
    };

    ControlPanel.prototype.predefinedActions = {
        remove: 'remove',
        toggle: 'toggle'
    };

    ControlPanel.prototype.tabContentTypes = {

        defaultContent: 'default',
        form: 'form',
        grid: 'grid'
    };

    // #region init

    ControlPanel.prototype._init = function () {

        this._initMembers();
        this._cacheElements();
        this._addHandlers();
    };

    ControlPanel.prototype._initMembers = function () {

        this._initializedTabs = {};
    };

    ControlPanel.prototype._cacheElements = function () {

        this.$element = this.element;
        this.$head = this.$element.find(this.options.panelHeadingSelector);
        this.$body = this.$element.find(this.options.panelBodySelector);
        this.$sideMenu = this.$element.find(this.options.sideMenuSelector);
        this.$quickSearch = this.$element.find(this.options.quickSearchSelector);
    };

    ControlPanel.prototype._addHandlers = function () {

        this.$element.on('click', this.options.panelActionSelector, $.proxy(this.evActionClick, this));
        this.$element.on('click', this.options.tabButtonSelector, $.proxy(this.evTabButtonSelectorClick, this));

        if (!this.$element.attr('id') || this.options.preventAnchoring) {

            this.$element.find(this.options.titleSelector).on('click', 'a:not(.control-panel-action)', function (e) {
                e.preventDefault();
            });
        }

        this._initQuickSearch();
    };

    ControlPanel.prototype._initQuickSearch = function () {

        this.$quickSearch.on('keyup', $.proxy(this._evQuickSearchKeyUp, this));

        this.$element.find(this.options.gridSelector).on('bsgridbeforereset', $.proxy(this._evGridReset, this));
    };

    // #endregion

    // #region event handlers

    ControlPanel.prototype._evGridReset = function (e, data) {

        if (this.$quickSearch.is(':visible')) {
            this.$quickSearch.val('');
        }
    };

    ControlPanel.prototype.evTabButtonSelectorClick = function (e) {

        e.preventDefault();

        var $button = $(e.target),
            tabId = $button.attr('data-tabid');

        var $tab = this._getTab(tabId),
            $currentTab = this._getCurrentTab();

        var quickSearchIsVisible = $button.attr('data-showquicksearch') === 'True';

        var $listItem = $button.parents('li:first');

        if (!$listItem.hasClass('active')) {

            $currentTab.hide();
            $tab.show();

            if (!this._initializedTabs[tabId]) {

                var initializationData = {
                    tabId: tabId,
                    $tab: $tab,
                    contentType: $tab.attr('data-content-type')
                };

                var initializationResult = this._initializeTab(initializationData);

                if (initializationResult && typeof initializationData.done == 'function') {

                    initializationResult.done($.proxy(function (ev, data) {

                        this._trigger('afterTabInitialization', e, initializationData);

                    }, this));

                } else {

                    this._trigger('afterTabInitialization', e, initializationData);
                }

                this._initializedTabs[tabId] = true;

            }

            if (quickSearchIsVisible) {
                this.$quickSearch.show();
            } else {
                this.$quickSearch.hide();
            }

            this.$element.find('.active').removeClass('active');
            $listItem.addClass('active');
        }
    };

    ControlPanel.prototype.evActionClick = function (e) {

        e.preventDefault();

        var $target = $(e.target).is('a') ? $(e.target) : $(e.target).parents('a:first');

        var action = $target.attr('data-action');

        if (this.predefinedActions[action]) {
            this._executePredefinedAction(action);
        }
    };

    ControlPanel.prototype._evQuickSearchKeyUp = function (e) {

        var $currentTab = this._getCurrentTab();

        var $grid = $currentTab.find(this.options.gridSelector);

        if ($grid.length == 0) {
            return;
        }

        var instantQuickSearch = this.options.instantQuickSearch,
            quickSearchTimeout = this.options.quickSearchTimeout;

        var searchValue = this.$quickSearch.val();

        if (instantQuickSearch) {

            if (this.quickSearchTimeout) {
                window.clearTimeout(this.quickSearchTimeout);
            }
            
            this.quickSearchTimeout = window.setTimeout($.proxy(function () {
                
                $grid.bsGrid('search', searchValue, true);

            }, this), quickSearchTimeout);

        } else if (e.which == 13 || e.keyCode == 13) {
            
            $grid.bsGrid('search', searchValue, true);

        }

    };

    // #endregion

    // #region private methods

    ControlPanel.prototype._initializeTab = function (tabData) {

        var $tab = tabData.$tab,
            contentType = tabData.contentType;

        var result;

        var tabOptions = this.options.tabOptions ? (this.options.tabOptions[tabData.tabId] || {}) : {};

        switch (contentType) {

            case this.tabContentTypes.defaultContent:
                {
                    break;
                }
            case this.tabContentTypes.form:
                {
                    result = $tab.find('form').bsForm(tabOptions);

                    break;
                }
            case this.tabContentTypes.grid:
                {
                    result = $tab.find(this.options.gridSelector).bsGrid(tabOptions);

                    break;
                }
            default:
                {
                    break;
                }
        }

        return result;
    };

    ControlPanel.prototype._executePredefinedAction = function (action) {

        switch (action) {
            case this.predefinedActions.remove: {

                this.remove();

                break;
            }
            case this.predefinedActions.toggle: {

                this.toggle();

                break;
            }
            default: {
                break;
            }
        }
    };

    ControlPanel.prototype._toggleElement = function ($element, toggled, animate) {

        animate = typeof animate != 'undefined' ? animate : true;

        if (typeof toggled == 'undefined') {
            toggled = $element.is(':hidden');
        }

        if (toggled) {

            if (animate) {
                $element.stop().slideDown(300);
            } else {
                $element.show();
            }

        } else {

            if (animate) {
                $element.stop().slideUp(300);
            } else {
                $element.hide();
            }
        }
    };

    ControlPanel.prototype._getTab = function (tabId) {

        return this.$element.find(this.options.tabSelector + '[data-tabid="' + tabId + '"]');
    }

    ControlPanel.prototype._getCurrentTab = function () {

        return this.$element.find(this.options.tabSelector + ':visible');
    }

    // #endregion

    // #region public methods

    ControlPanel.prototype.toggle = function (toggled) {

        if (typeof toggled == 'undefined') {
            toggled = this.$body.is(':hidden');
        }

        var $toggle = this.$head.find('[data-action="toggle"] .glyphicon');

        $toggle.removeClass('glyphicon-chevron-up');
        $toggle.removeClass('glyphicon-chevron-down');

        if (toggled) {
            $toggle.addClass('glyphicon-chevron-up');
        } else {
            $toggle.addClass('glyphicon-chevron-down');
        }

        this._toggleElement(this.$body, toggled, true);
    };

    ControlPanel.prototype.remove = function () {

        this.$element.hide();
    };

    // #endregion

    $.widget('bforms.bsControlPanel', ControlPanel.prototype);

    return ControlPanel;
};

if (typeof define == 'function' && define.amd) {
    define('bforms-controlPanel', ['jquery', 'jquery-ui-core'], factory)
} else {
    factory(window.jQuery);
}