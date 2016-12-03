using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskList
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private int atlPageNo = 0;
        private int atlPageSize = 50;
        internal ObservableCollection<Task> ctl = new ObservableCollection<Task>();
        internal ObservableCollection<Task> atl = new ObservableCollection<Task>();
        internal ObservableCollection<User> users = new ObservableCollection<User>();
        internal ObservableCollection<User> deletingUsers = new ObservableCollection<User>();

        public MainWindow()
        {
            InitializeComponent();
            Dao.TryInit();
            InitTable();
            NetWork.Init(this.Dispatcher);
            StartTimer();
        }


        private void ctlDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow dataGridRow = e.Row;
            Task dataRow = e.Row.Item as Task;
            if (dataRow.Status == Constant.FINISHED)
            {
                dataGridRow.Background = Brushes.Green;
                dataGridRow.Foreground = Brushes.White;
            }
            else if (TimeUtil.FromUnixTimeStamp(dataRow.T2Time) < DateTime.Now.AddMinutes(Constant.WARNING_PERIOD_MINUTE))
            {
                dataGridRow.Background = Brushes.Red;
                dataGridRow.Foreground = Brushes.White;
            }
            else
            {
                dataGridRow.Background = Brushes.White;
                dataGridRow.Foreground = Brushes.Black;
            }
        }

        private void userDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow dataGridRow = e.Row;
            User dataRow = e.Row.Item as User;
            if (dataRow != null && dataRow.OnDuty != null && dataRow.OnDuty != "")
            {
                dataGridRow.Background = Brushes.Green;
                dataGridRow.Foreground = Brushes.White;
            }
            else if (dataRow != null && dataRow.IsOK != "1")
            {
                dataGridRow.Background = Brushes.Red;
                dataGridRow.Foreground = Brushes.White;
            }
        }


        private void StartTimer()
        {
            Timer refreshTimer = new Timer(Constant.REFRESH_TABLE_PERIOD_MINUTE * 60 * 1000);
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();

            //Timer networktimer = new Timer(Constant.WORK_PERIOD_MINUTE * 60 * 1000);
            //networktimer.Elapsed += NetWorkTimer_Elapsed;
            //networktimer.Start();
            // 立刻开始执行一次
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => { while (true) DoNetWork(); }));
            thread.Start();

        }

        private void NetWorkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoNetWork();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RefreshTable();
        }


        private void InitTable()
        {
            this.ctl = Dao.SelectCtl();

            this.atl = Dao.SelectAtl(atlPageSize, atlPageNo);
            this.users = Dao.SelectAllUsers();
            this.users.CollectionChanged += Users_CollectionChanged;
            Binding();
        }

        private void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (User u in e.OldItems)
                {
                    deletingUsers.Add(u);
                }
            }
        }

        private void RefreshTable()
        {
            // 刷新当前工单表
            this.Dispatcher.Invoke(new Action(() => this.ctl.Clear()));
            foreach (Task t in Dao.SelectCtl())
            {
                if (t.Status == Constant.FINISHED && TimeUtil.FromUnixTimeStamp(t.T2Time).AddDays(Constant.ARCH_PERIOD_DAY) < DateTime.Now)
                {
                    Dao.ArchTask(t.EID);
                    if (atlPageNo == 0)
                    {
                        if (this.atl.Count > 0)
                        {
                            this.Dispatcher.Invoke(new Action(() => this.atl.Remove(this.atl.Last())));
                        }
                        this.Dispatcher.Invoke(new Action(() => this.atl.Insert(0, t)));
                    }
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() => this.ctl.Add(t)));
                }
            }
        }

        private void Binding()
        {
            Binding binding1 = new Binding();
            binding1.Source = ctl;
            ctlDataGrid.SetBinding(DataGrid.ItemsSourceProperty, binding1);

            Binding binding2 = new Binding();
            binding2.Source = atl;
            atlDataGrid.SetBinding(DataGrid.ItemsSourceProperty, binding2);

            Binding binding3 = new Binding();
            binding3.Source = users;
            userDataGrid.SetBinding(DataGrid.ItemsSourceProperty, binding3);
        }

        private void saveUsersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (User u in deletingUsers)
            {
                Dao.Delete(u);
            }
            deletingUsers = new ObservableCollection<User>();
            foreach (User u in users)
            {
                if (u.Password == null || u.Password == "")
                {
                    u.Password = Constant.DEFAULT_PASSWORD;
                }
                Dao.Save(u);
            }
            // 刷新当前工单表
            this.Dispatcher.Invoke(new Action(() => this.users.Clear()));
            foreach (User t in Dao.SelectAllUsers())
            {
                this.Dispatcher.Invoke(new Action(() => this.users.Add(t)));
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            deletingUsers = new ObservableCollection<User>();
            // 刷新当前工单表
            this.Dispatcher.Invoke(new Action(() => this.users.Clear()));
            foreach (User t in Dao.SelectAllUsers())
            {
                this.Dispatcher.Invoke(new Action(() => this.users.Add(t)));
            }
        }

        private void firstPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (atlPageNo != 0)
            {
                this.atlPageNo = 0;
                this.atl.Clear();
                foreach (Task t in Dao.SelectAtl(atlPageSize, atlPageNo))
                {
                    this.atl.Add(t);
                }
            }
        }

        private void previousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (atlPageNo > 0)
            {
                this.atlPageNo -= 1;
                this.atl.Clear();
                foreach (Task t in Dao.SelectAtl(atlPageSize, atlPageNo))
                {
                    this.atl.Add(t);
                }
            }
        }

        private void nextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (atl.Count >= atlPageSize)
            {
                this.atlPageNo += 1;
                this.atl.Clear();
                foreach (Task t in Dao.SelectAtl(atlPageSize, atlPageNo))
                {
                    this.atl.Add(t);
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                NetWork.HideBrowser();
            }
            if (e.Key == Key.F12)
            {
                NetWork.ShowBrowser();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetWork.CloseBrowser();
        }

        private void DoNetWork()
        {
            try
            {
                NetWork.doWork();
            }
            catch(Exception ex)
            {
                MessageBox.Show("网络连接超时，请重新启动软件重试");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
