var Chart = require('./../models/chart.model');
function ChartDirective () {
    return {
        restrict: 'E',
        template: '<div class="analytics-chart"></div>',
        replace: true,
        scope: {
            title: '@',
            point: '='
        },
        link: function($scope) {
            console.log($scope);
        },
        controller: function($scope, $element) {
            var vm = this;

            $scope.$watch('vm.point.value', function() {
               console.log(this);
               vm.chart.addPoint(vm.point.value);
            });

            vm.chart = new Chart(vm.title);
            vm.chart.draw($element[0]);

        },
        bindToController: true,
        controllerAs: 'vm'
    };
}

 module.exports = ChartDirective;