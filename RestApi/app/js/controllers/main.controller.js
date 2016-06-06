function MainController($scope, $interval, constants, dataService) {

    $scope.charts = [constants.charts.processors, constants.charts.benchmark];
    $scope.data = {
        hosts: [],
        jobs: [],
        chartsData: []
    };

    $scope.jobStatuses = constants.jobStatuses;

    $interval(function() {
        dataService.getData().then(function(response) {
            $scope.data.hosts = response[0];
            angular.merge($scope.data.jobs, response[1]);
            $scope.data.chartsData = angular.copy($scope.data.jobs);
        });
    }, 1000);
}

module.exports = MainController;