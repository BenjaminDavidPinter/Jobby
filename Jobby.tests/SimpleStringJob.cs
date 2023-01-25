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
    public void TestSimpleString()
    {
        var testRunner = _provider.GetService<IJobbyJobRunner<string>>();
        testRunner.StartJobs();
        System.Threading.Thread.Sleep(1000);
        Assert.That(testRunner._backingQueue._JobResultInternal.First(x => x.Item1 == "Simple String Job").Item2.Any(y => y.Contains("Hello Jobby!"))
        , Is.True);
    }
}

public class StringJob : IJobbyJob<string>
{
    public string JobName => "Simple String Job";
    public TimeSpan CycleTime => TimeSpan.FromMilliseconds(100);
    public TimeSpan TimeOut => TimeSpan.FromDays(1);
    public TimeOnly StartTime => new(00, 00);
    public TimeOnly EndTime => new(23, 59);
    public Guid Id => Guid.NewGuid();
    public int ConcurrentThreads => 2;
    public List<(Func<string>, TaskContinuationOptions)> Continuations => null!;

    public string Run()
    {
        return $"Hello Jobby! {DateTime.Now}";
    }
}