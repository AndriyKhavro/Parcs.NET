function MainController($scope, $timeout, constants, dataService, authService, $uibModal) {

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

    $scope.cancelJob = function(job, $event) {
        $event.preventDefault();
        $event.stopPropagation();
        dataService.cancelJob(job).then(function() {

        });
    };

    $scope.addJob = function() {
        var modalInstance = $uibModal.open({
            animation: true,
            templateUrl: 'app/views/addJobModal.html',
            controller: 'addJobModalController',
            controllerAs: 'modal',
            size: 'sm',
            backdrop: 'static'
        });
    };
    
    $scope.isAuthenticated = function() { return authService.authentication.isAuth; }
}

module.exports = MainController;