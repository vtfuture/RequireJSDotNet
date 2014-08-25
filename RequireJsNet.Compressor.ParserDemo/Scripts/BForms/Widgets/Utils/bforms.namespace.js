/*!
 * BForms v1.0.0 Alpha
 * http://bforms.stefanprodan.eu
 *
 * Copyright 2013 Stefan Prodan and contributors
 * http://github.com/stefanprodan/BForms/contributors
 * Licensed under the MIT license
 */

define('bforms-namespace', [
        'jquery',
        'jquery-migrate'
], function (jQuery) {

    jQuery.nsx = function (ns_string) {
        var parts = ns_string.split('.'),
            parent = jQuery,
            i;

        // strip redundant leading global
        if (parts[0] === "$") {
            parts = parts.slice(1);
        } else if (parts[0] === "jQuery") {
            parts = parts.slice(1);
        }

        for (i = 0; i < parts.length; i += 1) {
            // create a property if it doesn't exist
            if (typeof parent[parts[i]] === "undefined") {
                parent[parts[i]] = {};
            }
            parent = parent[parts[i]];
        }
        return parent;
    };

    jQuery.nsx('bforms');

    var Utils = function () {};
    
    //#region scrollToElement
    Utils.prototype.scrollToElement = function (id) {
        var el = $(id);
        if (el.length > 0) {
            var viewport = jQuery.bforms.getViewport();

            var offsetTop = $(id).offset().top;
            var middle = viewport.height / 2;

            var goTo = offsetTop;

            if (middle < offsetTop) {
                goTo = offsetTop - middle;
            } else {
                goTo = 0;
            }

            if ($.browser.msie && $.browser.mobile) {
                $('html').scrollTop(goTo);
            } else {
               $('html,body').animate({ scrollTop: goTo }, 500);
            }

        }
    };
    //#endregion

    //#region getViewport
    Utils.prototype.getViewport = function () {
        var w = window, d = document, e = d.documentElement, g = d.getElementsByTagName('body')[0], x = w.innerWidth || e.clientWidth || g.clientWidth, y = w.innerHeight || e.clientHeight || g.clientHeight;

        return { width: x, height: y };
    };
    //#endregion

    //#region param
    Utils.prototype.param = function (a, traditional) {
        var prefix, s = [],
            add = function (key, value) {
                // If value is a function, invoke it and return its value
                value = jQuery.isFunction(value) ? value() : (value == null ? "" : value);
                s[s.length] = encodeURIComponent(key) + "=" + encodeURIComponent(value);
            };

        // Set traditional to true for jQuery <= 1.3.2 behavior.
        if (traditional === undefined) {
            traditional = jQuery.ajaxSettings && jQuery.ajaxSettings.traditional;
        }

        // If an array was passed in, assume that it is an array of form elements.
        if (jQuery.isArray(a) || (a.jquery && !jQuery.isPlainObject(a))) {
            // Serialize the form elements
            jQuery.each(a, function () {
                add(this.name, this.value);
            });

        } else {
            // If traditional, encode the "old" way (the way 1.3.2 or older
            // did it), otherwise encode params recursively.
            for (prefix in a) {
                this.buildParams(prefix, a[prefix], traditional, add);
            }
        }

        // Return the resulting serialization
        return s.join("&").replace("/%20/g", "+");
    };

    Utils.prototype.buildParams = function (prefix, obj, traditional, add) {
        var name;

        if (jQuery.isArray(obj)) {
            // Serialize array item.
            jQuery.each(obj, $.proxy(function (i, v) {
                if (traditional || /\[\]$/.test(prefix)) {
                    // Treat each array item as a scalar.
                    add(prefix, v);

                } else {
                    // Item is non-scalar (array or object), encode its numeric index.
                    this.buildParams(prefix + "[" + i + "]", v, traditional, add);
                }
            }, this));

        } else if (!traditional && jQuery.type(obj) === "object") {
            // Serialize object item.
            for (name in obj) {
                this.buildParams(prefix + "." + name, obj[name], traditional, add);
            }

        } else {
            // Serialize scalar item.
            add(prefix, obj);
        }
    };
    //#endregion

    //#region inherit
    Utils.prototype.inherit = function (derived, base) {
        for (var property in base) if (base.hasOwnProperty(property)) derived[property] = base[property];
        function __() { this.constructor = derived; }
        __.prototype = base.prototype;
        derived.prototype = new __();
    };
    //#endregion

    $.extend(true, $.bforms, new Utils());

    // return module
    return Utils;
});