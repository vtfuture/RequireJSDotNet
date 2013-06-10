/*
 * RequireJS for .NET
 * Version 1.0.0.1
 * Release Date 06/09/0212
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

require([
        'Mods/myPublicModule',
        'jquery',
        'app-global'
], function (myPublicModule) {

    var Page = function (opt) {

        this.options = $.extend(true, {}, opt);

        //create a new object
        var module = new myPublicModule();

        //call public method
        module.connect();

        //access public member
        setTimeout($.proxy(function () {
            console.log(module.connection.mode);
        }, this), 200);

        //try to access private member
        //will return undefined
        console.log(module.isMobile);

        //try to call private method
        //will throw object has no method 'getNetworkMode'
        console.log(module.getNetworkMode());
    }

    //create object on DOM ready
    $(document).ready(function () {
        var page = new Page(requireConfig.pageOptions);
    });
});
