require([
        'jquery',
        'jquery-ui',
        'amplify',
        'jsrender',
        'redactor',
        'app-helpers',
        'app-global'
], function () {

    //constructor
    var HomeIndex = function(opt) {

        this.options = $.extend(true, { }, opt);

        //members
        this.mode;

        //init methods
        this.create();
        this.run();
    };

    //methods
    HomeIndex.prototype = $.extend(true, HomeIndex.prototype, {

        create: function () {
            
            amplify.subscribe('HomeLoad', $.proxy(function () {

            }, this));
        },

        run: function () {
            amplify.publish('HomeLoad');
        }
    });

    //create object
    $(document).ready(function () {
        var page = new HomeIndex(requireConfig.pageOptions);
    });

});