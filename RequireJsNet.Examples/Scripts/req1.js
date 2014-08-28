define('req1', [], function() {
    console.log('req1');
    require(["req1"]);
});