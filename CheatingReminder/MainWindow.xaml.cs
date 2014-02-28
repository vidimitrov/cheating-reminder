using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CheatingReminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeButtons();
            InitializeCheatingTasks();                    //Gets the cheating tasks as IList<string>
            GetMatchesFromMachine(cheatingTasks);         //Find matches with the cheating tasks and current machine processes
            FillTheTasksList(currentCheatingTasks);       //Fill the ScrollViewer with the matches
        }

        private const string InputCheatingListUriAsString = @"http://vidimitrov.com/CheatingTasks.txt"; 
        private const string LocalFileName = "CheatingTasksList.txt";
        private IList<string> cheatingTasks = new List<string>();
        private IList<Task> currentCheatingTasks = new List<Task>();

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = "Process name...";
        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Border).Name.ToString() == "KillButton")
            {
                KillTasks(currentCheatingTasks);
                Thread.Sleep(1000);
                RefreshTasksList();
            }
            else if ((sender as Border).Name.ToString() == "AddButton")
            {
                if (!string.IsNullOrEmpty(this.ProcessToAdd.Text) && (this.ProcessToAdd.Text != "Process name..."))
                {
                    AddNewTask(this.ProcessToAdd.Text);
                    MessageBox.Show("New task successfully added!");
                    this.ProcessToAdd.Text = "";
                    RefreshTasksList();
                }
                else
                {
                    MessageBox.Show("Incorrect process name!");
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            (sender as Border).Background = (Brush)bc.ConvertFrom("#D83C3D");
        }
  
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            (sender as Border).Background = (Brush)bc.ConvertFrom("#C63B3E");
        }

        private void InitializeButtons()
        {
            this.AddButton.MouseEnter += new MouseEventHandler(Button_MouseEnter);
            this.AddButton.MouseLeave += new MouseEventHandler(Button_MouseLeave);
            this.AddButton.MouseLeftButtonDown += new MouseButtonEventHandler(Button_MouseLeftButtonDown);
            
            this.KillButton.MouseEnter += new MouseEventHandler(Button_MouseEnter);
            this.KillButton.MouseLeave += new MouseEventHandler(Button_MouseLeave);
            this.KillButton.MouseLeftButtonDown += new MouseButtonEventHandler(Button_MouseLeftButtonDown);
        }
  
        private void InitializeCheatingTasks()
        {
            if (File.Exists("CheatingTasksList.txt"))
            {
                string sMessageBoxText = "There is already a local copy of the cheating tasks list. Do you want to download the latest version even though?";
                string sCaption = "Cheating Reminder";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                MessageBoxImage icnMessageBox = MessageBoxImage.Question;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                if (rsltMessageBox == MessageBoxResult.Yes)
                {
                    //If the user want to download the latest version merge the local file and the file that is on server.
                    //Don't replace everything. If you do that you will lose the user's data.
                    //TODO: MergeLocalAndServerFiles(); 
                    LoadCheatingListFromServer();
                }
            }
            else
            {
                LoadCheatingListFromServer();
            }

            LoadTasksList();
        }

        private void LoadCheatingListFromServer()
        {
            var webClient = new WebClient();

            try
            {
                webClient.DownloadFile(new Uri(InputCheatingListUriAsString), LocalFileName);
            }
            catch (Exception)
            {
                string sMessageBoxText = "There are some problems with your internet connection. If you want to make a local custom cheating list and continue click \"Yes\"!";
                string sCaption = "Cheating Reminder";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                MessageBoxImage icnMessageBox = MessageBoxImage.Question;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                if (rsltMessageBox == MessageBoxResult.Yes)
                {
                    File.Create(LocalFileName);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        private void LoadTasksList()
        {
            cheatingTasks = new List<string>();
            StreamReader inputCheatingTasksList = new StreamReader(LocalFileName);
            using (inputCheatingTasksList)
            {
                string line = inputCheatingTasksList.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    cheatingTasks.Add(line);
                }
                while (line != null)
                {
                    line = inputCheatingTasksList.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        cheatingTasks.Add(line);
                    }
                }
            }
        }

        private void GetMatchesFromMachine(IList<string> cheatingTasks)
        {
            currentCheatingTasks = new List<Task>();
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                foreach (var cheatingTask in cheatingTasks)
                {
                    if (process.ProcessName == cheatingTask)
                    {
                        currentCheatingTasks.Add(new Task(cheatingTask, process));
                    }
                }
            }
        }

        private void FillTheTasksList(IList<Task> currentCheatingTasks)
        {
            foreach (var task in currentCheatingTasks)
            {
                CheckBox taskCheckBox = new CheckBox();
                taskCheckBox.Content = task.Name;
                taskCheckBox.FontFamily = new FontFamily("SegoeUI");
                taskCheckBox.FontSize = 16;
                taskCheckBox.Checked += TaskCheckBox_Changed;
                taskCheckBox.Unchecked += TaskCheckBox_Changed;
                var bc = new BrushConverter();
                taskCheckBox.Foreground = (Brush)bc.ConvertFrom("#FCFFF1");
                this.TaskList.Children.Add(taskCheckBox);
            }
        }

        void TaskCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            
            if (chkBox.IsChecked == true)
            {
                if (chkBox.Name == "SelectAll")
                {
                    foreach (var checkBox in this.TaskList.Children)
                    {
                        (checkBox as CheckBox).IsChecked = true;
                        currentCheatingTasks.Where(t => t.Name == (checkBox as CheckBox).Content).FirstOrDefault().IsChecked = true;
                    }
                }
                else
                {
                    currentCheatingTasks.Where(t => t.Name == chkBox.Content).FirstOrDefault().IsChecked = true;
                }
            }
            else if (chkBox.IsChecked == false)
            {
                if (chkBox.Name == "SelectAll")
                {
                    foreach (var checkBox in this.TaskList.Children)
                    {
                        (checkBox as CheckBox).IsChecked = false;
                        currentCheatingTasks.Where(t => t.Name == (checkBox as CheckBox).Content).FirstOrDefault().IsChecked = false;
                    }
                }
                else 
                {
                    currentCheatingTasks.Where(t => t.Name == chkBox.Content).FirstOrDefault().IsChecked = false;
                }
            }
        }

        private void KillTasks(IList<Task> tasksToKill)
        {
            var killedTasks = new List<Task>();

            foreach (var task in tasksToKill)
            {
                if (task.IsChecked)
                {
                    task.Process.Kill();
                    killedTasks.Add(task);
                }
            }

            if (killedTasks.Count == 0)
            {
                MessageBox.Show("You must select some task first.");
                return;
            }

            foreach (var task in killedTasks)
            {
                currentCheatingTasks.Remove(task);
            }
        }

        private void AddNewTask(string taskName)
        {
            StreamWriter outputCheatingTasksList = new StreamWriter(LocalFileName, true);
            outputCheatingTasksList.WriteLine("\n" + taskName);
            outputCheatingTasksList.Close();
        }

        private void RefreshTasksList()
        {
            this.TaskList.Children.Clear();
            LoadTasksList();
            GetMatchesFromMachine(cheatingTasks);
            FillTheTasksList(currentCheatingTasks);
        }
    }
}