using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Digitalroot.Unity3d.Log
{
  public class TraceLogger
  {
    private readonly FileInfo _traceFileInfo;
    private readonly string _source;
    public bool IsTraceEnabled { get; private set; }

    public TraceLogger(string source, bool enableTrace)
    {
      try
      {
        _source = source;
        IsTraceEnabled = enableTrace;
        var logDirectoryInfo = new DirectoryInfo(Application.dataPath);
        if (!logDirectoryInfo.Exists) throw new DirectoryNotFoundException($"{logDirectoryInfo.FullName} does not exist.");

        var gamePath = logDirectoryInfo.Parent?.FullName ?? throw new DirectoryNotFoundException($"_logDirectoryInfo.Parent of {logDirectoryInfo.FullName} is null");
        var bepInExDirectoryInfo = new DirectoryInfo(Path.Combine(gamePath, "BepInEx"));
        if (!bepInExDirectoryInfo.Exists) throw new DirectoryNotFoundException($"{bepInExDirectoryInfo.FullName} does not exist.");
        var bepInExLogsDirectoryInfo = new DirectoryInfo(Path.Combine(bepInExDirectoryInfo.FullName, "logs"));
        if (!bepInExLogsDirectoryInfo.Exists) bepInExLogsDirectoryInfo.Create();

        _traceFileInfo = new FileInfo(Path.Combine(bepInExLogsDirectoryInfo.FullName, $"{_source}.Trace.log"));

        if (_traceFileInfo.Exists)
        {
          _traceFileInfo.Delete();
          _traceFileInfo.Refresh();
        }

      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }

    public void EnableTrace()
    {
      IsTraceEnabled = true;
    }

    public void DisableTrace()
    {
      IsTraceEnabled = false;
    }

    public void Trace(string message)
    {
      if (!IsTraceEnabled) return;

      using var mutex = new Mutex(false, $"Digitalroot.Unity3d.TraceLogger.{_source}");
      mutex.WaitOne();
      try
      {
        var msg = $"[{"Trace",-7}:{_source,10}] {message}{Environment.NewLine}";
        File.AppendAllText(_traceFileInfo.FullName, msg, Encoding.UTF8);
      }
      finally
      {
        mutex.ReleaseMutex();
      }
    }
  }
}
