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
    public struct FloodgateTimeframe
    {
        /// <summary>Reassigns all fields, with the intention of reusing the timeframe</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(long ts, long attr, long bucketSendLimit, int attritionBase, AccurateIntegerLogarithmTool logTool)
        {
            SkippedCount = 0;
            ReceivedDate = ts;
            ReceivedCount = 0;
            AttritionCount = attr;
            SendLimit = Math.Max(1L, bucketSendLimit / logTool.Log(AttritionCount + attritionBase));
            IsValid = true;
        }

        public long SkippedCount;
        public long ReceivedCount;
        public long ReceivedDate;
        public long AttritionCount;
        public long SendLimit;
        public bool IsValid;
    }

}

