function MainController($scope, $interval, constants) {

    $scope.charts = [{
        title: constants.chartTitles.processors
    }, {
        title: constants.chartTitles.benchmark
    }];
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