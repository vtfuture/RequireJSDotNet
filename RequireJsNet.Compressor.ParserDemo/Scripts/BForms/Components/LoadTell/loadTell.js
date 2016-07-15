(function () {

    var loadTell = function (options) {

        this.options = this._extend(this.options, options);

        if (this.options.loggerUrl == null && this.options.preventRemoveLogger != true && window.requireConfig.websiteOptions != null && window.requireConfig.websiteOptions.loadLoggerUrl != null) {
            this.options.loggerUrl = window.requireConfig.websiteOptions.loadLoggerUrl;
        }

        this.init();
    };

    loadTell.prototype.options = {
        retryAfter: 5000
    };

    loadTell.prototype._timeouts = [];

    loadTell.prototype.init = function () {
        this._addEvents();
    };

    loadTell.prototype._addEvents = function () {
        window.addEventListener('load', function () {
            window.setTimeout(this._onLoad.bind(this), 50);
        }.bind(this));
    };

    loadTell.prototype._onLoad = function () {

        var perfTime = this._getPerformance();

        for (var key in perfTime) {
            perfTime[key] = JSON.stringify(perfTime[key]);
        }

        if (window.requireConfig != null && window.requireConfig.websiteOptions !== null && window.requireConfig.websiteOptions.requestKey != null) {
            perfTime["RequestKey"] = window.requireConfig.websiteOptions.requestKey;
        }

        if (window.requireConfig != null && window.requireConfig.websiteOptions !== null && window.requireConfig.websiteOptions.requestStartTime != null) {
            perfTime["RequestStartTime"] = window.requireConfig.websiteOptions.requestStartTime;
        }

        this._send(perfTime);
    };

    //#region xhr
    loadTell.prototype._send = function (perfTime) {

        if (this.options.loggerUrl) {
            try {

                var data = JSON.stringify(this._serializeJsonData(perfTime));

                this._makeXhr(data);

            } catch (ex) {
            }
        }

    };

    loadTell.prototype._makeXhr = function (formattedData, guid) {

        try {

            guid = guid != null ? guid : this._guid();

            var xhr = new XMLHttpRequest();

            xhr.open('POST', this.options.loggerUrl, true);
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.setRequestHeader("Content-Type", 'application/json; charset=utf-8');
            xhr.timeout = this.options.retryAfter;
            xhr.send(formattedData);

            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.aborted !== true && xhr.status === 200) {
                        if (this._timeouts[guid] != null) {
                            window.clearTimeout(this._timeouts[guid]);
                        }
                    } else {
                        this._timeouts[guid] = window.setTimeout(function () {
                            this._makeXhr(formattedData, guid);
                        }.bind(this), xhr.timeout);
                    }
                }
            }.bind(this);

        } catch (ex) {
            //capture exception
        }
    }
    //#endregion

    //#region performance
    loadTell.prototype._getPerformance = function() {
        return {
            navigation: this._getNavigation(),
            memory: this._getMemory(),
            timing: this._getTiming()
        };
    };

    loadTell.prototype._getNavigation = function() {
        if (window.performance.navigation == null) return null;

        return {
            redirectCount: window.performance.navigation.redirectCount,
            type: window.performance.navigation.type
        };
    };

    loadTell.prototype._getMemory = function () {
        if (window.performance.memory == null) return null;

        return {
            jsHeapSizeLimit: window.performance.memory.jsHeapSizeLimit,
            totalJSHeapSize: window.performance.memory.totalJSHeapSize,
            usedJSHeapSize: window.performance.memory.usedJSHeapSize
        };
    };

    loadTell.prototype._getTiming = function() {
        if (window.performance.timing == null) return null;

        return {
            connectEnd: window.performance.timing.connectEnd,
            connectStart: window.performance.timing.connectStart,
            domComplete: window.performance.timing.domComplete,
            domContentLoadedEventEnd: window.performance.timing.domContentLoadedEventEnd,
            domContentLoadedEventStart: window.performance.timing.domContentLoadedEventStart,
            domInteractive: window.performance.timing.domInteractive,
            domLoading: window.performance.timing.domLoading,
            domainLookupEnd: window.performance.timing.domainLookupEnd,
            domainLookupStart: window.performance.timing.domainLookupStart,
            fetchStart: window.performance.timing.fetchStart,
            loadEventEnd: window.performance.timing.loadEventEnd,
            loadEventStart: window.performance.timing.loadEventStart,
            navigationStart: window.performance.timing.navigationStart,
            redirectEnd: window.performance.timing.redirectEnd,
            redirectStart: window.performance.timing.redirectStart,
            requestStart: window.performance.timing.requestStart,
            responseEnd: window.performance.timing.responseEnd,
            responseStart: window.performance.timing.responseStart,
            secureConnectionStart: window.performance.timing.secureConnectionStartv,
            unloadEventEnd: window.performance.timing.unloadEventEnd,
            unloadEventStart: window.performance.timing.unloadEventStart
        };
    };
    //#endregion

    //#region helpers
    loadTell.prototype._guid = function () {
        var g = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });

        return g.replace(/-/g, '');
    };

    loadTell.prototype._extend = function () {
        var extendedObject = {};

        for (var i = 0; i < arguments.length; i++)
            for (var key in arguments[i])
                if (arguments[i].hasOwnProperty(key))
                    extendedObject[key] = arguments[i][key];

        return extendedObject;
    };

    loadTell.prototype._serializeJsonData = function (data) {

        var xhrData;

        if (data instanceof Array) {
            xhrData = data.slice(0);
        } else {
            xhrData = this._extend({}, data);
        }

        for (var key in xhrData) {
            if (xhrData.hasOwnProperty(key)) {

                if (typeof xhrData[key] === "function") {
                    delete xhrData[key];
                } else {

                    if (typeof xhrData[key] === "number") {
                        xhrData[key] += "";
                    } else {
                        if (typeof xhrData[key] === "object") {
                            xhrData[key] = this._serializeJsonData(xhrData[key]);
                        }
                    }
                }
            }
        }

        return xhrData;
    };

    loadTell.prototype._expandObject = function (result, obj) {

        for (var prop in obj) {
            if (obj[prop] != null) {
                if (typeof window.performance[prop] !== 'function') {
                    if (typeof obj[prop] === "object") {
                        var temp = {};
                        this._expandObject(temp, obj[prop]);
                        result[prop] = temp;
                    } else {
                        result[prop] = obj[prop];
                    }
                }
            }
        }
    };
    //#endregion

    window.loadTell = new loadTell();
})();