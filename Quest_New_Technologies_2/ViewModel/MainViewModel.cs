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
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Globalization;

namespace Quest_New_Technologies_2.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        #region Переменные
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True";
        SqlConnection connection = new SqlConnection(connectionString);
        SqlDataAdapter adapter;

        static CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        private DataTable phonesTable;
        public DataTable PhonesTable
        {
            get => phonesTable;
            set
            {
                if (phonesTable == value) return;
                phonesTable = value;
                OnPropertyChanged(nameof(PhonesTable));
            }
        }

        private bool enableButtonExportCSV;
        public bool EnableButtonExportCSV
        {
            get => enableButtonExportCSV;
            set
            {
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
                mainWindowDataGrid = value;
                OnPropertyChanged(nameof(MainWindowDataGrid));
            }
        }

        public ICommand ExportCSVFile { get; set; }
        public ICommand ExitExportCSV { get; set; }
        public ICommand ExitExportCSVTestExit { get; set; }
        public ICommand ExportDataToBase { get; set; }
        public ICommand ImportDataToBase { get; set; }
        #endregion

        #region Конструктор
        public MainViewModel()
        {
            RunExportCSVVisiblity = Visibility.Collapsed;
            EnableButtonExportCSV = true;
            ExportCSVFile = new RelayCommand(ExportCSVFileExecute);
            ExitExportCSV = new RelayCommand(ExitExportCSVExecute);
            ExitExportCSVTestExit = new RelayCommand(ExitExportCSVTestExitExecute);
            ExportDataToBase = new RelayCommand(ExportDataToBaseExecute);
            ImportDataToBase = new RelayCommand(ImportDataToBaseExecute);
        }
        #endregion

        #region Функции

        #region Получение данных из базы
        private void ImportDataToBaseExecute(object obj)
        {
            try
            {
                EnableButtonExportCSV = false;
                Task.Run(() =>
                {
                    string sql = "SELECT * FROM DATA_TABLE";
                    phonesTable = new DataTable();

                    SqlCommand command = new SqlCommand(sql, connection);
                    adapter = new SqlDataAdapter(command);

                    connection.Open();
                    adapter.Fill(phonesTable);
                    OnPropertyChanged(nameof(PhonesTable));
                });
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка! \n\n" + exp, "Ошибка!");
            }
            finally
            {
                EnableButtonExportCSV = true;
                if (connection != null)
                    connection.Close();
            }
        }
        #endregion

        #region Загрузка в базу
        private void ExportDataToBaseExecute(object obj)
        {
            EnableButtonExportCSV = false;
            try
            {
                Task.Run(() =>
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
                });
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка! \n\n" + exp, "Ошибка!");
            }
            finally
            {
                EnableButtonExportCSV = true;
                if (connection != null)
                    connection.Close();
            }

        }
        #endregion

        #region Тест для остановки из файла (милион строк)
        private async void ExitExportCSVTestExitExecute(object obj)
        {
            try
            {
                MainWindowDataGrid = new ObservableCollection<MainWindowDataGridModel>();
                cts = new CancellationTokenSource();
                token = cts.Token;
                await Task.Run(() =>
                {
                    EnableButtonExportCSV = false;
                    PhonesTable = TestExitExcute(token);
                    OnPropertyChanged(nameof(PhonesTable));
                });
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка! \n\n" + exp, "Ошибка!");
            }
            finally
            {
                EnableButtonExportCSV = true;
            }
        }
        #endregion

        #region Завершение процедуры загрузки
        private void ExitExportCSVExecute(object obj)
        {
            cts.Cancel();
        }

        #endregion

        #region Загрузка данных из CSV
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
                    });

                }
                catch (System.IO.IOException)
                {
                    MessageBox.Show("Выбранный файл занят. Прожалуйста выбирете другой файл.", "Ошибка!");
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Непредвиденная ошибка!\n\n" + exp, "Ошибка!");
                }
                finally
                {
                    EnableButtonExportCSV = true;
                }
            }
        }
        #endregion

        #region Процедура получение данных из CSV
        private DataTable ReadFileExcute(string FileName, CancellationToken token)
        {
            try
            {
                RunExportCSVVisiblity = Visibility.Visible;
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

                    ProgressBarMax = System.IO.File.ReadAllLines(FileName).Length - 1;
                    ProgressBarValue = 0;

                    string getLine;
                    List<int> check_colomn = new List<int>();
                    int j = 0;
                    while ((getLine = sr.ReadLine()) != null)
                    {
                        string[] getSplitLine = getLine.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (getSplitLine.Length == 12 && j > 0)
                        {
                            var row = localOC.NewRow();
                            row.SetField("loc_Date", getSplitLine[check_colomn.IndexOf(0)]);
                            row.SetField("Object_A", getSplitLine[check_colomn.IndexOf(1)]);
                            row.SetField("Type_A", getSplitLine[check_colomn.IndexOf(2)]);
                            row.SetField("Object_B", getSplitLine[check_colomn.IndexOf(3)]);
                            row.SetField("Type_B", getSplitLine[check_colomn.IndexOf(4)]);
                            row.SetField("Direction", getSplitLine[check_colomn.IndexOf(5)]);
                            row.SetField("Color", getSplitLine[check_colomn.IndexOf(6)]);
                            row.SetField("Intensity", getSplitLine[check_colomn.IndexOf(7)]);
                            row.SetField("LatitudeA", double.Parse(getSplitLine[check_colomn.IndexOf(8)], CultureInfo.InvariantCulture));
                            row.SetField("LongitudeA", double.Parse(getSplitLine[check_colomn.IndexOf(9)], CultureInfo.InvariantCulture));
                            row.SetField("LatitudeB", double.Parse(getSplitLine[check_colomn.IndexOf(10)], CultureInfo.InvariantCulture));
                            row.SetField("LongitudeB", double.Parse(getSplitLine[check_colomn.IndexOf(11)], CultureInfo.InvariantCulture));
                            localOC.Rows.Add(row);

                            ProgressBarValue += 1;
                            if (token.IsCancellationRequested)
                            {
                                return localOC;
                            }
                        }

                        if (j == 0)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                switch (getSplitLine[i])
                                {
                                    case "Date":
                                        check_colomn.Add(0);
                                        break;
                                    case "Object A":
                                        check_colomn.Add(1);
                                        break;
                                    case "Type A":
                                        check_colomn.Add(2);
                                        break;
                                    case "Object B":
                                        check_colomn.Add(3);
                                        break;
                                    case "Type B":
                                        check_colomn.Add(4);
                                        break;
                                    case "Direction":
                                        check_colomn.Add(5);
                                        break;
                                    case "Color":
                                        check_colomn.Add(6);
                                        break;
                                    case "Intensity":
                                        check_colomn.Add(7);
                                        break;
                                    case "LatitudeA":
                                        check_colomn.Add(8);
                                        break;
                                    case "LongitudeA":
                                        check_colomn.Add(9);
                                        break;
                                    case "LatitudeB":
                                        check_colomn.Add(10);
                                        break;
                                    case "LongitudeB":
                                        check_colomn.Add(11);
                                        break;
                                }
                            }
                            j++;
                        }
                    }
                    return localOC;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Ошибка\n\n" + exp, "Ошибка!");
                return null;
            }
            finally
            {
                RunExportCSVVisiblity = Visibility.Collapsed;
            }

        }
        #endregion

        #region Процедура генерации миллиона строк
        private DataTable TestExitExcute(CancellationToken token)
        {
            try
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
                        return localOC;
                    }
                }
                return localOC;
            }
            catch (Exception exp)
            {
                MessageBox.Show("Непредвиденная ошибка!\n\n" + exp, "Ошибка!");

                return null;
            }
            finally
            {
                RunExportCSVVisiblity = Visibility.Collapsed;
            }

        }
        #endregion

        #endregion
    }
}
