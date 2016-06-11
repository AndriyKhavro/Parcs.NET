'use strict';
AddJobModalController.$inject = ['$scope', 'constants', '$uibModalInstance', 'dataService'];

function AddJobModalController ($scope, constants, $uibModalInstance, dataService) {

    $scope.jobs = constants.jobs;
    $scope.selectedJob = $scope.jobs[0];
    $scope.jobOptions = {
        priority: 0,
        pointCount: 1,
        matrixSize: 2000
    };

    $scope.startJob = function() {
        dataService.startJob($scope.jobOptions).then(function() {
            $uibModalInstance.close('ok');
        });
    };

    $scope.cancel = function() {
        $uibModalInstance.dismiss('cancel');
    };
}

module.exports = AddJobModalController;
