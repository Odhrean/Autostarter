using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace AutoStarter
{
    class AutostartProcess : IDisposable
    {
        public const string NORMAL = "Normal";
        public const string MAXIMIZED = "Maximized";
        public const string MINIMIZED = "Minimized";
        public const string HIDDEN = "Hidden";

        public string ProcessName { get; set; }
        public string Programm { get; set; }
        public string Argumente { get; set; }
        public int ReloadTime { get; set; }
        public string WindowStyle { get; set; }
        public System.Windows.Forms.Screen Display { get; set; }
        public bool KeepProcessAlive { get; set; }
        public List<AutostartTask> sheduleTasks { get; set; }
        private CancellationTokenSource _cancelProcess;
        private CancellationTokenSource _cancelStartProcess;

        public ProcessStartInfo getProcessStartInfo()
        {
            ProcessStartInfo _processStartInfo = new ProcessStartInfo();

            _processStartInfo.FileName = Programm;
            _processStartInfo.Arguments = Argumente;
            if (WindowStyle == NORMAL)
                _processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            if (WindowStyle == MAXIMIZED)
                _processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            if (WindowStyle == MINIMIZED)
                _processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            if (WindowStyle == HIDDEN)
                _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;


            // Wenn der Internet Explorer gestartet werden soll und das Argument -nomerge fehlt dieses anfuegen!
            if (_processStartInfo.FileName.Contains("iexplore") && !_processStartInfo.Arguments.Contains("-nomerge"))
                _processStartInfo.Arguments = "-nomerge " + _processStartInfo.Arguments;

            _processStartInfo.UseShellExecute = true;

            return _processStartInfo;
        }

        public bool startMaximized()
        {
            if (WindowStyle == MAXIMIZED)
                return true;
            else
                return false;
        }

        public void startTasks()
        {
            if (sheduleTasks != null)
            {
                bool startStartTask = false;
                if (_cancelStartProcess == null)
                {
                    _cancelStartProcess = new CancellationTokenSource();
                    startStartTask = true;
                }
                if (_cancelStartProcess.IsCancellationRequested)
                {
                    _cancelStartProcess = new CancellationTokenSource();
                    startStartTask = true;
                }

                _cancelProcess = new CancellationTokenSource();

                foreach (AutostartTask task in sheduleTasks)
                {
                    if (task.isStart)
                    {
                        if (startStartTask) 
                        { 
                            Debug.Print("({0}) Starte START-Task : {1} um {2}", ProcessName, task.TaskID, task.taskTime);
                            task.ScheduleTask(_cancelStartProcess.Token);
                        }
                    }else
                    {
                        Debug.Print("({0}) Starte Task : {1} um {2}", ProcessName, task.TaskID, task.taskTime);
                        task.ScheduleTask(_cancelProcess.Token);
                    }
                }

                Debug.Print("----- Alle Tasks gestartet ------------");
            }
        }

        public void stopTasks(bool all = false)
        {
            if(sheduleTasks != null)
            {
                if (all)
                {
                    Debug.Print("({0}) Stoppe Start-Tasks", ProcessName);
                    _cancelStartProcess.Cancel();
                }
                Debug.Print("({0}) Stoppe Stop-Tasks", ProcessName);
                _cancelProcess.Cancel();

                Debug.Print("----- Tasks gestoppt ------------");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(AutostartProcess))
                return false;

            AutostartProcess test = obj as AutostartProcess;

            return this.ProcessName == test.ProcessName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)172166136261;
                hash = hash * 16777619 ^ ProcessName.GetHashCode();
                return hash;
            }
        }



        public void Dispose()
        {
            if(_cancelProcess != null)
                _cancelProcess.Dispose();
            if(_cancelStartProcess != null)
                _cancelStartProcess.Dispose();
        }
    }
}
