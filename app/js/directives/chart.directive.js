var Chart = require('./../models/chart.model');
function ChartDirective (chartService) {
    return {
        restrict: 'E',
        template: '<div class="analytics-chart"></div>',
        replace: true,
        scope: {
            title: '@',
            data: '='
        },
        link: function($scope) {
            console.log($scope);
        },
        controller: function($scope, $element) {
            var vm = this;

            $scope.$watch('vm.data.response', function() {
               var pointValue = chartService.getChartData(vm.title);
               vm.chart.addPoint(pointValue);
            });

            vm.chart = new Chart(vm.title);
            vm.chart.draw($element[0]);

        },
        bindToController: true,
        controllerAs: 'vm'
    };
}

 module.exports = ChartDirective;