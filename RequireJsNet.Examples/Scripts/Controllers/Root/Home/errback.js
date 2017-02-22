require(['errback-file'], function (config) {

    alert("home/errback loaded config: " + config.mystring);

}, function (err) {
    //The errback, error callback. The error has a list of modules that failed

    var failedId = err.requireModules && err.requireModules[0];
    alert('Establishing a fallback location for "' + failedId + '".');

    if (failedId === 'errback-file') {

        requirejs.undef(failedId);
        requirejs.config({ paths: { 'errback-file': 'errback-local' } });

        require(['errback-file'], function () {
            alert('errback-local successfully required in place of errback-file.');
        });

    } else {
        //Some other error. Maybe show message to the user.
        alert('Errback called for unexpected module. FAILED!');
    }

});
