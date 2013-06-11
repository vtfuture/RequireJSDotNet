/*
 * RequireJS for .NET
 * Version 1.0.0.1
 * Release Date 10/06/0213
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

define([
    'jquery',
    'amplify',
    'app-helpers'
], function () {

    //constructor
    var Global = function(opt) {

        //members
        this.options = $.extend(true, { }, opt);
        this.isMobile = $.browser.isMobile;

        //call methods
        this.ajaxConfig();

    };

    //methods
    Global.prototype = $.extend(true, Global.prototype, {

        create: function () {

        },

        ajaxConfig: function () {
            
            //show loader on ajax start
            $('.loading_global').ajaxSend(function () {
                $(this).show();
            });

            //hide loader on ajax complete
            $('.loading_global').ajaxSuccess(function () {
                $(this).hide();
            });

            //register global decoder
            amplify.request.decoders.appEnvelope = function (data, status, xhr, success, error) {

                switch (data.Status) {
                    case 1: //success
                        success(data);
                        break;
                    case 2: //server error
                        error(data, data.Status);
                        break;
                    case 3: //unauthorized
                        window.location = this.options.loginUrl;
                        break;
                    case 4: //invalid data
                        error(data, data.Status);
                        break;
                    case 5: //redirect required
                        window.location = data.RedirectUrl;
                        break;
                }
            };

            //log ajax abort
            amplify.subscribe("request.error", function (settings, data, status) {
                if (status === "abort") {
                    console.log('the ajax request was aborted');
                }
            });
        }
    });

    //self-executing module
    $(document).ready(function () {
        var app = new Global(requireConfig.websiteOptions);
    });

    return Global;
});