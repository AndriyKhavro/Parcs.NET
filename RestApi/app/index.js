'use strict';

var angular = require('angular');

angular.module('parcs', []);

// one require statement per sub directory instead of one per file

require('./js/controllers');
require('./js/services');
require('./js/directives');