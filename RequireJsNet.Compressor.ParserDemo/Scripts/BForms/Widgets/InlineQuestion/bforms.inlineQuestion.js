(function(factory) {
    if (typeof define === "function" && define.amd) {
            define('bforms-inlineQuestion', ['jquery', 'bootstrap', 'jquery-ui-core', 'icanhaz'], factory);
    } else {
        factory(window.jQuery);
    }
})(function($) {

    var bsInlineQuestion = function(options) {
        this.options = $.extend(true, {}, this.options, options);
    };

    bsInlineQuestion.prototype.options = {
        template :  '<p>{{question}}</p>'+
                    '<hr />' + 
                    '{{#buttons}}' + 
                        '<button type="button" class="btn bs-popoverBtn {{cssClass}}"> {{text}} </button> ' +
                    '{{/buttons}}',
        contentTemplate: '{{{content}}}' +
                          '<hr />' +
                         '{{#buttons}}' +
                             '<button type="button" class="btn bs-popoverBtn {{cssClass}}"> {{text}} </button> ' +
                         '{{/buttons}}',
        placement: 'left',
        placementArray : 'left,right,bottom,top',
        content: undefined,
        stretch : false,
        closeOnOuterClick: true
    };

    //#region init and render
    bsInlineQuestion.prototype._init = function () {

        if (this.element.hasClass('bs-hasInlineQuestion')) return this.element;
        
        this.$element = this.element;

        this.$element.addClass('bs-hasInlineQuestion');

        if (this.options.placement === 'auto') {
            this._placementArray = this.options.placementArray.split(',');

            this.options.placement = $.proxy(this._placementMethod, this);
        }

        if (typeof ich.renderPopoverQuestion !== "function") {
            ich.addTemplate('renderPopoverQuestion', this.options.template);
        }
        
        if (typeof ich.renderPopoverContent !== "function") {
            ich.addTemplate('renderPopoverContent', this.options.contentTemplate);
        }

        this._addPopover();
        
        if (this.options.stretch == true) {
            this.$tip.css('max-width', 'none');
        }
    };

    bsInlineQuestion.prototype._delegateEvents = function () {
        this.$tip.on('click', '.bs-popoverBtn', $.proxy(function (e) {
            var idx = this.$tip.find('.bs-popoverBtn').index(e.currentTarget),
                btn = this.options.buttons[idx];

            if (btn != null && typeof btn.callback === "function") {
                e.preventDefault();
                e.stopPropagation();

                btn.callback.apply(this, arguments);
            }

        }, this));

        if (this.options.closeOnOuterClick && $(document).data('bsPopoverClickHandler') !== true) {
            $(document).on('click', function (e) {
                var $target = $(e.target);
                
                var $openPopovers = $('.bs-hasInlineQuestion').filter(function(idx, elem) {
                    var $elem = $(elem),
                        popover = $elem.data('bs.popover');

                    if (typeof popover !== "undefined" && popover != null) {
                        var $tip = popover.$tip;
                        return $tip.is(':visible') && $elem[0] != $target[0] && $elem.find($target).length == 0 && $target[0] != $tip[0] && $tip.find($target).length == 0;
                    }

                    return false;

                });

                $openPopovers.bsInlineQuestion('toggle');

            }).data('bsPopoverClickHandler', true);
        }
    };

    bsInlineQuestion.prototype._addPopover = function () {

        this.$element.popover($.extend(true, {
            html: true,
            placement: this.options.placement,
            content: this.options.cacheContent === true ? this._renderPopover() : $.proxy(this._renderPopover, this)
        }, this.options.popoverOptions)).addClass('bs-hasInlineQuestion');

        this.$element.on('show.bs.popover', $.proxy(function () {
            this._trigger('show', 0, arguments);
        },this));

        this.$element.on('shown.bs.popover', $.proxy(function() {
            this._trigger('shown', 0, arguments);
        }, this));
        
        this.$element.on('hide.bs.popover', $.proxy(function () {
            this._trigger('hide', 0, arguments);
        }, this));
        
        this.$element.on('hidden.bs.popover', $.proxy(function () {
            this._trigger('hidden', 0, arguments);
        }, this));


        this.$tip = this.$element.data('bs.popover').tip();
        this._delegateEvents();
    };

    bsInlineQuestion.prototype._renderPopover = function () {
        return typeof this.options.content === "undefined" ? ich.renderPopoverQuestion(this.options, true) : ich.renderPopoverContent(this.options, true);
    };
    //#endregion

    //#region public
    bsInlineQuestion.prototype.hide = function () {
        this.$element.popover('hide');
        
        return this;
    };

    bsInlineQuestion.prototype.show = function () {
        this.$element.popover('show');
        
        return this;
    };

    bsInlineQuestion.prototype.toggle = function () {
        this.$element.popover('toggle');
        
        return this;
    };

    bsInlineQuestion.prototype.content = function (data, overwriteIfNull) {

        if (typeof data !== "undefined") {

            if (typeof data.content !== "undefined" || overwriteIfNull) {
                this.options.content = data.content;
            }

            if (typeof data.question !== "undefined" || overwriteIfNull) {
                this.options.question = data.question;
            }

            if (typeof data.buttons !== "undefined" || overwriteIfNull) {
                this.options.buttons = data.buttons;
            }
        }

        return this;
    };

    bsInlineQuestion.prototype.destroy = function () {
        
        this.hide();
        this.$element.popover('destroy');

        this.$element.removeData('bformsBsInlineQuestion');
        this.$element.removeClass('bs-hasInlineQuestion');
        this.$tip.remove();
    };

    bsInlineQuestion.prototype._placementMethod = function (tip, button) {
        var $tip = $(tip),
            $button = $(button),
            offset = $button.offset(),
            top = offset.top,
            left = offset.left,
            windowHeight = $(window).outerHeight(),
            windowWidth = $(window).outerWidth();

        var $tipClone = $tip.clone().appendTo($('body')),
            tipWidth = $tipClone.outerWidth(),
            tipHeight = $tipClone.outerHeight();

        $tipClone.remove();

        var positionsSpace = {
            'left': left,
            'right': windowWidth - $button.outerWidth() - left,
            'top': top - $(window).scrollTop(),
            'bottom': windowHeight - $button.outerHeight() - (top - $(window).scrollTop())
        };

        for (var placementKey in this._placementArray) {
            var placement = this._placementArray[placementKey];
            
            if (placement == 'right' || placement == 'left') {
                if (positionsSpace[placement] > tipWidth) return placement;
            }
            
            if (placement == 'top' || placement == 'bottom') {
                if (positionsSpace[placement] > tipHeight) return placement;
            }
        }

        return this._placementArray[0] || 'left';
    };
    //#endregion

    $.widget('bforms.bsInlineQuestion', bsInlineQuestion.prototype);
});
