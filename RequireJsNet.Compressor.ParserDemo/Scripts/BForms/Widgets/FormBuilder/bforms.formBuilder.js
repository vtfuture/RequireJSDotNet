var factory = function ($, FormRenderer, models) {

    var FormBuilder = function (options) {

        $.extend(true, this.options, options);

        this._init();
    };

    FormBuilder.prototype.options = {
        controlsTabSelector: '.form_builder-tab.form_builder-controls',
        formSelector: 'form.form_builder-form',
        formContainerSelector: '.form_container',
        controlItemSelector: '.form_builder-controlItem',
        controlItemsListSelector: '.form_builder-controlsList',
        controlItemAddSelector: '.form_builder-controlItem-add',
        controlItemLabelSelector: '.form_builder-controlItem-label',
        formTabSelector: '.form_builder-tab.form_builder-form',
        controlTabSelector: '.form_builder-tabBtn',
        controlTabsContainerSelector: '.form_builder-tabControl',
        controlsTabWrapperSelector: '.form_builder-controlsWrapper',
        formControlSelector: '.form_builder-formControl',
        formControlAddonSelector: '[data-addon-toggle]',
        propertiesTabSelector: '.form_builder-tab.form_builder-properties',
        settingsContainerSelector: '.form_builder-settingsContainer',
        inputGroupSelector: '.input-group'
    };

    FormBuilder.prototype._addons = {
        grab: {
            name: 'grab',
            glyphicon: 'glyphicon-sort',
            title: 'Sort'
        },
        up: {
            name: 'up',
            glyphicon: 'glyphicon-chevron-up',
            title: 'Move up'
        },
        down: {
            name: 'down',
            glyphicon: 'glyphicon-chevron-down',
            title: 'Move down'
        },
        settings: {
            name: 'settings',
            glyphicon: 'glyphicon-wrench',
            title: 'Settings'
        },
        remove: {
            name: 'remove',
            glyphicon: 'glyphicon-remove-circle',
            title: 'Remove'
        }
    };

    FormBuilder.prototype._controlActions = {
        all: 'all',
        grab: 'grab',
        up: 'up',
        down: 'down',
        settings: 'settings',
        remove: 'remove'
    };

    // #region init

    FormBuilder.prototype._init = function () {

        this._cacheElements();

        this._initMembers();
        this._addDelegates();
        this._initControls();
        this._initForm();
        this._initProperties();
        this._applyDisplayFixes();
    };

    FormBuilder.prototype._cacheElements = function () {

        this.$element = this.element;
        this.$controlsTab = this.$element.find(this.options.controlsTabSelector);
        this.$controlsWrapper = this.$controlsTab.find(this.options.controlsTabWrapperSelector);
        this.$controlItemsList = this.$controlsTab.find(this.options.controlItemsListSelector);
        this.$tabsContainer = this.$controlsTab.find(this.options.controlTabsContainerSelector);
        this.$formTab = this.$element.find(this.options.formTabSelector);
        this.$propertiesTab = this.$element.find(this.options.propertiesTabSelector);
        this.$settingsContainer = this.$propertiesTab.find(this.options.settingsContainerSelector);
        this.$form = this.$element.find(this.options.formSelector);
    };

    FormBuilder.prototype._initMembers = function () {

        this._defaultActions = this._getDefaultActions();
        this._customActions = this._getCustomActions();

        this.renderer = new FormRenderer({
            defaultAddons: this._defaultActions.map($.proxy(this._mapControlActionToControlAddon, this))
        });

        this.formModel = [];
        this.resource = this.options.resource;
        this.draggedControl = null;

        var controlsData = this.$element.attr('data-controls') ? this.$element.attr('data-controls') : '{}';

        var controlsOptions = JSON.parse(controlsData);

        this.availableControls = controlsOptions ? (controlsOptions.controls || []) : [];

        this._settingsTabs = {};

        this._saveModel = [];
    };

    FormBuilder.prototype._initControls = function () {

        this._renderInitialControls(this.availableControls);

        this._initControlsTabs();
        this._initControlsDrag();
        this._initTabsReceive();
    };

    FormBuilder.prototype._initForm = function () {

        // this._initSpoofModel();

        this._assignUids(this.formModel);
        this._renderForm(this.formModel);
        this._initFormControlsSort();
        this._initFormControlsResize();
        this._initFormControlActions();
    };

    FormBuilder.prototype._initSpoofModel = function () {

        this.formModel.push({
            type: models.controlTypes.textBox,
            label: 'First name',
            name: 'FirstName',
            glyphicon: 'user',
            size: 12,
            placeholder: 'Type your first name here',
            required: true
        });

        this.formModel.push({
            type: models.controlTypes.textBox,
            label: 'Last name',
            name: 'LastName',
            glyphicon: 'user',
            size: 12,
            placeholder: 'Type your last name here',
            required: true
        });

        this.formModel.push({
            type: models.controlTypes.singleSelect,
            label: 'Location',
            name: 'Location',
            glyphicon: 'map-marker',
            size: 12,
            items: [
                {
                    text: 'Bucharest',
                    value: 1
                },
                {
                    text: 'Militari',
                    value: 2
                }
            ],
            widget: 'select2',
            widgetOptions: {
                query: function () {

                }
            },
            required: false
        });

        this.formModel.push({
            type: models.controlTypes.radioButtonList,
            label: 'Role',
            name: 'Role',
            glyphicon: 'star',
            size: 12,
            selectedValue: 2,
            items: [
                {
                    value: 1,
                    label: 'Lead'
                },
                {
                    value: 2,
                    label: 'Dev'
                },
                {
                    value: 3,
                    label: 'Tester'
                }
            ],
            required: true
        });

        this.formModel.push({
            type: models.controlTypes.textArea,
            label: 'Description',
            name: 'Description',
            glyphicon: 'font',
            size: 12,            
            required: false 
        });

        this.formModel.push({
            type: models.controlTypes.tagList,
            label: 'Languages',
            name: 'Languages',
            glyphicon: 'tags',
            size: 12,
            items: [
                {
                    value: 'C#',
                    text: 'C#'
                },
                {
                    value: 'JavaScript',
                    text: 'JavaScript'
                },
                {
                    value: 'Perl',
                    text: 'Perl'
                }
            ],       
            required: false 
        });
    };

    FormBuilder.prototype._initProperties = function () {

        var $properties = this.renderer.renderControlProperties(models.controlTypes.textBox);

        // this.$propertiesTab.append($properties);
    };

    FormBuilder.prototype._applyDisplayFixes = function () {

        var items = [this.$controlsTab, this.$formTab, this.$propertiesTab];

        var largestElementIndex,
            maxHeight = 0;

        for (var i in items) {

            var $item = items[i],
                height = this._getHeight($item);

            if (height > maxHeight) {
                maxHeight = height;
                largestElementIndex = i;
            }
        }

        for (var i in items) {

            var $item = items[i];

            if (i !== largestElementIndex) {
                $item.css('border-left', 'none');
                $item.css('border-right', 'none');
            }
        }

        //this.$form.css('min-height', maxHeight);
        this.$form.find('.form_container').css('min-height', '200px');

        //this.$propertiesTab.css('min-height', maxHeight);
    };

    FormBuilder.prototype._initControlsDrag = function () {

        var $controlItemsList = this.$controlsTab.find(this.options.controlItemsListSelector),
            $controlItems = $controlItemsList.find(this.options.controlItemSelector);

        $controlItems.css('background-color', 'white');

        $controlItemsList.sortable({
            cursor: 'move',
            zIndex: 100,
            opacity: '0.85',
            connectWith: this.$form.find(this.options.formContainerSelector),
            drag: $.proxy(this._evControlItemsSortDrag, this),
            helper: $.proxy(this._evControlItemsSortHelper, this),
            update: $.proxy(this._evControlItemsSortUpdate, this)
        });
    };

    FormBuilder.prototype._initControlsTabs = function () {

        this.$controlsTab.on('click', this.options.controlTabSelector, $.proxy(this._evControlTabClick, this));

        this._updateTabsPositions();
    };

    FormBuilder.prototype._initTabsReceive = function () {

        var $tabsContainer = this.$controlsTab.find(this.options.controlTabsContainerSelector),
            $tabs = $tabsContainer.find(this.options.controlTabSelector);

        $tabsContainer.sortable({
            items: this.options.controlTabSelector,
            containment: 'document',
            start: $.proxy(this._evTabsSortStart, this),
            stop: $.proxy(this._evTabsSortStop, this)
        });

        $tabs.droppable({
            accept: this.options.controlItemSelector,
            tolerance: 'pointer',
            drop: $.proxy(this._evTabReceive, this),
            over: $.proxy(this._evTabOver, this),
            out: $.proxy(this._evTabOut, this)
        });
    };

    FormBuilder.prototype._initFormControlsSort = function () {

        this.$form.find(this.options.formContainerSelector).sortable({
            items: this.options.formControlSelector,
            handle: '[data-addon-toggle="grab"]',
            cursor: 'move',
            opacity: '0.85',
            tolerance: 'pointer',
            receive: $.proxy(this._evFormReceive, this)
        });
    };

    FormBuilder.prototype._initFormControlsResize = function () {

        var $controlItems = this.$form.find(this.options.formControlSelector),
            $resizeHandles = this.$form.find(this.options.formControlAddonSelector + '[data-addon-toggle="resize"]');

        $resizeHandles.addClass('ui-resizable-e ui-resizable-handle');
        $resizeHandles.css('cursor', 'ew-resize');

        var context = this;

        $controlItems.each(function (idx, el) {

            var $item = $(el),
                $handle = $item.find(context.options.formControlAddonSelector + '[data-addon-toggle="resize"]');

            var width = parseInt($item.css('width').replace('px', ''));

            if ($handle.length != 0) {
                $item.resizable({
                    handles: {
                        e: $handle
                    },
                    grid: [width / 2, width / 2]

                });
            }
        });
    };

    FormBuilder.prototype._initFormControlActions = function () {

        this.$form.on('click', this.options.formControlAddonSelector, $.proxy(this._evFormControlActionClick, this));
    };

    FormBuilder.prototype._renderInitialControls = function (initialControls) {

        for (var i in initialControls) {

            var model = this._mapControlModelToControlListModel(initialControls[i]);

            var $controlItem = this.renderer.renderControlListItem(model),
                $controlItemsList = this._getTab(model.tabId);

            $controlItemsList.append($controlItem);
        }

        var $controlItemsLists = this.$element.find(this.options.controlItemsListSelector);

        var context = this;

        $controlItemsLists.each(function (idx, el) {

            var $items = $(el).find(context.options.controlItemSelector);

            $items.each(function (i, e) {
                $(e).attr('data-position', i + 1);
            });
        });
    };

    FormBuilder.prototype._renderForm = function (controls) {

        var $formContainer = this.$form.find(this.options.formContainerSelector);

        if (controls) {

            $formContainer.empty();

            for (var i in controls) {
                this._appendControl(controls[i], { noAnimate: true });
            }
        }
    };

    FormBuilder.prototype._renderControl = function (model) {

        var addons = this._getAddons(model);

        $.extend(true, model, { addons: addons });

        switch (model.type) {

            case models.controlTypes.textBox: {
                return this.renderer.renderTextBox(model);
            }
            case models.controlTypes.textArea: {
                return this.renderer.renderTextArea(model);
            }
            case models.controlTypes.singleSelect: {
                return this.renderer.renderDropdown(model);
            }
            case models.controlTypes.radioButtonList: {
                return this.renderer.renderRadioButtonList(model);
            }
            case models.controlTypes.tagList: {
                return this.renderer.renderTagList(model);
            }
            case models.controlTypes.title: {
                return this.renderer.renderTitle(model);
            }
            case models.controlTypes.listBox: {
                return this.renderer.renderListBox(model);
            }
            case models.controlTypes.datePicker: {
                return this.renderer.renderDatePicker(model);
            }
            case models.controlTypes.datePickerRange: {
                return this.renderer.renderDatePickerRange(model);
            }
            case models.controlTypes.checkBox: {
                return this.renderer.renderCheckBox(model);
            }
            case models.controlTypes.checkBoxList: {
                return this.renderer.renderCheckBoxList(model);
            }
            case models.controlTypes.numberPicker: {
                return this.renderer.renderNumberPicker(model);
            }
            case models.controlTypes.numberPickerRange: {
                return this.renderer.renderNumberPickerRange(model);
            }
            case models.controlTypes.pagebreak: {
                return this.renderer.renderPageBreak(model);
            }
            case models.controlTypes.customControl: {
                return this.renderer.renderCustomControl(model.controlName, model);
            }
        }

        return null;
    };

    FormBuilder.prototype._addDelegates = function () {

        this.$controlsTab.on('click', this.options.controlItemAddSelector, $.proxy(this._evControlItemAddClick, this));
    };

    // #endregion

    // #region event handlers

    FormBuilder.prototype._evControlItemAddClick = function (e) {

        e.preventDefault();

        var $target = $(e.target).is('a') ? $(e.target) : $(e.target).parents('a:first'),
            $controlItem = $target.parents(this.options.controlItemSelector),
            $lastFormControl = this.$form.find(this.options.formControlSelector + ':last'),
            controlType = $controlItem.attr('data-controlType'),
            displayName = $controlItem.attr('data-name'),
            glyphicon = $controlItem.attr('data-glyphicon');

        var newControl = this._createNewControl(controlType, displayName, glyphicon);

        this._appendControl(newControl, { after: $lastFormControl });
    };

    FormBuilder.prototype._evControlItemsSortUpdate = function (e, ui) {

        var $list = ui.item.parents(this.options.controlItemsListSelector),
            $items = $list.find(this.options.controlItemSelector);

        $items.each(function (idx, el) {
            $(el).attr('data-position', idx + 1);
        });
    };

    FormBuilder.prototype._evControlItemsSortDrag = function (e, ui) {

        var $controlItem = ui.item;

        this.draggedControlType = $controlItem.attr('data-controltype');
    };

    FormBuilder.prototype._evControlItemsSortHelper = function (e, $item) {

        return $item.get(0);
    };

    FormBuilder.prototype._evFormReceive = function (e, ui) {

        var $controlItem = ui.item,
            type = parseInt($controlItem.attr('data-controltype')),
            name = $controlItem.attr('data-name'),
            position = $controlItem.attr('data-position'),
            glyphicon = $controlItem.attr('data-glyphicon'),
            displayName = $controlItem.attr('data-name'),
            tabId = $controlItem.attr('data-tabid'),
            $newControlItem = this.renderer.renderControlListItem({
                type: type,
                name: name,
                glyphicon: glyphicon,
                order: position,
                tabId: tabId
            });

        var newControl = this._createNewControl(type, displayName, glyphicon);

        this._appendControlItem($newControlItem, {
            position: position,
            tabId: tabId
        });

        this._appendControl(newControl, { replace: ui.item });
    };

    FormBuilder.prototype._evFormControlActionClick = function (e) {

        e.preventDefault();

        var $action = $(e.target),
            $formControl = $action.parents(this.options.formControlSelector),
            actionName = $action.attr('data-addon-toggle'),
            controlModel = this._getFormControlModel($formControl);

        this._handleAction(actionName, controlModel, {
            hide: $action.hasClass('selected')
        });
    };

    FormBuilder.prototype._evControlTabClick = function (e) {

        e.preventDefault();

        var $btn = $(e.target).is(this.options.controlTabSelector) ? $(e.target) : $(e.target).parents(this.options.controlTabSelector),
            alreadySelected = $btn.hasClass('selected');

        if (alreadySelected) {
            return;
        }

        var $btnGroup = $btn.parent(),
            tabId = $btn.attr('data-tabid');

        var $lists = this.$element.find(this.options.controlItemsListSelector),
            $listToShow = $lists.filter('[data-tabid="' + tabId + '"]');

        $lists.hide();

        var selectedTabPosition = $btnGroup.find('.selected').attr('data-position'),
            clickedTabPosition = $btn.attr('data-position');

        var slideFromRight = parseInt(selectedTabPosition) <= parseInt(clickedTabPosition);

        $btnGroup.find(this.options.controlTabSelector).removeClass('selected');
        $btn.addClass('selected');

        var direction = slideFromRight ? models.directions.right : models.directions.left;

        this._slideTab($listToShow, direction);
    };

    FormBuilder.prototype._evTabReceive = function (e, ui) {

        var $controlItem = ui.draggable,
            $tab = $(e.target),
            tabId = $tab.attr('data-tabid');

        $tab.removeClass('over');

        var type = $controlItem.attr('data-controltype'),
            name = $controlItem.attr('data-name'),
            glyphicon = $controlItem.attr('data-glyphicon');

        var $newTab = this._getTab(tabId),
            $newTabItems = $newTab.find(this.options.controlItemSelector);

        var position = $newTabItems.length + 1;

        $controlItem.remove();

        var appendOptions = {
            tabId: tabId
        };

        if (tabId == $controlItem.attr('data-tabid')) {

            position = parseInt($controlItem.attr('data-position'));

            $.extend(true, appendOptions, {
                position: position
            });
        }

        var $newControlItem = this.renderer.renderControlListItem({
            type: type,
            name: name,
            glyphicon: glyphicon,
            order: position,
            tabId: tabId
        });

        this._appendControlItem($newControlItem, appendOptions);
    };

    FormBuilder.prototype._evTabOver = function (e, ui) {

        var $helper = ui.helper,
            $tab = $(e.target);

        $tab.addClass('over');
        $helper.css('opacity', 0.2);

        this.$draggedHelper = $helper;
    };

    FormBuilder.prototype._evTabOut = function (e, ui) {

        var $tab = $(e.target);

        $tab.removeClass('over');

        if (this.$draggedHelper) {
            this.$draggedHelper.css('opacity', 1);
        }
    };

    FormBuilder.prototype._evTabsSortStart = function (e, ui) {

        var $visibleTab = this.$controlsTab.find(this.options.controlItemSelector + ':visible');

        $visibleTab.addClass('loading');
    };

    FormBuilder.prototype._evTabsSortStop = function (e, ui) {

        var $visibleTab = this.$controlsTab.find(this.options.controlItemSelector + ':visible');

        $visibleTab.removeClass('loading');
    };

    FormBuilder.prototype._evTabsSortUpdate = function (e, ui) {

        this._updateTabsPositions();
    };

    // #endregion

    // #region private methods

    FormBuilder.prototype._appendControl = function (control, options) {

        if (options && options.replace) {
            options.replace.empty();
        }

        var controlHtml = '',
            caughtException;

        try {
            controlHtml = this._renderControl(control);
        } catch (e) {
            caughtException = e;
        }

        var $formContainer = this.$form.find(this.options.formContainerSelector),
            $control = $(controlHtml),
            $after = options ? options.after : null,
            $toReplace = options ? options.replace : null;

        $control.attr('data-controlType', control.type);
        $control.attr('data-uid', this._generateUid());
        $control.attr('data-displayName', control.label);

        $control.hide();

        if (!options) {
            $formContainer.append($control);
        } else {

            if ($after && $after.length !== 0) {
                $after.after($control);
            } else {

                if ($toReplace && $toReplace.length !== 0) {
                    $toReplace.replaceWith($control);
                } else {
                    $formContainer.append($control);
                }
            }
        }

        if (caughtException) {
            throw caughtException;
        }

        var showOptions = options.noAnimate ? null : {
            easing: 'swing',
            duration: 200
        };

        $control.show(showOptions);

        this.applyWidget({
            control: control,
            $element: $control
        });

        this.formModel.push(control);
    };

    FormBuilder.prototype.applyWidget = function (controlModel) {

        var $control = controlModel.$element,
            control = controlModel.control;

        if (typeof control.applyWidget == 'function') {
            control.applyWidget($control);
        } else {
            if (!control.noInitUI) {
                $control.bsInitUI();
            }
        }
    };

    FormBuilder.prototype._appendControlItem = function ($item, options) {

        var position = options.position,
            tabId = options.tabId;

        position = parseInt(position);

        var $controlItemsList = this._getTab(tabId),
            $items = $controlItemsList.find(this.options.controlItemSelector);

        if (isNaN(position) || $items.length == 0 || $items.length + 1 === position) {
            $controlItemsList.append($item);
        } else {

            $items.each(function (idx, el) {
                if (idx + 1 === position) {
                    $(el).before($item);
                }
            });
        }
    };

    FormBuilder.prototype._createNewControl = function (type, displayName, glyphicon) {

        type = parseInt(type);

        var name = this._getControlTypeName(type),
            defaultModel = {
                type: type,
                name: 'New.' + name,
                label: 'New ' + displayName,
                glyphicon: this.renderer._getGlyphiconName(glyphicon)
            },
            model = {};

        var defaultListItems = [];

        var defaultRadioListItems = [];

        var defaultDatePickerModel = {
            textValueName: 'New.' + name + '.TextValue',
            textValueId: this.renderer._generateIdFromName('New.' + name + '.TextValue'),
            dateValueName: 'New.' + name + '.DateValue',
            dateValueId: this.renderer._generateIdFromName('New.' + name + '.DateValue')
        };

        var defaultNumberPickerModel = {
            textValueName: 'New.' + name + '.TextValue',
            textValueId: this.renderer._generateIdFromName('New.' + name + '.TextValue'),
            dateValueName: 'New.' + name + '.ItemValue',
            dateValueId: this.renderer._generateIdFromName('New.' + name + '.ItemValue')
        };

        switch (type) {
            case models.controlTypes.title:
                {
                    model = {
                        text: 'Title',
                        glyphicon: 'glyphicon-header'
                    };

                    break;
                }
            case models.controlTypes.singleSelect:
                {
                    model = {
                        items: defaultListItems
                    };

                    break;
                }
            case models.controlTypes.listBox:
                {
                    model = {
                        items: defaultListItems
                    };

                    break;
                }
            case models.controlTypes.tagList:
                {
                    model = {
                        items: defaultListItems
                    };

                    break;
                }
            case models.controlTypes.radioButtonList:
                {
                    model = {
                        items: defaultRadioListItems
                    };

                    break;
                }
            case models.controlTypes.datePicker:
                {
                    model = defaultDatePickerModel;

                    break;
                }
            case models.controlTypes.datePickerRange:
                {
                    var fromName = 'New.' + name + '.From.TextValue',
                        toName = 'New.' + name + '.To.TextValue';

                    var rangePickerModel = {
                        fromName: fromName,
                        fromId: this.renderer._generateIdFromName(fromName),
                        toName: toName,
                        toId: this.renderer._generateIdFromName(toName)
                    };

                    $.extend(true, model, defaultDatePickerModel, rangePickerModel);

                    break;
                }
            case models.controlTypes.checkBox:
                {
                    model = {
                        items: defaultRadioListItems
                    };

                    break;
                }
            case models.controlTypes.checkBoxList:
                {
                    model = {
                        items: defaultRadioListItems
                    };

                    break;
                }
            case models.controlTypes.numberPicker:
                {
                    model = defaultNumberPickerModel;

                    break;
                }
            case models.controlTypes.numberPickerRange:
                {
                    model = $.extend(true, defaultNumberPickerModel, {
                        textValueName: 'New.' + name + '.TextValue',
                        textValueId: this.renderer._generateIdFromName('New.' + name + '.TextValue'),
                        fromName: 'New.' + name + '.From.ItemValue',
                        fromId: this.renderer._generateIdFromName('New.' + name + '.From.ItemValue'),
                        toName: 'New.' + name + '.To.ItemValue',
                        toId: this.renderer._generateIdFromName('New.' + name + '.To.ItemValue'),
                        fromValue: 1,
                        fromTextValue: '1',
                        fromDisplay: 'From',
                        toValue: 10,
                        toTextValue: '10',
                        toDisplay: 'To'
                    });

                    break;
                }
            default:
                {
                    break;
                }
        }

        return $.extend(true, defaultModel, model);
    };

    FormBuilder.prototype._updateTabsPositions = function () {

        var $tabs = this.$controlsTab.find(this.options.controlTabSelector);

        $tabs.each(function (idx, el) {
            $(el).attr('data-position', idx + 1);
        });
    };

    FormBuilder.prototype._assignUids = function (items) {

        for (var i in items) {

            var item = items[i];

            if (typeof item.uid == 'undefined') {
                item.uid = this._generateUid();
            }
        }
    };

    FormBuilder.prototype._getDefaultActions = function () {

        var defaultActionsOptions = this.$element.attr('data-defaultactions') ? this.$element.attr('data-defaultactions') : '{}';

        var actions = JSON.parse(defaultActionsOptions).actions || [];

        actions = this._getAvailableActions(actions);

        return actions;
    };

    FormBuilder.prototype._getCustomActions = function () {

        var customActionsOptions = this.$element.attr('data-customactions') ? this.$element.attr('data-customactions') : '{}';

        var customActions = JSON.parse(customActionsOptions).actions || [];

        customActions = customActions.map($.proxy(this._mapCustomActionModelToCustomAction, this));

        return customActions;
    };

    FormBuilder.prototype._getAvailableActions = function (actionsToCheck) {

        var actions = actionsToCheck.slice();

        if (actions.indexOf(this._controlActions.all) != -1) {

            actions = [];

            for (var actionName in this._controlActions) {
                if (this._controlActions.hasOwnProperty(actionName) && actionName != 'all') {
                    actions.push(actionName);
                }
            }
        }

        return actions;
    };

    FormBuilder.prototype._getAddons = function (model) {

        for (var i in this.availableControls) {

            var control = this._mapControlModelToControlListModel(this.availableControls[i]);

            if (control.type === model.type) {

                var actions = this._getAvailableActions(control.actions),
                    addons = actions.map($.proxy(this._mapControlActionToControlAddon, this));

                addons = addons.concat(control.customActions);

                return addons;
            }
        }

        return null;
    };

    FormBuilder.prototype._handleAction = function (actionName, controlModel, actionParams) {

        if (this._controlActions[actionName]) {
            this._handleDefaultAction(actionName, controlModel, actionParams);
        } else {
            this._handleCustomAction(actionName, controlModel, actionParams);
        }
    };

    FormBuilder.prototype._handleDefaultAction = function (actionName, controlModel, actionParams) {

        switch (actionName) {

            case this._controlActions.grab:
                {
                    break;
                }
            case this._controlActions.up:
                {
                    this._moveFormElement(models.directions.up, controlModel);

                    break;
                }
            case this._controlActions.down:
                {
                    this._moveFormElement(models.directions.down, controlModel);

                    break;
                }
            case this._controlActions.settings:
                {
                    this._toggleSettings(controlModel, actionParams.hide, true);

                    break;
                }
            case this._controlActions.remove:
                {
                    this._removeFormElement(controlModel);

                    break;
                }
            default:
                {
                    break;
                }
        }
    };

    FormBuilder.prototype._moveFormElement = function (direction, controlModel) {

        var $formControl = this._getFormControl(controlModel.uid);

        var $formControls = this.$form.find(this.options.formControlSelector);

        var previousIndex, nextIndex, currentIndex;

        $formControls.each($.proxy(function (idx, el) {

            var $formControl = $(el);

            if ($formControl.attr('data-uid') == controlModel.uid) {
                currentIndex = idx;
            }

        }, this));

        currentIndex = currentIndex + 1;
        previousIndex = currentIndex - 1;
        nextIndex = currentIndex + 1;

        var $previous = this.$form.find(this.options.formControlSelector + ':nth-child(' + previousIndex + ')'),
            $next = this.$form.find(this.options.formControlSelector + ':nth-child(' + nextIndex + ')');



        if (direction === models.directions.up) {
            //  $previous.before($formControl);

            this._swapElements($formControl, $previous, direction);
        } else {
            // $next.after($formControl);

            this._swapElements($formControl, $next, direction);
        }
    };

    FormBuilder.prototype._toggleSettings = function (controlModel, hide, abortIfOpen) {

        if (controlModel.uid == null) {
            return;
        }

        var $formControl = this._getFormControl(controlModel.uid),
            $settingsBtn = this._getFormControlAddon(controlModel.uid, this._controlActions.settings);

        if ($settingsBtn.hasClass('selected') && abortIfOpen) {
            return;
        }

        if (hide) {

            $formControl.removeClass('selected');
            $settingsBtn.removeClass('selected');
        }

        if (this._openedSettingsUid === controlModel.uid) {
            return;
        }

        this._toggleSettings({ uid: this._openedSettingsUid }, true);

        this._openedSettingsUid = controlModel.uid;

        $formControl.addClass('selected');
        $settingsBtn.addClass('selected');

        var settingsInitialized = true;

        var $formSettings = this._settingsTabs['' + controlModel.uid];

        if (!$formSettings) {

            settingsInitialized = false;

            $formSettings = this._getFormSettingsTab(controlModel.type).clone();

            $formSettings.hide();

            $formSettings.attr('data-uid', controlModel.uid);

            this.$settingsContainer.append($formSettings);

            this._settingsTabs['' + controlModel.uid] = $formSettings;
        }

        var $otherPropertiesTabs = this.$settingsContainer.find('[data-properties-for][data-uid]:not([data-uid="' + controlModel.uid + '"])').hide();

        if (hide) {

            $formSettings.slideUp('fast');

        } else {

            $formSettings.slideDown('fast');

            if (!settingsInitialized) {

                $formSettings.find('form').each($.proxy(this._initPropertiesForm, this, controlModel));
            }
        }
    };

    FormBuilder.prototype._initPropertiesForm = function(controlModel, idx, el){

        var $form = $(el);

        var baseUniqueName = 'settings|' + controlModel.uid;

        var formOptions = {
            validate: true,
            parse: true,
            actions: [
                {
                    selector: '.btn-theme',
                    handler: $.proxy(this._handlePropertiesSave, this, $form, controlModel),
                    parse: true,
                    validate: true
                },
                {
                    selector: '.btn-warning',
                    handler: function (e, data) {
                        $formSettings.find('form').bsForm('reset');
                    },
                    parse: true,
                    validate: true
                }
            ]
        };

        var uniqueName = baseUniqueName + '|' + idx;

        $form.bsForm($.extend(true, formOptions, {
            uniqueName: uniqueName
        }));
    };

    FormBuilder.prototype._handlePropertiesSave = function ($form, controlModel, parsedForm) {

        var controlType = controlModel.type,
            uid = controlModel.uid;

        var savedPropertyName = $form.attr('data-property-name');

        var eventModel = {
            controlModel: controlModel,
            propertyName: savedPropertyName,
            $form: $form,
            parsedForm: parsedForm
        };

        switch (controlType) {
            case models.controlTypes.textBox: {

                this._handleTextBoxPropertiesSave(uid, eventModel);

                break;
            }
            case models.controlTypes.textArea: {

                this._handleTextAreaPropertiesSave(uid, eventModel);

                break;
            }
            case models.controlTypes.singleSelect: {

                this._handleSingleSelectPropertiesSave(uid, eventModel);

                break;
            }
            case models.controlTypes.tagList: {

                this._handleTagListPropertiesSave(uid, eventModel);

                break;
            }
            case models.controlTypes.listBox: {

                this._handleListBoxPropertiesSave(uid, eventModel);

                break;
            }
            case models.controlTypes.radioButtonList: {

                this._handleRadioButtonListPropertiesSave(uid, eventModel);

                break;
            }
        }

        var model = this._getFormModel(controlModel.uid);

        if (model == null) {
            model = {
                uid: controlModel.uid,
                type: controlModel.type,
                name: controlModel.name
            };
        }

        model[savedPropertyName] = $.extend(true, {}, parsedForm);

        this._updateFormModel(model);
    };

    FormBuilder.prototype._handleDefaultPropertiesChange = function ($element, $formGroup, properties) {

        var $label = $formGroup.find('label'),
            $glyphiconAddon = $formGroup.find('.input-group-addon:first');

        $element.attr('name', properties.Name);
        $element.attr('name', properties.Name);

        $label.text(properties.Label);

        if (properties['Required.SelectedValues'] === '' + models.yesNoValues.yes) {
            $label.addClass('required');
        }

        var glyphicon = properties['GlyphiconAddon.SelectedValues'];

        if (glyphicon) {
            $glyphiconAddon.attr('class', 'input-group-addon glyphicon ' + glyphicon);
        } else {
            $glyphiconAddon.attr('class', 'input-group-addon glyphicon');
        }
    };

    FormBuilder.prototype._handleTextBoxPropertiesSave = function (uid, data) {

        var $formControl = this._getFormControl(uid);

        var $inputGroup = $formControl.find(this.options.inputGroupSelector);

        var parsedForm = data.parsedForm,
            propertyName = data.propertyName;

        var $input = $inputGroup.find('input');

        if (propertyName === models.propertyNames.defaultProperties) {

            this._handleDefaultPropertiesChange($input, $formControl, parsedForm);

        } else {

            $input.attr('placeholder', parsedForm.Placeholder);
            $input.attr('value', parsedForm.InitialValue);
            $input.attr('type', models.inputTypes['' + parsedForm['Type.SelectedValues']]);
        }

    };

    FormBuilder.prototype._handleTextAreaPropertiesSave = function (uid, data) {

        var $formControl = this._getFormControl(uid);

        var $inputGroup = $formControl.find(this.options.inputGroupSelector);

        var parsedForm = data.parsedForm,
            propertyName = data.propertyName;

        var $textArea = $inputGroup.find('textarea');

        if (propertyName === models.propertyNames.defaultProperties) {

            this._handleDefaultPropertiesChange($textArea, $formControl, parsedForm);

        } else {

        }

    };

    FormBuilder.prototype._handleSingleSelectPropertiesSave = function (uid, data) {

        var $formControl = this._getFormControl(uid);

        var $inputGroup = $formControl.find(this.options.inputGroupSelector);

        var parsedForm = data.parsedForm,
            propertyName = data.propertyName;

        var $select = $inputGroup.find('select');

        if (propertyName === models.propertyNames.defaultProperties) {

            this._handleDefaultPropertiesChange($select, $formControl, parsedForm);

        } else {

            var items = this._getSelectItemsFromSavedProperties(parsedForm, 'Items.SelectedValues');

            var options = this._getOptionsFromSelectItems(items);

            $select.html(options);
            $select.select2('val', '');
        }

    };

    FormBuilder.prototype._handleTagListPropertiesSave = function (uid, data) {

        var $formControl = this._getFormControl(uid);

        var $inputGroup = $formControl.find(this.options.inputGroupSelector);

        var parsedForm = data.parsedForm,
            propertyName = data.propertyName;

        var $select = $inputGroup.find('.bs-tag-list');

        if (propertyName === models.propertyNames.defaultProperties) {

            this._handleDefaultPropertiesChange($select, $formControl, parsedForm);

        } else {

            return;

            var items = this._getSelectItemsFromSavedProperties(parsedForm, 'Items.SelectedValues');

            var options = this._getOptionsFromSelectItems(items);

            var data = items.map(function (item) {
                return {
                    id: item,
                    text: item
                };
            });

            $select.filter('select').html(options);
            $select.select2('val', '');
        }

    };

    FormBuilder.prototype._handleListBoxPropertiesSave = function (uid, data) {

        var $formControl = this._getFormControl(uid);

        var $inputGroup = $formControl.find(this.options.inputGroupSelector);

        var parsedForm = data.parsedForm,
            propertyName = data.propertyName;

        var $select = $inputGroup.find('select');

        if (propertyName === models.propertyNames.defaultProperties) {

            this._handleDefaultPropertiesChange($select, $formControl, parsedForm);

        } else {

            var items = this._getSelectItemsFromSavedProperties(parsedForm, 'Items.SelectedValues');

            var options = this._getOptionsFromSelectItems(items);

            $select.html(options);
            $select.select2('val', '');
        }

    };

    FormBuilder.prototype._handleRadioButtonListPropertiesSave = function (uid, data) {

        var $formControl = this._getFormControl(uid);

        var $inputGroup = $formControl.find(this.options.inputGroupSelector);

        var parsedForm = data.parsedForm,
            propertyName = data.propertyName;

        var $buttons = $inputGroup.find('.btn-group-justified');

        if (propertyName === models.propertyNames.defaultProperties) {

            this._handleDefaultPropertiesChange($buttons, $formControl, parsedForm);

        } else {

            var items = this._getSelectItemsFromSavedProperties(parsedForm, 'Items.SelectedValues');

            var buttonsHtml = '';

            for (var i in items) {

                var buttonHtml = '<a class="option" value="' + items[i] + '">' + items[i] + '</a>';

                buttonsHtml += buttonHtml;
            }

            $buttons.html(buttonsHtml);
        }

    };

    FormBuilder.prototype._removeFormElement = function (controlModel) {

        var uid = controlModel.uid;

        var formModelIndex = null;

        for (var i in this.formModel) {

            if (this.formModel[i].uid === uid) {

                formModelIndex = i;

                break;
            }
        }

        if (formModelIndex !== null) {
            this.formModel.splice(formModelIndex, 1);
        }

        var $formControl = this._getFormControl(uid);

        this._openedSettingsUid = null;

        this._toggleSettings({ uid: uid }, true);

        $formControl.hide({
            duration: 200,
            easing: 'swing',
            done: function () {
                $formControl.remove();
            }
        });
    };

    FormBuilder.prototype._handleCustomAction = function (actionName, controlModel) {

        var actionHandler;

        if (this.options.customActions instanceof Array) {

            for (var i in this.options.customActions) {

                var customAction = this.options.customActions[i];

                if (customAction.name === actionName) {

                    actionHandler = customAction.handler;
                }
            }
        }

        if (typeof actionHandler == 'function') {
            actionHandler(controlModel);
        } else {
            throw 'Handler not defined for custom action "' + actionName + '"';
        }
    };

    // #endregion

    // #region public methods

    FormBuilder.prototype.serialize = function () {

        var model = this.formModel.map(function (item) {

            return $.extend(true, item, {
                Type: item.type,
                Name: item.DefaultProperties ? item.DefaultProperties.Name : null
            });
        });

        return model;
    };

    // #endregion

    // #region helpers

    FormBuilder.prototype._mapControlModelToControlListModel = function (controlModel) {

        var model = {
            type: controlModel.Type,
            name: controlModel.Text,
            glyphicon: controlModel.Glyphicon,
            order: controlModel.Order,
            tabId: controlModel.TabId,
            controlName: controlModel.ControlName,
            actions: controlModel.Actions,
            customActions: controlModel.CustomActions ? controlModel.CustomActions.map($.proxy(this._mapCustomActionModelToCustomAction, this)) : []
        };

        return model;
    };

    FormBuilder.prototype._mapCustomActionModelToCustomAction = function (customActionModel) {

        return {
            name: customActionModel.Name,
            glyphicon: customActionModel.Glyphicon,
            title: customActionModel.Title
        };
    };

    FormBuilder.prototype._mapControlActionToControlAddon = function (controlAction) {

        var addon = this._addons[controlAction];

        return {
            name: addon.name,
            glyphicon: addon.glyphicon,
            title: addon.title,
        };
    };

    FormBuilder.prototype._generateUid = function () {

        this._uid = this._uid ? this._uid + 1 : 1;

        return this._uid;
    };

    FormBuilder.prototype._getControlTypeName = function (controlType) {

        for (var name in models.controlTypes) {
            if (models.controlTypes.hasOwnProperty(name) && models.controlTypes[name] === controlType) {
                return name;
            }
        }

        return null;
    };

    FormBuilder.prototype._getLocalizedString = function (string) {

        if (typeof string != 'string') {
            return null;
        }

        if (typeof this.resource == 'object' && this.resource != null) {
            return this.resource[string];
        }

        if (typeof this.resource == 'function') {
            return this.resource(string);
        }

        return string;
    };

    FormBuilder.prototype._getTab = function (tabId) {

        var $tab = this.$element.find(this.options.controlItemsListSelector + '[data-tabid="' + tabId + '"]');

        return $tab;
    }

    FormBuilder.prototype._slideTab = function ($tab, direction) {

        $tab.show('slide', { direction: direction }, 250);
    };

    FormBuilder.prototype._toggleElements = function (toggled, animate) {

        var $controlItemsList = this.$element.find(this.options.controlItemsListSelector),
            $form = this.$form,
            $propertiesContainer = this.$element.find(this.options.settingsContainerSelector);

        var $elements = $controlItemsList.add($form).add($propertiesContainer);

        if (toggled) {
            if (animate) {
                $elements.slideDown('fast');
            } else {
                $elements.show();
            }
        } else {
            if (animate) {
                $elements.slideUp('fast');
            } else {
                $elements.hide();
            }
        }
    };

    FormBuilder.prototype._getHeight = function ($element) {

        return parseInt($element.css('height').replace('px', ''));
    };

    FormBuilder.prototype._getFormControlModel = function ($formControl) {

        var model = {
            uid: parseInt($formControl.attr('data-uid')),
            displayName: $formControl.attr('data-displayname'),
            type: parseInt($formControl.attr('data-controltype'))
        };

        return model;
    };

    FormBuilder.prototype._getFormControl = function (uid) {

        return this.$form.find(this.options.formControlSelector + '[data-uid="' + uid + '"]');
    };

    FormBuilder.prototype._getFormControlAddon = function (formControlUid, actionName) {

        return this._getFormControl(formControlUid).find('[data-addon-toggle="' + actionName + '"]');
    };

    FormBuilder.prototype._getFormSettingsTab = function (type, uid) {

        return this.$element.find('[data-properties-for="' + type + '"]:first');
    };

    FormBuilder.prototype._swapElements = function ($first, $second, direction) {

        $first.hide();
        $second.hide();

        var animationOptions = {
            easing: null,
            duration: 200
        };

        animationOptions = null;

        $first.show(animationOptions);

        if (direction === models.directions.up) {

            $second.before($first);
            $second.show(animationOptions);
        } else {

            $second.after($first);
            $second.show(animationOptions);
        }
    };

    FormBuilder.prototype._getFormModel = function (uid) {

        for (var i in this.formModel) {

            if (this.formModel[i].uid === uid) {
                return this.formModel[i];
            }
        }

        return null;
    };

    FormBuilder.prototype._updateFormModel = function (model) {

        var index = 0;

        for (var i in this.formModel) {

            if (this.formModel[i].uid === model.uid) {

                index = i;

                break;
            }
        }

        this.formModel.splice(index, 1);

        this.formModel.push(model);

    };

    FormBuilder.prototype._getSelectItemsFromSavedProperties = function (properties, propertyPrefix) {

        var items = [];

        for (var prop in properties) {
            if (properties.hasOwnProperty(prop) && prop.indexOf(propertyPrefix) == 0) {
                items.push(properties[prop]);
            }
        }

        return items;
    };

    FormBuilder.prototype._getOptionsFromSelectItems = function (items) {

        var html = '';

        for (var i in items) {

            var option = '<option value="' + items[i] + '">' + items[i] + '</option>';

            html += option;
        }

        return html;
    };

    // #endregion

    $.widget('bforms.bsFormBuilder', FormBuilder.prototype);

    return FormBuilder;
};

if (typeof define == 'function' && define.amd) {
    define('bforms-formBuilder', [
                                    'jquery',
                                    'bforms-formBuilder-formRenderer',
                                    'bforms-formBuilder-models',
                                    'jquery-ui-core',
                                    'amplify',
                                    'select2',
                                    'bforms-form'
    ], factory);
} else {
    factory(window.jQuery);
}