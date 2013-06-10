/*
 * Keep Alive Module RequireJS for .NET
 * Version 1.0.0.1
 * Release Date 06/09/0212
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

define('keepalive-service', [
    'jquery',
    'amplify'
], function () {
    
    //#region private variables
    var self;
    var state = {
        running: 'running',
        stopped: 'stopped',
        paused: 'paused'
    };
    var status = state.stopped;
    //#endregion
    
    //#region private functions
    var init = function() {

        //register ping decoder
        amplify.request.decoders.pingEnvelope = function(data, status, xhr, success, error) {
            if (data && data.Status) {
                switch (data.Status) {
                case 1: //success
                    success(data);
                    break;
                case 2: //server error
                case 4: //invalid data
                    error(data.Error, data.Status);
                    break;
                case 3: //unauthorized
                case 5: //redirect required
                    window.location = data.RedirectUrl;
                    break;
                }
            } else {
                error('network failure', -1);
            }
        };

        //register ajax request
        amplify.request.define("ping", "ajax", {
            url: self.options.pingUrl,
            type: "GET",
            decoder: "pingEnvelope"
        });
        
        $.ajaxSetup({
            async: true,
            //increase timeout for long-polling
            timeout: 1000*60
        });
    };
    
    var log = function (info) {
        if (!self.options.noLog && typeof console != undefined) {
            console.log(info);
        }
    };

    var logStatus = function () {
        log('Keep Alive service is ' + status);
    };
    //#endregion
    
    //#region constructor
    var Service = function (opt) {
        
        //default options
        if (opt == undefined) {
            opt = {
                autoStart: true,
                noLog: false,
                interval: 3000,
                maxRetries: 5,
                pauseAfterReceive: true,
                pingUrl: '/Keepalive/PingAsync'
            };
        }

        //context for private methods
        self = this;

        //public variables
        this.options = $.extend(true, {}, opt);
        this.retries = 0;
        
        //initialization
        init();

        //auto-start on DOM ready
        if (this.options.autoStart === true) {
            $(document).ready($.proxy(this.start, this));
        }

    };
    //#endregion
    
    //#region public functions
    Service.prototype = $.extend(true, Service.prototype, {

        ping: function () {
            
            if (status == state.running) {
                amplify.request({
                    resourceId: "ping",
                    success: $.proxy(function(envelope) {

                        if(envelope.Messages < 1) {
                            log('No new messages');
                        }

                        if (envelope.Messages > 0) {
                            log('New messages ' + envelope.Messages);

                            //let notified module to decide when to resume the service
                            if (this.options.pauseAfterReceive) {
                                this.pause();
                            }

                            //broadcasting server messages to other modules
                            amplify.publish('Ping|DataReceived', {
                                data: envelope.Messages
                            });
                        }

                        //log server warning if any
                        if (envelope.Error.length > 0) {
                            log(envelope.Error);
                        }
                        
                        //recurse
                        if (!this.options.pauseAfterReceive || envelope.Messages < 1) {
                            this.start();
                        }
                        
                    }, this),
                    error: $.proxy(function(error, statusCode) {

                        log('Keep Alive service error: ' + error + ' code: ' + statusCode);
                        
                        //increment retries
                        if (++this.retries < this.options.maxRetries) {
                            //pause for 3 sec
                            this.pause(this.options.interval);
                        } else {
                            //stop if max retries reached
                            this.stop();
                        }
                        
                    }, this)
                });
            }
        },
        
        start: function () {
            //change service status
            if (status != state.running) {
                status = state.running;
                logStatus();
            }
            
            //launch request after 3 sec
            setTimeout(
                $.proxy(this.ping, this),
                this.options.interval
            );
        },

        stop: function () {
            //change service status
            status = state.stopped;
            logStatus();
        },

        pause: function (timeout) {
            //change service status
            status = state.paused;
            logStatus();

            //restart service after timeout expires
            if (timeout) {
                setTimeout(
                    $.proxy(this.start, this),
                    timeout
                );
            }
        },

        getStatus: function () {
            return status;
        }
    });
    //#endregion
    
    return Service;
});

/* 
    Expected address on server
    /Keepalive/Ping
    /Keepalive/PingAsync

    Expected json respons from server
    {
        Status = <int>,
        Messages = <int>,
        Error = <string>,
        RedirectUrl = <url>
    }
*/