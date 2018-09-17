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
    /// Implements a concurrencey model for handling multiple simultaneous actors performing events
    /// in multiple threads.  Backing store for TKey is ConcurrentDictionary so any TKey that
    /// can be used by ConcurrentDictionary should be good to go here.
    ///
    /// Each actor has a completely distinct floodgate applied to it.
    /// </summary>
    public class FloodgateOrchestrator<TKey>
    {
        public FloodgateOrchestrator(FloodgateSettings settings) {
            _settings = settings;
        }
        FloodgateSettings _settings;
        ConcurrentDictionary<TKey, FloodgateActor> _lookup = new ConcurrentDictionary<TKey, FloodgateActor>();

        public FloodgateResponse NextEvent(TKey key)
        {
            var data = _lookup.GetOrAdd(key, (_) => new FloodgateActor(_settings));

            lock (data)
            {
              return data.NextEvent();
            }
        }
    }
}
