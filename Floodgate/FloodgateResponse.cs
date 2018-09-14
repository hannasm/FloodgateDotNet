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

        public class FloodgateResponse
        {
            public DateTime LastSend;
            public bool ShouldSend;
            public long NumSkipped;
        }
}

