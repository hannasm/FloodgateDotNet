using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

namespace Floodgate.Tests {
    [TestClass]
    [MemoryDiagnoser]
    [Config(typeof(Benchmarks.CustomConfig))]
    public class Benchmarks
    {
        public class CustomConfig : BenchmarkDotNet.Configs.ManualConfig {
            public CustomConfig() {
                AddColumn(new AttributeColumn<NumIterationsAttribute>("#Iterations", a=>a.Value.ToString()));
                AddColumn(new AttributeColumn<CallsPerActorAttribute>("Calls/Actor", a=>a.Value.ToString()));
                AddColumn(new AttributeColumn<NumActorsAttribute>("#Actors", a=>a.Value.ToString()));
                AddColumn(new AttributeColumn<NumWindowsAttribute>("#Windows", a=>a.Value.ToString()));
            }
        }

		public class NumIterationsAttribute : Attribute {
			public NumIterationsAttribute(int value) {
                this.Value = value;
            }
            public readonly int Value;
		}
		public class CallsPerActorAttribute : Attribute {
			public CallsPerActorAttribute(int value) {
                this.Value = value;
            }
            public readonly int Value;
		}
		public class NumActorsAttribute : Attribute {
			public NumActorsAttribute(int value) {
                this.Value = value;
            }
            public readonly int Value;
		}
		public class NumWindowsAttribute : Attribute {
			public NumWindowsAttribute(int value) {
                this.Value = value;
            }
            public readonly int Value;
		}

        public class AttributeColumn<T> : IColumn
			where T : Attribute
		{
		 	private readonly Func<T, string> getTag;

			public string Id { get; }
			public string ColumnName { get; }

			public AttributeColumn(string columnName, Func<T, string> getTag)
			{
				this.getTag = getTag;
				ColumnName = columnName;
				Id = nameof(AttributeColumn<T>) + "." + ColumnName;
			}

			public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
			public string GetValue(Summary summary, BenchmarkCase benchmarkCase) {
                var attr = getAttribute(benchmarkCase);
                if (attr == null) { return "NA"; }
                return getTag(attr);
            }

			static T getAttribute(BenchmarkCase benchmarkCase) {
				var attrs = benchmarkCase.Descriptor.WorkloadMethod.GetCustomAttributes(typeof(T), true);
				var result = attrs.OfType<T>().FirstOrDefault();
				return result;
			}

			public bool IsAvailable(Summary summary) => true;
			public bool AlwaysShow => true;
			public ColumnCategory Category => ColumnCategory.Custom;
			public int PriorityInCategory => 0;
			public bool IsNumeric => false;
			public UnitType UnitType => UnitType.Dimensionless;
			public string Legend => $"Custom '{ColumnName}' attribute defined column";
			public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
			public override string ToString() => ColumnName;
        }

        public static long Iterations {
            get {
                var settings = new FloodgateSettings();
                var eventsPerSecond = 10000;
                int timeframesToIterate = 10;
                var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
                var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
                return (long)iterationCount;
            }
        }

