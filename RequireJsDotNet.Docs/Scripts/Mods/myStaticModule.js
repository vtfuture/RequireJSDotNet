define([
    'jquery'
], function () {
    //private variable
    var copyright = 'Stefan Prodan';

    return {
        //static property
        version: '1.0.0.0',

        //static functions
        isUrl: function (str) {
            urlRegex = /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/;
            return str.match(urlRegex) != null;
        },

        //readonly emulation
        GetCopyright: function () {
            return copyright;
        }
    }
});