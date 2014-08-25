(function (factory) {
    if (typeof define === "function" && define.amd) {
        define('jquery-migrate', ['jquery'], factory);
    } else {
        factory();
    }
}(function () {
    /*!
 * jQuery Migrate - v1.2.1 - 2013-05-08
 * https://github.com/jquery/jquery-migrate
 * Copyright 2005, 2013 jQuery Foundation, Inc. and other contributors; Licensed MIT
 */
    (function (jQuery, window, undefined) {
        // See http://bugs.jquery.com/ticket/13335
        // "use strict";


        var warnedAbout = {};

        // List of warnings already given; public read only
        jQuery.migrateWarnings = [];

        // Set to true to prevent console output; migrateWarnings still maintained
        jQuery.migrateMute = true;

        // Show a message on the console so devs know we're active
        if (!jQuery.migrateMute && window.console && window.console.log) {
            window.console.log("JQMIGRATE: Logging is active");
        }

        // Set to false to disable traces that appear with warnings
        if (jQuery.migrateTrace === undefined) {
            jQuery.migrateTrace = false;
        }

        // Forget any warnings we've already given; public
        jQuery.migrateReset = function () {
            warnedAbout = {};
            jQuery.migrateWarnings.length = 0;
        };

        function migrateWarn(msg) {
            var console = window.console;
            if (!warnedAbout[msg]) {
                warnedAbout[msg] = true;
                jQuery.migrateWarnings.push(msg);
                if (console && console.warn && !jQuery.migrateMute) {
                    console.warn("JQMIGRATE: " + msg);
                    if (jQuery.migrateTrace && console.trace) {
                        console.trace();
                    }
                }
            }
        }

        function migrateWarnProp(obj, prop, value, msg) {
            if (Object.defineProperty) {
                // On ES5 browsers (non-oldIE), warn if the code tries to get prop;
                // allow property to be overwritten in case some other plugin wants it
                try {
                    Object.defineProperty(obj, prop, {
                        configurable: true,
                        enumerable: true,
                        get: function () {
                            migrateWarn(msg);
                            return value;
                        },
                        set: function (newValue) {
                            migrateWarn(msg);
                            value = newValue;
                        }
                    });
                    return;
                } catch (err) {
                    // IE8 is a dope about Object.defineProperty, can't warn there
                }
            }

            // Non-ES5 (or broken) browser; just set the property
            jQuery._definePropertyBroken = true;
            obj[prop] = value;
        }

        if (document.compatMode === "BackCompat") {
            // jQuery has never supported or tested Quirks Mode
            migrateWarn("jQuery is not compatible with Quirks Mode");
        }


        var attrFn = jQuery("<input/>", { size: 1 }).attr("size") && jQuery.attrFn,
            oldAttr = jQuery.attr,
            valueAttrGet = jQuery.attrHooks.value && jQuery.attrHooks.value.get ||
                function () { return null; },
            valueAttrSet = jQuery.attrHooks.value && jQuery.attrHooks.value.set ||
                function () { return undefined; },
            rnoType = /^(?:input|button)$/i,
            rnoAttrNodeType = /^[238]$/,
            rboolean = /^(?:autofocus|autoplay|async|checked|controls|defer|disabled|hidden|loop|multiple|open|readonly|required|scoped|selected)$/i,
            ruseDefault = /^(?:checked|selected)$/i;

        // jQuery.attrFn
        migrateWarnProp(jQuery, "attrFn", attrFn || {}, "jQuery.attrFn is deprecated");

        jQuery.attr = function (elem, name, value, pass) {
            var lowerName = name.toLowerCase(),
                nType = elem && elem.nodeType;

            if (pass) {
                // Since pass is used internally, we only warn for new jQuery
                // versions where there isn't a pass arg in the formal params
                if (oldAttr.length < 4) {
                    migrateWarn("jQuery.fn.attr( props, pass ) is deprecated");
                }
                if (elem && !rnoAttrNodeType.test(nType) &&
                    (attrFn ? name in attrFn : jQuery.isFunction(jQuery.fn[name]))) {
                    return jQuery(elem)[name](value);
                }
            }

            // Warn if user tries to set `type`, since it breaks on IE 6/7/8; by checking
            // for disconnected elements we don't warn on $( "<button>", { type: "button" } ).
            if (name === "type" && value !== undefined && rnoType.test(elem.nodeName) && elem.parentNode) {
                migrateWarn("Can't change the 'type' of an input or button in IE 6/7/8");
            }

            // Restore boolHook for boolean property/attribute synchronization
            if (!jQuery.attrHooks[lowerName] && rboolean.test(lowerName)) {
                jQuery.attrHooks[lowerName] = {
                    get: function (elem, name) {
                        // Align boolean attributes with corresponding properties
                        // Fall back to attribute presence where some booleans are not supported
                        var attrNode,
                            property = jQuery.prop(elem, name);
                        return property === true || typeof property !== "boolean" &&
                            (attrNode = elem.getAttributeNode(name)) && attrNode.nodeValue !== false ?

                            name.toLowerCase() :
                            undefined;
                    },
                    set: function (elem, value, name) {
                        var propName;
                        if (value === false) {
                            // Remove boolean attributes when set to false
                            jQuery.removeAttr(elem, name);
                        } else {
                            // value is true since we know at this point it's type boolean and not false
                            // Set boolean attributes to the same name and set the DOM property
                            propName = jQuery.propFix[name] || name;
                            if (propName in elem) {
                                // Only set the IDL specifically if it already exists on the element
                                elem[propName] = true;
                            }

                            elem.setAttribute(name, name.toLowerCase());
                        }
                        return name;
                    }
                };

                // Warn only for attributes that can remain distinct from their properties post-1.9
                if (ruseDefault.test(lowerName)) {
                    migrateWarn("jQuery.fn.attr('" + lowerName + "') may use property instead of attribute");
                }
            }

            return oldAttr.call(jQuery, elem, name, value);
        };

        // attrHooks: value
        jQuery.attrHooks.value = {
            get: function (elem, name) {
                var nodeName = (elem.nodeName || "").toLowerCase();
                if (nodeName === "button") {
                    return valueAttrGet.apply(this, arguments);
                }
                if (nodeName !== "input" && nodeName !== "option") {
                    migrateWarn("jQuery.fn.attr('value') no longer gets properties");
                }
                return name in elem ?
                    elem.value :
                    null;
            },
            set: function (elem, value) {
                var nodeName = (elem.nodeName || "").toLowerCase();
                if (nodeName === "button") {
                    return valueAttrSet.apply(this, arguments);
                }
                if (nodeName !== "input" && nodeName !== "option") {
                    migrateWarn("jQuery.fn.attr('value', val) no longer sets properties");
                }
                // Does not return so that setAttribute is also used
                elem.value = value;
            }
        };


        var matched, browser,
            oldInit = jQuery.fn.init,
            oldParseJSON = jQuery.parseJSON,
            // Note: XSS check is done below after string is trimmed
            rquickExpr = /^([^<]*)(<[\w\W]+>)([^>]*)$/;

        // $(html) "looks like html" rule change
        jQuery.fn.init = function (selector, context, rootjQuery) {
            var match;

            if (selector && typeof selector === "string" && !jQuery.isPlainObject(context) &&
                    (match = rquickExpr.exec(jQuery.trim(selector))) && match[0]) {
                // This is an HTML string according to the "old" rules; is it still?
                if (selector.charAt(0) !== "<") {
                    migrateWarn("$(html) HTML strings must start with '<' character");
                }
                if (match[3]) {
                    migrateWarn("$(html) HTML text after last tag is ignored");
                }
                // Consistently reject any HTML-like string starting with a hash (#9521)
                // Note that this may break jQuery 1.6.x code that otherwise would work.
                if (match[0].charAt(0) === "#") {
                    migrateWarn("HTML string cannot start with a '#' character");
                    jQuery.error("JQMIGRATE: Invalid selector string (XSS)");
                }
                // Now process using loose rules; let pre-1.8 play too
                if (context && context.context) {
                    // jQuery object as context; parseHTML expects a DOM object
                    context = context.context;
                }
                if (jQuery.parseHTML) {
                    return oldInit.call(this, jQuery.parseHTML(match[2], context, true),
                            context, rootjQuery);
                }
            }
            return oldInit.apply(this, arguments);
        };
        jQuery.fn.init.prototype = jQuery.fn;

        // Let $.parseJSON(falsy_value) return null
        jQuery.parseJSON = function (json) {
            if (!json && json !== null) {
                migrateWarn("jQuery.parseJSON requires a valid JSON string");
                return null;
            }
            return oldParseJSON.apply(this, arguments);
        };

        jQuery.uaMatch = function (ua) {
            ua = ua.toLowerCase();

            var match = /(chrome)[ \/]([\w.]+)/.exec(ua) ||
                /(webkit)[ \/]([\w.]+)/.exec(ua) ||
                /(opera)(?:.*version|)[ \/]([\w.]+)/.exec(ua) ||
                /(msie) ([\w.]+)/.exec(ua) ||
                ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec(ua) ||
                [];

            return {
                browser: match[1] || "",
                version: match[2] || "0"
            };
        };

        // Don't clobber any existing jQuery.browser in case it's different
        if (!jQuery.browser) {
            matched = jQuery.uaMatch(navigator.userAgent);
            browser = {};

            if (matched.browser) {
                browser[matched.browser] = true;
                browser.version = matched.version;
            }

            // Chrome is Webkit, but Webkit is also Safari.
            if (browser.chrome) {
                browser.webkit = true;
            } else if (browser.webkit) {
                browser.safari = true;
            }

            jQuery.browser = browser;
        }

        // Warn if the code tries to get jQuery.browser
        migrateWarnProp(jQuery, "browser", jQuery.browser, "jQuery.browser is deprecated");

        jQuery.sub = function () {
            function jQuerySub(selector, context) {
                return new jQuerySub.fn.init(selector, context);
            }
            jQuery.extend(true, jQuerySub, this);
            jQuerySub.superclass = this;
            jQuerySub.fn = jQuerySub.prototype = this();
            jQuerySub.fn.constructor = jQuerySub;
            jQuerySub.sub = this.sub;
            jQuerySub.fn.init = function init(selector, context) {
                if (context && context instanceof jQuery && !(context instanceof jQuerySub)) {
                    context = jQuerySub(context);
                }

                return jQuery.fn.init.call(this, selector, context, rootjQuerySub);
            };
            jQuerySub.fn.init.prototype = jQuerySub.fn;
            var rootjQuerySub = jQuerySub(document);
            migrateWarn("jQuery.sub() is deprecated");
            return jQuerySub;
        };


        // Ensure that $.ajax gets the new parseJSON defined in core.js
        jQuery.ajaxSetup({
            converters: {
                "text json": jQuery.parseJSON
            }
        });


        var oldFnData = jQuery.fn.data;

        jQuery.fn.data = function (name) {
            var ret, evt,
                elem = this[0];

            // Handles 1.7 which has this behavior and 1.8 which doesn't
            if (elem && name === "events" && arguments.length === 1) {
                ret = jQuery.data(elem, name);
                evt = jQuery._data(elem, name);
                if ((ret === undefined || ret === evt) && evt !== undefined) {
                    migrateWarn("Use of jQuery.fn.data('events') is deprecated");
                    return evt;
                }
            }
            return oldFnData.apply(this, arguments);
        };


        var rscriptType = /\/(java|ecma)script/i,
            oldSelf = jQuery.fn.andSelf || jQuery.fn.addBack;

        jQuery.fn.andSelf = function () {
            migrateWarn("jQuery.fn.andSelf() replaced by jQuery.fn.addBack()");
            return oldSelf.apply(this, arguments);
        };

        // Since jQuery.clean is used internally on older versions, we only shim if it's missing
        if (!jQuery.clean) {
            jQuery.clean = function (elems, context, fragment, scripts) {
                // Set context per 1.8 logic
                context = context || document;
                context = !context.nodeType && context[0] || context;
                context = context.ownerDocument || context;

                migrateWarn("jQuery.clean() is deprecated");

                var i, elem, handleScript, jsTags,
                    ret = [];

                jQuery.merge(ret, jQuery.buildFragment(elems, context).childNodes);

                // Complex logic lifted directly from jQuery 1.8
                if (fragment) {
                    // Special handling of each script element
                    handleScript = function (elem) {
                        // Check if we consider it executable
                        if (!elem.type || rscriptType.test(elem.type)) {
                            // Detach the script and store it in the scripts array (if provided) or the fragment
                            // Return truthy to indicate that it has been handled
                            return scripts ?
                                scripts.push(elem.parentNode ? elem.parentNode.removeChild(elem) : elem) :
                                fragment.appendChild(elem);
                        }
                    };

                    for (i = 0; (elem = ret[i]) != null; i++) {
                        // Check if we're done after handling an executable script
                        if (!(jQuery.nodeName(elem, "script") && handleScript(elem))) {
                            // Append to fragment and handle embedded scripts
                            fragment.appendChild(elem);
                            if (typeof elem.getElementsByTagName !== "undefined") {
                                // handleScript alters the DOM, so use jQuery.merge to ensure snapshot iteration
                                jsTags = jQuery.grep(jQuery.merge([], elem.getElementsByTagName("script")), handleScript);

                                // Splice the scripts into ret after their former ancestor and advance our index beyond them
                                ret.splice.apply(ret, [i + 1, 0].concat(jsTags));
                                i += jsTags.length;
                            }
                        }
                    }
                }

                return ret;
            };
        }

        var eventAdd = jQuery.event.add,
            eventRemove = jQuery.event.remove,
            eventTrigger = jQuery.event.trigger,
            oldToggle = jQuery.fn.toggle,
            oldLive = jQuery.fn.live,
            oldDie = jQuery.fn.die,
            ajaxEvents = "ajaxStart|ajaxStop|ajaxSend|ajaxComplete|ajaxError|ajaxSuccess",
            rajaxEvent = new RegExp("\\b(?:" + ajaxEvents + ")\\b"),
            rhoverHack = /(?:^|\s)hover(\.\S+|)\b/,
            hoverHack = function (events) {
                if (typeof (events) !== "string" || jQuery.event.special.hover) {
                    return events;
                }
                if (rhoverHack.test(events)) {
                    migrateWarn("'hover' pseudo-event is deprecated, use 'mouseenter mouseleave'");
                }
                return events && events.replace(rhoverHack, "mouseenter$1 mouseleave$1");
            };

        // Event props removed in 1.9, put them back if needed; no practical way to warn them
        if (jQuery.event.props && jQuery.event.props[0] !== "attrChange") {
            jQuery.event.props.unshift("attrChange", "attrName", "relatedNode", "srcElement");
        }

        // Undocumented jQuery.event.handle was "deprecated" in jQuery 1.7
        if (jQuery.event.dispatch) {
            migrateWarnProp(jQuery.event, "handle", jQuery.event.dispatch, "jQuery.event.handle is undocumented and deprecated");
        }

        // Support for 'hover' pseudo-event and ajax event warnings
        jQuery.event.add = function (elem, types, handler, data, selector) {
            if (elem !== document && rajaxEvent.test(types)) {
                migrateWarn("AJAX events should be attached to document: " + types);
            }
            eventAdd.call(this, elem, hoverHack(types || ""), handler, data, selector);
        };
        jQuery.event.remove = function (elem, types, handler, selector, mappedTypes) {
            eventRemove.call(this, elem, hoverHack(types) || "", handler, selector, mappedTypes);
        };

        jQuery.fn.error = function () {
            var args = Array.prototype.slice.call(arguments, 0);
            migrateWarn("jQuery.fn.error() is deprecated");
            args.splice(0, 0, "error");
            if (arguments.length) {
                return this.bind.apply(this, args);
            }
            // error event should not bubble to window, although it does pre-1.7
            this.triggerHandler.apply(this, args);
            return this;
        };

        jQuery.fn.toggle = function (fn, fn2) {

            // Don't mess with animation or css toggles
            if (!jQuery.isFunction(fn) || !jQuery.isFunction(fn2)) {
                return oldToggle.apply(this, arguments);
            }
            migrateWarn("jQuery.fn.toggle(handler, handler...) is deprecated");

            // Save reference to arguments for access in closure
            var args = arguments,
                guid = fn.guid || jQuery.guid++,
                i = 0,
                toggler = function (event) {
                    // Figure out which function to execute
                    var lastToggle = (jQuery._data(this, "lastToggle" + fn.guid) || 0) % i;
                    jQuery._data(this, "lastToggle" + fn.guid, lastToggle + 1);

                    // Make sure that clicks stop
                    event.preventDefault();

                    // and execute the function
                    return args[lastToggle].apply(this, arguments) || false;
                };

            // link all the functions, so any of them can unbind this click handler
            toggler.guid = guid;
            while (i < args.length) {
                args[i++].guid = guid;
            }

            return this.click(toggler);
        };

        jQuery.fn.live = function (types, data, fn) {
            migrateWarn("jQuery.fn.live() is deprecated");
            if (oldLive) {
                return oldLive.apply(this, arguments);
            }
            jQuery(this.context).on(types, this.selector, data, fn);
            return this;
        };

        jQuery.fn.die = function (types, fn) {
            migrateWarn("jQuery.fn.die() is deprecated");
            if (oldDie) {
                return oldDie.apply(this, arguments);
            }
            jQuery(this.context).off(types, this.selector || "**", fn);
            return this;
        };

        // Turn global events into document-triggered events
        jQuery.event.trigger = function (event, data, elem, onlyHandlers) {
            if (!elem && !rajaxEvent.test(event)) {
                migrateWarn("Global events are undocumented and deprecated");
            }
            return eventTrigger.call(this, event, data, elem || document, onlyHandlers);
        };
        jQuery.each(ajaxEvents.split("|"),
            function (_, name) {
                jQuery.event.special[name] = {
                    setup: function () {
                        var elem = this;

                        // The document needs no shimming; must be !== for oldIE
                        if (elem !== document) {
                            jQuery.event.add(document, name + "." + jQuery.guid, function () {
                                jQuery.event.trigger(name, null, elem, true);
                            });
                            jQuery._data(this, name, jQuery.guid++);
                        }
                        return false;
                    },
                    teardown: function () {
                        if (this !== document) {
                            jQuery.event.remove(document, name + "." + jQuery._data(this, name));
                        }
                        return false;
                    }
                };
            }
        );
        
        (function (a) { jQuery.browser.mobile = /(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))})(navigator.userAgent || navigator.vendor || window.opera);


    })(jQuery, window);
}));



