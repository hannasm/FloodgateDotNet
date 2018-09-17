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
    /// <summary>
    /// Data store used to configure floodgate algorithm behavior.
    ///
    /// Some of these fields could be changed in realtime and the floodgate algorithm will begin using them. Others
    /// will be ignored or possibly even cause catastrophic failure.
    ///
    /// Best practice would be to setup the configuration the way you want first, and then pass it into your floodgate algorithm
    /// afterwards, never tweaking the values once they are in use.
    /// </summary>
    public class FloodgateSettings
    {
        /// <summary>
        /// This is the minimum amount of attrition that can be removed from an actor over a single timeframe.
        /// The default of -1 causes the most obvious and predictable behavior where attrition score increases linearly over periods of any misbehavior.
        /// The more negative this value is set, the more dramatically that attrition score can change due to misbehavior. The more dramatically
        /// the misbehavior, the more dramatic the attrition score change. The actual amount considers both the amount of time that the
        /// misbehavior has taken place, as well as the severity of the misbehavior over that time period.
        /// </summary>
        public long InverseAttritionDeltaMin = -1;

        /// <summary>Minimum value for attrition, there is no reason to change this and it could break things if you do</summary>
        public long AttritionMin = 0;

        /// <summary>
        /// Maximum number of contiguous timeframes to track
        /// </summary>
        public int WindowSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _windowCount; }
            set
            {
                _windowCount = value;
                _windowsTicks = _timeframeTicks * value;
            }
        }

        /// <summary>
        /// Number of milliseconds per timeframe
        /// </summary>
        public long TimeframeSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _timeframeSize; }
            set
            {
                _timeframeSize = value;
                _timeframeTicks = TimeSpan.TicksPerMillisecond * value;
                _windowsTicks = _timeframeTicks * WindowSize;
            }
        }

        /// <summary>
        /// Total number of events allowed per timeframe
        /// </summary>
        public long TimeframeSpilloverThreshold = 8;

        /// <summary>
        /// Total number of events allowed while actor is not in throttling mode
        /// </summary>
        public long SpilloverThreshold = 16;

        /// <summary>
        /// Logarihtmic base over which the attrition coutn grows.
        ///
        /// At log 10, we accept 1/2 as many events per timeframe after 90 timeframes of attrition, (about 10 minutes with a 5 second timeframe) and 1/3 as many after 990 timeframes of attrition (about 90 minutes with a 5 second timeframe),
        /// and 1/4 as many after 10000 events (about 14 hours with a 5 second timeframe).
        ///
        /// At log 2, we send 1/2  as many events per timeframe after 4 timeframes of attrition (about 20 seconds with a 5 second timeframe), and 1/3 as maby after 8 timeframes (about 40 seconds),
        /// and 1/4 as many after 16 timeframes (about 1.5 minutes).
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
        /// </summary>
        public AccurateIntegerLogarithmTool LogTool {
            get {
                return  _logTool;
            }
        }

        /// <summary>
        /// (readonly) This is the amount of time stored in a single timeframe, in ticks
        /// </summary>
        public long TimeframeTicks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _timeframeTicks; }
        }

        /// <summary>
        /// (readonly) timeframes older than this value will be purged when a new timeframe is added, value in ticks
        /// </summary>
        public long WindowTicks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _windowsTicks; }
        }

        /// <summary>
        /// This field is used to generate the current timestamp for any event. It must
        /// never go backwards. It can be changed to simulate various situations where
        /// relying on the system clock just doesn't do what you need.
        /// </summary>
        public Func<long> GetTimestamp = () => DateTime.Now.Ticks;


        public FloodgateSettings()
        {
            WindowSize = 5;
            TimeframeSize = 5000;
        }

        int _windowCount;
        long _timeframeSize;
        long _timeframeTicks;
        long _windowsTicks;
    }
}
