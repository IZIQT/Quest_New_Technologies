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

namespace Quest_New_Technologies_2.ViewModel
{
    class MainViewModel:ViewModelBase
    {
        private bool enableButtonTestExportCSV;
        public bool EnableButtonTestExportCSV
        {
            get => enableButtonTestExportCSV;
            set
            {
                if (enableButtonTestExportCSV == value) return;
                enableButtonTestExportCSV = value;
                OnPropertyChanged(nameof(EnableButtonTestExportCSV));
            }
        }

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
        public MainViewModel()
        {
            RunExportCSVVisiblity = Visibility.Collapsed;
            EnableButtonExportCSV = true;
            EnableButtonTestExportCSV = true;
            ExportCSVFile = new RelayCommand(ExportCSVFileExecute);
            ExitExportCSV = new RelayCommand(ExitExportCSVExecute);
            ExitExportCSVTestExit = new RelayCommand(ExitExportCSVTestExitExecute);
        }

        private async void ExitExportCSVTestExitExecute(object obj)
        {
            MainWindowDataGrid = new ObservableCollection<MainWindowDataGridModel>();
            cts = new CancellationTokenSource();
            token = cts.Token;
            await Task.Run(() =>
            {
                EnableButtonTestExportCSV = false;
                MainWindowDataGrid = TestExitExcute(token);
                EnableButtonTestExportCSV = true;
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
                        MainWindowDataGrid = ReadFileExcute(openFileDialog.FileName, token);
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

        private ObservableCollection<MainWindowDataGridModel> ReadFileExcute(string FileName, CancellationToken token)
        {
            using (StreamReader sr = new StreamReader(FileName, Encoding.Default))
            {
                ObservableCollection<MainWindowDataGridModel> localOC = new ObservableCollection<MainWindowDataGridModel>();
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
                        localOC.Add(
                            new MainWindowDataGridModel()
                            {
                                Loc_Date = DateTime.Parse(getSplitLine[0]),
                                Object_A = getSplitLine[1],
                                Type_A = getSplitLine[2],
                                Object_B = getSplitLine[3],
                                Type_B = getSplitLine[4],
                                Direction = getSplitLine[5],
                                Color = getSplitLine[6],
                                Intensity = Convert.ToInt32(getSplitLine[7]),
                                LatitudeA = Convert.ToDouble(getSplitLine[8].Replace('.', ',')),
                                LongitudeA = Convert.ToDouble(getSplitLine[9].Replace('.', ',')),
                                LatitudeB = Convert.ToDouble(getSplitLine[10].Replace('.', ',')),
                                LongitudeB = Convert.ToDouble(getSplitLine[11].Replace('.', ','))
                            });
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

        private ObservableCollection<MainWindowDataGridModel> TestExitExcute(CancellationToken token)
        {
            ObservableCollection<MainWindowDataGridModel> localOC = new ObservableCollection<MainWindowDataGridModel>();
            ProgressBarMax = 9999999;
            ProgressBarValue = 0;
            RunExportCSVVisiblity = Visibility.Visible;
            for (int i = 0; i < 9999999; i++)
            {
                localOC.Add(
                new MainWindowDataGridModel()
                {
                    Loc_Date = DateTime.Parse("01,01,2020"),
                    Object_A = "test",
                    Type_A = "test",
                    Object_B = "test",
                    Type_B = "test",
                    Direction = "test",
                    Color = "test",
                    Intensity = 4,
                    LatitudeA = 60,
                    LongitudeA = 60,
                    LatitudeB = 60,
                    LongitudeB = 60
                });
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
