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

define([
    'jquery',
    'app-helpers'
], function () {

    //constructor
    var MyModule = function() {

        //public members
        this.connection = {
            status: 'offline',
            mode: 'unknown'
        };
    };
    
    //private members
    var isMobile = $.browser.isMobile;

    //private methods
    var getNetworkMode = function () {

        //use private member isMobile
        return isMobile ? 'mobile' : 'wireless';
    };

    //public methods
    MyModule.prototype = $.extend(true, MyModule.prototype, {

        connect: function () {

            //call private method
            mode = getNetworkMode();

            //set public member values
            this.connection.status = 'online';
            this.connection.mode = mode;     
        }
    });

    return MyModule;
});