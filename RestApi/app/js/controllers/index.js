'use strict';
var app = require('angular').module('parcs');

app.controller('MainController', require('./main.controller'));
app.controller('loginController', require('./login.controller'));
app.controller('signupController', require('./signup.controller'));