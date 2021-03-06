// Copyright (C) by Housemarque, Inc.

namespace Hopac.Core {
  using System.Threading;

  /// <summary>Provides a low overhead, single shot waitable event.</summary>
  internal static class Condition {
    internal static bool warned;

    internal static void Pulse(object o, ref int v) {
      var w = v;
      if (w < 0 || 0 != Interlocked.Exchange(ref v, ~w)) {
        Monitor.Enter(o);
        Monitor.Pulse(o);
        Monitor.Exit(o);
      }
    }

    internal static void Wait(object o, ref int v) {
      if (0 <= v) {
        Monitor.Enter(o);
        var w = v;
        if (0 <= w) {
          if (0 == Interlocked.Exchange(ref v, ~w)) {
            if (!warned && 0 < Worker.RunningWork) {
              warned = true;
              StaticData.writeLine(
                "WARNING: You are making a blocking call to run a Hopac job " +
                "from within a Hopac job, which means that your program may " +
                "deadlock.");
              StaticData.writeLine("First occurrence (there may be others):");
              StaticData.writeLine(System.Environment.StackTrace);
            }
            Monitor.Wait(o);
          }
        }
      }
    }
  }
}
