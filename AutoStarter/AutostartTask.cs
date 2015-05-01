using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace AutoStarter
{
    class AutostartTask
    {
        public int TaskID { get; set; }
        public DateTime taskTime { get; set; }
        // Tasks die als Start-Job deklariert sind werden nicht beendet beim stoppen durch Task
        public bool isStart { get; set; }
        public Action doTask { get; set; }

        public void setTaskTime(string zeit)
        {
            taskTime = DateTime.Today.AddHours(Convert.ToInt32(zeit.Substring(0, 2)));
            taskTime = taskTime.AddMinutes(Convert.ToInt32(zeit.Substring(3, 2)));
        }

        
        public async void ScheduleTask(CancellationToken cancelToken)
        {
            //Nur wenn Zeit in Zukunft liegt Task warten lassen, ansonstend direkt beenden
            if (DateTime.Now.CompareTo(taskTime) >= 0)
            {
                Debug.Print("Task {0} wird nicht gestartet, da Zeit {1} in der Vergangenheit liegt!", TaskID, taskTime);
                return;
            }
            Debug.Print("Task {0} wird fuer eine Ausfuehrung in {1} ms gestartet", TaskID, Convert.ToInt32(taskTime.Subtract(DateTime.Now).TotalMilliseconds));
            try
            {
                await Task.Delay(Convert.ToInt32(taskTime.Subtract(DateTime.Now).TotalMilliseconds), cancelToken);
                Debug.Print("TRIGGER -> Führe Task {0} aus", TaskID);
                doTask();
            }
            catch(TaskCanceledException)
            {
                Debug.Print("Stoppe Task {0} (is Start-Task: {1} )", TaskID, isStart);
            }
            catch(Exception e)
            {
                Debug.Print("Task {0}: Exception -> {1}", TaskID,e.Message);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(AutostartTask))
                return false;

            AutostartTask test = obj as AutostartTask;

            return this.TaskID == test.TaskID && this.taskTime == test.taskTime;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)172166136261;
                hash = hash * 16777619 ^ TaskID.GetHashCode();
                hash = hash * 16777619 ^ taskTime.GetHashCode();
                return hash;
            }
        }
    }
}
