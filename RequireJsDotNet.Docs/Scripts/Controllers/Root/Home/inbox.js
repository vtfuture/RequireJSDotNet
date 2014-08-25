/*
 * RequireJS.NET
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
require([
        'keepalive-service', //add dependency
        'app-global'
], function (keepalive) {

    var numToWords = function (num) {
        var ones = ['', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine'];
        return ones[num];
    };

    var Page = function (opt) {

        this.options = $.extend(true, {}, opt);

        //create Keep Alive Service
        this.ping = new keepalive({
            autoStart: true,
            noLog: false,
            interval: 3000,
            maxRetries: 5,
            pauseAfterReceive: true,
            pingUrl: '/Keepalive/PingAsync'
        });

        //handler for Keep Alive Service on data received
        amplify.subscribe('Ping|DataReceived', this, function (envelope) {

            //display new messages count
            var li = $('#counter');
            var nr = numToWords(envelope.data);
            var now = new Date();
            li.html('<h5>new messages</h5> last checked at <mark>' +
                now.getMinutes() + ':' + now.getSeconds() + 'sec <mark>')
                .removeClass().addClass(nr).show();

            //resume the Keep Alive Service
            this.ping.start();
        });

        this.registerEvents();
    };

    //methods
    Page.prototype = $.extend(true, Page.prototype, {
        registerEvents: function () {

            $('#togglePing').on("click", $.proxy(function (e) {
                e.preventDefault();
                var action = $(e.currentTarget);
                if (action.text() == "Stop") {
                    this.ping.stop();
                    action.text("Start");
                } else {
                    this.ping.start();
                    action.text("Stop");
                }

            }, this));

        }
    });

    //create object on DOM ready
    $(document).ready(function () {
        var page = new Page(requireConfig.pageOptions);
    });
});
