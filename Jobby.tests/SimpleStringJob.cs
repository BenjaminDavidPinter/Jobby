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

    [Test]
    public void TestJobQueueInit()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        testRunner.StartJobs();
        Assert.That(testRunner._backingQueue._JobQueueInternal.Any(x => x.Item1 == "Simple String Job"));
    }

    [Test]
    public void TestResultQueueInit()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        testRunner.StartJobs();
        Assert.That(testRunner._backingQueue._JobResultInternal.Any(x => x.Item1 == "Simple String Job"));
    }

    [Test]
    public void TestErrorQueueInit()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        testRunner.StartJobs();
        Assert.That(testRunner._backingQueue._JobErrorQueueInternal.Any(x => x.Item1 == "Simple String Job"));
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