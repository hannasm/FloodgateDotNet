using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Floodgate
{
  public class LogarithmTool {
    struct Node {
      public int output;
      public long input;
      public bool valid;
    }

    public LogarithmTool(int @base) {
      _base = @base;

      List<Node> tmp = new List<Node>();
      long oldc = -1;
      unchecked { 
        for (long c = _base, i = 1; c > oldc; oldc = c, c *= _base, i++) {
          tmp.Add(new Node { input = c, output = (int)i, valid = true });
        }
      }

      var last = tmp.Last();
      MaxInput = last.input;
      MaxOutput = last.output;

      _lookup = new Node[nextPowerOf2(tmp.Count)*2];
      Build(tmp, tmp.Count / 2, 0, tmp.Count, 0);
    }

    int pow(int val, int exp) {
      if (exp == 0) { return 1; }
      while (--exp > 0) {
        val *= val;
      }
      return val;
    }

    public int Log(long value) {
      bool sign = value < 0;
      if (sign) { value = -value; }
      if (value < _base) { return 0; }

      var res = DoLog(value);

      if (sign) { value = -value; }

      return res;
    }

    int DoLog(long value) {
      int i = 0;
      int mini = -1, maxi = -1;
      while (true) {
        if (i >= _lookup.Length || !_lookup[i].valid) {
          return mini;
        }
        if (_lookup[i].input >= value) { maxi = i; i = i*2+1; }
        else { mini = i; i = i*2+2; }
      }
    }

    void Build(List<Node> source, int sc, int smin, int smax, int dc) {
      if (sc < smin || sc >= smax) { return; }

      _lookup[dc] = source[sc];

      if (smin >= smax) { return; }

      Build(source, sc - (sc - smin)/2, smin, sc, dc*2 + 1);
      Build(source, sc + (smax - sc)/2, sc+1, smax, dc*2 + 2);
    }

    static int nextPowerOf2(int n)
    {
      var val = 1;
      while (n > val) {
        val *= 2;
      }
      return val;
    }

    public override string ToString() {
      var res = new StringBuilder();
      res.Append("LogLookupTable<");
      res.Append(_base);
      res.Append(">(");
      var prefix = string.Empty;
      for (int i = 0; i < _lookup.Length; i++) {
        res.Append(prefix);
        res.Append("[");
        res.Append("index=" + i + ",input=");
        res.Append(_lookup[i].input.ToString());
        res.Append(",output=");
        res.Append(_lookup[i].output.ToString());
        res.Append("]");
        prefix = ",";
      }
      return res.ToString();
    }

    Node [] _lookup;
    int _base;
    public long MaxInput;
    public long MaxOutput;
  }
}
