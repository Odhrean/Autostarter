using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using WindowScrape;
using WindowScrape.Types;
using WindowScrape.Constants;

namespace AutoStarter
{
    static class Program
    {

        private static NotifyIcon notico;
        private static List<Thread> monitoredThreads;
        private static List<AutostartProcess> aktiveRunningProcesses;
        private static volatile bool stop = false;
        private static ContextMenu cm;

        static AutoStarter.Properties.Settings userSettings
        {
            get
            {
                return new AutoStarter.Properties.Settings();
            }
        }
        [STAThread]
        public static void Main(string[] args)
        {
            /*
            HwndObject notes = HwndObject.GetWindowByTitle("Notes");

            if (notes.Hwnd == (IntPtr)0)
                notes = HwndObject.GetWindowByTitle("IBM Notes Mail - Eingang");
            if (notes.Hwnd == (IntPtr)0)
                notes = HwndObject.GetWindowByTitle("Mail - Eingang - IBM Notes");

            notes.Title = "Notes";
            

            //notes.SetWindowPos((IntPtr)(-1), screen1.WorkingArea.X, screen1.WorkingArea.Y, screen1.Bounds.Width, screen1.Bounds.Height, 0x0040);
            int style = notes.GetWindowStyle();

            notes.DisplayOnScreen(System.Windows.Forms.Screen.AllScreens.ToList<Screen>().Find(x => x.DeviceName.Contains("DISPLAY2")));
            */
            notico = new NotifyIcon();
            notico.Icon = Properties.Resources.stute_favicon;
            notico.Text = "Auto-Starter";
            notico.Visible = true;
            cm = new ContextMenu();
            cm.Popup += new System.EventHandler(createContextMenu);
            notico.ContextMenu = cm;

            monitoredThreads = new List<Thread>();
            aktiveRunningProcesses = new List<AutostartProcess>();

            Thread monitorThread = new Thread(monitorDB);
            monitorThread.IsBackground = true;
            monitorThread.Start();


            Debug.Print("Alle Threads gestartet...");

            Application.Run();

        }

        // Datenbank-Config lesen und bei Änderungen Programme starten/stoppen
        private static void monitorDB()
        {

           String sql = "SELECT [Prozess_Name],[Programm],[Argumente],[Reload_Time_Sec],ISNULL(w.WindowStyle,'Normal') WindowStyle, a.ID, "
                       +"CASE WHEN (select top 1 AKTION "
                       +"from autostarter.Schedule  "
                       +"where ID_Autostart=a.ID "
                       +"and dateadd(mi,cast(substring(Uhrzeit,4,3) as int),dateadd(hh,cast(substring(Uhrzeit,0,3) as int),dateadd(dd, datediff(dd,0, getDate()), 0))) > getdate() "
                       +"order by dateadd(mi,cast(substring(Uhrzeit,4,3) as int),dateadd(hh,cast(substring(Uhrzeit,0,3) as int),dateadd(dd, datediff(dd,0, getDate()), 0)))) "
                       +"= 'START' THEN 0 ELSE 1 END Start_Proc "
                       + "from autostarter.AUTOSTART a "
                       + "LEFT OUTER JOIN autostarter.WindowStyle w on w.ID=a.[WindowStyle] "
                       + "where a.Hostname='" + System.Environment.MachineName + "' and a.Aktiv=1 ";

           while (!stop)
           {
               clsDatabase db = new clsDatabase(sql, "AutoStarter -> monitorDB");

               // Liste der Prozesse die jetzt laufen sollten nach DB-Config, wird später mit den aktuell laufenden abgeglichen um die zu finden die beendet werden sollen
               List<AutostartProcess> autostartProcess = new List<AutostartProcess>();

               while (db.getDataReader().Read())
               {
                   AutostartProcess autoProc = new AutostartProcess();
                   autoProc.ProcessName = db.getDataReader().GetString(0);
                   autoProc.Programm = db.getDataReader().GetString(1);
                   autoProc.Argumente = !db.getDataReader().IsDBNull(2) ? db.getDataReader().GetString(2) : "";
                   autoProc.ReloadTime = !db.getDataReader().IsDBNull(3) ? db.getDataReader().GetInt32(3) : 0;
                   autoProc.WindowStyle = !db.getDataReader().IsDBNull(4) ? db.getDataReader().GetString(4) : "Normal";
                   
                   // Process initial starten? Wenn nein wird das Programm zu einer bestimmten Uhrzeit gestartet :-D
                   bool startProcInitial = (!db.getDataReader().IsDBNull(5) ? db.getDataReader().GetInt32(6) : 0) == 1;
                   // Suche nach dem Prozess in der Liste der bereits überwachten Prozess auf dem Client
                   // Wenn er gefunden wird => alles i.O. nichts machen
                   // Wenn er nicht gefunden wird dann neu starten und in die Liste aufnehmen

                   // Läuft der Prozess bereits in der Überwachung ... 
                   AutostartProcess runningProc = aktiveRunningProcesses.Find(item => item.ProcessName == autoProc.ProcessName);

                   if (runningProc == null) // ... nein, neu aufnehmen und starten 
                   {
                       autoProc.sheduleTasks = readScheduleTasks(db.getDataReader().GetInt32(5), autoProc);
                       if (startProcInitial)
                       {
                           startMonitoringThread(autoProc);
                       }
                       else
                       {
                           autoProc.startTasks();
                       }

                       aktiveRunningProcesses.Add(autoProc);
                   }
                   else // .. Ja, weiter prüfen ob die Tasks sich geändert haben
                   {
                       // Task-Liste aus der DB wie sie aktuell aussieht
                       List<AutostartTask> tasks = readScheduleTasks(db.getDataReader().GetInt32(5), autoProc);

                       // gibt es eine Differenz zur Liste auf dem Client?
                       var diff = runningProc.sheduleTasks.Except(tasks)
                           .Union(
                                tasks.Except(runningProc.sheduleTasks)
                           ).ToList();                       

                       // es gibt eine Änderung in den Tasks, alle vorhanden stoppen, neue übernehmen und starten wenn Prozess aktuell laueft
                       if (diff.Count > 0)
                       {
                           bool isRunning = monitoredThreads.Find(item => item.Name == autoProc.ProcessName) != null;
                           if (isRunning)
                           {
                               runningProc.stopTasks(true);
                           }
                           aktiveRunningProcesses.Remove(runningProc);
                           autoProc.sheduleTasks = tasks;
                           autoProc.startTasks();
                           
                           aktiveRunningProcesses.Add(autoProc);
                       }
                   }

                   // in die Vergleichsliste aufnehmen
                   autostartProcess.Add(autoProc);

               }
               db.close();

               // Suche alle Prozesse die gestoppt werden muessen ...
               List<AutostartProcess> stopingProcess = aktiveRunningProcesses.Except(autostartProcess).ToList();
               // .. und beende sie 
               if (stopingProcess.Count > 0)
               {
                   foreach (AutostartProcess stopProc in stopingProcess)
                   {
                       stopMonitoredThread(stopProc.ProcessName,true);
                       aktiveRunningProcesses.Remove(stopProc);
                   }
               }

               Thread.Sleep(userSettings.Reload_Interval * 1000);

           }
        }

