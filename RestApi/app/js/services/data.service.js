module.exports = function($http, $q, constants) {

    return {
        getData: getData,
		cancelJob: cancelJob,
		startJob: startJob,
        getAvailableModules: getAvailableModules,
        saveAvailableModules: saveAvailableModules
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

    function cancelJob(job) {
        return $http.post(constants.urls.cancelJob, {number: job.number});
    }

    function startJob(options) {
        return $http.post(constants.urls.startJob, options);
    }
    
    
    var availableModules;
    
    function getAvailableModules() {
        return availableModules;
    }
    
    function saveAvailableModules() {
        return $http.get(constants.urls.modules).then(function(response){
           availableModules = response.data;
           return availableModules; 
        });
    }
};



