using CefSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebUtility;

namespace TaskList
{
    /// <summary>
    /// Browser.xaml 的交互逻辑
    /// </summary>
    public partial class Browser : Window
    {
        private User user = null;
        private Task task = null;
        internal Boolean IsBusy = false;
        private readonly string loginUrl = "/ultrawf/UltraWF/share/login.jsp";
        private readonly string mainUrl = "/ultrawf/UltraWF/share/main.jsp";
        private readonly string listUrl = "/ultrawf/UltraWF/manageprocess/all_workFlow.jsp?type=NotAssigned&invalidSearch=no";
        private readonly string taskUrl = "/arsys/forms/BMCC-EOMS-05/WF%3ABMCC_EOMS_ITDealFault/Default+Admin+View/?eid={0}";
        private readonly string infoUrl = "/arsys/BackChannel/?param=100%2FGetEntry%2F12%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault0%2F19%2FARRoot147236736612415%2F{0}2%2F0%2F1%2F1";
        private readonly string progressUrl = "/ultrawf/UltraWF/manageprocess/BaseInfoView.jsp?baseschema=WF:BMCC_EOMS_ITDealFault&baseid={0}";

        private string source = "";
        public Browser()
        {
            InitializeComponent();
           
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           // e.Cancel = true;
           // this.Hide();
        }

        internal bool Deal(User user)
        {
            this.IsBusy = true;
            this.user = user;
            this.task = null;
            GoToUrl(Constant.SERVER_ROOT + mainUrl);
            return true;
        }

        internal bool Deal(Task task)
        {
            this.IsBusy = true;
            this.task = task;
            this.user = Dao.SelectUser(task.UserName);
            GoToUrl(Constant.SERVER_ROOT + mainUrl);
            return true;
        }

        private async void watchsource(IFrame frame)
        {
            this.source = await frame.GetTextAsync();
        }

        private void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            watchsource(e.Frame);
            if (e.Url.ToString().Contains(loginUrl))
            {
                Login(e.Frame);
            }
            else if (e.Url.ToString().Contains(mainUrl))
            {
                if (this.task == null)
                {
                    GoToUrl(Constant.SERVER_ROOT + listUrl);
                }
                else
                {
                    GoToUrl(Constant.SERVER_ROOT + string.Format(taskUrl, task.EID));
                }
            }
            else if (e.Url.ToString().Contains(listUrl))
            {
                string rule = "";
                if(user.Type == Constant.TYPE_CHUANSHU)
                {
                    rule = string.Format(@"&& ******** === '{0}'", Constant.TYPE_CHUANSHU);
                }else
                {
                    rule = string.Format(@"&& ******** !== '{0}'", Constant.TYPE_CHUANSHU);
                }

                string s = string.Format(@"var trCol = document.getElementsByTagName('tr');");
                s += string.Format(@"
                for(var i = 0; i < trCol.length; i++){
                    if(trCol[i].className === 'querytr' {0}){
                        var pri = trCol[i].getAttribute('onclick');
                        var u = pri.substr(pri.indexOf('\'') + 1);
                        u =  u.substr(0, u.indexOf('\''));
                        window.location.href=(u);
                        break;
                    }
                }", rule);
                e.Frame.ExecuteJavaScriptAsync(s);
                this.IsBusy = false;

            }
            else if (e.Url.ToString().Contains("Default+Admin+View") && e.Url.ToString().Contains("cacheid") && !e.Url.ToString().Contains("&amp;") && e.Url.ToString().Contains("cacheid="))
            {
                DealTask(e.Frame);
                GoToUrl(Constant.SERVER_ROOT + string.Format(infoUrl, task.EID));
            }
            else if (e.Url.ToString().Contains(@"/arsys/BackChannel/?param=100"))
            {
                SaveTask1(e.Frame);
                GoToUrl(Constant.SERVER_ROOT + string.Format(progressUrl, task.EID));
            }
            else if (e.Url.ToString().Contains(@"/ultrawf/UltraWF/manageprocess/BaseInfoView.jsp"))
            {
                SaveTask2(e.Frame);
                GoToUrl(Constant.SERVER_ROOT + listUrl);
            }

            // this.IsBusy = false;
        }

        private async void SaveTask1(IFrame frame)
        {
            string res = await frame.GetSourceAsync();
            TaskParser1(res);
        }

        private async void SaveTask2(IFrame frame)
        {
            string res = await frame.GetSourceAsync();
            TaskParser2(res);
            Dao.Save(this.task);
            this.task = null;
        }

        private void GoToUrl(string url)
        {
            this.Dispatcher.Invoke(new Action(() => { this.WebBrowser.Address = url; }));
        }

        private void Login(IFrame frame)
        {
            string s = string.Format(@"document.getElementById('username-id').value = '{0}';", this.user.UserName);
            s += string.Format(@"document.getElementById('pwd-id').value = '{0}';", this.user.Password);
            s += string.Format(@"document.getElementsByName('login')[0].click();");
            frame.ExecuteJavaScriptAsync(s);
        }

        private void DealTask(IFrame frame)
        {
            if (this.task == null)
            {
                this.task = new Task();
            }
            task.UserName = user.UserName;
            task.IsArched = "0";
            task.EID = EIDParser(frame.Url);
           // string url = string.Format(infoUrl, task.EID);
            string s = @"function confirm(){";
            s += @"return true;";
            s += @"};";
            s += @"function alert(){};";
            frame.ExecuteJavaScriptAsync(s);
            s = (@"var inter = setInterval(function(){
                        var as = document.getElementsByTagName('a');
                        for(var i = 0; i < as.length; i++){
                            if(as[i].className.indexOf('ardbnBtn_Start') > 0){
                                window.clearInterval(inter); 
                                if(as[i].getAttribute('style').indexOf('inherit')){                            
                                    as[i].click();
                                }
                               
                                break;
                        }}
                    }, 1000);
");
            frame.ExecuteJavaScriptAsync(s);
        }

        private string EIDParser(string url)
        {
            string res = url.Substring(url.IndexOf("eid="));
            return res.Substring(4, res.IndexOf("&") - 4);
        }

        private void TaskParser1(string res)
        {

            List<Item> items = JSON.parse<List<Item>>(res.Substring("this.result=".Length));
            foreach (Item item in items)
            {
                if (item.i == Constant.ITEM_TaskId)
                {
                    task.TaskId = item.v;
                }
                if (item.i == Constant.ITEM_Topic)
                {
                    task.Topic = item.v;
                }
                if (item.i == Constant.ITEM_Type)
                {
                    task.Type = item.v;
                }
                if (item.i == Constant.ITEM_T2time)
                {
                    task.T2Time = item.v;
                }
                if (item.i == Constant.ITEM_HappenTime)
                {
                    task.HappenTime = item.v;
                }
                if (item.i == Constant.ITEM_Descrition)
                {
                    task.Descrition = item.v;
                }
                if (item.i == Constant.ITEM_Status)
                {
                    task.Status = item.v;
                }
                if(item.i == Constant.ITEN_Location)
                {
                    task.Location = item.v;
                }
            }

        }
        private void TaskParser2(string res)
        {
            string result = "";
            MatchCollection matches = Regex.Matches(res, Regex.Escape(@"<TD class=ColumntableTD colSpan=7>") + @"[\s\S]*?" + Regex.Escape("</TD>"));
            foreach (Match match in matches)
            {
                result += match.Value.Replace("<TD class=ColumntableTD colSpan=7>", "").Replace("</TD>", "").Replace("&nbsp;", "").Trim() + "\r\n";
            }
            this.task.Progress = result;
        }
    }
}
