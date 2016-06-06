'use strict';

var angular = require('angular');
require('angular-ui-bootstrap');

angular.module('parcs', ['ui.bootstrap']);

// one require statement per sub directory instead of one per file

require('./js/controllers');
require('./js/services');
require('./js/directives');