module.exports = function(constants) {

    var chartDataMap = {};
    chartDataMap[constants.charts.processors.title] = getProcessorsPerformanceChartValue;
    chartDataMap[constants.charts.benchmark.title] = getBenchmarkPerformanceChartValue;

    return {
        getChartData: getChartData
    };

    function getChartData(chartTitle, response) {
        return chartDataMap[chartTitle](response);
    }

    function getProcessorsPerformanceChartValue(response) {
       return Math.random() * 10;
    }

    function getBenchmarkPerformanceChartValue(response) {
        return Math.random() * 100;
    }
};



