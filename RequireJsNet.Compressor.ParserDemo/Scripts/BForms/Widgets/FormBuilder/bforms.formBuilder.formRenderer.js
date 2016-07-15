var factory = function ($, templates, models) {

    var FormRenderer = function (options) {

        $.extend(true, this.options, options);

        this._init();
    };

    FormRenderer.prototype.options = {
        idDotReplacement: '_',
        applyValidation: undefined
    };

    // #region init

    FormRenderer.prototype._init = function () {

        this.customRenderers = {};

        this._initTemplates();
    };

    FormRenderer.prototype._initTemplates = function () {

        for (var name in templates) {
            if (templates.hasOwnProperty(name)) {
                if (typeof ich[name] != 'function') {
                    ich.addTemplate(name, templates[name]);
                }
            }
        }
    };

    // #endregion

    // #region rendering methods

    FormRenderer.prototype.renderControlProperties = function (controlType) {

        var model = {},
            controls = [];

        for (var i in models.propertiesModels) {

            var propertiesModel = models.propertiesModels[i];

            if (propertiesModel.type === controlType) {
                model = propertiesModel;
                break;
            }
        }

        for (var i in model.settings) {
            controls.push(this.renderFormControlWrapper(model.settings[i]));
        }

        var propertiesTabModel = $.extend(true, {}, model, {
            controls: controls
        });

        return ich.propertiesTab(model);
    };

    FormRenderer.prototype.renderControlListItem = function (model) {

        return ich.controlListItem(model);
    };

    FormRenderer.prototype.renderFormControlWrapper = function (model) {

        var size = this._getSizeModel(model);

        var wrapperModel = {
            colLg: size.lg,
            colMd: size.md,
            colSm: size.sm,
            colXs: size.xs,
            control: model.control
        };

        return ich.formControlWrapper(wrapperModel, true);
    };

    FormRenderer.prototype.renderFormGroup = function (model) {

        var size = {};

        if (typeof model.size == 'object' || !model.size) {
            size = $.extend(true, {}, model.size || {}, {
                lg: 12,
                md: 12,
                sm: 12,
                xs: 12
            });
        } else {
            size = {
                lg: model.size,
                md: model.size,
                sm: model.size,
                xs: model.size
            };
        }

        var formGroupModel = {
            colLg: size.lg,
            colMd: size.md,
            colSm: size.sm,
            colXs: size.xs,
            label: model.label,
            addon: model.addon,
            control: model.control,
            validation: model.validation,
            cssClass: model.cssClass,
            controlAddons: model.controlAddons
        };

        var formGroup = ich.formGroup(formGroupModel, true);

        return formGroup;
    };

    FormRenderer.prototype.renderControlGroup = function (model, controlHtml) {

        var labelText = model.label,
            name = model.name,
            id = this._generateIdFromName(name),
            type = model.type,
            glyphicon = model.glyphicon,
            description = model.description;

        var required = model.required || false;

        var labelModel = {
            text: labelText,
            controlId: id,
            required: required
        },
            glyphiconModel = {
                glyphicon: glyphicon
            },
            validationModel = {
                replace: 'true',
                controlName: name
            },
            controlAddons =  model.addons || this.options.defaultAddons;

        controlAddons = this._removeDuplicateAddons(controlAddons);

        var label = ich.label(labelModel, true),
            addon = glyphicon ? ich.glyphiconAddon(glyphiconModel, true) : '',
            control = controlHtml,
            validation = ich.validation(validationModel, true);

        var formGroupModel = {
            cssClass: 'form_builder-formControl',
            label: label,
            addon: addon,
            control: control,
            validation: validation,
            size: model.size,
            controlAddons: controlAddons
        };

        var formGroup = this.renderFormGroup(formGroupModel);

        return formGroup;

    };

    FormRenderer.prototype.renderInput = function (model) {

        // basic properties
        var name = model.name,
            id = this._generateIdFromName(name),
            type = model.type,
            placeholder = model.placeholder,
            required = model.required || false;

        var inputModel = {
            name: name,
            id: id,
            type: type,
            placeholder: placeholder,
            required: required,
            applyValidation: this.options.applyValidation
        };

        var input = ich.input(inputModel, true);

        return this.renderControlGroup(model, input);
    };

    FormRenderer.prototype.renderTextBox = function (model) {

        var textBoxModel = $.extend(true, {}, model, {
            type: 'text',
            applyValidation: this.options.applyValidation
        });

        return this.renderInput(textBoxModel);
    };

    FormRenderer.prototype.renderDropdown = function (model) {

        model.name = model.name + '.SelectedValues';

        model = $.extend(true, {}, model, {
            id: this._generateIdFromName(model.name),
            applyValidation: this.options.applyValidation
        });

        var dropdown = ich.dropdown(model, true);

        return this.renderControlGroup(model, dropdown);
    };

    FormRenderer.prototype.renderRadioButtonList = function (model) {

        model.name = model.name + '.SelectedValues';

        var id = this._generateIdFromName(model.name);

        var items = model.items ? model.items.map(function (item, index) {

            return {
                value: item.value,
                label: item.label,
                controlName: model.name,
                id: id + '_' + index,
                selected: model.selectedValue === item.value
            };

        }) : [];

        var radioListModel = $.extend(true, {}, model, {
            items: items,
            applyValidation: this.options.applyValidation
        });

        var radioButtonList = ich.radioButtonList(radioListModel, true);

        return this.renderControlGroup(radioListModel, radioButtonList);
    };

    FormRenderer.prototype.renderTextArea = function (model) {

        model = $.extend(true, {}, model, {
            id: this._generateIdFromName(model.name),
            applyValidation: this.options.applyValidation
        });

        var textArea = ich.textArea(model, true);

        return this.renderControlGroup(model, textArea);
    };

    FormRenderer.prototype.renderTagList = function (model) {

        model.name = model.name + '.SelectedValues';

        model = $.extend(true, {}, model, {
            id: this._generateIdFromName(model.name),
            applyValidation: this.options.applyValidation
        });

        var multiSelect = ich.tagList(model, true);

        return this.renderControlGroup(model, multiSelect);
    };

    FormRenderer.prototype.renderListBox = function (model) {

        model.name = model.name + '.SelectedValues';
        model.applyValidation = this.options.applyValidation;

        var listBox = ich.listBox(model, true);

        return this.renderControlGroup(model, listBox);
    };

    FormRenderer.prototype.renderTitle = function (model) {

        var title = ich.formTitle(model, true);

        model.glyphicon = null;

        return this.renderControlGroup(model, title);
    };

    FormRenderer.prototype.renderDatePicker = function (model) {

        model = $.extend(true, {}, model, {
            id: this._generateIdFromName(model.name),
            applyValidation: this.options.applyValidation
        });

        var datePicker = ich.datePicker(model, true);

        return this.renderControlGroup(model, datePicker);
    };

    FormRenderer.prototype.renderDatePickerRange = function (model) {

        model = $.extend(true, {}, model, {
            id: this._generateIdFromName(model.name),
            applyValidation: this.options.applyValidation
        });

        var datePicker = ich.datePickerRange(model, true);

        return this.renderControlGroup(model, datePicker);
    };

    FormRenderer.prototype.renderCheckBox = function (model) {

        model = $.extend(true, {}, model, {
            id: this._generateIdFromName(model.name),
            applyValidation: this.options.applyValidation
        });

        var checkBox = ich.checkBox(model, true);

        return this.renderControlGroup(model, checkBox);
    };

    FormRenderer.prototype.renderCheckBoxList = function (model) {

        model.name = model.name + '.SelectedValues';

        var id = this._generateIdFromName(model.name);

        model = $.extend(true, {}, model, {
            id: id
        });

        var items = model.items ? model.items.map(function (item, index) {

            return {
                value: item.value,
                label: item.label,
                controlName: model.name,
                id: id + '_' + index,
                selected: model.selectedValue === item.value
            };

        }) : [];

        var checkBoxListModel = $.extend(true, {}, model, {
            items: items,
            applyValidation: this.options.applyValidation
        });

        var checkBoxList = ich.checkBoxList(checkBoxListModel, true);

        return this.renderControlGroup(model, checkBoxList);
    };

    FormRenderer.prototype.renderNumberPicker = function (model) {

        var id = this._generateIdFromName(model.name);

        model = $.extend(true, {}, model, {
            id: id,
            applyValidation: this.options.applyValidation
        });

        var numberPickerModel = $.extend(true, model, {});

        var numberPicker = ich.numberPicker(numberPickerModel, true);

        return this.renderControlGroup(model, numberPicker);
    };

    FormRenderer.prototype.renderNumberPickerRange = function (model) {

        var id = this._generateIdFromName(model.name);

        model = $.extend(true, {}, model, {
            id: id,
            applyValidation: this.options.applyValidation
        });

        var numberPickerRangeModel = $.extend(true, model, {});

        var numberPickerRange = ich.numberPickerRange(numberPickerRangeModel, true);

        return this.renderControlGroup(model, numberPickerRange);
    };

    FormRenderer.prototype.renderPageBreak = function (model) {

        model.glyphicon = null;

        var pageBreak = ich.pageBreak(model, true);

        return this.renderControlGroup(model, pageBreak);
    };

    FormRenderer.prototype.renderCustomControl = function (controlName, model) {

        var method = this.customRenderers[controlName];

        if (typeof method != 'function') {
            throw 'No method for rendering ' + controlName + ' custom control was found';
        }

        $.extend(true, model, {
            applyValidation: this.options.applyValidation
        });

        return method(model);
    };

    FormRenderer.prototype.renderTabControl = function (model) {

        return ich.tabControl(model, true);
    };

    // #endregion

    // #region private methods

    FormRenderer.prototype._generateIdFromName = function (name) {

        if (typeof name != 'string') {
            return null;
        }

        var dotReplacement = this.options.idDotReplacement || '_',
            id = name.replace(/\./g, dotReplacement);

        return id;
    };

    FormRenderer.prototype._getSizeModel = function (size) {

        var model = {};

        if (typeof size == 'object' || !size) {
            model = $.extend(true, {}, size || {}, {
                lg: 12,
                md: 12,
                sm: 12,
                xs: 12
            });
        } else {
            model = {
                lg: size,
                md: size,
                sm: size,
                xs: size
            };
        }

        return model;
    };

    FormRenderer.prototype._getGlyphiconName = function (glyphiconClass) {

        if (typeof glyphiconClass != 'string') {
            return '';
        }

        return glyphiconClass.replace('glyphicon-', '');
    };

    FormRenderer.prototype._removeDuplicateAddons = function (controlAddons) {

        var addons = [],
            encountered = {};

        for (var i in controlAddons) {

            var addon = controlAddons[i];

            if (!encountered[addon.name]) {
                addons.push(addon);
                encountered[addon.name] = true;
            }
        }

        return addons;
    };

    // #endregion

    // #region public methods

    // #endregion

    return FormRenderer;
};

if (typeof define == 'function' && define.amd) {
    define('bforms-formBuilder-formRenderer', [
                                                'jquery',
                                                'bforms-formBuilder-templates',
                                                'bforms-formBuilder-models',
                                                'icanhaz',
                                                'bforms-form'
                                              ], factory);
} else {
    factory(window.jQuery);
}