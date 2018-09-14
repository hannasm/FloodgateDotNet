using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Floodgate {
    /// <summary>
    /// Based on a 
    /// Handles all the thinking of determining whether to write a message to the log or not with the intention
    /// of limiting the number of messages written to the log in short periods of time. All
    /// decision making is performed over a rolling window of time by grouping events into buckets spanning fixed intervals of time.
    /// 
    /// Handling Messages in the Not Throttling State
    ///     * If we are in the 'not throttling state' and there has been less than ThrottleStartThreshold messages received over the entire rolling window, it will be logged
    ///     * If we are in the 'not throttling state' and there has been more than ThrottleStartThreshold messages received over the entire rolling window, then we enter 'throttling' state
    /// 
    /// Handling Messages in the Throttling State
    ///     * If there have been less than BucketSendLimit messages received over a single bucketed interval, the message will be logged
    ///     * If more than BucketSendLimit messages are received over a single bucketed interval, those messages will be ignored instead
    ///     
    /// 
    /// Once in the throttling state, we begin tracking how long throttling has been taking place. This is caleld the AttritionCount, and represents approximately how long a continuous stream of messages
    /// has been received (as a count of bucket intervals). 
    /// 
    /// There is a startup grace period, where AttritionCount will remain 0 even though throttling has begun. The duration that this startup grace period lasts
    /// will vary depending on how much prior activity there was before throttling began. If throttling began after a pro-longed period of receiving messages without any throttling, there may be no startup
    /// grace period because the system is already 'warmed up'. On the other hand, if we go immediately from no-activtiy to receiving a flood of messages, the startup grace period 
    /// would span the period of time equal to the total number of buckets.
    /// 
    /// Once the flood of messages has stopped, there is a ramp down period during which throttling safeguards are slowly relaxed. 
    /// 
    /// Recall, during throttling, the total number of messages sent over a single bucketed time interval is controlled by BucketSendLimit. We
    /// will gradually decrease the BucketSendLimit as the AttritionCount increases, up to a maximum where we write only one log message per bucket. The reduction
    /// in message volume is based on the logarithm of the AttritionCount over a given base.
    /// </summary>
    public class FloodgateOrchestrator<TKey>
    {
        public FloodgateOrchestrator(FloodgateSettings settings) {
            _settings = settings;
        }
        FloodgateSettings _settings;
        ConcurrentDictionary<TKey, FloodgateActor> _lookup = new ConcurrentDictionary<TKey, FloodgateActor>();

        public FloodgateResponse ShouldSpillover(TKey key)
        {
            var data = _lookup.GetOrAdd(key, (_) => new FloodgateActor(_settings));

            lock (data)
            {
              return data.ShouldSpillover();
            }
        }
    }
}
