
#### This is a fork of the RequireJsDotNet Project by Veritech:

Link to the original repository: https://github.com/vtfuture/RequireJSDotNet

Main objectives for this fork are to integrate with Typescript's external module syntax, 
to allow full use of the functionality of requirejs and to implement and effective cache-busting mechanism. 

##### The following changes have been made to the compressor

* The YUI Compressor has been replaced by the ASP.Net Web Optimization Framework 
in order to achieve a better MVC integration.

* A BundlePathResolver collection has been added similar to the EntryPointResolver collection 
to enable an override of the bundle paths.
This can be used for a cache-busting mechanism based on the bundle's content in the style of the web optimization framework.

* The simple bundle processor and the ability to exclude files have been removed to simplify things, it may be added again later. Currently only auto bundles are supported.

* Several bugfixes and minor changes have been made to the auto bundling algorithm, allowing the use of CDN files, dynamic require calls and eliminating case transformations of require module paths. 

##### The following changes have been made to the RequireJs HTML Helper

* The helper has been enhanced to allow global require calls. This may come in handy in a multi page application where 
tracking scripts, layout functions, error handlers etc. have to be loaded on every page.


See the RequireJsNet.Examples project for a use scenario of these features.
