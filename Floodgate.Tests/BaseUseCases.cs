using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floodgate.Tests {
    public abstract class BaseUseCases    
    {
        public virtual FloodgateSettings Settings {
          get {
            return new FloodgateSettings();
          }
        }

        public long Exponent(long value, int count) {
          var result = 1L;
          while (count-- > 0) {
            result *= value;
          }
          return result;
        }
            
        public long IterationLogarithm(int iter) {
            return Exponent(Settings.AttritionLogarithmBase, iter+1) - Exponent(Settings.AttritionLogarithmBase, iter);
        }

        [TestMethod]
        public virtual void Test001()
        {
            var settings = Settings;
            long currentTime = DateTime.Now.Ticks;
            settings.GetTimestamp = () => currentTime;

            var algo = new FloodgateOrchestrator<int>(settings);

            FloodgateResponse res;
            int unq1 = 123456;
            var expectedSkippedCount = 0;

            for (int i = 0; i < settings.SpilloverThreshold; i++)
            {
                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumSkipped, "NumSkipped at " + i);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at " + i);
            }

            res = algo.ShouldSpillover(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");

            for (int j = 0; j < IterationLogarithm(1) - 1; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < IterationLogarithm(2); j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }


            for (int j = 0; j < IterationLogarithm(3); j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 3; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold and j=" + j);
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit thresholdand j=" + j);
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < IterationLogarithm(4); j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 4; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }
            
            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 256; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 8; j++)
            {
                currentTime += settings.TSInterval;

                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }
            }
        }

        [TestMethod]
        [Description("After a period of inactivtiy, being well behaved for a single bucket resets the system back to the non-throttling case")]
        public void Test002()
        {
            var settings = Settings;
            long currentTime = DateTime.Now.Ticks;
            settings.GetTimestamp = () => currentTime;

            var algo = new FloodgateOrchestrator<int>(settings);

            FloodgateResponse res;
            int unq1 = 123456;
            var expectedSkippedCount = 0;

            for (int i = 0; i < settings.SpilloverThreshold; i++)
            {
                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumSkipped, "NumSkipped at " + i);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at " + i);
            }

            res = algo.ShouldSpillover(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");

            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < 3; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }


            currentTime += settings.TSInterval * 4;
            expectedSkippedCount = 1;
            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TSInterval;

                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at j= " + j);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at j= " + j);
                expectedSkippedCount = 0;
            }

            // we already sent one, and then we need to sent threshold -1  more to hit the threshold

            currentTime += settings.TSInterval;
            for (int i = 0; i < settings.SpilloverThreshold - 1; i++)
            {
                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumSkipped, "NumSkipped at " + i);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at " + i);
            }

            res = algo.ShouldSpillover(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
        }

        [TestMethod]
        [Description("After hitting several threshold violations and undergoing rate-limtiing, experience period of inactivity and then hit rate limits again")]
        public void Test003()
        {
            var settings = Settings;
            long currentTime = DateTime.Now.Ticks;
            settings.GetTimestamp = () => currentTime;

            var algo = new FloodgateOrchestrator<int>(settings);

            FloodgateResponse res;
            int unq1 = 123456;
            var expectedSkippedCount = 0;

            for (int i = 0; i < settings.SpilloverThreshold; i++)
            {
                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumSkipped, "NumSkipped at " + i);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at " + i);
            }

            res = algo.ShouldSpillover(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");

            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < 3; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }


            currentTime += settings.TSInterval * 4;

            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < 5; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }


            for (int j = 0; j < 24; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 4; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < 256; j++)
            {
                currentTime += settings.TSInterval;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }
        }

        [TestMethod]
        [Description("5 intervals of activity without exceeding any send limits and then experience major threshold violations")]
        public void Test004()
        {
            var settings = Settings;
            long currentTime = DateTime.Now.Ticks;
            settings.GetTimestamp = () => currentTime;

            var algo = new FloodgateOrchestrator<int>(settings);

            FloodgateResponse res;
            int unq1 = 123456;
            var expectedSkippedCount = 0;

            for (int j = 0; j < 5; j++)
            {
                currentTime += settings.TSInterval;

                for (int i = 0; i < 1; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }
            }


            currentTime += settings.TSInterval;
            for (int i = 0; i < settings.SpilloverThreshold - 4; i++)
            {
                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumSkipped, "NumSkipped at " + i);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at " + i);
            }

            res = algo.ShouldSpillover(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");

            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < 4; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }


            for (int j = 0; j < 8; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 3; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 16; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 4; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 256; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 8; j++)
            {
                currentTime += settings.TSInterval;

                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }
            }
        }


        [TestMethod]
        [Description("50 intervals of activity without exceeding any send limits and then experience major threshold violations")]
        public void Test005()
        {
            var settings = Settings;
            long currentTime = DateTime.Now.Ticks;
            settings.GetTimestamp = () => currentTime;

            var algo = new FloodgateOrchestrator<int>(settings);

            FloodgateResponse res;
            int unq1 = 123456;
            var expectedSkippedCount = 0;

            for (int j = 0; j < 50; j++)
            {
                currentTime += settings.TSInterval;

                for (int i = 0; i < 1; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }
            }


            currentTime += settings.TSInterval;
            for (int i = 0; i < settings.SpilloverThreshold - 4; i++)
            {
                res = algo.ShouldSpillover(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumSkipped, "NumSkipped at " + i);
                Assert.AreEqual(true, res.ShouldSend, "SHouldSend at " + i);
            }

            res = algo.ShouldSpillover(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");

            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            for (int j = 0; j < 4; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }


            for (int j = 0; j < 8; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 3; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 16; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 4; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 256; j++)
            {
                currentTime += settings.TSInterval;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.ShouldSpillover(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumSkipped, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldSend, "ShouldSend after hit threshold");
            }

            // we will continue sending one message for each bucket indefinitley
            for (int j = 0; j < 8; j++)
            {
                currentTime += settings.TSInterval;

                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.ShouldSpillover(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumSkipped, "NumSkipped at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldSend, "SHouldSend at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }
            }
        }
    }
}

