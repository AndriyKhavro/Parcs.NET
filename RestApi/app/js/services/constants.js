module.exports = function() {
    return {
        charts: {
            processors: {
                title: 'Performance by Processors',
                color: '#28ABE3'
            },
            benchmark: {
                title: 'Performance by Benchmark',
                color: '#1FDA9A'
            }
        },

        urls: {
            jobs: '/api/parcs/job',
            hosts: '/api/parcs/host/list',
            logs: '/api/log',
            cancelJob: '/api/parcs/cancelJob',
            startJob: 'api/module/matrix'
        },

        jobStatuses: {
            running: 'Running',
            partlyRunning: 'PartlyRunning',
            pending: 'Pending',
            finished: 'Finished',
            cancelled: 'Cancelled'
        },

        jobs: [{
            name: "Matrix"
        }, {
            name: "Knapsack"
        }],

        serverQueryTimeout: 3000
    }
};