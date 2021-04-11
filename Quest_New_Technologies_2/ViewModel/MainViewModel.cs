using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Quest_New_Technologies_2.Common.MVVM;
using Quest_New_Technologies_2.Model;
using System.Configuration;
using System.Data.SqlClient;

using System.Windows.Controls;
using System.Data;

namespace Quest_New_Technologies_2.ViewModel
{
    class MainViewModel:ViewModelBase
    {
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True";
        SqlDataAdapter adapter;
        //DataTable phonesTable;
        SqlConnection connection = new SqlConnection(connectionString);

        private DataTable phonesTable;
        public DataTable PhonesTable
        {
            get => phonesTable;
            set
            {
                //if (phonesTable == value) return;
                phonesTable = value;
                OnPropertyChanged(nameof(PhonesTable));
            }
        }

        //private bool enableButtonTestExportCSV;
        //public bool EnableButtonTestExportCSV
        //{
        //    get => enableButtonTestExportCSV;
        //    set
        //    {
        //        if (enableButtonTestExportCSV == value) return;
        //        enableButtonTestExportCSV = value;
        //        OnPropertyChanged(nameof(EnableButtonTestExportCSV));
        //    }
        //}

        private bool enableButtonExportCSV;
        public bool EnableButtonExportCSV
        {
            get => enableButtonExportCSV;
            set
            {
                if (enableButtonExportCSV == value) return;
                enableButtonExportCSV = value;
                OnPropertyChanged(nameof(EnableButtonExportCSV));
            }
        }
        
        private Visibility runExportCSVVisiblity;
        public Visibility RunExportCSVVisiblity
        {
            get => runExportCSVVisiblity;
            set
            {
                if (runExportCSVVisiblity == value) return;
                runExportCSVVisiblity = value;
                OnPropertyChanged(nameof(RunExportCSVVisiblity));
            }
        }

        private int progressBarMax;
        public int ProgressBarMax
        {
            get => progressBarMax;
            set
            {
                if (progressBarMax == value) return;
                progressBarMax = value;
                OnPropertyChanged(nameof(ProgressBarMax));
            }
        }

        private int progressBarValue;
        public int ProgressBarValue
        {
            get => progressBarValue;
            set
            {
                if (progressBarValue == value) return;
                progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }

        private string selectedCSVFile;
        public string SelectedCSVFile
        {
            get => selectedCSVFile;
            set
            {
                if (selectedCSVFile == value) return;
                selectedCSVFile = value;
                OnPropertyChanged(nameof(SelectedCSVFile));
            }
        }

        private ObservableCollection<MainWindowDataGridModel> mainWindowDataGrid;
        public ObservableCollection<MainWindowDataGridModel> MainWindowDataGrid
        {
            get => mainWindowDataGrid;
            set
            {
                if (mainWindowDataGrid == value) return;
                mainWindowDataGrid = value;
                OnPropertyChanged(nameof(MainWindowDataGrid));
            }
        }

        public ICommand ExportCSVFile { get; set; }
        public ICommand ExitExportCSV { get; set; }
        public ICommand ExitExportCSVTestExit { get; set; }
        public ICommand ExportDataToBase { get; set; }
        public ICommand ImportDataToBase { get; set; }
        public MainViewModel()
        {
            RunExportCSVVisiblity = Visibility.Collapsed;
            EnableButtonExportCSV = true;
            //EnableButtonTestExportCSV = true;
            ExportCSVFile = new RelayCommand(ExportCSVFileExecute);
            ExitExportCSV = new RelayCommand(ExitExportCSVExecute);
            ExitExportCSVTestExit = new RelayCommand(ExitExportCSVTestExitExecute);
            ExportDataToBase = new RelayCommand(ExportDataToBaseExecute);
            ImportDataToBase = new RelayCommand(ImportDataToBaseExecute);

            //connectionString = ConfigurationManager.ConnectionStrings[2].ConnectionString;
            //connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True";
        }

        private void ImportDataToBaseExecute(object obj)
        {
            EnableButtonExportCSV = false;
            Task.Run(() => {
                string sql = "SELECT * FROM DATA_TABLE";
                //if(phonesTable == null)
                //{
                phonesTable = new DataTable();
                //}
                try
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    adapter = new SqlDataAdapter(command);

                    connection.Open();
                    adapter.Fill(phonesTable);
                    OnPropertyChanged(nameof(PhonesTable));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }
                EnableButtonExportCSV = true;
            });
            
        }

        private void ExportDataToBaseExecute(object obj)
        {
            EnableButtonExportCSV = false;
            Task.Run(() => {
                try
                {
                    if (phonesTable == null)
                    {
                        phonesTable = new DataTable();
                    }
                    string sql = "SELECT * FROM DATA_TABLE";
                    SqlCommand command = new SqlCommand(sql, connection);
                    adapter = new SqlDataAdapter(command);

                    connection.Open();
                    SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter);
                    adapter.AcceptChangesDuringUpdate = true;
                    adapter.DeleteCommand = comandbuilder.GetDeleteCommand(true);
                    adapter.UpdateCommand = comandbuilder.GetUpdateCommand(true);
                    adapter.InsertCommand = comandbuilder.GetInsertCommand(true);

                    adapter.Update(phonesTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }
                EnableButtonExportCSV = true;
            });
        }

        private async void ExitExportCSVTestExitExecute(object obj)
        {
            MainWindowDataGrid = new ObservableCollection<MainWindowDataGridModel>();
            cts = new CancellationTokenSource();
            token = cts.Token;
            await Task.Run(() =>
            {
                EnableButtonExportCSV = false;
                PhonesTable = TestExitExcute(token);
                OnPropertyChanged(nameof(PhonesTable));
                EnableButtonExportCSV = true;
            });
            
        }

