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
        bool isFirstStart = true;
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


            do
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
                    // Lass das Fenster starten... ohne Wartezeit gibt es eine Chance das das Handle noch nicht vorhanden ist...
                    Thread.Sleep(200);
                    HwndObject window = new HwndObject(proc.MainWindowHandle);

                    if (isFirstStart)
                    {
                        // Beim ersten Start das Programm auf das gewünschte Display setzen
                        window.DisplayOnScreen(_processInfo.Display, _processInfo.startMaximized());
                        isFirstStart = false;
                    }
                    else
                    {
                        // Wenn das Programm neu geladen wird die letzte Position einnehmen die es beim beenden inne hatte
                        //window.SetWindowPlacement(_lastPlacement);
                        if (_processInfo.Display != System.Windows.Forms.Screen.PrimaryScreen)
                            window.DisplayOnScreen(_processInfo.Display, _processInfo.startMaximized());
                    }

                    /*
                    _lastPlacement = window.GetWindowPlacement();
                    window.Restore();
                    */
                    // Programm regelmaessig neu laden oder permanent offen halten?
                    if (_processInfo.ReloadTime == 0)
                    {
                        // Programm immer maximiert neu starten wenn beendet wird, überschreibe die Konfig aus der DB
                        _processInfo.WindowStyle = AutostartProcess.MAXIMIZED;
                        proc.WaitForExit();
                    }
                    else
                    {
                        if (proc.WaitForExit(_processInfo.ReloadTime * 1000) && proc.ExitCode == 1)
                        {
                            _processInfo.WindowStyle = AutostartProcess.NORMAL;
                        }
                    }

                    //int wstyle = window.GetWindowStyle();
                    //Debug.Print("Handle Start {0} Handle jetzt {1} Fenster-Position für Re-Start sichern...Window-Style {3}", window.Hwnd, proc.MainWindowHandle,wstyle);                        

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
                    if (abort.ExceptionState != null)
                    {
                        stopStartTask = (bool)abort.ExceptionState;
                    }
                    Debug.Print("Process-Abort ({0}), Beende Start-Task: {1} : {2}", _processInfo.ProcessName, stopStartTask, abort.Message);
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
            } while (_processInfo.KeepProcessAlive);

            // Thread beendet, trage auch aus Liste aus...
            Autostart.stopMonitoredThread(_processInfo.ProcessName); 
  
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
