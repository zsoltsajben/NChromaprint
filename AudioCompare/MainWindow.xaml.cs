using Microsoft.Win32;
using NChromaCompare;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NChromaprintUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Input> Inputs { get; set; }
        public Dictionary<string, Dictionary<string, float>> Similarities { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _dataIsProcessed;
        private bool _isProcessing;
        public bool DataIsProcessed
        {
            get { return _dataIsProcessed; }
            private set
            {
                _dataIsProcessed = value;
                NotifyPropertyChanged("DataIsProcessed");
            }
        }
        public bool IsProcessing
        {
            get { return _isProcessing; }
            private set
            {
                _isProcessing = value;
                NotifyPropertyChanged("IsProcessing");
            }
        }

        private readonly BackgroundWorker worker;


        public MainWindow()
        {
            Inputs = new ObservableCollection<Input>();
            DataIsProcessed = false;

            InitializeComponent();

            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            inputFilesListbox.SelectionChanged += inputFilesListbox_SelectionChanged;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            worker.Dispose();
        }


        void inputFilesListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Similarities != null)
            {
                var lb = (ListBox)sender;
                var items = lb.Items;

                if (items != null && items.Count > 1)
                {
                    var selected = lb.SelectedItem as Input;

                    if (selected != null)
                    {
                        similarityDataGrid.ItemsSource = Similarities[selected.FilePath];

                        // a %-ok szerinti rendezés lesz a default, ha nincs más beállítva
                        if (similarityDataGrid.Items.SortDescriptions.Count == 0)
                        {
                            similarityDataGrid.Items.SortDescriptions.Add(
                                new SortDescription("Value", ListSortDirection.Descending));
                        }
                        else
                        {
                            var sortDescriptions = new SortDescriptionCollection();
                            foreach (var sd in similarityDataGrid.Items.SortDescriptions)
                            {
                                sortDescriptions.Add(sd);
                            }

                            similarityDataGrid.Items.SortDescriptions.Clear();

                            foreach (var sd in sortDescriptions)
                            {
                                similarityDataGrid.Items.SortDescriptions.Add(sd);
                            }
                        }

                        return;
                    }
                }
            }

            similarityDataGrid.ItemsSource = null;
        }

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }

        private void InvalidateSimilarityData()
        {
            Similarities = null;
            DataIsProcessed = false;
        }

        private void addFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Supported audio files|*.mp3;*.wav;*.aiff";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == true)
            {
                foreach (var filePath in dialog.FileNames)
                {
                    if (!Inputs.Any(item => item.FilePath.Equals(filePath)))
                    {
                        Inputs.Add(new Input(filePath));
                    }
                }

                InvalidateSimilarityData();
            }
        }

        private void removeFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedIdx = inputFilesListbox.SelectedIndex;

            if (selectedIdx >= 0)
            {
                Inputs.RemoveAt(selectedIdx);

                InvalidateSimilarityData();
            }
        }

        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            if (Inputs.Count < 2)
            {
                MessageBox.Show("There aren't enough files in the list to compare!");
                return;
            }

            inputFilesListbox.UnselectAll();
            IsProcessing = true;
            InvalidateSimilarityData();
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var comparer = new NChromaComparer();
                Similarities = comparer.CompareFiles(Inputs);
            }
            catch (Exception)
            {
                MessageBox.Show("Couldn't process all of the files, at least one of them is in an unsupported format!");
                return;
            }

            DataIsProcessed = true;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
        }
    }
}
