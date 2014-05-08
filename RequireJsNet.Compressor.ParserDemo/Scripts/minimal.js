(function (factory){
    if (true) {
        require(['a', 'b'], function () {
            require('');
        });
    }

    define(['d'], factory);

}(function () {
    var c = 5;
    require('jay queery');
}))