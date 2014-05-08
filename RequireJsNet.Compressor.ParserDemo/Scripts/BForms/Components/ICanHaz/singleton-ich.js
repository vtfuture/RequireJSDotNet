define('singleton-ich', [
        //'i18n!nls/resources',
        'icanhaz'
], function (Resources, i) {

    var IchSingleton = (function () {
        var instance;

        var loadTemplate = function (inst, html) {

            var $wrapper = $('<div></div>').append(html),
                scripts = $wrapper.find('script[type="text/html"], script[type="text/x-icanhaz"], script[type="text/mustache"]');

            scripts.each(function (idx, script) {
                inst.addTemplate(script.id, script.innerHTML.trim());
            });

        };

        var getInstance = function (opt) {
            if (!instance) {
                instance = ich;
                //loadTemplate(instance, common);
                //instance.setResources(Resources);
                instance.grabTemplates();
            }
            return instance;
        };

        return {
            getInstance: function (opts) {
                return getInstance(opts);
            },
            loadTemplate: function (html) {
                loadTemplate(getInstance(), html);
            }
        };

    })();

    return IchSingleton;

});