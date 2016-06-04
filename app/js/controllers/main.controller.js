function MainController($scope, $interval, constants) {

    $scope.charts = [constants.charts.processors, constants.charts.benchmark];
    $scope.title = "chart1";
    $scope.chartsData = {
        response: []
    };

    $interval(function() {
        //get data from server
        $scope.chartsData.response = [];
    }, 1000);
}

module.exports = MainController;