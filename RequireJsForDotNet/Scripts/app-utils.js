define([
    'jquery'
], function () {
    return {

        removeCaps: function (str) {
            str += '';
            if (str == str.toUpperCase()) {
                return str.charAt(0) + str.substr(1).toLowerCase();
            }
            return str;
        },

        isWP: function () {
            return /Windows Phone/i.test(navigator.userAgent);
        },

        //source http://stackoverflow.com/questions/4179708
        isCharacterKeyPress: function (evt) {
            var result = false;
            if (typeof evt.which === "undefined") {
                result = true;
            } else if (typeof evt.which === "number" && evt.which > 0) {
                result = !evt.ctrlKey && !evt.metaKey && !evt.altKey;
            }
            return result;
        }
    }
});
