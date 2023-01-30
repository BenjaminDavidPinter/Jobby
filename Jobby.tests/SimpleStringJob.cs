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
        string queueName = "Simple String Job";
        Console.WriteLine("TestJobInit:");
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
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
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        Console.WriteLine("Ensure_JobPlacesResultsInQueue:");
        testRunner.StartJobs();
        Thread.Sleep(1000);
        var simpleStringJobResults = testRunner.GetJobQueue("Simple String Job");
        Console.Write($"\tMore than 0 Results");
        if (simpleStringJobResults.Count > 0)
        {
            Console.WriteLine("...√");
        }
        else
        {
            Console.WriteLine("...x");
        }
        Assert.That(testRunner.GetResults("Simple String Job").Count, Is.GreaterThan(1));
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