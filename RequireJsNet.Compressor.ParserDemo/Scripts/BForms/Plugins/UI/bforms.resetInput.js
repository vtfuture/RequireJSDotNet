(function(factory) {

    if(typeof define === "function" && define.amd) {
        define('bforms-resetInput', ['jquery'], factory);
    }else {
        factory(window.jQuery);
    }

})(function($) {

    $.fn.extend({
        bsResetInput: function() {
            return $(this).each(function() {
                if (!$(this).hasClass('hasResetInput')) {
                    return new ResetInput($(this));
                }
            });
        }
    });

    var ResetInput = (function() {

        function ResetInput(self) {

            function UpdateButton(self, button) {
                if (self.val() != '') {
                    button.show();
                } else {
                    button.hide();
                }
            }

            function BuildButton(self) {

                var $button = $('<button type="button" class="delete js-resetInputBtn">x</button>'),
                    $container = $('<div class="reset-container"></div>');

                if (self.val() == '') {
                    $button.hide();
                }

                self.wrapAll($container);

                self.after($button);
                UpdateButton(self, $button);
                return $button;
            }

            var button = BuildButton(self);
            self.addClass('hasResetInput');

            self.on('change', { self: self, button: button }, function(e) {
                UpdateButton(e.data.self, e.data.button);
            });

            button.on('click', { self: self }, function(e) {
                e.data.self.val('');
                button.hide();
            });

        }

        return ResetInput;
    })();

});