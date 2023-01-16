# Jobby - A Job Runner

Do you often find yourself running some random SQL Script, or Powershell Command regularly to maintain some process? Are your fingers the lynchpin which holds together a critical moment in your business?

Jobby solves a simple problem; Do something every so often for me. Sometimes that might be sending an email. Perhaps running a folder clean up script. Maybe running a statistical query on your DB. 

Why setup a Windows Scheduled Task, SQL Job, and whatever else is out there when you could do it in one program? 

TODO:
- Define interface for jobs
- Test best method for running multiple jobs (ThreadPool vs. Tasks)
- Build TSQLJob and PowershellJob modules

Probably need to reorganize the structure of the code; Right now, it feels a bit like a mishmosh of folders, but we should stick the standard; Service Layer/Model/Data Access

Mental Map of the code so far

```
JobbyJobRunner
    |_JobbyJobQueue
        |_BackingTaskQueue
        |_BackingResultsQueue
```

There are many job Runner types; That is where the distinction occurs. JobbySqlJobRunner, and JobbyTerminalJobRunner for instance.

Maybe I should also include some kind of JobbyJobOrchestrator? Which can run all kinds of jobby jobs? This is probably a good idea because I think I want the end-user to really not focus on *running*, more job creation.

Also, why not I not start with Terminal jobs, I can't test SQL Jobs on this machine...lol