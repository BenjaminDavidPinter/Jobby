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

Implemented scheduled jobs. Learned about a new object type; ```TimeOnly```, which only represents a time of day, and nothing else. Super useful for exactly this. Most of the implementation was spent trying to wrap my head around doing time math. 

The next thing I need to implement is queue clean up. Right now, if you let a task run 1000 times, 1000 empty tasks live in the task queue. We need some way to clean up completed tasks.

Some options I'm considering;
1. Child thread which observes the state of all queues, and cleans them up (expensive, but easy to implement);
2. Somehow, have tasks remove themselves from the queue once they are completed (more complicated, probably better).

The problem with #2 is forcing a task to find itself within the backing queue. All jobs just live within their parent queue. I need some kind of mechanism to ID the tasks when they get added. Like IJobbyJobM<T> needs some kind of behavoir on construction to ID itself. But the ideal way to do *that* is probably implement IJobbyJob<T> from JobbyJob<T>, and perform a default constructor. I'm sure C# interfaces have some horrid 2023-esque way to do this, but IDK if it would be ideal.

### 1-22-23
I implemented queue clean up, but it might be a bit overkill; Right now, before any task waits it takes time to review and delete and completed tasks from the queue. I'm not sure how this is performance, wise, but with testing, it works.

Moved requeue to continuation task.

Addex exception job queue.

### 1-23-23
Let's see if it's possible for me to add an option to spin a given thread up twice. 

That was almost a little too easy. Since the 'requeue' function is split from the method which queues the initial job
group, I was able to add code which simply adds N jobs to the queue on first boot, and let them requeue themselves independenly.

Next I think I need to work on failures; What should happen when a job fails? Should it try again after the designated amount of time? Should I provide a mechanism to support 'on failure' 'on success' etc, calls? 

I know that I could potentially mandate at the interface level, some kind of ```Func<T>``` which the JobRunner could use in continuation clauses.

I'm imagining something like this;
```csharp
public enum StandardTaskOperation {
    NoOp = 0,
    RequeueStandard = 1,
    RequeueExtendedWaitTime = 2,
    CancelFutureTasks = 3
}

public void OnSuccess(Func<T> handler, StandardTaskOperation operation = 0) => {
    //Do what you want on success
}

/*
Meanwhile...in JobbyJobRunner
*/
Task jobbyTask = New Task(() => ...);
Task.ContinueWith(job.OnSuccess, TaskContinuationOptions.RanToCompletion);
```

Basically just let the developer dictatate what happens when the task is completed, with some standard options available.

### 1-24-2023

While roughing in the implementation for continuation tasks, I wanted to clean up the implementation layer.

I'm noticing a trend; As an IJobbyJob<T> gains more features, the list of interface fields grows. Naturally, interfaces force concrete classes to implement the the getters on those properties. 

I've never been a huge fan of interface implementation, but in this instance, it makes sense. Hide the lesser-used properties by providing a default implementation. For example, ConcurrentThreads. Not all jobs may be interested in running on two threads, so let's just set that value to 1 by default.

This all leads to the way the continuation tasks are implemented;
```csharp
public List<(Func<string>, TaskContinuationOptions)> Continuations => new();
```

The idea being that, should users want to, they can implement some kind of behavoir on completion, otherwise, they don't even need to specify the property value.

### 1-25-23

I should do some research into whether this line is actually what it implies;

```csharp
public int ConcurrentThreads => 2;
```
When I start two instances of tasks for this object, is it truly a 'thread'? This is something I need to look into before I call it finished.

### 1-28-23

I wanted to come back to flesh out some features, and perform refactor work. Addressing the previous note; Right now, we have this notion of 'start date' and 'end date', but what if we wanted this to run according to some other metric. Perhaps; When a stored procedure runs data, or when some other task is completed?

Exposing a func<bool> would offer greater control over when tasks would run, and would probably allow me to remove the start time and end time members of IJobbyJob.

Also, wow do I need better tests. We should bring Moq into the test framework and start to work out some actual tests.

### 1-30-23

Sitting in the hospital this morning, waiting for my Popop to have some outpatient work done. Thinking about what the next best steps are for jobby. 

I think it's time for a refactor; Namely, some members and properties are exposed which I had no initially expected to be, and they sit with ugly underscores before their name, and they have 'internal' appended to them. So let's spend the time to fix all those up first.

### 1-31-23

Last night, I spent time dealing with compiler warnings.
Namely some warnings for deref'ing null pointers, and there's still more to be done.
It certainly brought up some of my weaknesses in C#. I *can* ensure that we don't get null reference
exceptions, but there's so many ways to do it, that I can't be sure which is optimal.

I learned about some crazy new syntax last night;
```csharp
if (Activator.CreateInstance(job) is not IJobbyJob<T> instance) throw new Exception("Error while creating job queues");
```

Which I assume helps reduce the chance of a null reference being thrown. I think key is here,
I need to spend more time learning about null coalesce, and ways to deal with null objects in the future.

### 9-24-23
Back at it.

Enforced that jobs extend a particular class ```JobbyJobResult```, in order to include additional metadata about
the job when it's running. The first of which is benchmarks.

## Additional Features List
- ~~Clean up task queue when a task is completed.~~
- ~~Some kind of way to elegantly fail a job.~~
- Ability to cancel a job after N failures
- ~~Specify number of threads on a job~~
- Event Driven Task Continuations
    - Expose methods through IJobbyJob<T> which allow the user to control what happens when a jobby job fails/succeeds.
        - These methods should have some standard options for;'RequeueJob', 'Wait(X Mills)' etc.
- ~Add custom run conditions; ```Func<bool>``` which users can implement to stop their jobs from running accoding to some custom condition.~
- Pass a CancellationToken to IJobbyJob<T> Run() method
- ~~Benchmark Jobs~~
- ~~Disambiguate Jobs~~