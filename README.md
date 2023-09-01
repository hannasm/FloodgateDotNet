# Versioning
This is version 1.0.2 of the FloodgateDotNet library

This package is available on nuget at: https://www.nuget.org/packages/Floodgate/1.0.2

The source for this package is available on github at: https://github.com/hannasm/FloodgateDotNet/releases/tag/1.0.2

# FloodgateDotNet
This library implements smart logic to determine whether an actor has been flooding a system with events. This logic could be used to identify and mitigate denial of service attacks from a malicious user. It can be used to keep the same log message about some yet to be fixed system error that you've known about for months, from showing up in your logs 1,000,000 times a day and hiding any other useful log messages from appearing. The highly focused design of this code could also be applied inversely, so that an action is not allowed until the smart threshold has been reached instead.

# Usage

Simple usage is to rely on the default configuration entirely:

```csharp
using Floodgate;

var settings = new FloodgateSettings();
var actor = new FloodgateActor(settings);

for (int i = 0; i < 100; i++) {
    var res = actor.NextEvent()
    if (!res.ShouldAllow) {
        Console.WriteLine("This iteration was rate limited");
    } else {
        Console.WriteLine("This event was allowed");
    }
}
```

A separate class called FloodgateOrchestrator was created around the FloodgateActor. The orchestrator takes care of handling multi-threading and managing separate floodgates for many actors. This class has various storage strategies implemented which can be configured depending your exact usage patterns.

> [!NOTE]
> When using FloodgateOrchestrator memory management needs to be made a concern because we are tracking a set of heuristics for each actor, and as the number of actors increases so does memory consumption. See the section below on memory management for further consideration


```csharp
using Floodgate;

var settings = new FloodgateSettings();
var orchestrator = new FloodgateOrchestrator<int>(settings);

for (int i = 0; i < 100; i++) {
    for (int actorId = 0; actorId < 10; actorId++) {
        var res = orchestrator.NextEvent(actorId)
        if (!res.ShouldAllow) {
            Console.WriteLine("This iteration was rate limited");
        } else {
            Console.WriteLine("This event was allowed");
        }
    }
}
```

It is possible to control the clock used by the floodgate as well, which comes in handy when trying to setup unit tests or any other situation where the system clock just isn't the best way of defining your timeframes.

```csharp
using Floodgate;

var settings = new FloodgateSettings();

long currentTime = DateTime.Now.Ticks;
settings.GetTimestamp = ()=>currentTime;

var orchestrator = new FloodgateOrchestrator<int>(settings);

for (int i = 0; i < 100; i++) {
    for (int actorId = 0; actorId < 10; actorId++) {
        // advance time by one timeframe
        currentTime += settings.TimeframeTicks;

        var res = orchestrator.NextEvent(actorId)
        if (!res.ShouldAllow) {
            // advance time completely past window 
            currentTime += settings.WindowTicks;
            Console.WriteLine("This iteration was rate limited");
        } else {
            Console.WriteLine("This event was allowed");
        }
    }
}
```

# Algorithm 

The floodgate algorithm recives as input a series of events from an actor. As output the algorithm answers the question, should we allow this message. The message should be disallowed if the actor has performed more events than would be considered an acceptable normal (flooding).

Each actor is monitored over a windowed interval. As activity ocurrs, only events happening within that window are generally considered as part of the actors history of activity. Events that have transpired inside the window are 'forgotten' once they becom older than the windowed intervals duration.

Within a window there are a series of timeframes. Each timeframe corresponds to a fixed duration interval of time over which the actor is expected not to exceed a certain number of events. 

* The TimeframeSpilloverThresold defines the maximum number of events allowed within a single timeframe.

* The TimeframeSize defines the number of milliseconds that a single timeframe considered.

* The WindowSize defines the number of timeframes that are considered.

Before an actor is restricted to performing TimeframeSpilloverThreshold events per timeframe, they must first be identified as flooding.

* The SpilloverThreshold represents the minimum amount of activity required to flag an actor as flooding. In a non flooding state an actor may process events un-hindered. Once an actor is flagged as flooding they are more severely restricted in how many events they may perform, and must meet more stringent requirements before they can clear this flag. 

Once an actor is identified as being in the flooding state they are immediately limited to performing TimeframeSpilloverThreshold events per timeframe. However, as their misbehavior continues, and as the degree of their misbehavior grows, this threshold shrinks smaller until the actor is eventually limited to sending 1 message per timeframe.

The degree of misbehavior is increased once per TimeframeSpilloverThreshold events performed. This means that the more events an actor has, the more quickly events will become disallowed. There is configuration that allows us to limit how much this plays a factor in the calculation, and the default value disables it entirely.

