var factory = function ($) {

    var Sortable = function (options) {

    };

    Sortable.prototype.options = {
        containerSelector: '.sortable-container',
        toolbarFormSelector: '.grid_toolbar_form',
        sortableListSelector: '.bs-sortable',
        itemSelector: '.bs-sortable-item',
        containerClass: 'sortable-container',
        itemClass: 'bs-sortable-item',
        sortableListClass: 'bs-sortable',
        navPillsClass: 'nav nav-pills nav-stacked',
        preventMigration: false
    };

    Sortable.prototype.renderers = {

        base: 'base',
        light: 'light'
    };

    Sortable.prototype.rendererSpecificOptions = {

        base: {

            handle: 'span',
            items: 'li',
            toleranceElement: '> span',
            listType: 'ol'
        },
        light: {

            handle: 'a',
            items: 'li',
            toleranceElement: 'a',
            listType: 'ul',
            cursor: 'pointer'
        }
    };

    Sortable.prototype.serializationMethods = {

        hierarchy: 'hierarchy',
        array: 'array'
    };

    Sortable.prototype._init = function () {

        this.$element = this.element;
        this.$container = this.$element.parents(this.options.containerSelector);
        this.renderer = this.$container.data('renderer') || this.renderers.base;

        this._initStyleOptions();

        this.$element.nestedSortable(this._getSortableOptions());
    };

    Sortable.prototype._initStyleOptions = function () {

        this.$container.css('overflow', 'auto');

        if (this.renderer === this.renderers.light) {

            this.$container.find(this.options.itemSelector).css('cursor', 'default');

            this.$container.on('click', 'a', function (e) {

                e.preventDefault();

                return;
            });
        }
    };

    Sortable.prototype._getSortableOptions = function () {

        var options = {

            handle: 'span',
            items: 'li',
            toleranceElement: '> span',
            listType: 'ol',
            helper: $.proxy(this._getSortableHelper, this),
            isAllowed: $.proxy(this._dropIsAllowed, this),
            update: $.proxy(this._evSortUpdate, this),
            start: $.proxy(this._evSortStart, this),
            stop: $.proxy(this._evSortStop, this)
        };

        var renderer = this.$container.data('renderer') || this.renderers.base,
            specificOptions = this.rendererSpecificOptions[renderer];

        options = $.extend(true, options, specificOptions);

        return options;
    };

    // #region event handlers

    Sortable.prototype._evSortUpdate = function (e, ui) {

        var $item = ui.item,
            $parent = $item.parents('ul:first');

        // re-apply light renderer style options

        if (this.renderer === this.renderers.light) {

            if (!$parent.hasClass(this.options.navPillsClass)) {
                $parent.addClass(this.options.navPillsClass);
            }

            if (!$parent.parent().hasClass(this.options.containerClass)) {

                $parent.css({
                    'margin-top': '4px',
                    'margin-left': '40px'
                });
            }
        }

        // expose the change event

        this._trigger('sortUpdate', e, {
            event: e,
            ui: ui,
            id: $item.data('id'),
            parentId: $parent.data('id')
        });
    };

    Sortable.prototype._evSortStop = function (e, ui) {

        var $item = ui.item;

        // fix classes on nearby list elements

        var listType = this.rendererSpecificOptions[this.renderer].listType,
            $parentList = $item.parents(listType + ':first'),
            $subList = $item.find(listType + ':first');

        if (!$parentList.hasClass(this.options.sortableListClass)) {
            $parentList.addClass(this.options.sortableListClass);
        }

        if (!$subList.hasClass(this.options.sortableListClass)) {
            $subList.addClass(this.options.sortableListClass);
        }

        // update data-order attribute

        var $siblings = $item.parents(this.options.sortableListSelector + ':first').find(this.options.itemSelector),
            isLightRenderer = this.renderer === this.renderers.light;

        $siblings.each(function (idx, el) {

            $(el).data('order', '' + (idx + 1));
            $(el).attr('data-order', '' + (idx + 1));

            if (isLightRenderer) {
                $(el).css('margin-top', '4px');
            }
        });
    };

    // #endregion

    // #region helpers

    Sortable.prototype._getSortableHelper = function (e, $element) {

        var $helper = $element.clone(),
            opacity = parseInt($helper.css('opacity'));

        $helper.css('opacity', 0.7 * opacity);

        return $helper;
    };

    Sortable.prototype._dropIsAllowed = function ($item, $parent) {

        // if no parent is provided, the move is valid

        if ($item == null || $parent == null) {
            return true;
        }

        // if the provided parent is actually the item's parent, the move is valid

        var parentId = $parent.data('id'),
            previousParentId = $item.parents(this.options.itemSelector + ':first').data('id');

        if (parentId === previousParentId) {
            return true;
        }

        // if migration is disabled, the move is invalid

        if (this.options.preventMigration || this.$element.data('migration-permited') === 'False') {
            return false;
        }

        // if the provided parent's id is contained in the item's "appends-to" property, the move is valid

        var validParents = $item.data('appends-to') != null ? $item.data('appends-to').split(' ') : [];

        return validParents.indexOf('' + parentId) != -1;
    };

    Sortable.prototype._serializeAsArray = function () {

        var $items = this.$element.find(this.options.itemSelector),
            serializedItems = [];

        $items.each($.proxy(function (idx, el) {

            var $item = $(el),
                $parent = $item.parents(this.options.itemSelector + ':first'),
                id = $item.data('id'),
                parentId = $parent.length != 0 ? $parent.data('id') : null,
                order = $item.data('order');

            serializedItems.push({
                value: id,
                order: order,
                parentId: parentId
            });
        }, this));

        return serializedItems;
    };

    Sortable.prototype._serializeAsHierarchy = function ($list) {

        $list = $list || this.$element;

        var serializedItems = [];

        var $items = $list.children(this.options.itemSelector);

        $items.each($.proxy(function (idx, el) {

            var $item = $(el),
                $subItems = $item.find(this.options.sortableListSelector + ':first').children(this.options.itemSelector),
                children = $subItems.length != 0 ? this._serializeAsHierarchy($item.find(this.options.sortableListSelector + ':first')) : null;

            var item = {
                value: $item.data('id'),
                children: children
            };

            serializedItems.push(item);

        }, this));

        return serializedItems;
    };

    // #endregion

    // #region public methods

    Sortable.prototype.serialize = function (serializationMethod) {

        serializationMethod = serializationMethod || this.serializationMethods.hierarchy;

        switch (serializationMethod) {

            case this.serializationMethods.hierarchy:
                {
                    return this._serializeAsHierarchy();
                }
            case this.serializationMethods.array:
                {
                    return this._serializeAsArray();
                }
            default:
                {
                    return null;
                }
        }
    };

    // #endregion

    $.widget('bforms.bsSortable', Sortable.prototype);

    return Sortable;
};

if (typeof define == 'function' && define.amd) {
    define('bforms-sortable', ['jquery',
            'jquery-ui-core',
            'nestedsortable'], factory);
} else {
    factory(window.jQuery);
}