        private static List<AutostartTask> readScheduleTasks(int autostartId,AutostartProcess proc)
        {
            List<AutostartTask> tasks = new List<AutostartTask>();
            String sql = "select ID,Aktion,Uhrzeit "
                        +"from autostarter.Schedule "
                        + "where ID_Autostart=" + autostartId + " and Aktiv=1 ";

            clsDatabase db = new clsDatabase(sql, "AutoStarter -> readScheduleTasks");
            while (db.getDataReader().Read())
            {
                AutostartTask task = new AutostartTask();
                task.TaskID = db.getDataReader().GetInt32(0);
                task.setTaskTime(db.getDataReader().GetString(2));

                if (db.getDataReader().GetString(1).Equals("START"))
                {
                    task.doTask = () => { 
                        Program.startMonitoringThread(proc); 
                    };
                    task.isStart = true;
                }
                if (db.getDataReader().GetString(1).Equals("STOP"))
                {
                    task.doTask = () => { 
                        Program.stopMonitoredThread(proc.ProcessName); 
                    };
                }
                if (db.getDataReader().GetString(1).Equals("RESTART"))
                {
                    task.doTask = () => {
                        Program.stopMonitoredThread(proc.ProcessName);
                        Program.startMonitoringThread(proc); 
                    };
                    task.isStart = true;
                }
                

                tasks.Add(task);
            }
            db.close();

            return tasks;
        }

        public static void createContextMenu(System.Object sender, System.EventArgs e)
        {
            cm.MenuItems.Clear();

            MenuItem about = new MenuItem("Info");
            about.Click += (o, i) =>
            {
                new About().Show();
            };
            cm.MenuItems.Add(about);

            foreach (var prc in aktiveRunningProcesses)
            {
                MenuItem mi = new MenuItem(prc.ProcessName);

                MenuItem miStart = new MenuItem("Starten");
                miStart.Click += (o, i) =>
                {
                    startMonitoringThread(prc);
                };
                mi.MenuItems.Add(miStart);

                MenuItem miStop = new MenuItem("Beenden");
                miStop.Click += (o, i) =>
                {
                    stopMonitoredThread(prc.ProcessName,true);
                };
                mi.MenuItems.Add(miStop);

                cm.MenuItems.Add(mi);
            }


            MenuItem ende = new MenuItem("Exit");
            ende.Click += (o, i) =>
            {
                foreach (var th in monitoredThreads)
                {
                    Debug.Print("Beende " + th.Name + "...");

                    th.Abort();
                    th.Join();

                }
                notico.Dispose();

                Environment.Exit(0);
            };
            cm.MenuItems.Add(ende);

        }

        public static void startMonitoringThread(AutostartProcess proc)
        {
            Thread thread = monitoredThreads.Find(item => item.Name == proc.ProcessName);

            // Thread ist bereits im Monitoring => jeder Thread wird 1x ueberwacht!
            if (thread != null)
                return;

            ProcessWatcher watcherObject = new ProcessWatcher(proc);            
            thread = new Thread(watcherObject.KeepingAlive);
            thread.Name = proc.ProcessName;
            thread.Start();
            while (!thread.IsAlive) ;
            monitoredThreads.Add(thread);

        }

        public static void stopMonitoredThread(String threadName,bool stopAllTask=false)
        {
            Thread thread = monitoredThreads.Find(item => item.Name == threadName);

            if (thread == null)
                return;

            Debug.Print("Beende " + threadName + "...");

            if (stopAllTask)
            {
                thread.Abort(true);
            }
            else
            {
                thread.Abort(false);
            }
            thread.Join();

            monitoredThreads.Remove(thread);

        }

    }
}
