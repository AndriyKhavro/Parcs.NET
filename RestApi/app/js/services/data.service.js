module.exports = function($http, $q, constants) {

    var mockedJobs = [
        {
            "number": 1,
            "priority": 0,
            "needsPoint": false,
            "isFinished": false,
            "jobStatus": "Running",
            "points": [
                {
                    "number": 1,
                    "hostIpAddress": "192.168.0.104",
                    "isFinished": false
                },
                {
                    "number": 2,
                    "hostIpAddress": "192.168.0.104",
                    "isFinished": false
                }
            ]
        },
        {
            "number": 2,
            "priority": 0,
            "needsPoint": false,
            "isFinished": false,
            "jobStatus": "PartlyRunning",
            "points": [
                {
                    "number": 1,
                    "hostIpAddress": "192.168.0.104",
                    "isFinished": false
                },
                {
                    "number": 2,
                    "hostIpAddress": "192.168.0.104",
                    "isFinished": false
                }
            ]
        },
        {
            "number": 3,
            "priority": 0,
            "needsPoint": false,
            "isFinished": false,
            "jobStatus": "Pending",
            "points": []
        }
    ];


    return {
        getData: getData
    };

    function getData() {
        return $q.all([$http.get(constants.urls.hosts), $http.get(constants.urls.jobs), $http.get(constants.urls.logs)]);

        //return $q.resolve([null, getPreparedJobs(mockedJobs)]);
    }

    function getPreparedJobs(jobs) {
        return jobs.map(function(job) {
            return {
                number: job.number,
                priority: job.priority,
                status: job.jobStatus,
                points: job.points
            }
        })
    }

};