The period of time over which this misbehavior ocurrs is tracked and measured as Attrition. Attrition grows for each consecutive timeframe where misbehavior has ocurred. As Attrition grows, the number of events an actor is allowed to perform shrinks smaller. The number of events is inversely propritional to Log(AttritionCount).

`
ActualTimeframeSpilloverThreshold = TimeframeSpilloverThreshold / Math.Truncate( Math.Log( AttritionCount + AttritionLogarithmBase, AttritionLogarithmBase ) )
`

* AttritionLogarithmBase defines the numerical base over which the attrition count restricts actor activity. The default value of 2 means that an actors threshold will be cut in half after 2 timeframes of misbehavior, and will be cut in thirds after 4 more timeframes, and cut in fourths after 8 more. With a base of 10 instead, the threshold will be cut in half after 90 timeframes, and thirds after 990 and fourths after 9990 instead. This logarithm base chosen corresponds with how much attrition is required to increase the actors activity more severely. Each level of restriction is always exponentially more difficult to reach so the larger the base, the more slowly the system responds to the most egregious behavior and the more forgiving it is to the mildly misbehaved actor.

* InverseAttritionDeltaMin defines the maximum speed (as a negative value) by which attrition may change in a single timeframe. The default value of -1 means that attrition can grow by at most 1 per timeframe. A value of -5 would allow attrition to grow by up to 5 per timeframe instead. The more negative this value becomes, the more quickly a misbehaving actor will be limited in its activity over a single timeframe. As the AttritionLogarithmBase grows larger, it becomes more and more important to configure this properly to properly restrict the behavior of more misbehaved actors.

# Memory Managemnet
Floodgate Orchestrator needs to maintain a table of actors to track their usage pattern. As the number of actors grows so does memory consumption. If your system is going to be dealing with a seemingly unlimited supply of new actors, this can eventually exhaust all available memory.

There are several storage strategies available to help with managing actors and memory use. 
 
  * Cleanup - When cleanup is involved, actors that have been idle for a long enough period of time will be purged from the floodgate orchestrators table. This is important when dealing with a seemingly infinite supply of new actors during during the lifetime of a long-lived appliation. Cleanup is a non-trivial operation and can _slightly_ affect performance.
  * WithoutCleanup - When cleanup is not implemented, all actors will remain in the table until the Floodgate is garbage collected by the .NET runtime. If your actors are long-lived and generally wont change throughout your programs lifetime, and/or there will never be enough actors to substantially impact the memory usage of your program, this can offer some minor performance benefits at the cost of more memory use.

## DictionaryStorageWithCleanupStrategy

```csharp
using Floodgate;

var settings = new FloodgateSettings();
settings.CleanupDelay = TimeSpan.FromMilliseconds(60);
var orchestrator = new FloodgateOrchestrator<int>(
    new DictionaryStorageWithCleanupStrategy(settings)
);
```

This storage strategy uses a Dictionary<> and a lock() to manage actors. Through benchmarking it seems that this is both faster and allocates significantly less than the  ConcurrentDictionaryStrategies described below.

*This is the default storage strategy.*

