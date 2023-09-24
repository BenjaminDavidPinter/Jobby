using Jobby.lib.Core.JobTypes;
using Jobby.lib.Core.Model;
using Jobby.Lib.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Jobby.tests;
public class SimpleStringJob
{
    private ServiceCollection? ServCollection { get; set; }
    private ServiceProvider? ServProvider { get; set; }

    [SetUp]
    public void Setup()
    {
        ServCollection = new();
        ServCollection.AddScoped(typeof(IJobbyJobRunner<>), typeof(JobbyJobRunner<>));
        ServCollection.AddScoped(typeof(IJobbyJobQueue<>), typeof(JobbyJobQueue<>));

        ServProvider = ServCollection.BuildServiceProvider();
    }

    /*
    These tests are simply to confirm that, upon registration, the job queues are properly initialized.
    */
    #region Simple Job Setup Tests
    [Test]
    public void TestJobInit()
    {
        string queueName = "Simple String Job";
        Console.WriteLine("TestJobInit:");
        var testRunner = ServProvider.GetService<IJobbyJobRunner<SimpleStringJobResult>>();
        testRunner.StartJobs();
        var queueInit = testRunner.GetJobQueue(queueName);
        var resultsInit = testRunner.GetResults(queueName);
        var errorInit = testRunner.GetErrors(queueName);
        if (queueInit != null)
        {
            Console.WriteLine("\tJob Queue...√");
        }
        else
        {
            Console.WriteLine("\tJob Queue...x");
        }

        if (resultsInit != null)
        {
            Console.WriteLine("\tResults Queue...√");
        }
        else
        {
            Console.WriteLine("\tResults Queue...x");
        }
        if (errorInit != null)
        {
            Console.WriteLine("\tError Queue...√");
        }
        else
        {
            Console.WriteLine("\tError Queue...x");
        }

        Assert.That(queueInit != null && resultsInit != null && errorInit != null);
    }
    #endregion

    [Test]
    public void Ensure_JobPlacesResultsInQueue()
    {
        var testRunner = ServProvider.GetService<IJobbyJobRunner<SimpleStringJobResult>>();
        Console.WriteLine("Ensure_JobPlacesResultsInQueue:");
        testRunner.StartJobs();
        Thread.Sleep(1000);
        var simpleStringJobTasks = testRunner.GetJobQueue("Simple String Job");
        Console.Write($"\tMore than 0 Tasks");
        if (simpleStringJobTasks.Count > 0)
        {
            Console.WriteLine($"...√ ({simpleStringJobTasks.Count} active jobs)");
        }
        else
        {
            Console.WriteLine("...x");
        }

        var simpleStringJobResults = testRunner.GetResults("Simple String Job");
        Console.Write($"\tMore than 0 Results");
        if (simpleStringJobResults.Count > 0)
        {
            Console.WriteLine($"...√ ({simpleStringJobResults.Count} results)");
        }
        else
        {
            Console.WriteLine("...x");
        }

        Console.WriteLine($"\tLongest Runtime...{simpleStringJobResults.OrderByDescending(x => x.Runtime).First().Runtime}");

        Assert.That(testRunner.GetResults("Simple String Job").Count, Is.GreaterThan(1));
    }
}

public class StringJob : IJobbyJob<SimpleStringJobResult>
{
    public string JobName => "Simple String Job";
    public TimeSpan CycleTime => TimeSpan.FromMilliseconds(100);
    public TimeSpan TimeOut => TimeSpan.FromDays(1);
    public Guid Id => Guid.NewGuid();
    public int ConcurrentThreads => 10;

    //TODO: Allow for specific failure types
    //TODO: public bool OnFailure => FailureType.Stop | FailureType.Delay | FailureType.Continue;

    //TODO: Allow for total failures before stopping?
    //TODO: public int MaxFailure => 5;

    //NOTE: Determine if this job is eligible to run.
    public Func<bool> JobCondition => () =>
    {
        return true;
    };

    //Do your thing inside the job.
    public SimpleStringJobResult Run()
    {
        return new() { Foo = "Hello World!"};
    }
}

public class SimpleStringJobResult : JobbyJobResult
{
    public string Foo {get;set;}
}