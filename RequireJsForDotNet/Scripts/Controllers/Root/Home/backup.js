//cod used in article

//#region templates
define([
    'jquery'
], function () {

    //constructor
    var Global = function (opt) {
        this.options = $.extend(true, {}, opt);
        this.create();
    }

    Global.prototype = $.extend(true, Global.prototype, {
        create: function () {
            //app wide UI behavior in here
        }
    });

    //self-executing module
    $(document).ready(function () {
        var myModule = new Global(requireConfig.websiteOptions);
    });
    
    //reusable module
    return Global;
});

require([
        'jquery',
        'app-global'
], function () {

    //constructor
    var HomeIndex = function (opt) {
        this.options = $.extend(true, {}, opt);
        this.create();
    }

    HomeIndex.prototype = $.extend(true, HomeIndex.prototype, {
        create: function () {
            //page UI behavior in here
        }
    });

    //create object on DOM ready
    $(document).ready(function () {
        var myPage = new HomeIndex(requireConfig.pageOptions);
    });
});
//#endregion

//#region utlis
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
        }
    }
});


require([
        'app-utils',
        'app-global'

], function (utils) {
    //utils param points to the first dependency

    var Page = function (opt) {
        this.options = $.extend(true, {}, opt);
        this.create();
    }

    Page.prototype = $.extend(true, Page.prototype, {
        create: function () {
            //call functions defined in app-utils.js
            console.log(utils.removeCaps('TEST'));
        }
    });

    //create object on DOM ready
    $(document).ready(function () {
        var myPage = new Page(requireConfig.pageOptions);
    });
});
//#endregion

//#region $.proxy
require([
        'jquery',
        'app-global'
], function () {

    //constructor
    var Page = function (opt) {

        //memebers
        this.options = $.extend(true, {}, opt);
    }

    //methods
    Page.prototype = $.extend(true, Page.prototype, {

        registerEventsWithoutProxy: function () {

            $('a').on('click', function (e) {
                e.preventDefault();

                //<this> refers the anchor DOM element
                $link = $(this);
                console.log($link.text());

                //<this.options> is undefined
                //the Page object is not accesible
                console.log(this.options.homeUrl);

            });
        },

        registerEventsWithProxy: function () {

            //use $.proxy in order to acces 
            //the module context <this> inside the function
            $('a').on('click', $.proxy(function (e) {
                e.preventDefault();

                //use $(e.currentTarget) instead of $(this)
                $link = $(e.currentTarget);
                console.log($link.text());

                //<this> refers the Page object
                console.log(this.options.homeUrl);

            }, this));
        }

    });

    //init object
    $(function () {
        //load options sent by the server-side controller
        var myPage = new Page(requireConfig.pageOptions);
    });
});
//#endregion

//#region Public & Static Module
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
        console.log(module.connection.mode);

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

define([
    'jquery'
], function () {

    //private variable
    var copyright = 'Stefan Prodan';

    //static methods
    return {
        //readonly emulation
        copyright: function () {
            return copyright;
        }
    }
});
//usage: myStaticModule.copyright()

require([
        'Mods/myStaticModule',
        'jquery',
        'app-global'
], function (myStaticModule) {

    var Page = function (opt) {

        this.options = $.extend(true, {}, opt);

        //get static property
        console.log(myStaticModule.version);
        console.log(myStaticModule.GetCopyright());
        //call static function
        console.log(myStaticModule.isUrl('www.stefanprodan.eu'));

    }

    //create object on DOM ready
    $(document).ready(function () {
        var page = new Page(requireConfig.pageOptions);
    });
});

//#endregion

define('MyModule',[
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

define([
    'jquery'
], function () {

    //constructor
    var Global = function (opt) {
        this.options = $.extend(true, {}, opt);
        this.create();
    }

    Global.prototype = $.extend(true, Global.prototype, {
        create: function () {
            //app wide UI behavior in here
        }
    });

    //self-executing module
    $(document).ready(function () {
        var myModule = new Global(requireConfig.websiteOptions);
    });

    //reusable module
    return Global;
});

require([
        'jquery',
        'app-global'
], function () {

    //constructor
    var HomeIndex = function (opt) {
        this.options = $.extend(true, {}, opt);
        this.create();
    }

    HomeIndex.prototype = $.extend(true, HomeIndex.prototype, {
        create: function () {
            //page UI behavior in here
        }
    });

    //create object on DOM ready
    $(document).ready(function () {
        var myPage = new HomeIndex(requireConfig.pageOptions);
    });
});

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
        }
    }
});


require([
        'app-utils',
        'app-global'

], function (utils) {
    //utils param points to the first dependency

    var Page = function (opt) {
        this.options = $.extend(true, {}, opt);
        this.create();
    }

    Page.prototype = $.extend(true, Page.prototype, {
        create: function () {
            //call functions defined in app-utils.js
            console.log(utils.removeCaps('TEST'));
        }
    });

    //create object on DOM ready
    $(document).ready(function () {
        var myPage = new Page(requireConfig.pageOptions);
    });
});

require([
        'jquery',
        'app-global'
], function () {

    //constructor
    var Page = function (opt) {

        //memebers
        this.options = $.extend(true, {}, opt);
    }

    //methods
    Page.prototype = $.extend(true, Page.prototype, {

        registerEventsWithoutProxy: function () {

            $('a').on('click', function (e) {
                e.preventDefault();

                //<this> refers the anchor DOM element
                $link = $(this);
                console.log($link.text());

                //<this.options> is undefined
                //the Page object is not accesible
                console.log(this.options.homeUrl);

            });
        },

        registerEventsWithProxy: function () {

            //use $.proxy in order to acces 
            //the module context <this> inside the function
            $('a').on('click', $.proxy(function (e) {
                e.preventDefault();

                //use $(e.currentTarget) instead of $(this)
                $link = $(e.currentTarget);
                console.log($link.text());

                //<this> refers the Page object
                console.log(this.options.homeUrl);

            }, this));
        }

    });

    //init object
    $(function () {
        //load options sent by the server-side controller
        var myPage = new Page(requireConfig.pageOptions);
    });
});

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
        console.log(module.connection.mode);

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