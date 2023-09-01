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

        public struct FloodgateResponse
        {
            /// <summary>Last time that an event was allowed</summary>
            public DateTime LastAllowed;
            /// <summary>Whether or not the next event should be allowed</summary>
            public bool ShouldAllow;
            /// <summary>The number of events which have been disallowed since the last one that was allowed</summary>
            public long NumDisallowed;
        }
}
