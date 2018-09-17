using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floodgate.Tests
{
    [TestClass]
    public class Log2UseCases : BaseUseCases
    {
      [TestMethod]
      public override void  Test001()
      {
        base.Test001();
      }
      [TestMethod]
      public override void  Test002()
      {
        base.Test002();
      }

        [TestMethod]
        [Description("After hitting several threshold violations and undergoing rate-limtiing, experience period of inactivity and then hit rate limits again")]
        public virtual void Test003()
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
                res = algo.NextEvent(unq1);

                Assert.AreEqual(expectedSkippedCount = 0, res.NumDisallowed, "NumDisallowed at " + i);
                Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at " + i);
            }

            res = algo.NextEvent(unq1);
            Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
            Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");

            for (int j = 0; j < IterationLogarithm(1) - 1; j++)
            {
                currentTime += settings.TimeframeTicks;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold; i++)
                {
                    res = algo.NextEvent(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumDisallowed, "NumDisallowed at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.NextEvent(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");
            }

            for (int j = 0; j < IterationLogarithm(2) - 1; j++)
            {
                currentTime += settings.TimeframeTicks;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.NextEvent(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumDisallowed, "NumDisallowed at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.NextEvent(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");
            }


            currentTime += settings.TimeframeTicks * 4;

            // The mathematical explanation for how the spillover threshold
            // is changing elludes me, but it is a predictable amount somewhat
            // related to how much spillover has taken place prior
            // to the break in activity
            for (int j = 0; j < 1; j++)
            {
                currentTime += settings.TimeframeTicks;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.NextEvent(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumDisallowed, "NumDisallowed at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.NextEvent(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");
            }

            // The mathematical explanation for how the spillover threshold
            // is changing elludes me, but it is a predictable amount somewhat
            // related to how much spillover has taken place prior
            // to the break in activity
            for (int j = 0; j < IterationLogarithm(2) + 1; j++)
            {
                currentTime += settings.TimeframeTicks;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 2; i++)
                {
                    res = algo.NextEvent(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumDisallowed, "NumDisallowed at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.NextEvent(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");
            }


            // at this point the chaos after the break in activity has normalized
            // and we are back to business as usual
            for (int j = 0; j < IterationLogarithm(3) + IterationLogarithm(4); j++)
            {
                currentTime += settings.TimeframeTicks;
                expectedSkippedCount = 1;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 4; i++)
                {
                    res = algo.NextEvent(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumDisallowed, "NumDisallowed at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.NextEvent(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");
            }

            for (int j = 0; j < 256; j++)
            {
                currentTime += settings.TimeframeTicks;
                for (int i = 0; i < settings.TimeframeSpilloverThreshold / 8; i++)
                {
                    res = algo.NextEvent(unq1);

                    Assert.AreEqual(expectedSkippedCount, res.NumDisallowed, "NumDisallowed at i=" + i + " and j= " + j);
                    Assert.AreEqual(true, res.ShouldAllow, "ShouldAllow at i=" + i + " and j= " + j);
                    expectedSkippedCount = 0;
                }

                res = algo.NextEvent(unq1);
                Assert.AreEqual(expectedSkippedCount = 1, res.NumDisallowed, "Num skipped after hit threshold");
                Assert.AreEqual(false, res.ShouldAllow, "ShouldAllow after hit threshold");
            }
        }

      [TestMethod]
      public override void  Test004()
      {
        base.Test004();
      }
      [TestMethod]
      public override void  Test005()
      {
        base.Test005();
      }
    }
}

