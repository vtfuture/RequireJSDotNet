define(['req1', "jquery"], function(req1, $) {
    $(function () {
        alert('jquery loaded');

        require(["cart", "store", "store/util"], function (cart, store, util) {

            alert('commonJS packages cart, store and util has been loaded');

        });

    });
});