function MainController($scope, $timeout, constants, dataService, authService) {

    $scope.charts = [constants.charts.processors, constants.charts.benchmark];
    $scope.data = {
        hosts: [],
        jobs: [],
        logs: [],
        chartsData: []
    };

    $scope.jobStatuses = constants.jobStatuses;

    (function getDataFromServer() {
        dataService.getData().then(function(response) {
            angular.merge($scope.data.hosts, response[0].data);
            angular.merge($scope.data.jobs, response[1].data);
            $scope.data.logs = response[2].data;
            $scope.data.chartsData = angular.copy($scope.data.hosts);
        });

        $timeout(getDataFromServer, constants.serverQueryTimeout);
    })();

    $scope.cancelJob = function(job) {
        dataService.cancelJob(job).then(function() {

        });
    };
    
    $scope.isAuthenticated = function() { return authService.authentication.isAuth; }
}

module.exports = MainController;