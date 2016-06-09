'use strict';
aboutController.$inject = ['$scope'];

function aboutController ($scope) {

    $scope.surprise = "";

    $scope.onSurpriseMouseOver = function() {
        $scope.surprise = "app/resources/surprise.png"
    };

    $scope.onSurpriseMouseLeave = function() {
        $scope.surprise = "";
    };
}

module.exports = aboutController;