        public void BenchmarkLongDurationOneActor(FloodgateSettings settings,FloodgateOrchestrator<int> algo, ref long clock) {
            var eventsPerSecond = 10;
            int timeframesToIterate = 10000;
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) /  (decimal)eventsPerSecond;
            BenchmarkTest(algo, (long)iterationCount, (long)tickIncrement, 1, ref clock);
        }
        public void BenchmarkLongDurationTenActor(FloodgateSettings settings,FloodgateOrchestrator<int> algo, ref long clock) {
            var eventsPerSecond = 10;
            int timeframesToIterate = 10000;
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) /  (decimal)eventsPerSecond;
            BenchmarkTest(algo, (long)iterationCount, (long)tickIncrement, 10, ref clock);
        }
        public void BenchmarkLongDurationHundredActor(FloodgateSettings settings,FloodgateOrchestrator<int> algo, ref long clock) {
            var eventsPerSecond = 10;
            int timeframesToIterate = 10000;
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) / (decimal)eventsPerSecond;
            BenchmarkTest(algo, (long)iterationCount, (long)tickIncrement, 100, ref clock);
        }
        public void BenchmarkOneActor(FloodgateSettings settings,FloodgateOrchestrator<int> algo, ref long clock) {
            var eventsPerSecond = 10000;
            int timeframesToIterate = 10;
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) /  (decimal)eventsPerSecond;
            BenchmarkTest(algo, (long)iterationCount, (long)tickIncrement, 1, ref clock);
        }
        public void BenchmarkTenActor(FloodgateSettings settings,FloodgateOrchestrator<int> algo, ref long clock) {
            var eventsPerSecond = 10000;
            int timeframesToIterate = 10;
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) /  (decimal)eventsPerSecond;
            BenchmarkTest(algo, (long)iterationCount, (long)tickIncrement, 10, ref clock);
        }
        public void BenchmarkHundredActor(FloodgateSettings settings,FloodgateOrchestrator<int> algo, ref long clock) {
            var eventsPerSecond = 10000;
            int timeframesToIterate = 10;
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerSecond * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) / (decimal)eventsPerSecond;
            BenchmarkTest(algo, (long)iterationCount, (long)tickIncrement, 100, ref clock);
        }
        public void BenchmarkTest(FloodgateOrchestrator<int> algo, long iterationCount, decimal clockIncrement, long actorCount, ref long clock) {
            long ci = (long)clockIncrement;
            for (long i = 0; i < iterationCount; i++) {
                var res = algo.NextEvent((int)(i % actorCount));

                clock += ci;
            }
        }

        public void BenchmarkBaselineTest(FloodgateSettings settings, long actorCount, long eventsPerTimeframe, long timeframesToIterate, ref long clock) {
            var actors = new FloodgateActor[actorCount];
            for (int i = 0; i < actorCount; i++) {
                actors[i] = new FloodgateActor(settings);
            }
            var ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
            var iterationCount = eventsPerTimeframe * (settings.TimeframeTicks / 1000M / ticksPerMillisecond)  * timeframesToIterate;
            var tickIncrement = (settings.TimeframeTicks - 1) /  (decimal)eventsPerTimeframe;

            long ci = (long)tickIncrement;
            for (long i = 0; i < iterationCount; i++) {
                for (int j = 0; j < actorCount; j++) {
                    actors[j].NextEvent();
                }

                clock += ci;
            }
        }

#region Baseline Benchmarks
        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10)]
        [TestMethod]
        [Description("Demonstrate actor performance without orchestrator involvement")]
        public virtual void BenchmarkBaselineOneActor()
        {
            // do all the looping without any of the stuff regarding FloodgateOrchestrator
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock;
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkBaselineTest(settings, 1, 10000, 10, ref clock);
            }
        }
        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(10)]
        [NumWindows(10)]
        [TestMethod]
        [Description("Demonstrate actor performance without orchestrator involvement")]
        public virtual void BenchmarkBaselineTenActor()
        {
            // do all the looping without any of the stuff regarding FloodgateOrchestrator
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock;
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkBaselineTest(settings, 10, 10000, 10, ref clock);
            }
        }
        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(100)]
        [NumWindows(10)]
        [TestMethod]
        [Description("Demonstrate actor performance without orchestrator involvement")]
        public virtual void BenchmarkBaselineHundredActor()
        {
            // do all the looping without any of the stuff regarding FloodgateOrchestrator
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock;
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkBaselineTest(settings, 100, 10000, 10, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10000)]
        [TestMethod]
        [Description("Demonstrate actor performance without orchestrator involvement")]
        public virtual void BenchmarkBaselineLongDurationOneActor()
        {
            // do all the looping without any of the stuff regarding FloodgateOrchestrator
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock;
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkBaselineTest(settings, 1, 10, 10000, ref clock);
            }
        }
        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(10)]
        [NumWindows(10000)]
        [TestMethod]
        [Description("Demonstrate actor performance without orchestrator involvement")]
        public virtual void BenchmarkBaselineLongDurationTenActor()
        {
            // do all the looping without any of the stuff regarding FloodgateOrchestrator
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock;
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkBaselineTest(settings, 10, 10, 10000, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(100)]
        [NumWindows(10000)]
        [TestMethod]
        [Description("Demonstrate actor performance without orchestrator involvement")]
        public virtual void BenchmarkBaselineLongDurationHundredActor()
        {
            // do all the looping without any of the stuff regarding FloodgateOrchestrator
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock;
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkBaselineTest(settings, 100, 10, 10000, ref clock);
            }
        }
