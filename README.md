# Jobby - A Job Runner
![WorkflowBadge](https://github.com/BenjaminDavidPinter/Jobby/actions/workflows/dotnet.yml/badge.svg)

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

### 1-17-2023 :
I think at this point, I have the entire 'queueing' process in place. I can implement and IJobbyJob<ResultT> type and the runner should pick it up,
initialize the queue, and begin to store all types directly in the processing queue. There's still two peices which are critical to the success of the project;
1. I need to requeue jobs after they finish, and I need to place the results of a given job in its results queue.

### 1-18-2023 :
I started writing some unit tests today, to get a feel for the ergonomics of the system. I have to admit, I was unhappy with how it all fit together. There were some inconsistencies for how the jobs were meant to run. I refactored IJobbyJob<T> so that the job definition was held on the class, and not passed into the runner itself. The way it was setup before, the runner was constrained to one job type, which is not the intended scenario. It was meant to run jobs of similar type, but not function.

However, after setting up a one hour focus timer, I was able to refactor everything, and get two sets of simple tests up; General DI tests to ensure the platform works with DI, and one string job which inserts a single string result into the results queue.

### 1-21-2023 :
Today we're going to try and implement requeue with continuation tasks. *Update*; Once again I find myself completely baffled by continuation tasks. I thought I could, on successful task completion, queue up another task recursively. However, the line I introduced;

```csharp
//Task created above

task.ContinueWith(() => queueAnotherTask());
```

Caused several hundred thousand tasks to get queued. Strange...I'm not sure why. It's something I will look into in the future, because I strongly believe that it is the ideal way to implement "When this task is done, requeue it". But I'm just not sure exactly how to get that to work yet.

The next feature I want to implement will be scheduling. Right now, you can queue a task, and have it run every so often, but another critical feature will be the ability to say 'I need this to only run between 9am and 5pm'. I originally wanted to put this information in attributes on IJobbyJob<T>, but I cannot enforce the implementation of that. Instead, I'll probably make it another attribute on the interface. I want to avoid scenarios where someone has to 'search' for a way to do something. So enforcing it at the interface level is a good way to bring visibility to the feature.