using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Floodgate.Tests 
{
    [TestClass]
    public class Log10UseCases : BaseUseCases
    {
      public override FloodgateSettings Settings {
        get {
          var result = base.Settings;

          result.AttritionLogarithmBase = 10;

          return result;
        }
      }
    }
}

