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
    public struct FloodgateTimeframe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(long ts, long attr, long bucketSendLimit, long attritionBase)
        {
            SkippedCount = 0;
            ReceivedDate = ts;
            ReceivedCount = 0;
            AttritionCount = attr;
            SendLimit = Math.Max(1L, bucketSendLimit / (long)(Math.Log(AttritionCount + attritionBase, attritionBase)+0.001));
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

