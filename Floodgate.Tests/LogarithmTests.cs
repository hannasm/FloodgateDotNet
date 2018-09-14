using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floodgate.Tests {
  [TestClass]
    public class LogarithmTests
    {
        [TestMethod]
        public virtual void Test001()
        {
          var b = 10;
          var table = new LogarithmTool(b);

          var i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }
    }
}