#endregion

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkOneActorCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkOneActor(settings, algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkOneActorWithoutCleanup()
        {
            var settings = new FloodgateSettings();

            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new DictionaryStorageWithoutCleanupStrategy<int>(settings));
            {
                BenchmarkOneActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkTenActorCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkTenActorWithoutCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new DictionaryStorageWithoutCleanupStrategy<int>(settings));
            {
                BenchmarkTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkHundredActorCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkHundredActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkHundredActorWithoutCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new DictionaryStorageWithoutCleanupStrategy<int>(settings));
            {
                BenchmarkHundredActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkOneActorCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithCleanupStorageStategy<int>(settings));
            {
                BenchmarkOneActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkOneActorWithoutCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithoutCleanupStorageStategy<int>(settings));
            {
                BenchmarkOneActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkTenActorCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithCleanupStorageStategy<int>(settings));
            {
                BenchmarkTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkTenActorWithoutCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithoutCleanupStorageStategy<int>(settings));
            {
                BenchmarkTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkHundredActorCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithCleanupStorageStategy<int>(settings));
            {
                BenchmarkHundredActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10000M * 10M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10)]
        [TestMethod]
        public virtual void BenchmarkHundredActorWithoutCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithoutCleanupStorageStategy<int>(settings));
            {
                BenchmarkHundredActor(settings,algo, ref clock);
            }
        }

#region Long Duration Benchmarks
        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationOneActorCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkLongDurationOneActor(settings, algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationOneActorWithoutCleanup()
        {
            var settings = new FloodgateSettings();

            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new DictionaryStorageWithoutCleanupStrategy<int>(settings));
            {
                BenchmarkLongDurationOneActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationTenActorCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkLongDurationTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationTenActorWithoutCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new DictionaryStorageWithoutCleanupStrategy<int>(settings));
            {
                BenchmarkLongDurationTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationHundredActorCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(settings);
            {
                BenchmarkLongDurationHundredActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationHundredActorWithoutCleanup()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new DictionaryStorageWithoutCleanupStrategy<int>(settings));
            {
                BenchmarkLongDurationHundredActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationOneActorCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithCleanupStorageStategy<int>(settings));
            {
                BenchmarkLongDurationOneActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(500000)]
        [NumActors(1)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationOneActorWithoutCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithoutCleanupStorageStategy<int>(settings));
            {
                BenchmarkLongDurationOneActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationTenActorCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithCleanupStorageStategy<int>(settings));
            {
                BenchmarkLongDurationTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(50000)]
        [NumActors(10)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationTenActorWithoutCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithoutCleanupStorageStategy<int>(settings));
            {
                BenchmarkLongDurationTenActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationHundredActorCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithCleanupStorageStategy<int>(settings));
            {
                BenchmarkLongDurationHundredActor(settings,algo, ref clock);
            }
        }

        [Benchmark]
        [NumIterations((int)(10M * 10000M * (5M * TimeSpan.TicksPerSecond / 1000M / TimeSpan.TicksPerMillisecond)))]
        [CallsPerActor(5000)]
        [NumActors(100)]
        [NumWindows(10000)]
        [TestMethod]
        public virtual void BenchmarkLongDurationHundredActorWithoutCleanupConcurrentDictionary()
        {
            var settings = new FloodgateSettings();
            var clock = DateTime.UtcNow.Ticks;
            settings.GetTimestamp = () => clock; // This is the default but just want to be clear what were doing
            var algo = new FloodgateOrchestrator<int>(new ConcurrentDictionaryWithoutCleanupStorageStategy<int>(settings));
            {
                BenchmarkLongDurationHundredActor(settings,algo, ref clock);
            }
        }
#endregion

        [TestMethod]
        public void BenchmarkDotNetMemoryStressTest002() {
            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}

