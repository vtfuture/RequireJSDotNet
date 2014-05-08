(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('bforms-checklist', ['jquery'], factory);
    } else {
        factory(window.jQuery);
    }
})(function ($) {

    $.fn.extend({
        bsCheckBoxList: function () {
            return $(this).each(function () {
                if (!$(this).hasClass("checkBoxList-done")) {
                    return new CheckBoxList($(this));
                }
            });
        }
    });

    $.fn.extend({
        bsCheckBoxListUpdateSelf: function (value) {
            return $(this).each(function () {
                if ($(this).hasClass("checkBoxList-done")) {
                    return new CheckBoxListUpdateSelf($(this), value);
                }
            });
        }
    });

    $.fn.extend({
        bsResetCheckboxList: function () {
            var $elem = $(this);
            if ($elem.hasClass('checkBoxList-done')) {
                return new CheckBoxListUpdateSelf($elem, $elem.data('initialvalue'));
            }
        }
    });

    $.fn.bsParseCheckList = function() {
        var $elem = $(this);
        if ($elem.hasClass('checkBoxList-done')) {
            return new CheckBoxListParse($elem);
        }
    };

    var CheckBoxList = (function () {
        function CheckBoxList(self) {
            //#region functions
            function BuildWrapper(self) {
                var wrapper = $("<div tabindex='0' style='display: none;' id='" +
                            self.attr("id") + "_checkBox' " +
                            "class='checkbox_replace'></div>");

                var $buttonsContainer = $('<div class="btn-group-justified"></div>');
                wrapper.append($buttonsContainer);

                if (self.hasClass('form-control')) wrapper.addClass('form-control');

                self.children().each(function () {
                    var anchorText = $(this).find("label").text();
                    var childSelect = $(this).find("input[type='checkbox']");
                    childSelect.data('noparse', true);
                    var value = childSelect.data('value');
                    
                    $buttonsContainer.append("<a data-value='" + value + "' class='option'>" + anchorText + "</a>");
                });
                UpdateWrapper(self, wrapper);
                return wrapper;
            }
            function UpdateWrapper(self, wrapper) {
                self.children().each(function () {
                    var selfCheckbox = $(this).find("input[type='checkbox']"),
                        value = selfCheckbox.data('value');

                    var wrapperCheckbox = wrapper.find("a[data-value='" + value + "']");

                    if (selfCheckbox.is(":checked")) {
                        wrapperCheckbox.addClass("selected");
                    }
                    else {
                        wrapperCheckbox.removeClass("selected");
                    }
                });
            }
            //#endregion

            //#region actions
            var wrapper = BuildWrapper(self);
            self.after(wrapper);
            self.hide();
            wrapper.show();
            self.addClass("checkBoxList-done");

            if (self.data("initialvalue") == undefined) {

                var initialValue = self.find('input[type="checkbox"]:checked').map(function () { return $(this).data('value'); });

                self.data("initialvalue", initialValue);
                self.data('value', initialValue);
            }
            //#endregion

            //#region registering events
            self.on("change", { self: self, wrapper: wrapper }, function (e) {
                UpdateWrapper(e.data.self, e.data.wrapper);
            });

            wrapper.on("click", "a.option", { self: self }, function (e) {
                e.preventDefault();
                e.data.self.bsCheckBoxListUpdateSelf($(this).data("value"));
                $(this).focus();
            });
            //#endregion
        }
        return CheckBoxList;
    })();

    var CheckBoxListUpdateSelf = (function () {
        function CheckBoxListUpdateSelf(self, value) {

            if (!$.isArray(value)) {
                value = [value];
            }

            self.find("input[type='checkbox']").each(function () {
                var $current = $(this),
                    checkboxId = $current.data('value');

                if ($.inArray(checkboxId, value) != -1) {

                    var wasChecked = $current.prop("checked"),
                        newVal = wasChecked ? false : true;

                    $current.prop("checked", newVal);

                    if (typeof $current.valid === "function") {
                        $current.valid();
                    }
                }
            });

            self.data('value', value);
            self.trigger("change");
        }
        return CheckBoxListUpdateSelf;
    })();

    var CheckBoxListParse = (function($elem) {
        function CheckBoxListParse(self) {

            var name = self.find('input[type="checkbox"]:first').prop('name'),
                data = { };

            var $checked = self.find('input[type="checkbox"]:checked');
            $checked.each(function(idx, elem) {
                var $elem = $(elem);
                if($elem.is(':checked')) {
                    data[name + '[' + idx + ']'] = $elem.data('value');
                }
            });

            return data;
        }

        return CheckBoxListParse($elem);
    });
})