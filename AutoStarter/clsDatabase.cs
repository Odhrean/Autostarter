using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;

namespace AutoStarter
{
    class clsDatabase
    {
        static AutoStarter.Properties.Settings userSettings
        {
            get
            {
                return new AutoStarter.Properties.Settings();
            }
        }

        BackgroundWorker backgroundThreadExecute;
        BackgroundWorker backgroundThreadFill;
        SqlConnection myConnection;
        SqlCommand myCommand;
        SqlDataReader myReader;
        SqlDataAdapter myAdapter;
        String sourceClass;
        public static int QUERY = 1;
        public static int PROC = 2;
        public static int SqlDataAdapter = 3;
        public int maxTry = 3;
        public int timeout = clsDatabase.userSettings.ConnectionTimeout;
        public long dbTimeMilis = 0; // Laufzeit in Milisekunden der letzten DB Abfrage
        int commandType;
        String query;
        String fillTable;

        public clsDatabase(String sourceClass)
        {
            //userSettings = new Leitstand.Properties.Settings();
            //userSettings = new Leitstand.Properties.Settings();
            //ConnectionTimeout = userSettings.ConnectionTimeout;
            //myConnection = new SqlConnection(clsDatabase.userSettings.dbstutekoelnConnectionString);
            this.sourceClass = sourceClass;
            commandType = QUERY;
        }

        public clsDatabase(String query, String sourceClass)
        {
            //userSettings = new Leitstand.Properties.Settings();
            //userSettings = new Leitstand.Properties.Settings();
            //ConnectionTimeout = userSettings.ConnectionTimeout;
            //myConnection = new SqlConnection(clsDatabase.userSettings.dbstutekoelnConnectionString);
            this.sourceClass = sourceClass;
            commandType = QUERY;
            this.query = query;
            execute();
        }

        public clsDatabase(String query, String sourceClass, int type)
        {
            //userSettings = new Leitstand.Properties.Settings();
            //userSettings = new Leitstand.Properties.Settings();
            //ConnectionTimeout = userSettings.ConnectionTimeout;
            //myConnection = new SqlConnection(clsDatabase.userSettings.dbstutekoelnConnectionString);
            this.sourceClass = sourceClass;
            commandType = type;
            this.query = query;
            //openCommand();
            if (type == QUERY)
            {
                execute();
            }
        }

        public void openCommand()
        {

            myCommand = new SqlCommand(query, getConnection());
            if (commandType == PROC)
            {
                myCommand.CommandType = CommandType.StoredProcedure;
            }
            if (commandType == QUERY)
            {
                myCommand.CommandType = CommandType.Text;
            }


            if (clsDatabase.userSettings.DebugApp)
            {
                Debug.WriteLine("[DBCommandQuery | " + sourceClass + "]: " + query);
            }

        }

        /*
         *  Execute, fill SqlDataReader with results
         */
        public void execute()
        {
            this.execute(true);
        }

        public void execute(bool withReader)
        {
            this.execute(this.query, withReader);
        }

        public void execute(bool withReader, bool disposeCommand)
        {
            this.execute(this.query, withReader, disposeCommand);
        }

        public void execute(String qs, bool withReader)
        {
            this.execute(qs, withReader, true);
        }

        public void execute(String qs, bool withReader, bool disposeCommand)
        {
            // Versuche 3x bei Fehler die Abfrage auszuführen...
            int tryCount = 0;
            while (tryCount < maxTry)
            {
                tryCount++;

                try
                {
                    this.query = qs;

                    long milis = DateTime.Now.Ticks;
                    getCommand().CommandTimeout = timeout;
                    if (clsDatabase.userSettings.DebugApp)
                    {
                        Debug.Write("[try " + tryCount + " von " + maxTry + "| exec  " + sourceClass + "... (timeout: " + timeout + " s)");
                    }
                    if (withReader)
                    {
                        myReader = getCommand().ExecuteReader();
                    }
                    else
                    {
                        getCommand().ExecuteNonQuery();
                    }

                    dbTimeMilis = (DateTime.Now.Ticks - milis) / 10000;

                    if (clsDatabase.userSettings.DebugApp)
                    {
                        Debug.WriteLine("... fertig, Ausfuehrungszeit]: " + dbTimeMilis + " ms | Query: " + query);
                    }
                    if (disposeCommand)
                    {
                        myCommand.Dispose();
                        myCommand = null;
                    }
                    // Abfrage erfolgreich, While-Schleife verlassen
                    tryCount = 4;

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Try " + tryCount + "> FEHLER: " + e.Message);
                    if (tryCount > maxTry)
                    {
                        throw e;
                    }
                    else
                    {
                        // Neuer Versuch, aber vorher die DB-Verbindung zu machen
                        this.close();
                    }

                }
            }
        }

