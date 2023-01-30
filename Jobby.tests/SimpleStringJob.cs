using Jobby.lib.Core.JobTypes;
using Jobby.Lib.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Jobby.tests;
public class SimpleStringJob
{
    ServiceCollection _collection { get; set; }
    ServiceProvider _provider { get; set; }

    [SetUp]
    public void Setup()
    {
        _collection = new();
        _collection.AddScoped(typeof(IJobbyJobRunner<>), typeof(JobbyJobRunner<>));
        _collection.AddScoped(typeof(IJobbyJobQueue<>), typeof(JobbyJobQueue<>));

        _provider = _collection.BuildServiceProvider();
    }

    /*
    These tests are simply to confirm that, upon registration, the job queues are properly initialized.
    */
    #region Simple Job Setup Tests
    [Test]
    public void TestJobInit()
    {
        Console.WriteLine("TestJobInit:");
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        testRunner.StartJobs();
        var queueInit = testRunner._backingQueue.JobQueue.Any(x => x.Item1 == "Simple String Job");
        var resultsInit = testRunner._backingQueue._JobResultInternal.Any(x => x.Item1 == "Simple String Job");
        var errorInit = testRunner._backingQueue._JobErrorQueueInternal.Any(x => x.Item1 == "Simple String Job");
        if (queueInit)
        {
            Console.WriteLine("\tJob Queue...√");
        }
        else
        {
            Console.WriteLine("\tJob Queue...x");
        }

        if (resultsInit)
        {
            Console.WriteLine("\tResults Queue...√");
        }
        else
        {
            Console.WriteLine("\tResults Queue...x");
        }
        if (errorInit)
        {
            Console.WriteLine("\tError Queue...√");
        }
        else
        {
            Console.WriteLine("\tError Queue...x");
        }

        Assert.That(queueInit && resultsInit && errorInit);
    }
    #endregion

    [Test]
    public void Ensure_JobPlacesResultsInQueue()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        Console.WriteLine("Ensure_JobPlacesResultsInQueue:");
        testRunner.StartJobs();
        Thread.Sleep(1000);
        var simpleStringJobResults = testRunner._backingQueue.GetJobResultQueue("Simple String Job");
        Console.Write($"\tMore than 0 Results");
        if (simpleStringJobResults.Count > 0)
        {
            Console.WriteLine("...√");
        }
        else
        {
            Console.WriteLine("...x");
        }
        Assert.That(testRunner._backingQueue._JobResultInternal.Where(x => x.Item1 == "Simple String Job").Any(x => x.Item2.Count() > 1));
    }
}

public class StringJob : IJobbyJob<string>
{
    public string JobName => "Simple String Job";
    public TimeSpan CycleTime => TimeSpan.FromMilliseconds(100);
    public TimeSpan TimeOut => TimeSpan.FromDays(1);
    public Guid Id => Guid.NewGuid();
    public int ConcurrentThreads => 2;

    public Func<bool> JobCondition => () =>
    {
        return true;
    };

    public string Run()
    {
        return $"Hello Jobby! {DateTime.Now}";
    }
}