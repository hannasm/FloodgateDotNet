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

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }

        
        [TestMethod]
        public virtual void Test002()
        {
          var b = 2;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }

        
        [TestMethod]
        public virtual void Test003()
        {
          var b = 3;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }

        [TestMethod]
        public virtual void Test004()
        {
          var b = 4;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }          
        }
        
        [TestMethod]
        public virtual void Test005()
        {
          var b = 5;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }
        [TestMethod]
        public virtual void Test006()
        {
          var b = 6;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }
        [TestMethod]
        public virtual void Test007()
        {
          var b = 7;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }
        [TestMethod]
        public virtual void Test008()
        {
          var b = 8;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }

        
        [TestMethod]
        public virtual void Test009()
        {
          var b = 9;
          var table = new LogarithmTool(b);

          long i = b;

          for (int j = 1; j < table.MaxOutput; j++) {
            Assert.AreEqual(j, table.Log(i));
            i *= b;
          }
          
        }
    }
}

