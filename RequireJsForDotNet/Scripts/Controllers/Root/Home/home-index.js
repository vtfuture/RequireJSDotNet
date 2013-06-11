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
require([
        'app-utils',
        'jquery',
        'jquery-ui',
        'jquery-validate-unobtrusive',
        'amplify',
        'app-global'
], function (utils) {
    //<utils> points to the first dependency app-utils

    //constructor
    var Page = function(opt) {

        this.options = $.extend(true, { }, opt);

        //selectors
        this.html = $('.main-content');

        //init
        this.create();

        //change page status
        this.run();
    };

    //methods
    Page.prototype = $.extend(true, Page.prototype, {

        create: function () {
            
            //access the id list sent by Home/Index
            var ids = this.options.ids;

            //subscribe to the change of page status
            //no need to use $.proxy, amplify has context param
            amplify.subscribe('Page|StatusChanged', this, function (status) {
                //read status sent by publisher
                if (status.state === 'running') {

                    //call function defined in app-utils.js
                    var msg = utils.removeCaps(this.options.successMsg);

                    //show message with delay
                    setTimeout($.proxy(function () {
                        //due to $.proxy, <this> refers the Page object
                        this.html.find('h3').html(msg);

                        //show steps with incremental delay
                        var delay = 100;
                        this.html.find('li').each(function (index, item) {

                            delay += 150;

                            setTimeout(function () {
                                $(item).show();
                            }, delay);
                        });

                    }, this), 500);
                }
            });

            //bind events
            this.registerClickEvents();
        },

        run: function () {
            //broadcasting the running state
            amplify.publish('Page|StatusChanged', {
                state: 'running'
            });
        },

        registerClickEvents: function () {
            $('.js-openDeps').on('click', $.proxy(function (e) {

                e.preventDefault();

                //use jquery-ui dialog
                $(".js-depsPopup").dialog();

            }, this));
        },

        //#region $.proxy usage
        registerEventsWithProxy: function () {

            //use $.proxy in order to acces 
            //the module context <this> inside the function
            $('a').on('click', $.proxy(function (e) {

                e.preventDefault();

                //use e.currentTarget to access the anchor DOM object
                var $link = $(e.currentTarget);
                console.log($link.text());

                //<this> refers the Page object
                console.log(this.options);

            }, this));
        },

        iterateItems: function () {

            //use $.proxy in order to acces 
            //the module context <this> inside the function
            $('p').each($.proxy(function (index, item) {

                //access the current <p> DOM object
                var $item = $(item);

                //<this> refers the Page object
                console.log(this.options);

            }, this));
        }
        //#endregion
    });

    //create object
    $(document).ready(function () {
        //load options sent by the server-side controller
        var page = new Page(requireConfig.pageOptions);
    });

});