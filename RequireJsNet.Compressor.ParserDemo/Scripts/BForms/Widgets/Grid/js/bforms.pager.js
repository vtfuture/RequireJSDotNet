define('bforms-pager', [
    'jquery',
    'jquery-ui-core'
], function () {

    var Pager = function (opt) {
        this.options = opt;
        this._create();
    };

    Pager.prototype.options = {
        pagesContainerSelector: '.bs-pages',
        pageSelector: 'a[data-page]',
        dataPageContainer: 'page',
        pageSizeSelector: '.bs-perPage',
        currentPageSelector: 'active',
        disabledPageSelector: 'disabled',
        topResultsMargin : '.bs-topResultsMargin',
        totalsContainerSelector: '.results_number span',
        pageSizeContainerSelector: '.results_per_page',
        perPageDisplaySelector: '.bs-perPageDisplay',
        goTopButtonSelector: '.bs-goTop',
        goTopTitle: 'Go top'
    };

    Pager.prototype._create = function () {

        this._initElements();
        this._addDelegates();

    };

    Pager.prototype._initElements = function () {
        if (typeof $(this.options.goTopButtonSelector) !== "undefined") {
            $(this.options.goTopButtonSelector).attr('title', this.options.goTopTitle);
        }
    };

    Pager.prototype._addDelegates = function () {

        this.element.on('click', this.options.pageSelector, $.proxy(this._evPageChanged, this));

        this.element.on('click', this.options.pageSizeSelector, $.proxy(this._evPageSizeChanged, this));

        this.element.on('click', this.options.goTopButtonSelector, $.proxy(this._evGoTopClick, this));

    };

    Pager.prototype._evPageChanged = function (e) {

        e.preventDefault();

        var $me = $(e.currentTarget),
            $parent = $me.parent();

        if ($parent.hasClass(this.options.currentPageSelector) ||
            $parent.hasClass(this.options.disabledPageSelector)) {
            return;
        }

        this._trigger('pagerUpdate', e, {
            page: $me.data(this.options.dataPageContainer),
            pageSize: this.element.find(this.options.pageSizeSelector + '.selected').data('value')
        });

    };

    Pager.prototype._evPageSizeChanged = function (e) {

        e.preventDefault();

        var value = $(e.currentTarget).data('value');
        this.element.find('.selected').removeClass('selected');
        $(e.currentTarget).addClass('selected');

        this.element.find(this.options.perPageDisplaySelector).html(value + '<span class="caret"></span>');

        this._trigger('pagerUpdate', e, {
            page: 1,
            pageSize: $(e.currentTarget).data('value')
        });

    };

    Pager.prototype._evGoTopClick = function (e) {
        this._trigger('pagerGoTop', e);
    };

    Pager.prototype._btnClickAjaxSuccess = function (data, callbackData) {
        if (typeof callbackData.handler === 'function') {
            callbackData.handler.call(this, callbackData.sent, data);
        }
    };

    Pager.prototype._btnClickAjaxError = function () {
        throw 'not implemented error handler';
    };

    Pager.prototype.update = function ($pagesHtml) {

        var $pageSizeContainer = this.element.find(this.options.pageSizeContainerSelector)

        var $pagesContainer = this.element.find(this.options.pagesContainerSelector);

        if ($pagesContainer.length > 0) {
            $pagesContainer.html($pagesHtml.children());
        } else {
            this.element.html($pagesHtml);
        }

        $pagesHtml.length == 0 ? $pageSizeContainer.hide() : $pageSizeContainer.show();
    };

    Pager.prototype.add = function () {

        var currentPage = window.parseInt(this.element.find('.' + this.options.currentPageSelector).find('a').data('page'),10);

        var oldTotal = window.parseInt(this.element.find(this.options.totalsContainerSelector).not(this.options.topResultsMargin).text(), 10);
        if (!window.isNaN(oldTotal)) {
            var newTotal = oldTotal + 1;

            this.element.find(this.options.totalsContainerSelector).not(this.options.topResultsMargin).html(newTotal);
        }

        var oldTopMargin = window.parseInt(this.element.find(this.options.topResultsMargin).text(), 10);

        if (!window.isNaN(oldTopMargin)) {
            var pageSize = this.getPageSize();
            if (oldTopMargin > pageSize * (currentPage - 1) &&  oldTopMargin < pageSize * currentPage) {
                var newTopMargin = oldTopMargin + 1;
                this.element.find(this.options.topResultsMargin).text(newTopMargin);
            }
        }
    };

    Pager.prototype.updateTotal = function (total) {
        this.element.find(this.options.totalsContainerSelector).html(total);
    };

    Pager.prototype.selectValue = function (value) {

        var $pageSize = this.element.find(this.options.pageSizeSelector + '[data-value="' + value + '"]');

        if ($pageSize.length) {
            this.element.find('.selected').removeClass('selected');
            $pageSize.addClass('selected');
            this.element.find(this.options.perPageDisplaySelector).html(value + '<span class="caret"></span>');
        }

    };

    Pager.prototype.getPageSize = function () {
        return this.element.find(this.options.pageSizeSelector + '.selected').data('value');
    };

    $.widget('bforms.bsPager', Pager.prototype);

    return Pager;

});