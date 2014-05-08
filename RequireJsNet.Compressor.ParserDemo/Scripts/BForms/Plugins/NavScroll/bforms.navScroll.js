(function (factory) {

    if (typeof define === "function" && define.amd) {
        define('bforms-navscroll', ['jquery'], factory);
    } else {
        factory(window.jQuery);
    }

}(function ($) {

    var navScroll = function ($element, options) {
        this.$element = $element;

        if (this.$element.length) {


            this.options = options;
            this.hash = null;
            this.breakOffset = null;

            this.$container = $(this.options.container);
            this.$sidebar = $(this.options.sidebar);
            this.$relativeElement = $(this.options.relativeElement);

            this.heightOffset = (this.$container.height() - this.$element.height() - (this.$element.outerHeight() - this.$element.height())) / 2;

            this.menuItems = this.$element.find('a');

            this.topBreak = this.options.topBreak;
            this.bottomBreak = this.options.bottomBreak;

            this.init();
        } 
    };

    navScroll.prototype.init = function () {

        this.updateHash();
        this._setPosition(true);
        this.addHandlers();

    };

    navScroll.prototype.updateHash = function () {
        this.hash = window.location.hash;

        if (this.hash != '') {
            this.updateNav(this.hash);
        } else {
            this._setHash();
        }
    };

    navScroll.prototype.addHandlers = function () {

        $(window).on('hashchange', $.proxy(this.updateHash, this));
        $(window).on('scroll', $.proxy(this.onScroll, this));

        this.$element.on('click', 'a:first', $.proxy(this._firstAnchorClick, this));
    };

    navScroll.prototype.onScroll = function (e) {
        this._setHash();
        this._setPosition(true);
    };

    navScroll.prototype._setHash = function () {
        var fromTop = $(window).scrollTop() + $('header').height() + 300;

        var visibleElements = this.menuItems.map(function (idx, anchor) {
            var $anchor = $(anchor),
                $elem = $($anchor.attr('href'));

            if ($elem.length === 1 && $elem.offset().top < fromTop) {
                return $elem;
            }
        });

        if (visibleElements != null) {

            var lastIndex = visibleElements.length - 1;
            var current = visibleElements[lastIndex];

            if (typeof current !== "undefined") {
                var id = current.prop('id');

                if (window.location.hash != '#' + id) {
                    current.prop('id', id + '_temp');
                    window.location.hash = '#' + id;
                    current.prop('id', id);
                }
            }
        }
    };

    navScroll.prototype._setPosition = function (callAgain) {

        var scrollTop = $(window).scrollTop();

        if (!this.$sidebar.hasClass('affix-bottom')) {

            if (this.$sidebar.offset().top + this.$sidebar.outerHeight() - this.heightOffset >= this.bottomBreak) {

                this.$sidebar.removeClass('affix-top');
                this.$sidebar.removeClass('affix');

                var top = this.bottomBreak - this.$sidebar.height() - this.$relativeElement.offset().top - this.heightOffset;

                this.$sidebar.css({
                    top: top
                });

                this.$sidebar.addClass('affix-bottom');
                this.breakOffset = scrollTop;
            } else if (scrollTop > this.topBreak) {

                this.$sidebar.removeClass('affix-top');
                this.$sidebar.addClass('affix');
                this.$sidebar.removeClass('affix-bottom');
                this.$sidebar.css({
                    top: 0
                });

            } else {
                this.$sidebar.removeClass('affix');
                this.$sidebar.removeClass('affix-bottom');
                this.$sidebar.addClass('affix-top');

                this.$sidebar.css({
                    top: 0
                });
            }
        } else if (this.breakOffset > scrollTop) {
            this.breakOffset = null;
            if (scrollTop > this.topBreak) {
                this.$sidebar.removeClass('affix-top');
                this.$sidebar.addClass('affix');
                this.$sidebar.removeClass('affix-bottom');
                this.$sidebar.css({
                    top: 0
                });

            } else {
                this.$sidebar.removeClass('affix');
                this.$sidebar.removeClass('affix-bottom');
                this.$sidebar.addClass('affix-top');
                this.$sidebar.css({
                    top: 0
                });
            }
        }

        if (callAgain) {
            this._setPosition();
        }
    };

    navScroll.prototype.updateNav = function (hash) {

        var $active = this.$element.find('*[href=' + hash + ']').parentsUntil(this.$element, this.options.receiver);

        if ($active.length) {
            this.removePrevious();
        }

        $active.addClass(this.options.activeClass);

    };

    navScroll.prototype._firstAnchorClick = function (e) {
        e.preventDefault();
        e.stopPropagation();

        $(window).scrollTop(0);
    };

    navScroll.prototype.removePrevious = function () {
        this.$element.find('.' + this.options.activeClass).removeClass(this.options.activeClass);
    };

    $.fn.bsNavScrollDefaults = {
        container: '.hidden-sm',
        sidebar: '.bs-sidebar',
        sidebarTopClass: 'affix-top',
        sidebarClass: 'affix',
        sidebarBottomClass: 'affix-bottom',
        activeClass: 'active',
        receiver: 'li'
    };

    $.fn.bsNavScroll = function (options) {
        return new navScroll($(this), $.extend(true, {}, options, $.fn.bsNavScrollDefaults));
    };
}));