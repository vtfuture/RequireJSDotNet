(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-themeSelect', ['jquery', 'jquery-ui-core', 'amplify'], factory);
    } else {
        factory(window.jQuery);
    }
}(function ($) {

    var opts = requireConfig.websiteOptions;
    
    String.prototype.toUpperCaseFirst = function () {
        return this.charAt(0).toUpperCase() + this.slice(1);
    };
    
    var themeSelect = function () {
    };

    themeSelect.prototype.options = {
        targets: 'body, .form_container, .grid_toolbar, .grid_view, .bs-datetime-picker, .bs-range-picker, .bs-pager',
        startTheme: opts.startTheme,
        themeEnum: opts.themeEnum,
        availableColors: 'turqoise orange green blue purple black',
        toggleSelector: '.bs-toggleThemeSelect',

        openSelector: '.bs-selectThemeOpen',
        closeSelector: '.bs-selectThemeClose',
        
        defaultTheme : 'turqoise',

        optionsSelector: '.bs-selectThemeOptions',

        localStorageSave: false,
        ajaxSaveUrl: opts.saveThemeUrl
    };

    themeSelect.prototype._init = function () {
        this.$element = this.element;
        
        this._currentColor = this.options.startTheme || "Default";
        
        if (this._currentColor == "Default") {
            this._isDefaultSelected = true;
        }
        
        this._currentColorClass = this._currentColor.toLowerCase();

        this._isOpen = this.$element.find(this.options.closeSelector).is(':visible');
        
        if (this._isDefaultSelected) {
            this.$element.find('*[data-isdefault="true"]').parent().hide();
        }

        this._addDelegates();
        this._restoreState();
    };

    themeSelect.prototype._addDelegates = function () {
        this.$element.on('click', this.options.optionsSelector + ' a', $.proxy(this._onColorSelect, this));
        this.$element.on('click', this.options.toggleSelector, $.proxy(this._onToggleClick, this));
    };

    themeSelect.prototype._onColorSelect = function (e) {
        e.preventDefault();

        var $target = $(e.currentTarget);
        
        if ($target.data('isdefault')) {
            this._isDefaultSelected = true;
        } else {
            this._isDefaultSelected = false;
        }
        
        var newColor = $target.data('color');
        this.setColor(newColor);
    };

    themeSelect.prototype._onToggleClick = function (e) {
        e.preventDefault();

        if (this._isOpen) {
            this.close();
        } else {
            this.open();
        }
    };

    themeSelect.prototype.setColor = function (color) {
        try {
            $(this.options.targets).removeClass(this.options.availableColors);
            $(this.options.targets).removeClass(color);
            $(this.options.targets).addClass(color);
            this._currentColor = color;
            this._currentColorClass = color;
            
            if (this._isDefaultSelected) {
                this.$element.find('*[data-isdefault="true"]').parent().hide();
            } else {
                this.$element.find('*[data-isdefault="true"]').parent().show();
            }
        } catch (e) {
            throw 'Invalid targets for themeSelect';
        }

        this._saveState();
    };

    themeSelect.prototype.getCurrentColorClass = function() {
        return this._currentColorClass;
    };

    themeSelect.prototype.open = function () {
        this.element.find(this.options.openSelector).hide();
        this.element.find(this.options.closeSelector).show();


        this.element.removeClass('collapsed');

        this._isOpen = true;

        //this._saveState();
    };

    themeSelect.prototype.close = function () {
        this.element.find(this.options.closeSelector).hide();
        this.element.find(this.options.openSelector).show();

        this.element.addClass('collapsed');


        this._isOpen = false;

        //this._saveState();
    };

    themeSelect.prototype._saveState = function () {

        if (this.options.localStorageSave) {
            var data = {
                Theme: this._currentColor.toUpperCaseFirst(),
                Open: this._isOpen
            };

            amplify.store('themeSelect', data);
        }
        if (this.options.ajaxSaveUrl != null) {
            $.ajax({
                url: this.options.ajaxSaveUrl,
                data: {
                    Theme: this._isDefaultSelected ? 'Default' : this.options.themeEnum[this._currentColor.toUpperCaseFirst()],
                    Open: this._isOpen,
                },
                type: 'POST'
            });
        }

    };

    themeSelect.prototype._restoreState = function () {

        if (this.options.localStorageSave === true) {
            var data = amplify.store('themeSelect');
            if (typeof data !== "undefined" && data != null) {
                if (data.Theme) {
                    var color = data.Theme.toLowerCase();
                    this.setColor(color);
                }

                if (data.open) {
                    this.open();
                }
            }
        }
    };

    $.widget('bforms.bsThemeSelect', themeSelect.prototype);

    return themeSelect;
}));