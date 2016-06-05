'use strict';

var angular = require('angular');
require('./node_modules/angular-local-storage/src/angular-local-storage.js');
require('./node_modules/angular-ui-router/release/angular-ui-router.min.js');

var app = angular.module('parcs', ['LocalStorageModule', 'ui.router']);

// one require statement per sub directory instead of one per file

require('./js/controllers');
require('./js/services');
require('./js/directives');


app.config(require('./js/authConfig'));
app.config(require('./js/routeConfig'));
app.run(require('./js/appRun'));
