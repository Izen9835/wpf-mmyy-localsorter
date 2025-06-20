using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderMMYYSorter_2.MVVM.Model;
using Microsoft.Data.SqlClient;
using Win32 = System.Windows;


namespace FolderMMYYSorter_2.IO
{
    class SqlHelper : INotifyPropertyChanged
    {
        public ObservableCollection<string> DatabaseNames { get; set; }

        private string _DBName = "";
        public string DBName
        {
            get { return _DBName; }
            set
            {
                if (_DBName != value)
                {
                    _DBName = value;
                    GetFileTypes();
                }
            }
        }

        private HashSet<string> IC_List;
        private HashSet<string> Gov_List;




        // event handler to reflect changes in ViewModel to UI
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SqlHelper()
        {
            DatabaseNames = new ObservableCollection<string>();
            GetDatabaseNames("localhost");
        }


        public List<string> GetDatabaseNames(string serverName)
        {
            var databaseNames = new List<string>();
            // Connect to the master database
            string connectionString =
                $"Server={serverName};" +
                $"Database=master;" +
                $"Integrated Security=True;" +
                $"Encrypt=True;" +
                $"TrustServerCertificate=True;";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name FROM sys.databases", con))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        databaseNames.Add(dr.GetString(0));
                    }
                }
            }

            // update UI
            Win32.Application.Current.Dispatcher.Invoke(() =>
            {
                DatabaseNames.Clear();

                foreach (var item in databaseNames)
                {
                    Debug.WriteLine(item);
                    DatabaseNames.Add(item);
                }
            });

            return databaseNames;
        }

        public bool isGettingFileTypes = false;


        public async void GetFileTypes()
        {
            isGettingFileTypes = true;

            string connectionString =
                $"Server='localhost';" +
                $"Database={DBName};" +
                $"Integrated Security=True;" +
                $"Encrypt=True;" +
                $"TrustServerCertificate=True;";

            string sqlStatementIC = "CompReg_GetIC";

            string sqlStatementGov = "CompReg_GetGov";


            await Task.Run(() =>
            {
                try
                {

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();



                        // get IC data
                        var rawIC = new List<string>();
                        SqlCommand cmd = new SqlCommand(sqlStatementIC, con);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                rawIC.Add(dr.GetString(0));
                            }
                        }

                        IC_List = new HashSet<string>(
                            rawIC.Select(s => s.Trim().Normalize(NormalizationForm.FormC)),
                            StringComparer.OrdinalIgnoreCase);

                        Debug.WriteLine("IC sql data stored");

                        // get Gov data
                        var rawGov = new List<string>();
                        cmd = new SqlCommand(sqlStatementGov, con);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                rawGov.Add(dr.GetString(0));
                            }
                        }

                        Gov_List = new HashSet<string>(
                            rawGov.Select(s => s.Trim().Normalize(NormalizationForm.FormC)),
                            StringComparer.OrdinalIgnoreCase);

                        Debug.WriteLine("Gov sql data stored");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQL DB read failed: " + ex);
                }
                Debug.WriteLine("SQL DB read success.");
            });

            isGettingFileTypes = false;

        }

        public bool hasSQLData()
        {
            if (IC_List == null || Gov_List == null) return false;
            return (IC_List.Count != 0 && Gov_List.Count != 0);
        }

        public string filetypeOf(string name)
        {
            name = name.Trim().Normalize(NormalizationForm.FormC);

            Debug.WriteLine(name);

            if (IC_List.Contains(name)) return "IC";
            if (Gov_List.Contains(name)) return "Gov";
            return "Unknown";
        }
    }
}
