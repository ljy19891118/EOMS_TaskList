using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace TaskList
{
    class NetWork
    {
        private static System.Windows.Threading.Dispatcher dispatcher;


        // private static Proxy proxy = new Proxy();
        private static Browser brower = new Browser();
        

        internal static void Init(System.Windows.Threading.Dispatcher dispatcher)
        {
            NetWork.dispatcher = dispatcher;
            brower.Show();
            brower.Hide();
        }

        internal static void ShowBrowser()
        {
            
            NetWork.dispatcher.Invoke(new Action(() => { brower.Show(); }));
        }

        internal static void HideBrowser()
        {
            
            NetWork.dispatcher.Invoke(new Action(() => { brower.Hide(); }));
        }

        internal static void CloseBrowser()
        {
                
            NetWork.dispatcher.Invoke(new Action(() => { brower.Close(); }));

        }

        internal static void ResetBrowser()
        {
            try
            { 
                brower.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            brower = new Browser();
            brower.Show();
            brower.Hide();
        }

        internal static void doWork()
        {
            foreach (User user in Dao.SelectAllOKUsers())
            {
                DealWork(user);
            }
        }


        private static void DealWork(User user)
        {
            NetWork.dispatcher.Invoke(new Action(() => { brower.Deal(user); }));
            for (int i = 0; i < Constant.WAIT_TIME; i++)
            {
                Thread.Sleep(1000);
                if (!brower.IsBusy)
                {
                    break;
                }
                else if (i == Constant.WAIT_TIME - 1)
                {
                    throw new TimeoutException();
                    //NetWork.dispatcher.Invoke(new Action(() => { ResetBrowser(); }));
                }
            }
            NetWork.dispatcher.Invoke(new Action(() => { brower.Finish(); }));
            for (int i = 0; i < Constant.WAIT_TIME; i++)
            {
                Thread.Sleep(1000);
                if (!brower.IsBusy)
                {
                    break;
                }
                else if (i == Constant.WAIT_TIME - 1)
                {
                    throw new TimeoutException();
                    //NetWork.dispatcher.Invoke(new Action(() => { ResetBrowser(); }));
                }
            }
        }

        //private static void DealWork(User user)
        //{
        //    List<Task> tasks = new List<Task>();
        //   // Proxy proxy = GetProxy(user.UserName);
        //    proxy.Login(user);


        //    if(user.OnDuty == null || user.OnDuty != Dao.GetToday())
        //    {
        //        foreach (Task task in Dao.SelectCtlByUser(user.UserName))
        //        {
        //            if (task.Status != Constant.FINISHED)
        //                RefreshWork(task);
        //        }
        //        proxy.Logout();
        //        return;
        //    }

        //    foreach (string eid in proxy.FindTaskEIDList())
        //    {
        //        Task task = proxy.FindTask(eid);
        //        if (task != null)
        //        {
        //            tasks.Add(task);
        //        }
        //    }
        //    foreach (Task task in tasks)
        //    {
        //        if(task.Status == Constant.EXCEPTION)
        //        {
        //            continue;
        //        }
        //        if(user.Type == Constant.TYPE_CHUANSHU && !task.Type.Contains(Constant.TYPE_CHUANSHU))
        //        {
        //            continue;
        //        }
        //        if(user.Type != Constant.TYPE_CHUANSHU && task.Type.Contains(Constant.TYPE_CHUANSHU)){
        //            continue;
        //        }
        //        NetWork.dispatcher.Invoke(new Action(() => { proxy.DealTask(task); }));
        //        for (int i = 0; i < Constant.WAIT_TIME; i++)
        //        {
        //            Thread.Sleep(1000);                  
        //            if (proxy.TaskIsFinished())
        //            {
        //                Dao.Save(task);
        //                break;
        //            }
        //            else if(i == Constant.WAIT_TIME - 1)
        //            {
        //                NetWork.dispatcher.Invoke(new Action(() => { proxy.ResetBrowser(); }));
        //            }
        //        }
        //    }
        //    NetWork.dispatcher.Invoke(new Action(() => { proxy.Done(); }));
        //    for (int i = 0; i < Constant.WAIT_TIME; i++)
        //    {
        //        Thread.Sleep(1000);
        //        if (proxy.TaskIsFinished())
        //        {
        //            break;
        //        }
        //        else if (i == Constant.WAIT_TIME - 1)
        //        {
        //            NetWork.dispatcher.Invoke(new Action(() => { proxy.ResetBrowser(); }));
        //        }
        //    }


        //    proxy.Logout();

        //}

        //private static void RefreshWork(Task task)
        //{

        //    Task task1 = proxy.FindTask(task.EID);
        //    if (task1 != null)
        //    {
        //        Dao.Save(task1);
        //    }

        //}

        //private static Proxy GetProxy(string userName)
        //{
        //    Proxy proxy = null;
        //    if (cacheP.ContainsKey(userName))
        //    {
        //        cacheP.TryGetValue(userName, out proxy);
        //    }
        //    else
        //    {
        //        proxy = new Proxy();
        //        cacheP.Add(userName, proxy);
        //    }
        //    return proxy;
        //}

    }
}