[In newer versions of dotnet this may no longer be the case, further benchmarking may be needed](https://www.singulink.com/codeindex/post/fastest-thread-safe-lock-free-dictionary)

The cleanup mechanism for this strategy uses a background job on the ThreadPool to periodically check for actors that have been idle and remove them.

## DictionaryStorageWithoutCleanupStrategy

```csharp
using Floodgate;

var settings = new FloodgateSettings();
var orchestrator = new FloodgateOrchestrator<int>(
    new DictionaryStorageWithoutCleanupStrategy(settings)
);
```

This storage strategy uses a Dictionary<> and a lock() to manage actors. It offers nearly identical performance to the `DictionaryStorageWithCleanupStrategy`, but should generally be even faster because it doesnt cleanup idle actors. 

## ConcurrentDictionaryStorageWithCleanupStrategy

```csharp
using Floodgate;

var settings = new FloodgateSettings();
settings.CleanupDelay = TimeSpan.FromMilliseconds(60);
var orchestrator = new FloodgateOrchestrator<int>(
    new ConcurrentDictionaryStorageWithCleanupStrategy(settings)
);
```

This storage strategy, as the name implies, uses a ConcurrentDictionary for storage. This makes the Floodgate code even smaller / simpler but through benchmarking seems to also result in significantly more memory being allocated and performance being on-par or even slightly slower than the `DictionaryStorage` strategies.

The cleanup strategy is simnilar to the one used by `DictionaryStorageWithClenaupStrategy` and uses a BackgroundThread on the ThreadPool to periodically find and purge idle actors.

## ConcurrentDictionaryStorageWithoutCleanupStrategy

```csharp
using Floodgate;

var settings = new FloodgateSettings();
var orchestrator = new FloodgateOrchestrator<int>(
    new ConcurrentDictionaryStorageWithoutCleanupStrategy(settings)
);
```

This storage strategy, as the name implies, uses a ConcurrentDictionary for storage. It also skips any background cleanup jobs improving performance slightly with the added tradeoffs with never removing idle actors.


# Configuration

 | Field                       | Default      | Description    |
 |-----------------------------|--------------|----------------|
 | InverseAttritionDeltaMin    | -1          | The more negative, the faster attrition grows. Shoudl never be 0 or positive |
 | WindowSize                  | 5           | The number of timeframes the floodgate analyzes |
 | TimeframeSize               | 5000        | Duration in milliseconds that a single timeframe lasts |
 | TimeframeSpilloverThreshold | 8           | Max Number of events allowed in a single timeframe |
 | SpilloverThreshold          | 16          | Maximum number of events allowed by an actor during the window before throttling begins |
 | AttritionLogarithmBase      | 2           | The numerical base which determines how quickly attrition causes an actor to be rate limited |
 | GetTimestamp                | ()=>DateTime.UtcNow.Ticks | Returns the timestamp for the current event, must never go backwards |
 | CleanupDelay                | 50ms        | The minimum interval between runs of the background cleanup job (only applies to some storage strategies)  |

# Release Notes

[For Release Notes See Here](Floodgate.ReleaseNotes.md)

# Latest Benchmark Results
These benchmarks were generated on a relatively underpowered virtual machine but should help gauge some of the relative performance tradeoffs of different floodgate features. These benchmark operations execute 500,000 iterations of the floodgate tool using various numbers of actors, calls from each actor, and over different windows of time.

The operations labeled `Baseline` below, bypass the work of the orchestrator and invoke floodgate actors directly (hence run without the added overhead of table lookups and managing concurrency)

[You can view the benchmark test suite here](https://github.com/hannasm/floodgatedotnet/blob/master/Floodgate.Tests/Benchmarks.cs)

You can run the benchmarks from the commandline with the statement:

`dotnet test --filter BenchmarkDotNetMemoryStressTest002 -c Release`

```

BenchmarkDotNet v0.13.7, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
Intel Xeon E-2146G CPU 3.50GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK 7.0.201
  [Host]     : .NET Core 2.1.30 (CoreCLR 4.6.30411.01, CoreFX 4.6.30411.02), X64 RyuJIT AVX2
  DefaultJob : .NET Core 2.1.30 (CoreCLR 4.6.30411.01, CoreFX 4.6.30411.02), X64 RyuJIT AVX2


```
|                                                              Method |        Mean |     Error |    StdDev | #Iterations | Calls/Actor | #Actors | #Windows |      Gen0 |     Gen1 |  Allocated |
|-------------------------------------------------------------------- |------------:|----------:|----------:|------------ |------------ |-------- |--------- |----------:|---------:|-----------:|
|                                           BenchmarkBaselineOneActor |    24.33 ms |  0.442 ms |  0.392 ms |      500000 |      500000 |       1 |       10 |         - |        - |      842 B |
|                                           BenchmarkBaselineTenActor |   170.46 ms |  2.303 ms |  2.042 ms |      500000 |      500000 |      10 |       10 |         - |        - |     4173 B |
|                                       BenchmarkBaselineHundredActor | 1,677.65 ms | 30.019 ms | 36.866 ms |      500000 |      500000 |     100 |       10 |         - |        - |    37336 B |
|                               BenchmarkBaselineLongDurationOneActor |    28.52 ms |  0.563 ms |  0.499 ms |      500000 |      500000 |       1 |    10000 |         - |        - |      842 B |
|                               BenchmarkBaselineLongDurationTenActor |   209.69 ms |  4.073 ms |  4.000 ms |      500000 |      500000 |      10 |    10000 |         - |        - |     4173 B |
|                           BenchmarkBaselineLongDurationHundredActor | 2,002.21 ms | 19.566 ms | 17.345 ms |      500000 |      500000 |     100 |    10000 |         - |        - |    37336 B |
|                                            BenchmarkOneActorCleanup |    36.77 ms |  0.733 ms |  1.377 ms |      500000 |      500000 |       1 |       10 |         - |        - |     1125 B |
|                                     BenchmarkOneActorWithoutCleanup |    35.58 ms |  0.652 ms |  1.035 ms |      500000 |      500000 |       1 |       10 |         - |        - |      973 B |
|                                            BenchmarkTenActorCleanup |    36.03 ms |  0.686 ms |  0.817 ms |      500000 |       50000 |      10 |       10 |         - |        - |     7981 B |
|                                     BenchmarkTenActorWithoutCleanup |    36.16 ms |  0.673 ms |  1.248 ms |      500000 |       50000 |      10 |       10 |         - |        - |     7828 B |
|                                        BenchmarkHundredActorCleanup |    37.04 ms |  0.720 ms |  0.707 ms |      500000 |        5000 |     100 |       10 |         - |        - |   228765 B |
|                                 BenchmarkHundredActorWithoutCleanup |    37.21 ms |  0.719 ms |  0.910 ms |      500000 |        5000 |     100 |       10 |         - |        - |   228613 B |
|                        BenchmarkOneActorCleanupConcurrentDictionary |    38.15 ms |  0.635 ms |  0.756 ms |      500000 |      500000 |       1 |       10 |         - |        - |     2085 B |
|                 BenchmarkOneActorWithoutCleanupConcurrentDictionary |    44.85 ms |  0.891 ms |  1.026 ms |      500000 |      500000 |       1 |       10 | 5083.3333 |        - | 32001157 B |
|                        BenchmarkTenActorCleanupConcurrentDictionary |    37.83 ms |  0.710 ms |  0.630 ms |      500000 |       50000 |      10 |       10 |         - |        - |     5685 B |
|                 BenchmarkTenActorWithoutCleanupConcurrentDictionary |    43.77 ms |  0.793 ms |  0.662 ms |      500000 |       50000 |      10 |       10 | 5083.3333 |        - | 32004757 B |
|                    BenchmarkHundredActorCleanupConcurrentDictionary |    38.81 ms |  0.739 ms |  0.851 ms |      500000 |        5000 |     100 |       10 |         - |        - |    50101 B |
|             BenchmarkHundredActorWithoutCleanupConcurrentDictionary |    44.75 ms |  0.742 ms |  0.794 ms |      500000 |        5000 |     100 |       10 | 5083.3333 |  83.3333 | 32046741 B |
|                                BenchmarkLongDurationOneActorCleanup |    40.51 ms |  0.668 ms |  0.686 ms |      500000 |      500000 |       1 |    10000 |         - |        - |     1125 B |
|                         BenchmarkLongDurationOneActorWithoutCleanup |    39.31 ms |  0.738 ms |  0.691 ms |      500000 |      500000 |       1 |    10000 |         - |        - |      973 B |
|                                BenchmarkLongDurationTenActorCleanup |    73.98 ms |  1.408 ms |  1.446 ms |      500000 |       50000 |      10 |    10000 |         - |        - |     8081 B |
|                         BenchmarkLongDurationTenActorWithoutCleanup |    74.11 ms |  1.443 ms |  1.825 ms |      500000 |       50000 |      10 |    10000 |         - |        - |     7833 B |
|                            BenchmarkLongDurationHundredActorCleanup |    60.51 ms |  1.183 ms |  1.162 ms |      500000 |        5000 |     100 |    10000 |         - |        - |   391751 B |
|                     BenchmarkLongDurationHundredActorWithoutCleanup |    61.62 ms |  1.207 ms |  2.237 ms |      500000 |        5000 |     100 |    10000 |         - |        - |   228616 B |
|            BenchmarkLongDurationOneActorCleanupConcurrentDictionary |    44.00 ms |  0.832 ms |  1.343 ms |      500000 |      500000 |       1 |    10000 |         - |        - |     2086 B |
|     BenchmarkLongDurationOneActorWithoutCleanupConcurrentDictionary |    50.95 ms |  1.019 ms |  1.730 ms |      500000 |      500000 |       1 |    10000 | 5000.0000 |        - | 32001158 B |
|            BenchmarkLongDurationTenActorCleanupConcurrentDictionary |    79.97 ms |  1.598 ms |  2.757 ms |      500000 |       50000 |      10 |    10000 |         - |        - |     5785 B |
|     BenchmarkLongDurationTenActorWithoutCleanupConcurrentDictionary |    83.20 ms |  1.651 ms |  1.622 ms |      500000 |       50000 |      10 |    10000 | 5000.0000 |        - | 32004763 B |
|        BenchmarkLongDurationHundredActorCleanupConcurrentDictionary |    64.15 ms |  1.255 ms |  1.395 ms |      500000 |        5000 |     100 |    10000 |         - |        - |    93050 B |
| BenchmarkLongDurationHundredActorWithoutCleanupConcurrentDictionary |    70.29 ms |  1.400 ms |  2.339 ms |      500000 |        5000 |     100 |    10000 | 5000.0000 | 125.0000 | 32046744 B |
