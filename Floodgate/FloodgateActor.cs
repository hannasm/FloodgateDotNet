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
    public class FloodgateActor
    {
        public FloodgateActor(FloodgateSettings settings)
        {
            _settings = settings;
            int wsize = _settings.WindowSize;
            Window = new FloodgateTimeframe[wsize];
            NextTimeframe = 0;
            CurrentTimeframe = wsize - 1;
            _cleanupTicks = (_settings.WindowSize+1) * _settings.TimeframeTicks;
        }
        private readonly long _cleanupTicks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddTimeframe(long ts)
        {
            Window[NextTimeframe].Init(ts, AttritionCount, _settings.TimeframeSpilloverThreshold, _settings.AttritionLogarithmBase, _settings.LogTool);
            CurrentTimeframe = NextTimeframe;
            NextTimeframe = (NextTimeframe + 1) % Window.Length;
            ValidWindowSize += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveTimeframe(int tid)
        {
            TotalReceived -= Window[tid].ReceivedCount;
            Window[tid].IsValid = false;
            ValidWindowSize -= 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CalculateAttrition(long attritionDeltaDelta)
        {
            // attrition scale must always be at least 1, or the attrition score would never change positively or negatively
            long attritionScale = AttritionCount / Window.LongLength + 1L;
            long attritionDelta = AttritionCount;

            // we are intentionally utilizing integer division truncation here so that attritionDeltaDelta
            // is multiplied after division and truncation occurs
            attritionDelta -= attritionDeltaDelta * attritionScale;

            // allowing attrition delta to go below -1 would cause attrition to increase more quickly
            // the more poorly behaved an actor was
            attritionDelta = Math.Max(_settings.InverseAttritionDeltaMin, attritionDelta);

            // letting attrition go negative might be an interesting way to give brownie points to well behaved actors
            // but in general would not be beneficial
            AttritionCount = Math.Max(_settings.AttritionMin, AttritionCount - attritionDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        long GetTimestamp()
        {
            // get new timestamp, it must always be an ascending value
            var ts = ((long)(_settings.GetTimestamp() / _settings.TimeframeTicks)) * _settings.TimeframeTicks;

            if (ts < LastTimestamp)
            {
                throw new InvalidOperationException("Timestamp must be ascending");
            }

            LastTimestamp = ts;

            // this call has some major side-affects and we need to wait to pull
            // any locals until after the call
            this.RecalcWindow(ts);

            return ts;
        }

        void RecalcWindow(long ts) {
            if (this.Window[this.CurrentTimeframe].ReceivedDate != ts)
            {
                // create a new timeframe, remove any that are outdated for the new window
                var spillover = _settings.TimeframeSpilloverThreshold;
                var cutoff = ts - _settings.WindowTicks;
                long attritionDeltaDelta = 0;

                int len = this.Window.Length;
                for (int i = 0; i < len; i += 1)
                {
                    var window = this.Window[i];
                    if (!window.IsValid) { continue; }

                    if (window.ReceivedDate <= cutoff)
                    {
                        this.RemoveTimeframe(i);
                    }

                    attritionDeltaDelta += (window.SkippedCount + spillover  - 1) / spillover;
                }

                this.CalculateAttrition(attritionDeltaDelta);
                this.AddTimeframe(ts);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadyForCleanup() {
            var ts = _settings.GetTimestamp();
            var actorTs = this.LastTimestamp;

            return actorTs == -1 || (actorTs + _cleanupTicks) < ts;
        }

        /// <summary>
        /// Check whether the next event by this actor should be allowed
        /// or disallowed based on past activity
        /// </summary>
        public FloodgateResponse NextEvent() {
            var ts = this.GetTimestamp();

            var cb = this.CurrentTimeframe;
            var recv = this.TotalReceived += 1;
            var brecv = this.Window[cb].ReceivedCount += 1;

            if (brecv <= this.Window[cb].SendLimit ||
                (this.AttritionCount == 0 && recv <= _settings.SpilloverThreshold))
            {
                var oldSkipped = this.TotalSkipped;
                this.TotalSkipped = 0;
                var oldTs = this.LastSend;
                this.LastSend = ts;
                return new FloodgateResponse
                {
                    ShouldAllow = true,
                    NumDisallowed = oldSkipped,
                    LastAllowed = new DateTime(oldTs)
                };
            }
            else
            {
                this.Window[cb].SkippedCount++;
                this.TotalSkipped++;
                return new FloodgateResponse
                {
                    ShouldAllow = false,
                    NumDisallowed = this.TotalSkipped,
                    LastAllowed = new DateTime(this.LastSend)
                };
            }
        }

        long AttritionCount;

        long TotalSkipped;
        long TotalReceived;
        long LastSend = 0;
        long LastTimestamp = -1;
        FloodgateSettings _settings;

        int NextTimeframe;
        int CurrentTimeframe;

        int ValidWindowSize;
        FloodgateTimeframe[] Window;
    }
}
