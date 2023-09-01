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
            _storage = new DictionaryStorageWithCleanupStrategy<TKey>(settings);
        }
        public FloodgateOrchestrator(IFloodgateOrchestratorStorageStrategy<TKey> storage) {
            _storage = storage;
        }
        private readonly IFloodgateOrchestratorStorageStrategy<TKey> _storage;
        public FloodgateResponse NextEvent(TKey key) {
            return _storage.NextEvent(key);
        }
    }
    public interface IFloodgateOrchestratorStorageStrategy<TKey> {
        FloodgateResponse NextEvent(TKey key);
        bool DoesCleanup { get; }
    }

    /// <summary>
    /// Benchmarking on older runtimes shows substantial benefits in terms of both allocatiosn
    /// and runtime performance using Dictionary<> with a lock() over ConcurrentDictioanry.
    /// </summary>
    public class DictionaryStorageWithCleanupStrategy<TKey> : IFloodgateOrchestratorStorageStrategy<TKey> {
        public DictionaryStorageWithCleanupStrategy(FloodgateSettings settings) {
            _settings = settings;
        }
        readonly FloodgateSettings _settings;
        readonly ManualResetEventSlim _reset = new ManualResetEventSlim(false);
        volatile Dictionary<TKey, FloodgateActor> _lookup = new Dictionary<TKey, FloodgateActor>();
        readonly object _lookupSync = new object();
        public bool DoesCleanup => true;

        public Exception LastCleanupException;
        void CleanupJob(object state) {
            try {
                HashSet<TKey> toRemove = null;
                foreach (var kvp in _lookup) {
                    var key = kvp.Key;
                    var actor = kvp.Value;

                    if (actor.ReadyForCleanup()) {
                        if (toRemove == null) { toRemove = new HashSet<TKey>(); }
                        toRemove.Add(key);
                    }
                }

                if (toRemove != null) {
                    lock (_lookupSync) {
                        var newLookup2 = _lookup.Where(kv=>!toRemove.Contains(kv.Key)).ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value);
                        _lookup = newLookup2;
                    }
                }
            } catch (Exception error) {
                LastCleanupException = error;
            } finally {
                Thread.Sleep(_settings.CleanupDelay);
                _reset.Reset();
            }
        }

        void ScheduleCleanup() {
            ThreadPool.QueueUserWorkItem(this.CleanupJob);
        }

        public FloodgateResponse NextEvent(TKey key) {
            bool isNew = false;
            if (!_lookup.TryGetValue(key, out var data)) {
                isNew = true;
                data = new FloodgateActor(_settings);
            }

            try {
                lock (data)
                {
                  return data.NextEvent();
                }
            }
            finally {
                if (isNew) {
                    lock(_lookupSync) {
                        if (!_lookup.TryGetValue(key, out _)) {
                            var newLookup = new Dictionary<TKey,FloodgateActor>(_lookup);
                            newLookup.Add(key, data);
                            _lookup = newLookup;
                        }
                    }
                }

                if (!_reset.IsSet) {
                    _reset.Set();
                    this.ScheduleCleanup();
                }
            }
        }
    }
    public class ConcurrentDictionaryWithCleanupStorageStategy<TKey> : IFloodgateOrchestratorStorageStrategy<TKey> {
        public ConcurrentDictionaryWithCleanupStorageStategy(FloodgateSettings settings) {
            _settings = settings;
        }
        readonly FloodgateSettings _settings;
        readonly ManualResetEventSlim _reset = new ManualResetEventSlim(false);
        readonly ConcurrentDictionary<TKey, FloodgateActor> _lookup = new ConcurrentDictionary<TKey, FloodgateActor>();
        readonly ConcurrentQueue<TKey> _cleanupList = new ConcurrentQueue<TKey>();
        public bool DoesCleanup => true;

        public Exception LastCleanupException;
        void CleanupJob(object state) {
            try {
                HashSet<TKey> toKeep = null;
                do {
                    if (!_cleanupList.TryDequeue(out var current)) {break;}

                    if (_lookup.TryGetValue(current, out var actor)) {
                        if (actor.ReadyForCleanup()) {
                            _lookup.TryRemove(current, out _);
                        } else {
                            if (toKeep == null) { toKeep = new HashSet<TKey>(); }
                            toKeep.Add(current);
                        }
                    } else {
                        throw new InvalidOperationException("This should never happen");
                    }
                } while (true);

                if (toKeep != null) {
                    foreach (var key in toKeep) {
                        _cleanupList.Enqueue(key);
                    }
                }
            } catch (Exception error) {
                LastCleanupException = error;
            } finally {
                Thread.Sleep(_settings.CleanupDelay);
                _reset.Reset();
            }
        }
        void ScheduleCleanup() {
            ThreadPool.QueueUserWorkItem(this.CleanupJob);
        }

        public FloodgateResponse NextEvent(TKey key) {
            bool isNew = false;
            if (!_lookup.TryGetValue(key, out var data)) {
                isNew = true;
                data = new FloodgateActor(_settings);
            }

            try {
                lock (data)
                {
                  return data.NextEvent();
                }
            }
            finally {
                if (isNew) {
                    _lookup.TryAdd(key, data);
                    _cleanupList.Enqueue(key);
                }

                if (!_reset.IsSet) {
                    _reset.Set();
                    this.ScheduleCleanup();
                }
            }
        }
    }
    /// <summary>
    /// This uses a dictionary for storage meaning it's faster on older versions of dotnet, and allocates less memory in general.
    /// It uses no cleanup strategy to remove old keys. In some cases where keys are long-lived this
    /// may be significantly faster. On the other hand if keys are short lived and are not re-used frequently or ever,
    /// this will create a runaway memory leak.
    ///
    /// You should only use this if you are sure you don't need cleanup.
    /// </summary>
    public class DictionaryStorageWithoutCleanupStrategy<TKey> : IFloodgateOrchestratorStorageStrategy<TKey> {
        public DictionaryStorageWithoutCleanupStrategy(FloodgateSettings settings) {
            _settings = settings;
        }
        readonly FloodgateSettings _settings;
        volatile Dictionary<TKey, FloodgateActor> _lookup = new Dictionary<TKey, FloodgateActor>();
        readonly object _lookupSync = new object();
        public bool DoesCleanup => false;

        public FloodgateResponse NextEvent(TKey key) {
            bool isNew = false;
            if (!_lookup.TryGetValue(key, out var data)) {
                isNew = true;
                data = new FloodgateActor(_settings);
            }

            try {
                lock (data)
                {
                  return data.NextEvent();
                }
            }
            finally {
                if (isNew) {
                    lock(_lookupSync) {
                        if (!_lookup.TryGetValue(key, out _)) {
                            var newLookup = new Dictionary<TKey,FloodgateActor>(_lookup);
                            newLookup.Add(key, data);
                            _lookup = newLookup;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// This uses a concurrent dictionary for storage meaning it's slightly faster in new versions of dotnet, but still allocates more memory during use.
    /// It uses no cleanup strategy to remove old keys. In some cases where keys are long-lived this
    /// may be significantly faster, on the other hand if keys are short lived and are not re-used, i
    /// this create a runaway memory leak.
    ///
    /// You should only use this if you are sure you don't need cleanup.
    /// </summary>
    /// <typeparam name="TKey"></typeparam> <s
    public class ConcurrentDictionaryWithoutCleanupStorageStategy<TKey> : IFloodgateOrchestratorStorageStrategy<TKey> {
        public ConcurrentDictionaryWithoutCleanupStorageStategy(FloodgateSettings settings) {
            _settings = settings;
        }
        readonly FloodgateSettings _settings;
        readonly ConcurrentDictionary<TKey, FloodgateActor> _lookup = new ConcurrentDictionary<TKey, FloodgateActor>();
        public bool DoesCleanup => false;

        public FloodgateResponse NextEvent(TKey key) {
            var data = _lookup.GetOrAdd(key, k=>new FloodgateActor(_settings));
            lock (data)
            {
                return data.NextEvent();
            }
        }
    }
}
