using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using WindowScrape.Types;
using WindowScrape.Constants;

namespace AutoStarter
{
    class ProcessWatcher
    {

        static AutoStarter.Properties.Settings userSettings
        {
            get
            {
                return new AutoStarter.Properties.Settings();
            }
        }

        AutostartProcess _processInfo;
        WINDOWPLACEMENT _lastPlacement;

        //String name;
        //ProcessStartInfo procInfo;
        // Alle restartTime Sekunden das Programm beenden und neu starten, 0= Permanent offen halten
        //int restartTime;
        
        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        //private volatile bool shouldStop = false;

        public ProcessWatcher(AutostartProcess processInfo)
        {
            _processInfo = processInfo;
            Debug.Print("ProcessWatcher initialized (" + _processInfo.ProcessName + ") with Restart " + _processInfo.ReloadTime + " sec : " + _processInfo.Programm + ", Args: " + _processInfo.Argumente);
        }

        public void KeepingAlive()
        {
            // Starte alle Schedule-Tasks für bestimmte Uhrzeiten die definiert sind
            _processInfo.startTasks();

                while (true)
                {
                    Process proc = Process.Start(_processInfo.getProcessStartInfo());     
                    
                    // Warte etwas um Child-Prozesse (falls vorhanden) spawnen zu lassen
                    Thread.Sleep(200);

                    // Wenn Child-Prozesse erzeugt wurden und der Hauptprozess beendet diese übernehmen und auf beendigung warten
                    List<Process> childProcs = GetChildProcesses(proc).ToList();

                    try
                        {
                            if (childProcs.Count > 0 && proc.HasExited)
                            {
                                proc.Close();
                                proc = childProcs[0];
                            }
                            HwndObject window = new HwndObject(proc.MainWindowHandle);
                            //   Setze Fenster-Größe und Position wieder auf vorige Werte
                            window.SetWindowPlacement(_lastPlacement);

                            // Programm regelmaessig neu laden oder permanent offen halten?
                            if (_processInfo.ReloadTime == 0)
                                proc.WaitForExit();
                            else
                            {
                                if (proc.WaitForExit(_processInfo.ReloadTime * 1000) && proc.ExitCode == 1)
                                {
                                    _processInfo.WindowStyle = AutostartProcess.NORMAL;
                                }
                            }
                          
                                               
                            if (proc != null && !proc.HasExited && proc.MainWindowHandle != IntPtr.Zero)
                            {                                
                                                               
                                switch (window.GetWindowStyle())
                                {
                                    case 1:
                                        _processInfo.WindowStyle = AutostartProcess.NORMAL;
                                        break;
                                    case 2:
                                        _processInfo.WindowStyle = AutostartProcess.MINIMIZED;
                                        break;
                                    case 3:
                                        _processInfo.WindowStyle = AutostartProcess.MAXIMIZED;
                                        break;
                                }

                                _lastPlacement = window.GetWindowPlacement();

                                // Shutdown Process gracefully
                                int tryCount = 0;
                                while (proc != null && !proc.HasExited && tryCount++ < 20)
                                {
                                    proc.CloseMainWindow();
                                }
                                

                            }
                        }
                        catch (ThreadAbortException abort)
                        {
                            bool stopStartTask = false;
                            if(abort.ExceptionState != null)
                            {
                                stopStartTask = (bool)abort.ExceptionState;
                            }
                            Debug.Print("Process-Abort ({0}), Beende Start-Task: {1} : {2}" , _processInfo.ProcessName,stopStartTask, abort.Message);
                            int tryCount = 0;
                            // Shutdown Process gracefully
                            while (proc != null && !proc.HasExited && tryCount++ < 20)
                            {
                                proc.CloseMainWindow();
                            }

                            // Beende alle laufenden Schedule-Tasks für den Process
                            _processInfo.stopTasks(stopStartTask);
                        }
                        catch (Exception e)
                        {
                            Debug.Print("Fehler (" + _processInfo.ProcessName + "): " + e.Message);
                           
                            if (proc != null && !proc.HasExited)
                            {
                                proc.CloseMainWindow();
                            }
                        }
                        finally
                        {
                            // Wenn Prozess jetzt noch offen ist dann killen
                            if (proc != null && !proc.HasExited)
                            {
                                proc.Kill();
                            }

                            // Wenn jetzt noch Child-Prozesse auf sind diese auch killen
                            if (childProcs != null)
                            {
                                foreach (Process child in childProcs)
                                {
                                    if (child != null && !child.HasExited)
                                    {
                                        child.Kill();
                                    }

                                    child.Close();

                                }
                            }

                            proc.Close();
                        }
                 }

                
        }


        public static IEnumerable<Process> GetChildProcesses(Process process)
        {
            List<Process> children = new List<Process>();
            ManagementObjectSearcher mos = new ManagementObjectSearcher(String.Format("Select * From Win32_Process Where ParentProcessID={0}", process.Id));

            foreach (ManagementObject mo in mos.Get())
            {
                children.Add(Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])));
            }

             return children;
         }

    }
}
