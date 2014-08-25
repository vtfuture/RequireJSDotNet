define(['text'], function (text) {

    return {

        //example: context!bar
        load: function (name, req, onLoad, config) {

            this.$$ = '$$';
            var $$ = this.$$;

            console.log(text);

            ////Use a method to load the text (provide elsewhere)
            ////by the plugin
            //fetchText(url, function (text) {
            //    //Transform the text as appropriate for
            //    //the plugin by using a transform()
            //    //method provided elsewhere in the plugin.
            //    text = transform(text);

            //    //Have RequireJS execute the JavaScript within
            //    //the correct environment/context.
            //    load.fromText(name, text);

            //    //Now get a handle on the evaluated module,
            //    //to return that value for this plugin-loaded
            //    //resource
            //    req([name], function (value) {
            //        load(value);
            //    });
            //});
        },

    };

});
