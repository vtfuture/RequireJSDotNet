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

define('MyModule', [
    'jquery',
    'amplify'
], function () {

    //private variable
    var status = 0;

    //private function
    var init = function () {

        status = 1;
        return true;
    };

    //constructor
    var MyObject = function (opt) {

        //public variable
        this.options = $.extend(true, {}, opt);

        //call private function
        if (init()) {
            //call public function on DOM ready
            $(document).ready($.proxy(this.onDocumentReady, this));
        }
    };

    //public functions
    MyObject.prototype = $.extend(true, MyObject.prototype, {

        onDocumentReady: function () {

            status = 2;
            console.log('A new object was created');
        }
    });
});