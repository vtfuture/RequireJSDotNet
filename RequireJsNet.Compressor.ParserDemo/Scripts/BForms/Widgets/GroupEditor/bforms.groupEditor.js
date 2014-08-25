define('bforms-groupEditor', [
    'jquery',
    'jquery-ui-core',
    'bforms-pager',
    'bforms-ajax',
    'bforms-namespace',
    'bforms-inlineQuestion',
    'bforms-form'
], function () {

    //#region Constructor and Properties
    var GroupEditor = function (opt) {
        this.options = $.extend(true, {}, this.options, opt);
        this._init();
    };

    GroupEditor.prototype.options = {
        uniqueName: '',

        tabsSelector: '.bs-tabs',
        groupsSelector: '.bs-groups',
        groupSelector: '.bs-group',

        navbarSelector: '.bs-navbar',
        toolbarBtnSelector: '.bs-toolbarBtn',
        editorFormSelector: '.bs-editorForm',
        pagerSelector: '.bs-pager',
        tabContentSelector: '.bs-tabContent',
        tabItemSelector: '.bs-tabItem',
        tabItemsListSelector: '.bs-tabItemsList',

        groupBulkMoveSelector: '.bs-moveToGroupBtn',
        groupEditorFormSelector: '.bs-groupForm',
        groupItemSelector: '.bs-groupItem',
        groupItemTemplateSelector: '.bs-itemTemplate',
        groupItemContentSelector: '.bs-itemContent',
        groupItemsWrapper: '.bs-itemsWrapper',
        groupItemsCounter: '.bs-counter',

        tabInlineSearch: '.bs-tabInlineSearch',

        removeBtn: '.bs-removeBtn',
        addBtn: '.bs-addBtn',
        editBtn: '.bs-editBtn',
        upBtn: '.bs-upBtn',
        downBtn: '.bs-downBtn',
        toggleExpandBtn: '.bs-toggleExpand',

        resetBtn: '.bs-resetGroupEditor',
        saveBtn: '.bs-saveGroupEditor',

        getTabUrl: '',

        validation: {},
        errorMessageContainer: '.bs-errorMessage',
        errorMessageHolder: '.bs-errorMessageHolder',
        noResultsItemTabSelector: '.bs-noResultsTabItem',
        
    };
    //#endregion

    //#region Init
    GroupEditor.prototype._init = function () {

        this.$element = this.element;

        if (!this.options.uniqueName) {
            this.options.uniqueName = this.$element.attr('id');
        }

        if (!this.options.uniqueName) {
            throw 'grid needs a unique name or the element on which it is aplied has to have an id attr';
        }

        this._initOptions();

        this._initSelectors();

        this._initComponents();

        this._addDelegates();

        this._initSelectedTab();

        this._initTemplates();

        this._initSortable();

        this._initGroupItemForms();
    };

    GroupEditor.prototype._initComponents = function () {
        this._countGroupItems();
        this._checkEditable();
    };

    GroupEditor.prototype._initOptions = function () {
        this.options.saveUrl = this.options.saveUrl || this.$element.find(this.options.saveBtn).attr('href');
    };

    GroupEditor.prototype._initSelectors = function () {
        this.$navbar = this.$element.find(this.options.navbarSelector);
        this.$tabs = this.$element.find(this.options.tabsSelector);
        this.$groups = this.$element.find(this.options.groupsSelector);
        this.$counter = this.$groups.find(this.options.groupItemsCounter);
        this.$groupForm = this.$element.find(this.options.groupEditorFormSelector);
    };

    GroupEditor.prototype._addDelegates = function () {
        this.$navbar.on('click', 'a', $.proxy(this._evChangeTab, this));
        this.$tabs.find('div[data-tabid]').on('click', 'button' + this.options.toolbarBtnSelector, $.proxy(this._evChangeToolbarForm, this));
        this.$tabs.on('click', this.options.addBtn, $.proxy(this._evAdd, this));
        this.$groups.on('click', this.options.removeBtn, $.proxy(this._evRemove, this));
        this.$groups.on('click', this.options.editBtn, $.proxy(this._evEdit, this));
        this.$groups.on('click', this.options.upBtn, $.proxy(this._evUp, this));
        this.$groups.on('click', this.options.downBtn, $.proxy(this._evDown, this));
        this.$groups.on('click', this.options.toggleExpandBtn, $.proxy(this._evToggleExpand, this));
        this.$element.on('click', this.options.resetBtn, $.proxy(this._evResetClick, this));
        this.$element.on('click', this.options.saveBtn, $.proxy(this._evSaveClick, this));
        this.$tabs.on('click', this.options.groupBulkMoveSelector, $.proxy(this._evBulkMoveClick, this));
    };

    GroupEditor.prototype._initSelectedTab = function () {
        this._initTab(this._getSelectedTab());
    };

    GroupEditor.prototype._initTemplates = function () {
        this.element.find(this.options.groupSelector).each($.proxy(function (idx, group) {
            var $group = $(group),
                $template = $group.find(this.options.groupItemTemplateSelector);

            $group.data('template', $template.children(':first').clone());
            $template.remove();

        }, this));
    };

    GroupEditor.prototype._initTab = function (tabModel, preventShow) {
        var self = this;

        if (tabModel) {
            var checkItems = true;
            if (!tabModel.loaded) {
                checkItems = false;
                this._ajaxGetTab({
                    tabModel: tabModel,
                    data: {
                        settings: {
                            TabId: tabModel.tabId
                        }
                    }
                });
            }

            if (tabModel.html) {
                tabModel.container.find(this.options.tabContentSelector).html(tabModel.html);
            }

            if (!tabModel.pager) {
                tabModel.pager = tabModel.container.find(this.options.pagerSelector);
                tabModel.pager.bsPager({
                    pagerUpdate: function (e, data) {
                        self._evChangePage(data, tabModel);
                    },
                    pagerGoTop: $.proxy(this._evGoTop, this)
                });
            }

            if (preventShow !== true) {
                tabModel.container.show();
            }

            if (checkItems) {
                this._checkItems();
            }

            if (tabModel.container.data('init') !== true) {

                this._trigger('onTabInit', 0, tabModel);

                var $editorForm = tabModel.container.find(this.options.editorFormSelector);

                $editorForm.each($.proxy(function (idx, form) {
                    var $form = $(form),
                        uid = $form.data('uid');

                    if (typeof this.options.initEditorForm === "function") {
                        this.options.initEditorForm($form, uid, tabModel);
                    }

                }, this));

                tabModel.container.on('change', this.options.tabInlineSearch, $.proxy(this._evInlineSearch, this, tabModel));

            }

            tabModel.container.data('init', true);

            this._initDraggable(tabModel);
            this._initBulkMove(tabModel);
        }
    };

    GroupEditor.prototype._initSortable = function () {
        this.$element.find(this.options.groupSelector).find(this.options.groupItemsWrapper).sortable({
            items: this.options.groupItemSelector,
            distance: 5,
            connectWith: this.options.groupSelector,
            start: $.proxy(this._sortStart, this),
            beforeStop: $.proxy(this._beforeSortStop, this),
            stop: $.proxy(this._sortStop, this)
        });
    };

    GroupEditor.prototype._initDraggable = function (tabModel) {

        tabModel.container.find(this.options.tabItemSelector).each($.proxy(function (index, tabItem) {

            var $tabItem = $(tabItem);

            this._checkItem($tabItem, tabModel.tabId, tabModel.connectsWith);

            $tabItem.draggable(this._getDraggableOptions(tabModel));
        }, this));

    };

    GroupEditor.prototype._initGroupItemForms = function () {

        if (this.$groupForm.length) {
            this.$groupForm.bsForm({
                uniqueName: 'groupEditorForm'
            });
        }

        var $groupItemForms = this.$element.find(this.options.groupsSelector)
                                       .find(this.options.editorFormSelector);

        $groupItemForms.each($.proxy(function (idx, form) {

            this._initGroupItemForm($(form));

        }, this));

    };

    GroupEditor.prototype._initGroupItemForm = function ($form) {

        $form.bsForm({
            uniqueName: $form.data('uid')
        });

    };

    GroupEditor.prototype._initBulkMove = function (tab) {
        tab.container.find(this.options.groupBulkMoveSelector).each($.proxy(function (idx, bulkMoveItem) {

            var $bulkMoveItem = $(bulkMoveItem);
            if (jQuery.inArray($bulkMoveItem.data('groupid'), tab.connectsWith) < 0) {
                $bulkMoveItem.parent().remove();
            }

        }, this));
    };
    //#endregion

    //#region Events
    GroupEditor.prototype._evUp = function (e) {
        e.preventDefault();
        var $item = $(e.currentTarget).parents(this.options.groupItemSelector).first(),
            $prevItem = $item.prevAll(this.options.groupItemSelector).first(),
            itemHeight = ($item.outerHeight() + window.parseInt($item.css('margin-bottom'))),
            prevHeight = $prevItem.outerHeight() + window.parseInt($prevItem.css('margin-top')),
            moveDistance = Math.min(itemHeight, prevHeight);



        if ($prevItem.length > 0) {

            $item.animate({
                top: -moveDistance
            });

            $prevItem.animate({
                top: moveDistance
            }, function () {

                $prevItem.css('top', 'auto');
                $item.css('top', 'auto');

                $prevItem.before($item);
            });

            this._rebuildNumbers();

        } else {
            this._shakeElement($item);
        }
    };

    GroupEditor.prototype._evDown = function (e) {
        e.preventDefault();
        var $item = $(e.currentTarget).parents(this.options.groupItemSelector).first(),
            $nextElem = $item.nextAll(this.options.groupItemSelector).first(),
            itemHeight = ($item.outerHeight() + window.parseInt($item.css('margin-bottom'))),
            nextHeight = ($nextElem.outerHeight() + window.parseInt($nextElem.css('margin-top'))),
            moveDistance = Math.min(itemHeight, nextHeight);


        var itemZIndex = $item.css('z-index'),
            nextZIndex = $nextElem.css('z-index');

        var nextParsedZIndex = window.parseInt(nextZIndex, 10);
        if (!window.isNaN(nextParsedZIndex)) {
            $item.css('z-index', nextParsedZIndex + 1);
        } else {
            $item.css('z-index', 16777271);
            $nextElem.css('z-index', 16777270);
        }


        if ($nextElem.length > 0) {

            $item.animate({
                top: moveDistance
            });

            $nextElem.animate({
                top: -moveDistance
            }, function () {

                $item.css('z-index', itemZIndex);
                $nextElem.css('z-index', nextZIndex);

                $nextElem.css('top', 'auto');
                $item.css('top', 'auto');

                $nextElem.after($item);
            });

            this._rebuildNumbers();
        } else {
            this._shakeElement($item);
        }
    };

    GroupEditor.prototype._evBulkMoveClick = function (e) {

        var targetGroupId = $(e.currentTarget).data('groupid');

        var currentTab = this._getSelectedTab(),
            $movableItems = currentTab.container.find(this.options.tabItemSelector),
            hasMoved = false;

        $movableItems.each($.proxy(function (idx, tabItem) {

            var $tabItem = $(tabItem);

            hasMoved = this.addToGroup({
                objId: $tabItem.data('objid'),
                itemModel: $tabItem.data('model'),
                tabId: currentTab.tabId
            }, targetGroupId, true) || hasMoved;

        }, this));

        if (!hasMoved) {
            this._showMoveError(this.$element);
        }

        e.preventDefault();
    };

    GroupEditor.prototype._evToggleExpand = function (e) {
        e.preventDefault();
        var $el = $(e.currentTarget),
            $group = $el.parents('[data-groupid]'),
            $container = $group.find(this.options.groupItemsWrapper);
        $container.toggle('fast', $.proxy(function () {
            $group.find(this.options.toggleExpandBtn).toggleClass('open');
        }, this));
    };

    GroupEditor.prototype._evChangeToolbarForm = function (e) {
        e.preventDefault();
        var $el = $(e.currentTarget),
            uid = $el.data('uid'),
            tab = this._getSelectedTab(),
            $container = tab.container,
            visibleForm = $container.find("div[data-uid]:visible"),
            visibleUid = visibleForm.data('uid');

        visibleForm.slideUp();

        if (visibleUid != uid) {
            $container.find("div[data-uid='" + uid + "']").slideDown();
        }
    };

    GroupEditor.prototype._evChangeTab = function (e) {
        e.preventDefault();
        var $el = $(e.currentTarget),
            tabId = $el.data('tabid'),
            $container = this._getTab(tabId),
            loaded = this._isLoaded($container),
            connectsWith = $container.data('connectswith');

        this._hideTabs();

        this._initTab({
            container: $container,
            loaded: loaded,
            tabId: tabId,
            connectsWith: connectsWith
        });
    };

    GroupEditor.prototype._evChangePage = function (data, tabModel) {
        this._ajaxGetTab({
            tabModel: tabModel,
            data: {
                settings: {
                    Page: data.page,
                    PageSize: data.pageSize || 5,
                    TabId: tabModel.tabId
                }
            }
        });
    };

    GroupEditor.prototype._evGoTop = function (e) {
        e.preventDefault();

        $.bforms.scrollToElement(this.$element.find(this.options.navbarSelector));
    };

    GroupEditor.prototype._evRemove = function (e) {
        e.preventDefault();
        var $el = $(e.currentTarget),
            $item = $el.parents(this.options.groupItemSelector);

        this._removeItemGroup($item);
    };

    GroupEditor.prototype._evAdd = function (e) {
        e.preventDefault();
        var $el = $(e.currentTarget),
            $tabItem = $el.parents(this.options.tabItemSelector),
            objId = $tabItem.data('objid'),
            tabModel = this._getSelectedTab(),
            tabId = tabModel.tabId,
            connectsWith = tabModel.connectsWith,
            $groups = this._getGroups(connectsWith);

        $.each($groups, $.proxy(function (idx, group) {

            var $group = $(group);

            if (!this._isInGroup(objId, tabId, $group)) {

                var allowMove = true,
                    tabItem = this._getTabItem(tabId, objId),
                    model = tabItem.data('model');

                if (typeof this.options.validateMove === "function") {
                    var validateResult = this.options.validateMove(model, tabId, $group);
                    allowMove = typeof validateResult === "boolean" ? validateResult : true;
                }

                if (allowMove) {

                    var $template = this._getGroupItemTemplate($(group), tabId, objId, model);

                    var view = this._renderGroupItem(model, group, tabId, objId);

                    $template.find(this.options.groupItemContentSelector).html(view);

                    this._checkEditableItem($template);

                    this._initGroupItemForm($template.find(this.options.editorFormSelector));

                    $group.find(this.options.groupItemsWrapper).append($template);

                    this._trigger('onTabItemAdd', 0, {
                        model: model,
                        tabId: tabId,
                        $row: $template,
                        $group: $group
                    });

                    this.validateUnobtrusive();

                    $tabItem.effect('transfer', {
                        to: $group.find($template)
                    });

                    this._checkItem(this._getTabItem(tabId, objId), tabId, connectsWith);

                    return false;
                }
            }
        }, this));

        this._countGroupItems();
    };

    GroupEditor.prototype._evEdit = function (e) {
        e.preventDefault();
        var $el = $(e.currentTarget),
            $item = $el.parents(this.options.groupItemSelector),
            objId = $item.data('objid'),
            tabId = $item.data('tabid'),
            groupId = $item.parents('[data-groupid]').data('groupid');

        throw "[objId: " + objId + ", groupId: " + groupId + ", tabId: " + tabId + "] . not implemented yet";
    };

    GroupEditor.prototype._evResetClick = function (e) {

        this.reset();

        e.preventDefault();
        e.stopPropagation();
    };

    GroupEditor.prototype._evSaveClick = function (e) {
        e.preventDefault();
        e.stopPropagation();

        var parseData = this.parse();

        if (parseData.valid) {

            var data = parseData.data;

            if (typeof this.options.additionalData !== "undefined") {
                $.extend(true, data, this.options.additionalData);
            }

            this._trigger('beforeSaveAjax', 0, data);

            this._ajaxSaveGroup(data);
        }
    };

    GroupEditor.prototype._evInlineSearch = function (tabModel, e) {

        var $target = $(e.currentTarget),
            searchVal = $target.val();

        tabModel.QuickSearch = searchVal;

        this.search(tabModel);
    };
    //#endregion

    //#region Ajax
    GroupEditor.prototype._ajaxSaveGroup = function (data) {

        $.bforms.ajax({
            name: this.options.uniqueName + '|saveGroupEditor',
            url: this.options.saveUrl,
            data: data,
            callbackData: data,
            context: this,
            success: $.proxy(this._ajaxSaveGroupSuccess),
            error: $.proxy(this._ajaxSaveGroupError),
            loadingElement: this.$element,
            loadingClass: 'loading'
        });

    };

    GroupEditor.prototype._ajaxSaveGroupSuccess = function (response, callback) {

        this._trigger('onSaveSuccess', 0, arguments);

    };

    GroupEditor.prototype._ajaxSaveGroupError = function (data, callbackData) {

        if (typeof data !== "undefined" && typeof data.Message === "string") {
            this._showValidationError(data.Message);
        }

        this._trigger('onSaveError', 0, arguments);
    };

    GroupEditor.prototype._ajaxGetTab = function (param) {

        if (typeof this.options.additionalData !== "undefined") {
            $.extend(true, param.data, this.options.additionalData);
        }

        this._trigger('beforeGetTab', 0, param);

        var ajaxOptions = {
            name: this.options.uniqueName + "|getTab|" + param.tabModel.tabId,
            url: this.options.getTabUrl,
            data: param.data,
            callbackData: param,
            context: this,
            success: $.proxy(this._ajaxGetTabSuccess, this),
            error: $.proxy(this._ajaxGetTabError, this),
            loadingElement: param.tabModel.container,
            loadingClass: 'loading'
        };

        $.bforms.ajax(ajaxOptions);
    };

    GroupEditor.prototype._ajaxGetTabSuccess = function (response, callback) {
        if (response) {

            this._trigger('onGetTabSuccess', 0, arguments);

            var $container = callback.tabModel.container;
            $container.data('loaded', 'true');

            if (response.Html) {
                this._initTab({
                    container: $container,
                    loaded: true,
                    html: response.Html,
                    tabId: callback.tabModel.tabId,
                    connectsWith: $container.data('connectswith')
                }, callback.data.preventShow);
            }
        }
    };

    GroupEditor.prototype._ajaxGetTabError = function () {
        console.trace();

        this._trigger('onGetTabError', 0, arguments);
    };
    //#endregion

    //#region Helpers
    GroupEditor.prototype._checkEditable = function () {
        $.each(this._getGroupItems(), $.proxy(function (idx, item) {
            this._checkEditableItem($(item));
        }, this));
    };

    GroupEditor.prototype._checkEditableItem = function ($item) {
        if (this._isTabEditable($item.data('tabid'))) {
            $item.find(this.options.editBtn).show();
        }
    };

    GroupEditor.prototype._rebuildNumbers = function () {

    };

    GroupEditor.prototype._shakeElement = function ($element) {
        $element.effect('shake', { times: 2, direction: 'up', distance: 10 }, 50);
    };

    GroupEditor.prototype._countGroupItems = function () {
        this.$counter.html(this._getGroupItems().length);
    };

    GroupEditor.prototype._checkItem = function ($item, tabId, connectsWith) {
        if (this._isItemSelected($item.data('objid'), tabId, connectsWith, $item.data('model'))) {
            this._toggleItemCheck($item, false);
        }
    };

    GroupEditor.prototype._checkItems = function () {
        var selectedTab = this._getSelectedTab(),
            $items = selectedTab.container.find(this.options.tabItemSelector);

        $.each($items, $.proxy(function (idx, item) {
            this._checkItem($(item), selectedTab.tabId, selectedTab.connectsWith);
        }, this));
    };

    GroupEditor.prototype._isItemSelected = function (objid, tabId, groupIds, itemModel) {
        var $groups = this._getGroups(groupIds),
            allowedGroupsMove = 0,
            inGroups = 0;

        $.each($groups, $.proxy(function (idx, group) {

            var $group = $(group),
                isInGroup = this._isInGroup(objid, tabId, $group),
                allowMove = true;

            if (typeof this.options.validateMove === "function") {
                var validateResult = this.options.validateMove(itemModel, tabId, $group);
                allowMove = typeof validateResult === "boolean" ? validateResult : true;
            }

            if (allowMove) {
                allowedGroupsMove++;
            }

            if (isInGroup) {
                inGroups++;
            }

        }, this));

        var itemCount = this.getItemCount(objid);

        return (allowedGroupsMove > 0 || itemCount > 0) ? (allowedGroupsMove == 0 ? (allowedGroupsMove + itemCount === inGroups) : allowedGroupsMove === inGroups) : false;
    };

    GroupEditor.prototype._isInGroup = function (objId, tabId, $group) {
        var isInGroup = false,
            $groupItems = $group.find(this.options.groupItemSelector);

        $.each($groupItems, function (idx, item) {

            var $item = $(item);

            if ($item.data('objid') == objId && $item.data('tabid') == tabId && !$item.hasClass('temp-sortable')) {
                isInGroup = true;
                return false;
            }
        });
        return isInGroup;
    };

    GroupEditor.prototype._toggleItemCheck = function ($item, forceUncheck) {
        var $glyph = $item.find('span.glyphicon'),
            addBtn = this.options.addBtn.replace(".", "");

        if (forceUncheck && !$item.hasClass('selected')) {
            return false;
        }

        if (!forceUncheck && $item.hasClass('selected')) {
            return false;
        }

        $item.toggleClass('selected');
        $glyph.parents('a:first').toggleClass(addBtn);

        if ($glyph.hasClass('glyphicon-ok')) {
            $glyph.removeClass('glyphicon-ok')
                .addClass('glyphicon-plus');
        } else {
            $glyph.removeClass('glyphicon-plus')
                .addClass('glyphicon-ok');
        }

        this._trigger('afterToggleItem', 0, arguments);
    };

    GroupEditor.prototype._removeItemGroup = function ($item) {
        var tabId = $item.data('tabid'),
            objId = $item.data('objid');

        $item.effect('drop', $.proxy(function () {
            $item.remove();
            this._toggleItemCheck(this._getTabItem(tabId, objId), true);
            this._countGroupItems();
        }, this));
    };

    GroupEditor.prototype._uncheckAllItems = function () {

        var $checked = this.$element.find(this.options.tabItemSelector + '.selected');

        $checked.each($.proxy(function (idx, item) {
            this._toggleItemCheck($(item), true);
        }, this));

    };

    GroupEditor.prototype._getSelectedTab = function () {
        var $container = this.$tabs.find('div[data-tabid]:visible'),
            tabId = $container.data('tabid'),
            connectsWith = $container.data('connectswith'),
            loaded = this._isLoaded($container);

        return {
            container: $container,
            tabId: tabId,
            connectsWith: connectsWith,
            loaded: loaded
        };
    };

    GroupEditor.prototype._hideTabs = function () {

        var $containers = this.$tabs.find('div[data-tabid]');

        $containers.hide();
    };

    GroupEditor.prototype._isLoaded = function ($element) {

        var dataLoaded = $element.data('loaded');

        return dataLoaded == "true" || dataLoaded == "True" || dataLoaded == true;
    };

    GroupEditor.prototype._getTab = function (tabId) {
        return this.$tabs.find('div[data-tabid="' + tabId + '"]');
    };

    GroupEditor.prototype._isTabEditable = function (tabId) {
        return this._getTab(tabId).data('editable');
    };

    GroupEditor.prototype._getTabItem = function (tabId, objId) {
        var $container = this._getTab(tabId),
            $item = $container.find(this.options.tabItemSelector + '[data-objid="' + objId + '"]');
        return $item;
    };

    GroupEditor.prototype._getGroup = function (groupId) {
        return this.$groups.find('div[data-groupid="' + groupId + '"]');
    };

    GroupEditor.prototype._getGroupItems = function () {
        return this.$groups.find(this.options.groupItemSelector + ':not(' + this.options.groupItemTemplateSelector + ' >)');
    };

    GroupEditor.prototype._getGroups = function (groupIds) {
        var $groups;
        if (groupIds) {
            $groups = [];
            $.each(groupIds, $.proxy(function (idx, groupId) {
                $groups.push(this._getGroup(groupId));
            }, this));
        } else {
            $groups = this.$groups.find('div[data-groupid]');
        }
        return $groups;
    };

    GroupEditor.prototype._getGroupItemTemplate = function ($group, tabId, objId, model) {
        var template = $group.data('template').clone();

        template.data('objid', objId);
        template.data('tabid', tabId);
        template.data('model', model);

        var html = template.html();

        html = html.replace(/__tabid__/gi, tabId);
        html = html.replace(/{{tabid}}/gi, tabId);
        html = html.replace(/__objid__/gi, tabId);
        html = html.replace(/{{objid}}/gi, objId);

        template.html(html);

        var data = {
            template: template
        };

        this._trigger('afterGetGroupItemTemplate', 0, data);

        return data.template;
    };

    GroupEditor.prototype._renderGroupItem = function (model, group, tabId, objId) {

        if (typeof this.options.buildGroupItem === "function") {
            return this.options.buildGroupItem(model, group, tabId, objId);
        } else {
            if (typeof model.Name === "undefined") {
                throw "No buildGroupItem method define and no Name property found on object";
            } else {
                return model.Name;
            }
        }
    };
    //#endregion

    //#region Dragable & Sortable helpers
    GroupEditor.prototype._getDraggableOptions = function (tabModel) {

        return {
            distance: 5,
            connectToSortable: this._buildConnectsWithSelector(tabModel.connectsWith),
            helper: typeof this.options.buildDragHelper === "function" ? $.proxy(this._buildDragElement, this) : 'clone',
            cursorAt: {
                top: typeof this.options.getCursorAtTop === "function" ? this.options.getCursorAtTop(tabModel) : 0,
                left: typeof this.options.getCursorAtLeft === "function" ? this.options.getCursorAtLeft(tabModel) : 0,
            },
            start: $.proxy(this._dragStart, this),
            stop: $.proxy(this._dragStop, this),
            cancel: '.selected'
        };
    };

    GroupEditor.prototype._buildDragElement = function (e) {

        var $dragged = $(e.currentTarget),
            model = $dragged.data('model'),
            $tab = $dragged.parents('[data-tabid]'),
            tabId = $tab.data('tabid'),
            connectsWith = $tab.data('connectswith');

        var $draggedHelper = $(this.options.buildDragHelper(model, tabId, connectsWith));

        return $draggedHelper;
    };

    GroupEditor.prototype._buildConnectsWithSelector = function (allowed) {
        var selector = '';
        for (var key in allowed) {
            selector += this.options.groupSelector + '[data-groupid="' + allowed[key] + '"]' + ' ' + this.options.groupItemsWrapper;

            if (key < allowed.length - 1) {
                selector += ', ';
            }
        }

        return selector;
    };

    GroupEditor.prototype._dragStart = function (e, ui) {

        var $originalElement = $(e.target),
            $tab = $originalElement.parents('[data-connectswith]'),
            connectsWith = $tab.data('connectswith');

        this._dragging = true;

        this._addOpacity(connectsWith);

        this._trigger('onDragStart', 0, arguments);

    };

    GroupEditor.prototype._dragStop = function () {

        this._dragging = false;
        this._removeOpacity();

        this._trigger('onDragStop', 0, arguments);

        this._countGroupItems();
    };

    GroupEditor.prototype._sortStart = function (e, ui) {

        var $item = ui.item,
            groupId = $item.parents(this.options.groupSelector).data('groupid');

        $item.data('groupid', groupId)
             .addClass('temp-sortable');

        this._trigger('onSortStart', 0, arguments);
    };

    GroupEditor.prototype._beforeSortStop = function (e, ui) {
    };

    GroupEditor.prototype._sortStop = function (e, ui) {

        this._trigger('onSortStop', 0, arguments);

        var $item = ui.item,
            $group = $item.parents(this.options.groupSelector).first(),
            tabModel = this._getSelectedTab(),
            connectsWith = tabModel.connectsWith || [],
            model = $item.data('model'),
            tabId = tabModel.tabId,
            groupId = $group.data('groupid'),
            objId = $item.data('objid');

        this._removeOpacity();

        //validation
        var isInGroup = this._isInGroup(objId, tabId, $group),
            isConnectable = connectsWith.indexOf(groupId) !== -1,
            allowMove = isConnectable && !isInGroup;

        if (this._dragging !== true) {

            //inner group sorting or moving from one group to another
            var sourceGroupId = $item.data('groupid'),
                targetGroupId = $group.data('groupid');

            if (sourceGroupId == targetGroupId) return;

        }

        //cleanup
        $item.removeData('groupid')
             .removeClass('temp-sortable');

        if (typeof this.options.validateMove === "function") {
            var validateResult = this.options.validateMove(model, tabId, $group);
            allowMove = typeof validateResult === "boolean" ? validateResult : allowMove;
        }

        if (allowMove) {
            var $template = this._getGroupItemTemplate($group, tabId, objId, model);

            var view = this._renderGroupItem(model, $group, tabId, objId);

            $template.find(this.options.groupItemContentSelector).html(view);

            this._initGroupItemForm($template.find(this.options.editorFormSelector));

            this._checkEditableItem($template);

            ui.item.replaceWith($template);

            this._trigger('onTabItemAdd', 0, {
                model: model,
                tabId: tabId,
                $row: $template,
                $group: $group
            });

            this.validateUnobtrusive();

            this._checkItem(this._getTabItem(tabId, objId), tabId, connectsWith);

        } else {

            this._showMoveError($group);

            if (!this._dragging) {
                return false;
            } else {
                ui.item.remove();
            }
        }

    };

    GroupEditor.prototype._showMoveError = function ($elem) {

        if (typeof this.options.showMoveError === "function") {
            this.options.showMoveError($elem);
        } else {
            this.$element.css({
                'cursor': 'not-allowed'
            });

            window.setTimeout($.proxy(function () {
                this.$element.css({
                    'cursor': ''
                });
            }, this), 300)
        }

    };

    GroupEditor.prototype._addOpacity = function (exclude) {

        var $groups = this.$element.find(this.options.groupSelector);

        exclude = exclude || [];

        $groups.each(function (idx, elem) {
            var $elem = $(elem),
                sortId = $elem.data('groupid');

            if (exclude.indexOf(sortId) === -1) {
                $elem.css('opacity', '0.3');
            }

        });
    };

    GroupEditor.prototype._removeOpacity = function () {
        var $groups = this.$element.find(this.options.groupSelector);
        $groups.css('opacity', '1');
    };
    //#endregion

    //#region Public methods
    GroupEditor.prototype.parse = function (preventValidation) {

        var parseData = {},
            $groupList = this.$groups.find(this.options.groupSelector),
            isValid = true,
            itemsCount = 0;

        $groupList.each($.proxy(function (index, group) {
            var $group = $(group),
                groupName = $group.data('propertyname'),
                groupData = {
                    Items: []
                };

            var $groupItems = $group.find(this.options.groupItemSelector).filter($.proxy(function (filterIndex, item) {
                return $(item).parents(this.options.groupItemTemplateSelector).length == 0;
            }, this));

            $groupItems.each($.proxy(function (itemIndex, item) {
                var $item = $(item),
                    itemModel = {};

                //get id and tabId
                itemModel.Id = $item.data('objid');
                itemModel.TabId = $item.data('tabid');

                //get form data ( if any)
                var $form = $item.find(this.options.editorFormSelector);
                if ($form.length) {

                    var prefix = 'prefix' + $form.data('uid') + '.';

                    $form.find('form').removeData('validator').removeData('unobstrusiveValidation');
                    $.validator.unobtrusive.parse($form.find('form'));

                    var validator = $form.find('form').validate(),
                        isFormValid = $form.find('form').valid(),
                        validationData = {
                            validator: validator,
                            valid: isFormValid,
                            $form: $form,
                            $item: $item,
                            $group: $group
                        };

                    this._trigger('onGroupItemFormValidation', 0, validationData);

                    isValid = isValid && validationData.valid;

                    //set form data to itemModel
                    itemModel.Form = $form.parseForm(prefix);
                }

                this._trigger('getExtraItemData', 0, [itemModel, $item, $group]);

                groupData.Items.push(itemModel);

                itemsCount++;

            }, this));

            this._trigger('getExtraGroupData', 0, [groupData, $group]);

            parseData[groupName] = groupData;

        }, this));

        if (this.$groupForm.length) {

            var groupFormData = this.$groupForm.parseForm();

            parseData.form = groupFormData;

            var $form = this.$groupForm.find('form');

            if ($form.length) {

                $.validator.unobtrusive.parse($form);
                var groupFormValidator = $form.validate(),
                    isGroupFormValid = $form.valid();

                var groupFormValidationData = {
                    $form: $form,
                    valid: isGroupFormValid,
                    validator: groupFormValidator
                };

                this._trigger('onGroupFormValidation', 0, groupFormValidationData);

                isValid = isValid && groupFormValidationData.valid;
            }
        }

        isValid = this.valid() && isValid;

        if (isValid) {
            this._removeValidationError();
        }

        return {
            data: parseData,
            valid: isValid
        };
    };

    GroupEditor.prototype.reset = function (preventItemsRemove) {

        this._trigger('beforeReset');

        if (typeof preventItemsRemove === "undefined" || preventItemsRemove == false) {
            this.$element.find(this.options.groupItemSelector).remove();
            //Reset group items counter
            this.$counter.html(0);
        }

        var $loadedTabs = this.$tabs.find('*[data-loaded]').filter(function () {
            var loaded = $(this).data('loaded');
            return loaded == true || loaded == 'true';
        });

        $loadedTabs.each($.proxy(function (idx, tab) {

            var $tab = $(tab),
               tabModel = {
                   container: $tab,
                   loaded: true,
                   tabId: $tab.data('tabid'),
                   connectsWith: $tab.data('connectswith')
               },
               data = {
                   settings: {
                       Page: 1,
                       PageSize: 5,
                       TabId: tabModel.tabId,
                   },
                   preventShow: true
               };

            this._trigger('beforeResetTab', 0, data);

            this._ajaxGetTab({
                tabModel: tabModel,
                data: data
            });
        }, this));

        this._uncheckAllItems();

        this._rebuildNumbers();

        this._removeValidationError();

        //reset forms
        this.$element.find('form').bsResetForm();

        this._trigger('afterReset');
    };

    GroupEditor.prototype.filterGroupItems = function (groupId, modelProperty, propertyValue) {
        var $group = this._getGroup(groupId);
        $group.find(this.options.groupItemSelector).each($.proxy(function (idx, itemGroup) {
            var $item = $(itemGroup);
            var model = $item.data('model');
            if (!model.hasOwnProperty(modelProperty) || model[modelProperty] != propertyValue) {
                this._removeItemGroup($item);
            }
        }, this));
    };

    GroupEditor.prototype.setTabContent = function (html) {

        var tabModel = this._getSelectedTab();

        tabModel.html = html;

        this._initTab(tabModel);
    };

    GroupEditor.prototype.addTabItem = function ($row) {

        var tabModel = this._getSelectedTab();

        tabModel.container.find(this.options.tabItemsListSelector).prepend($row);

        $row.draggable(this._getDraggableOptions(tabModel));

        tabModel.container.find('.bs-pager').bsPager('add');

        tabModel.container.find(this.options.noResultsItemTabSelector).remove();
    };

    GroupEditor.prototype.getItemCount = function (itemId, tabId) {

        var count = this.$groups.find(this.options.groupItemSelector).filter(function () {
            return $(this).data('objid') == itemId && (typeof tabId !== "undefined" ? $(this).data('tabid') == tabId : true);
        }).length;

        return count;
    };

    /**
     * @param data json containing objId, itemModel, tabId
     * @param groupId destination group
     */
    GroupEditor.prototype.addToGroup = function (data, groupId, preventAnimation) {

        var $group = this._getGroup(groupId),
            objId = data.objId,
            tabId = data.tabId,
            model = data.itemModel,
            $tab = this._getTab(tabId),
            connectsWith = $tab.data('connectswith'),
            $tabItem = this._getTabItem(tabId, objId);

        if ($tabItem.length && typeof model === "undefined") {
            model = $tabItem.data('model');
        }

        var allowMove = !this._isInGroup(objId, tabId, $group);

        if (typeof this.options.validateMove === "function") {
            var validateResult = this.options.validateMove(model, tabId, $group);
            allowMove = typeof validateResult === "boolean" ? validateResult : allowMove;
        }

        if (allowMove) {

            var $template = this._getGroupItemTemplate($group, tabId, objId, model);

            var view = this._renderGroupItem(model, $group, tabId, objId);

            $template.find(this.options.groupItemContentSelector).html(view);

            this._checkEditableItem($template);

            this._initGroupItemForm($template.find(this.options.editorFormSelector));

            $group.find(this.options.groupItemsWrapper).append($template);

            if ($tabItem.length) {

                if (preventAnimation !== true) {
                    $tabItem.effect('transfer', {
                        to: $group.find($template)
                    });
                }

                this._checkItem($tabItem, tabId, connectsWith);
            }
        }

        this._countGroupItems();

        return allowMove;
    };
    //only supports quick search
    GroupEditor.prototype.search = function (tab, searchModel) {

        var tabModel;

        if (typeof tab === "number") {

            var $tabContainer = this.$element.find('[data-tabid=' + tab + ']');
            tabModel = {
                container: $tabContainer,
                loaded: container.data('loaded'),
                tabId: tab,
                connectsWith: $tabContainer.data('connectswith')
            };

        } else {
            tabModel = tab;
        }

        var searchData = {
            tabModel: tabModel,
            data: {
                settings: {
                    Page: 1,
                    PageSize: 5,
                    TabId: tabModel.tabId,
                    QuickSearch: tabModel.QuickSearch
                },
                preventShow: true,
                search: searchModel
            }
        };


        this._trigger('beforeTabSearch', 0, searchData);

        this._ajaxGetTab({
            tabModel: searchData.tabModel,
            data: searchData.data
        });

    };
    //#endregion

    //#region Validation
    GroupEditor.prototype.valid = function() {

        var isValid = true;

        for (var key in this.options.validation) {
            if (typeof this["_validate_" + key] === "function") {
                isValid = this["_validate_" + key].apply(this, [true, key]) && isValid;
            }
        }

        return isValid;
    };

    GroupEditor.prototype.validateUnobtrusive = function() {
        var isValid = true;

        for (var key in this.options.validation) {
            if (this.options.validation[key].unobtrusive === true && typeof this["_validate_" + key] === "function") {

                isValid = this["_validate_" + key].apply(this, [true, key]) && isValid;
            }
        }

        if (isValid) {
            this._removeValidationError();
        }

        return isValid;
    };

    GroupEditor.prototype._showValidationError = function (message) {
        var $saveBtn = this.$element.find(this.options.saveBtn);

        if (!$saveBtn.hasClass('btn-danger')) {
            this.$element.find(this.options.saveBtn).removeClass('btn-white').addClass('btn-danger');
        }

        if (typeof message !== "undefined") {
            var $error = this._createErrorContainer(message);

            var $oldError = this.$element.find(this.options.errorMessageContainer);

            if ($oldError.length) {
                $oldError.replaceWith($error);
            } else {
                this.$element.find(this.options.groupSelector).first().before($error);
            }

            $.bforms.scrollToElement($error);
        }
    };

    GroupEditor.prototype._removeValidationError = function () {
        var $saveBtn = this.$element.find(this.options.saveBtn);

        if (!$saveBtn.hasClass('btn-white')) {
            this.$element.find(this.options.saveBtn).removeClass('btn-danger').addClass('btn-white');
        }

        this.$element.find(this.options.errorMessageContainer).remove();
    };

    GroupEditor.prototype._createErrorContainer = function(message) {
        var $error = $("<div></div>").addClass("alert alert-danger").addClass(this.options.errorMessageContainer.replace('.',''));

        var $closeBtn = $("<button></button>").addClass("close").attr('data-dismiss', 'alert').prop('type', 'button').text('×');

        var $messageHolder = $("<div></div>").addClass(this.options.errorMessageHolder.replace('.', ''));

        if (typeof message !== "undefined") {
            $messageHolder.html(message);
        }

        $error.append($closeBtn).append($messageHolder);

        return $error;
    };

    GroupEditor.prototype._validate_required = function (showError, rule) {

        var itemsCount = this.$element.find(this.options.groupItemSelector).length;

        var isValid = itemsCount > 0;

        if(!isValid && showError === true) {
            if (typeof this["_showError_" + rule] === "function") {
                this["_showError_" + rule].apply(this, [this.options.validation[rule].message]);
            } else {
                this._showValidationError(this.options.validation[rule].message);
            }
        }

        return isValid;
    };

    GroupEditor.prototype.addValidationRule = function(rule, validationFunc, errorFunc) {

        if (typeof validationFunc === "function") {
            this["_validate_" + rule] = validationFunc;
        }

        if (typeof errorFunc === "function") {
            this["_showError_" + rule] = errorFunc;
        }

    };
    //#endregion

    $.widget('bforms.bsGroupEditor', GroupEditor.prototype);

    return GroupEditor;
});