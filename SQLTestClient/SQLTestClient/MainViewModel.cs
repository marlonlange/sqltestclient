using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Threading;

namespace SQLTestClient
{
    public class MainViewModel: ViewModelBase
    {
        private DispatcherTimer availabilityTimer;
        private SqlConnection conn = new SqlConnection();
        //private SqlTransaction trans = new SqlTransaction();

        #region RelayCommands

        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand TransactionCommand { get; private set; }

        #endregion

        #region Bindings

        private string _server;
        public string server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
                base.RaisePropertyChanged();
            }
        }

        private string _database;
        public string database
        {
            get
            {
                return _database;
            }
            set
            {
                _database = value;
                base.RaisePropertyChanged();
            }
        }

        private string _user;
        public string user
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
                base.RaisePropertyChanged();
            }
        }

        private string _password;
        public string password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                base.RaisePropertyChanged();
            }
        }

        private string _output;
        public string output
        {
            get
            {
                return _output;
            }
            set
            {
                _output = value;
                base.RaisePropertyChanged();
            }
        }

        private string _availabilityStatus;
        public string availabilityStatus
        {
            get
            {
                return _availabilityStatus;
            }
            set
            {
                _availabilityStatus = value;
                base.RaisePropertyChanged();
            }
        }



        #endregion


        /// <summary>
        /// Standard-Konstruktor
        /// </summary>
        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(ConnectToServer);
            TransactionCommand = new RelayCommand(BeginTransaction);

            InitTimer();
        }

        /// <summary>
        /// Gibt die angegebene Log-Information in der RichTextBox aus.
        /// </summary>
        /// <param name="text">Information, die ausgegeben werden soll</param>
        public void WriteOutput(string text)
        {
            output = output + "\n" + text;
        }

        /// <summary>
        /// Baut einen Connection String aus gegebenen Parametern zusammen.
        /// </summary>
        /// <returns>Connection String</returns>
        private void BuildConnectionString()
        {
            conn.ConnectionString = "Server=" + server + ";" + "Database=" + database + ";" + "User=" + user + ";" + "Password=" + password;
        }

        private void ExecuteCommand()
        {
            using (conn)
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM TableName", conn);
            }
        }

        private void ConnectToServer()
        {
            WriteOutput("Versuche Verbindung herzustellen...");
            BuildConnectionString();

            
                try
                {
                    conn.Open();
                    WriteOutput("Verbindung zum Server erfolgreich");
                }
                catch (Exception ex)
                {
                    WriteOutput("Verbindung zum Server fehlgeschlagen");
                }
            
            
        }

        /// <summary>
        /// Initialisierung des Timers
        /// </summary>
        private void InitTimer()
        {
            availabilityTimer = new DispatcherTimer();
            availabilityTimer.Tick += new EventHandler(availabilityTimer_Tick);
            availabilityTimer.Interval = new TimeSpan(0,0,5);
            availabilityTimer.Start();
        }

        private void availabilityTimer_Tick(object sender, EventArgs e)
        {
            CheckServerAvailability();
        }

        /// <summary>
        /// Überprüft alle fünf Sekunden, ob Server online/offline ist
        /// </summary>
        private void CheckServerAvailability()
        {
            using (conn)
            {
                try
                {
                    conn.Open();
                    availabilityStatus = "Server online";
                }
                catch (Exception)
                {
                    availabilityStatus = "Server offline";
                }
            }
        }

        private void BeginTransaction()
        {
            BuildConnectionString();
            using (conn)
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                SqlTransaction transaction;

                transaction = conn.BeginTransaction("SampleTransaction");

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                try
                {
                    cmd.CommandText = "INSERT INTO [test_app].[dbo].[user] VALUES ('UserTest', 'ZZZ', '12414kmalksd')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO [test_app].[dbo].[auto] VALUES ('AutoTest', 'AAA', 'asdasdasd')";
                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                    WriteOutput("Alle INSERT-Anweisungen erfolgreich ausgeführt");
                }
                catch (Exception ex)
                {
                    WriteOutput("Fehler während des Commits aufgetreten" + ex);

                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        WriteOutput("Fehler während des Rollbacks aufgetreten");
                    }
                }
            }
        }
    }
}
