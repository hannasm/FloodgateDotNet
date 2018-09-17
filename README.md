# Versioning
This is version 1.0.0 of the FloodgateDotNet library

This package is available on nuget at: https://www.nuget.org/packages/Floodgate/1.0.0

The source for this package is available on github at: https://github.com/hannasm/FloodgateDotNet/releases/tag/1.0.0

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

A separate class called FloodgateOrchestrator was created around the FloodgateActor. The orchestrator takes care of handling multi-threading and managing separate floodgates for many actors. This class uses ConcurrentDictionary behind the scenes so any type that can be effectively used as a key there would also be acceptable with the FloodgateOrchestrator.

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

# Configuration

 | Field                       | Default      | Description    |
 |-----------------------------|--------------|----------------|
 | InverseAttritionDeltaMin    | -1          | The more negative, the faster attrition grows. Shoudl never be 0 or positive |
 | WindowSize                  | 5           | The number of timeframes the floodgate analyzes |
 | TimeframeSize               | 5000        | Duration in milliseconds that a single timeframe lasts |
 | TimeframeSpilloverThreshold | 8           | Max Number of events allowed in a single timeframe |
 | SpilloverThreshold          | 16          | Maximum number of events allowed by an actor during the window before throttling begins |
 | AttritionLogarithmBase      | 2           | The numerical base which determines how quickly attrition causes an actor to be rate limited |
 | GetTimestamp                | ()=>DateTime.Now.Ticks | Returns the timestamp for the current event, must never go backwards |
# Release Notes

[For Release Notes See Here](ExpressiveExpressionTrees.ReleaseNotes.md)

