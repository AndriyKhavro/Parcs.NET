import { Component, OnInit } from 'angular2/core';
import { RouterLink } from 'angular2/router';
import { HTTP_PROVIDERS }    from 'angular2/http';
import { JobInfoService }       from './jobInfo.service';
import { JobInfo } from './jobInfo'

@Component({
    selector: 'job-info',
    templateUrl: 'app/jobInfo/jobInfo.component.html',
    directives: [ RouterLink ],
    providers: [HTTP_PROVIDERS, JobInfoService]
})
export class JobInfoComponent implements OnInit {

    constructor(private _jobInfoService: JobInfoService) { }

    public jobs:JobInfo[];

    ngOnInit() { 
        this.getJobInfos();
    }
    
    getJobInfos() {
        this._jobInfoService.get()
            .subscribe(jobs => this.jobs = jobs);
    }

}