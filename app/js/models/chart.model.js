var Highcharts = require('highcharts');
var noData = require('highcharts/modules/no-data-to-display');
noData(Highcharts);

Highcharts.setOptions({
    global: {
        useUTC: false
    },
    lang: {
        noData: 'Waiting for data'
    }
});

var chartOptions = {
    chart: {
        type: 'spline',
        animation: Highcharts.svg,
        marginRight: 10,
        zoomType: 'x'
    },
    title: {
        text: null
    },
    xAxis: {
        type: 'datetime',
        tickPixelInterval: 150
    },
    yAxis: {
        title: {
            text: null
        }
    },
    tooltip: {
        useHtml: true,
        formatter: function () {
            return '<b>' + this.series.name + '</b><br/>' +
                Highcharts.dateFormat('%Y-%m-%d %H:%M:%S', this.x) + '<br/>' +
                Highcharts.numberFormat(this.y, 2);
        }
    },
    legend: {
        enabled: false
    },
    exporting: {
        enabled: false
    },
    credits: {
        enabled: false
    },
    series: [{
        data: []
    }]
};

function Chart(title) {
    this.setOptions(title);
}

Chart.prototype.draw = function(element) {
    this.chart = Highcharts.chart(element, this.options);
};

Chart.prototype.setOptions = function(title) {
    chartOptions.title.text = title;
    this.options = chartOptions;
};

Chart.prototype.addPoint = function(value) {
    var series = this.chart.series[0];
    series.addPoint([new Date().getTime(), value], true, series.points.length > 30); //move to constants

};

module.exports = Chart;