        static CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;
        private void ExitExportCSVExecute(object obj)
        {
            cts.Cancel();
        }

        private async void ExportCSVFileExecute(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Csv файлы(*.csv)|*.csv|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MainWindowDataGrid = new ObservableCollection<MainWindowDataGridModel>();
                    cts = new CancellationTokenSource();
                    token = cts.Token;
                    await Task.Run(() =>
                    {
                        EnableButtonExportCSV = false;
                        PhonesTable = ReadFileExcute(openFileDialog.FileName, token);
                        OnPropertyChanged(nameof(PhonesTable));
                        EnableButtonExportCSV = true;
                    });

                }
                catch (System.IO.IOException)
                {
                    MessageBox.Show("Выбранный файл занят. Прожалуйста выбирете другой файл.", "Ошибка!");
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Непредвиденная ошибка!\n]n" + exp, "Ошибка!");
                }
            }
        }

        private DataTable ReadFileExcute(string FileName, CancellationToken token)
        {
            using (StreamReader sr = new StreamReader(FileName, Encoding.Default))
            {
                DataTable localOC = new DataTable();

                localOC.Columns.Add("id", typeof(int));
                localOC.Columns.Add("loc_Date", typeof(DateTime));
                localOC.Columns.Add("Object_A", typeof(string));
                localOC.Columns.Add("Type_A", typeof(string));
                localOC.Columns.Add("Object_B", typeof(string));
                localOC.Columns.Add("Type_B", typeof(string));
                localOC.Columns.Add("Direction", typeof(string));
                localOC.Columns.Add("Color", typeof(string));
                localOC.Columns.Add("Intensity", typeof(int));
                localOC.Columns.Add("LatitudeA", typeof(double));
                localOC.Columns.Add("LongitudeA", typeof(double));
                localOC.Columns.Add("LatitudeB", typeof(double));
                localOC.Columns.Add("LongitudeB", typeof(double));

                ProgressBarMax = System.IO.File.ReadAllLines(FileName).Length -1;
                ProgressBarValue = 0;
                RunExportCSVVisiblity = Visibility.Visible;
                string getLine;
                while ((getLine = sr.ReadLine()) != null)
                {
                    string[] getSplitLine = getLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    DateTime tryToDate;
                    if (getSplitLine.Length == 12 && DateTime.TryParse(getSplitLine[0], out tryToDate))
                    {
                        var row = localOC.NewRow();
                        row.SetField("loc_Date", DateTime.Parse(getSplitLine[0]));
                        row.SetField("Object_A", getSplitLine[1]);
                        row.SetField("Type_A", getSplitLine[2]);
                        row.SetField("Object_B", getSplitLine[3]);
                        row.SetField("Type_B", getSplitLine[4]);
                        row.SetField("Direction", getSplitLine[5]);
                        row.SetField("Color", getSplitLine[6]);
                        row.SetField("Intensity", Convert.ToInt32(getSplitLine[7]));
                        row.SetField("LatitudeA", Convert.ToDouble(getSplitLine[8].Replace('.', ',')));
                        row.SetField("LongitudeA", Convert.ToDouble(getSplitLine[9].Replace('.', ',')));
                        row.SetField("LatitudeB", Convert.ToDouble(getSplitLine[10].Replace('.', ',')));
                        row.SetField("LongitudeB", Convert.ToDouble(getSplitLine[11].Replace('.', ',')));
                        localOC.Rows.Add(row);

                        ProgressBarValue += 1;
                        if (token.IsCancellationRequested)
                        {
                            RunExportCSVVisiblity = Visibility.Collapsed;
                            return localOC;
                        }
                    }
                }
                RunExportCSVVisiblity = Visibility.Collapsed;
                return localOC;
            }
        }

        private DataTable TestExitExcute(CancellationToken token)
        {
            DataTable localOC = new DataTable();

            localOC.Columns.Add("id", typeof(int));
            localOC.Columns.Add("loc_Date", typeof(DateTime));
            localOC.Columns.Add("Object_A", typeof(string));
            localOC.Columns.Add("Type_A", typeof(string));
            localOC.Columns.Add("Object_B", typeof(string));
            localOC.Columns.Add("Type_B", typeof(string));
            localOC.Columns.Add("Direction", typeof(string));
            localOC.Columns.Add("Color", typeof(string));
            localOC.Columns.Add("Intensity", typeof(int));
            localOC.Columns.Add("LatitudeA", typeof(double));
            localOC.Columns.Add("LongitudeA", typeof(double));
            localOC.Columns.Add("LatitudeB", typeof(double));
            localOC.Columns.Add("LongitudeB", typeof(double));

            ProgressBarMax = 9999999;
            ProgressBarValue = 0;
            RunExportCSVVisiblity = Visibility.Visible;
            for (int i = 0; i < 9999999; i++)
            {

                var row = localOC.NewRow();
                row.SetField("loc_Date", DateTime.Parse("01,01,2020"));
                row.SetField("Object_A", "test");
                row.SetField("Type_A", "test");
                row.SetField("Object_B", "test");
                row.SetField("Type_B", "test");
                row.SetField("Direction", "test");
                row.SetField("Color", "test");
                row.SetField("Intensity", 4);
                row.SetField("LatitudeA", 60);
                row.SetField("LongitudeA", 60);
                row.SetField("LatitudeB", 60);
                row.SetField("LongitudeB", 60);
                localOC.Rows.Add(row);

                ProgressBarValue += 1;
                if (token.IsCancellationRequested)
                {
                    RunExportCSVVisiblity = Visibility.Collapsed;
                    return localOC;
                }
            }
            RunExportCSVVisiblity = Visibility.Collapsed;
            return localOC;
        }

    }
}
