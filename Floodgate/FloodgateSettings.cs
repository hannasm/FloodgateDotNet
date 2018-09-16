using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AccurateIntegerLogarithm;

namespace Floodgate {

    public class FloodgateSettings
    {
        /// <summary>
        /// This is the minimum amount of attrition that can be removed from an actor by a single bucket.
        /// The default of -1 causes the most obvious and predictable behavior where attrition score increases linearly over periods of any misbehavior.
        /// The more negative this value is set, the more dramatically that attrition score can change due to misbehavior. The more dramatically
        /// the misbehavior, the more dramatic the attrition score change. The actual amount considers both the amount of time that the 
        /// misbehavior has taken place, as well as the severity of the misbehavior over that time period. 
        /// </summary>
        public long InverseAttritionDeltaMin = -1;

        /// <summary>
        /// This controls the minimum value for attrition score. If set to a negative number, attrition can become negative which will cause
        /// an actor who has been well behaved for a while to receive more leniency as a result of flooding comapred to an actor who is unknown.
        /// The default value of 0 causes all actors to be treated the same as 'unknown' actors.
        /// A value greater than 0 would put the systesm in throttling mode immediately for all actors.
        /// </summary>
        public long AttritionMin = 0;

        /// <summary>
        /// Maximum number of contiguous buckets to track (each bucket is a fixed interval of time)
        /// </summary>
        public int WindowSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _windowCount; }
            set
            {
                _windowCount = value;
                _tsthreshold = _tsinterval * value;
            }
        }

        /// <summary>
        /// Number of seconds per bucket
        /// </summary>
        public int TimeframeSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _timeframeSize; }
            set
            {
                _timeframeSize = value;
                _tsinterval = TimeSpan.TicksPerSecond * value;
                _tsthreshold = _tsinterval * WindowSize;
            }
        }

        /// <summary>
        /// Total number of messages allowed per bucket
        /// </summary>
        public long TimeframeSpilloverThreshold = 8;

        /// <summary>
        /// Total number of messgaes allowed total (within the rolling window) before throttling begins
        /// </summary>
        public long SpilloverThreshold = 16;

        /// <summary>
        /// Logarihtmic base over which the attrition coutn grows.
        /// 
        /// At log 10, we send 1/2 as many messages per bucket after 100 buckets of attrition, (about 10 minutes with a 5 second bucket) and 1/3 as many after 1000 buckets of attrition (about 90 minutes with a 5 second bucket),
        /// and 1/4 as many after 10000 messages (about 14 hours with a 5 second bucket).
        /// 
        /// At log 2, we send 1/2  as many messages per bucket after 4 buckets of attrition (about 20 seconds with a 5 second bucket), and 1/3 as mayn after 8 buckets (about 40 seconds), 
        /// and 1/4 as many after 16 buckets (about 1.5 minutes).
        /// </summary>
        public int AttritionLogarithmBase {
            get {
                return _attrLogBase;
            }
            set {
                _attrLogBase = value;
                _logTool = new AccurateIntegerLogarithmTool(value);
            }
        }

        int _attrLogBase = 2;
        AccurateIntegerLogarithmTool _logTool = new AccurateIntegerLogarithmTool(2);

        /// <summary>
        /// Used to compute integer logarithms through a lookup table
        public AccurateIntegerLogarithmTool LogTool {
            get {
                return  _logTool;
            }
        }

        /// <summary>
        /// This is the amount of time stored in a single bucket
        /// </summary>
        public long TSInterval
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _tsinterval; } }
        /// <summary>
        /// Buckets older than this value will be purged when a new bucket is added
        /// </summary>
        public long TSThreshold
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _tsthreshold; } }

        public Func<long> GetTimestamp = () => DateTime.Now.Ticks;


        /// <summary>
        /// We use this to kick off all the cached calculations
        /// </summary>
        public FloodgateSettings()
        {
            WindowSize = 5;
            TimeframeSize = 5;
        }

        int _windowCount;
        int _timeframeSize;
        long _tsinterval;
        long _tsthreshold;
    }
}