        public void Fill(DataSet set, String table)
        {
            if (commandType != SqlDataAdapter)
                throw new Exception("Fill kann nur mit SqlDataAdapter ausgeführt werden!");

            try
            {
                long milis = DateTime.Now.Ticks;
                if (clsDatabase.userSettings.DebugApp)
                {
                    Debug.WriteLine("[DBFillQuery | " + sourceClass + "]: " + query);
                    Debug.WriteLine("[FILL DataSet  " + sourceClass + "... (Timeout " + getDataAdapter().SelectCommand.CommandTimeout+ ")");
                }

                getDataAdapter().Fill(set, table);

                dbTimeMilis = (DateTime.Now.Ticks - milis) / 10000;
                if (clsDatabase.userSettings.DebugApp)
                {
                    Debug.WriteLine("... fertig, Ausfuehrungszeit]: " + dbTimeMilis + " ms");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("FEHLER: " + e.Message);
                throw e;
            }
        }

        public SqlCommand getCommand()
        {
            if (myCommand == null)
            {
                openCommand();
            }
            else if (myCommand.Connection.State != ConnectionState.Open)
            {
                myCommand.Connection = getConnection();
            }

            return myCommand;
        }

        public SqlDataReader getDataReader()
        {
            if (myReader == null || myReader.IsClosed)
            {
                Debug.WriteLine("[Execute vergessen...| " + sourceClass + "]: getDataReader() ohne initialisierung des SqlDataReaders aufgerufen (execute)");
            }
            return myReader;
        }

        public SqlDataAdapter getDataAdapter()
        {
            if (myAdapter == null)
            {
                myAdapter = new SqlDataAdapter(this.query, getConnection());
            }

            myAdapter.SelectCommand.CommandTimeout = timeout;
            
            return myAdapter;
        }

        public SqlConnection getConnection()
        {
            if (myConnection == null)
            {
                //if (clsDatabase.userSettings.DebugApp)
                //{
                //    Debug.WriteLine("[new connection | " + sourceClass + "] ");
                //}

                myConnection = new SqlConnection(clsDatabase.userSettings.ConnectionString);
            }
            if (myConnection.State != ConnectionState.Open)
            {
                //if (clsDatabase.userSettings.DebugApp)
                //{
                //    Debug.WriteLine("[open connection | " + sourceClass + "] ");
                //}

                myConnection.Open();
            }
            return myConnection;
        }

        public void close()
        {
            //if (clsDatabase.userSettings.DebugApp)
            //{
            //    Debug.WriteLine("[close | " + sourceClass + "] ---------------------------------------------------------------- ");
            //}
            try
            {
                if (myReader != null && !myReader.IsClosed)
                {
                    myReader.Close();
                }
                if (myConnection != null && myConnection.State == ConnectionState.Open)
                {
                    myConnection.Close();
                }
            }
            catch { }
        }

        // ab hier: DB im eigenen Thread abfragen sodas die GUI nicht hängt

        private void initializeThread(String modus)
        {
            if (modus == "execute")
            {
                if (backgroundThreadExecute == null)
                {
                    backgroundThreadExecute = new BackgroundWorker();

                    backgroundThreadExecute.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
                        {
                            this.execute();
                        });
                    }
            }

           if (modus == "fill")
           {
                if (backgroundThreadFill == null)
                {
                    backgroundThreadFill = new BackgroundWorker();

                    backgroundThreadFill.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
                    {
                        this.Fill((DataSet)e.Argument, this.fillTable);
                    });
                }

            }
        }

        public void executeThread()
        {

            initializeThread("execute");

            backgroundThreadExecute.RunWorkerAsync();

        }

        /// <summary>
        /// Ein System.Windows.Forms.DataGridView mit dem angegebenen Query füllen
        /// Dies wird in einem eigenen Background-Thread durchgeführt
        /// </summary>
        /// <param name="gridView">Das zu füllende DataGridView</param>
        /// <param name="statusLabel">Das Status-Label in dem Status-Infos angezeigt werden (Optional)</param>
        /// <param name="labelToHideWhenReady">Das Status-Label wird nach dem Lade-Ende ausgeblendet (Optional) </param>
        public void FillDataGridViewThread(DataGridView gridView, Label statusLabel=null, Label labelToHideWhenReady = null)
        {
            if(commandType != SqlDataAdapter)
                throw new Exception("FillDataGridViewThread kann nur mit SqlDataAdapter ausgeführt werden!");

            this.fillTable = "dummy_data";

            initializeThread("fill");
            DataSet ds = new DataSet();

            // Ereignis Funktion registrieren
            backgroundThreadFill.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                gridView.DataSource = ds;
                gridView.DataMember = this.fillTable;
                if(statusLabel != null)
                    statusLabel.Text = "Daten wurden in " + this.dbTimeMilis + " Milisekunden geladen ...";

                if (labelToHideWhenReady != null)
                    labelToHideWhenReady.Visible = false;

                Application.UseWaitCursor = false;             
            }
            );

            Application.UseWaitCursor = true;
            if (statusLabel != null)
                statusLabel.Text = "Daten werden aus der Datenbank geladen ... ";

            backgroundThreadFill.RunWorkerAsync(ds);

            // Ereignis-Funktion wieder deregistrieren
            backgroundThreadFill.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                gridView.DataSource = ds;
                gridView.DataMember = this.fillTable;
                if (statusLabel != null)
                    statusLabel.Text = "Daten wurden in " + this.dbTimeMilis + " Milisekunden geladen ...";

                if (labelToHideWhenReady != null)
                    labelToHideWhenReady.Visible = false;

                Application.UseWaitCursor = false;             
            }
            );
        }

        // Ende Threading Funktionen

        // Destructor
        ~clsDatabase()
        {
            try
            {
                this.close();
            }
            finally
            {
                //base.Finalize();
            }
        }

    }
}
