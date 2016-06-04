function MainController($scope, $interval) {

    $scope.title = "chart1";
    $scope.point = {
        value: 2
    };

    $interval(function() {
        $scope.point.value = Math.random() * 10;
    }, 1000);
}

module.exports = MainController;