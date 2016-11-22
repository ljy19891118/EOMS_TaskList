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
        public bool IsBusy = false; // 是否工作完成的标记
        private bool IsDealed = false; // 判断是否已经接过单子的标记位
        private ObservableCollection<Task> taskList = new ObservableCollection<Task>();

        public Object _LOCK = new Object();

        private readonly string loginUrl = "/ultrawf/UltraWF/share/login.jsp";
        private readonly string logoutUrl = "/ultrawf/UltraWF/ManageConfig/UserLogout";
        private readonly string mainUrl = "/ultrawf/UltraWF/share/main.jsp";
        private readonly string listUrl = "/ultrawf/UltraWF/manageprocess/all_workFlow.jsp?type=NotAssigned&invalidSearch=no";
        private readonly string taskUrl = "/arsys/forms/BMCC-EOMS-05/WF%3ABMCC_EOMS_ITDealFault/Default+Admin+View/?eid={0}";


        public Browser()
        {
            InitializeComponent();

        }


        //internal bool RefreshTask(User user)
        //{
        //    if (this.IsBusy)
        //    {
        //        return false;
        //    }
        //    this.user = user;
        //    this.taskList = Dao.SelectCtlByUser(user.UserName);
        //    GoToUrl(Constant.SERVER_ROOT + mainUrl);
        //    return true;
        //}

        internal bool Deal(User user)
        {
            lock (_LOCK)
            {
                if (this.IsBusy)
                {
                    return false;
                }
                this.IsBusy = true;
                // 获取当前任务,用来刷新
                this.taskList = Dao.SelectCtlByUser(user.UserName);
                this.user = user;
                GoToUrl(Constant.SERVER_ROOT + mainUrl);
                return true;
            }
        }
        // 没用的方法，但不要删除
        internal bool Deal(Task task)
        {
            lock (_LOCK)
            {
                if (this.IsBusy)
                {
                    return false;
                }
                this.IsBusy = true;
                this.task = task;
                this.user = Dao.SelectUser(task.UserName);
                GoToUrl(Constant.SERVER_ROOT + mainUrl);
                return true;
            }
        }

        internal bool Finish()
        {
            lock (_LOCK)
            {
                if (this.IsBusy)
                {
                    return false;
                }
                this.task = null;
                this.user = null;
                this.IsBusy = true;
                GoToUrl(Constant.SERVER_ROOT + mainUrl);
                return true;
            }
        }

        // 页面跳转逻辑
        private void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            Console.WriteLine(e.Url);
            if (e.Url.ToString().Contains(loginUrl))
            {
                if (user != null)
                {
                    Login(e.Frame);
                }
                else
                {
                    this.IsBusy = false;
                }
            }
            else if (e.Url.ToString().Contains(mainUrl))
            {
                if (user != null)
                {

                    if (taskList.Count > 0)
                    {
                        this.task = taskList.First();
                        GoToUrl(Constant.SERVER_ROOT + string.Format(taskUrl, this.task.EID));
                    }
                    else
                    {
                        // 是值班用户，本次还没有接过单，则进行接单
                        if (!this.IsDealed && user.OnDuty == Dao.GetToday())
                        {
                            this.IsDealed = true;
                            GoToUrl(Constant.SERVER_ROOT + listUrl);
                        }
                        else
                        {
                            this.IsDealed = false;
                            this.IsBusy = false;
                        }
                    }

                }
                else
                {
                    GoToUrl(Constant.SERVER_ROOT + logoutUrl);
                }
            }

            else if (e.Url.ToString().Contains("Default+Admin+View") && e.Url.ToString().Contains("cacheid") && !e.Url.ToString().Contains("&amp;") && e.Url.ToString().Contains("cacheid="))
            {
                DealTask(e.Frame);

            }
            else if (e.Url.ToString().Contains(listUrl))
            {
                GetTaskList(e.Frame);
            }
            Console.WriteLine("deal");
        }

        private async void GetTaskList(IFrame frame)
        {
            ObservableCollection<Task> tempList = new ObservableCollection<Task>();
            string js = @"var tableList = new Array();
                            var eidList = new Array();
                            tableList = document.getElementsByTagName('tr');
                            for (var i = 0; i < tableList.length; i++)
                                {
                                    var str = tableList[i].getAttribute('Onclick');
                                    if (str != null)
                                    {
                                        start = str.indexOf('eid') + 4;
                                        stop = str.indexOf('processid') - 1;
                                        eid = str.substring(start, stop);
                                        if (eidList.indexOf(eid) == -1)
                                            eidList.push(eid);
                                    }
                                }
                            tableList;

            ";
            //TODO 这里需要Debug一下看看返回值是啥，然后下一步作处理
            JavascriptResponse response = await frame.EvaluateScriptAsync(js);
            string[] results = response.Result as string[];
            foreach (string eid in results)
            {
                Task task = new Task();
                task.EID = eid;
                tempList.Add(task);
            }
            foreach(Task task in tempList)
            {
                this.taskList.Add(task);
            }

            // 判断是不是最后一页
            js = @"document.getElementById('selCurPage').value == document.getElementById('selCurPage').length";
            JavascriptResponse isEndRe = await frame.EvaluateScriptAsync(js);
            string isEnd = isEndRe.Result as string;

            // 如果不是最后一页则进行翻页
            if (isEnd == "false")
            {
                //TODO 这行不对，是伪码，需要修改
                js = @"button.click()";
                frame.ExecuteJavaScriptAsync(js);
            }
            else if (taskList.Count > 0)
            {
                GoToUrl(Constant.SERVER_ROOT + mainUrl);
            }
            else
            {
                this.IsBusy = false;
            }
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

        // 接单的关键方法
        private async void DealTask(IFrame frame)
        {
            taskList.Remove(this.task);

  
            string s = @"function confirm(){
                            return true;
                        }";
            frame.ExecuteJavaScriptAsync(s);
            s = @"function alert(){}";
            frame.ExecuteJavaScriptAsync(s);
            s = @"function onbeforeunload(){}";
            frame.ExecuteJavaScriptAsync(s);
            s = @"function ajax(options) {
                        options = options || {};
                        options.type = (options.type || 'GET').toUpperCase();
                        options.dataType = options.dataType || 'json';
                            var params = formatParams(options.data);

                            //创建 - 非IE6 - 第一步
                            if (window.XMLHttpRequest)
                            {
                                var xhr = new XMLHttpRequest();
                            }
                            else
                            { //IE6及其以下版本浏览器
                                var xhr = new ActiveXObject('Microsoft.XMLHTTP');
                            }

                            //接收 - 第三步
                            xhr.onreadystatechange = function() {
                                if (xhr.readyState == 4)
                                {
                                    var status = xhr.status;
                                    if (status >= 200 && status < 300)
                                    {
                                        options.success && options.success(xhr.responseText, xhr.responseXML);
                                    }
                                    else
                                    {
                                        options.fail && options.fail(status);
                                    }
                                }
                            }

                            //连接 和 发送 - 第二步
                            if (options.type == 'GET')
                            {
                                xhr.open('GET', options.url, true);
                                xhr.send(null);
                            }
                            else if (options.type == 'POST')
                            {
                                xhr.open('POST', options.url, true);
                                //设置表单提交时的内容类型
                                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                                xhr.send(params);
                            }
                        }

                        ajax({
                            url: '/arsys/BackChannel/?param=100%2FGetEntry%2F12%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault0%2F19%2FARRoot147660699558615%2F"+ this.task.EID + @"2%2F0%2F1%2F1',              //请求地址
                            type: 'GET',                       //请求方式
                            success: function (response, xml) {
                                taskData = response;
                            },
                            fail: function (status) {
                                taskData = 'error';
                            }
                        })";
            frame.ExecuteJavaScriptAsync(s);
            // 获取页面工单信息
            s = "taskData";
            for (int i = 0; i < Constant.WAIT_TIME; i++)
            {
                if (i == Constant.WAIT_TIME - 1)
                {
                    GoToUrl(Constant.SERVER_ROOT + mainUrl);
                    return;
                }
                Thread.Sleep(100);
                JavascriptResponse re = await frame.EvaluateScriptAsync(s);
                //TODO 这里需要Debug一下看看返回值是啥，然后下一步作处理
                if (re.Result != null)
                {
                    if(re.Result as string == "error")
                    {
                        break;
                    }
                    Task cTask = TaskParser(this.task.EID, re.Result as string);


                    // 判断用户类型是否匹配
                    if ((user.OnDuty == Dao.GetToday()) && (user.Type == Constant.TYPE_CHUANSHU && cTask.Type.Contains(Constant.TYPE_CHUANSHU))
                        || (user.Type == Constant.TYPE_DEFAULT && !cTask.Type.Contains(Constant.TYPE_CHUANSHU)))
                    {
                        string js = @"var as = document.getElementsByTagName('a');
                                  for(var i = 0; i < as.length; i++){
                                        if(as[i].className.indexOf('ardbnBtn_Start') > 0){
                                            if(as[i].getAttribute('style').indexOf('inherit') > 0){                            
                                                as[i].click();    
                                                break;                         
                                            }
                                        }
                                   }";
                        frame.ExecuteJavaScriptAsync(js);
                    }
                    Dao.Save(cTask);
                    break;
                }

            }
            GoToUrl(Constant.SERVER_ROOT + mainUrl);

            //            if (refreshFlag == 0)
            //            {
            //                s = (@"var inter = setInterval(function(){
            //                        var as = document.getElementsByTagName('a');
            //                        for(var i = 0; i < as.length; i++){
            //                            if(as[i].className.indexOf('ardbnBtn_Start') > 0){
            //                                window.clearInterval(inter); 
            //                                if(as[i].getAttribute('style').indexOf('inherit') > 0){                            
            //                                    as[i].click();    
            //                                    window.location.href = '" + mainUrl + @"'                           
            //                                }
            //                        }}
            //                    }, 1000);
            //");
            //                frame.ExecuteJavaScriptAsync(s);
        
        }
        // 没用的方法，但不要删除
        private void WebBrowser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            Console.WriteLine(e.Url);
            IFrame frame = e.Frame;
            if (e.Url.ToString().Contains("Default+Admin+View") && e.Url.ToString().Contains("cacheid") && !e.Url.ToString().Contains("&amp;") && e.Url.ToString().Contains("cacheid="))
            {
                string s = @"function confirm(){
                            return true;
                        }";
                frame.ExecuteJavaScriptAsync(s);
                s = @"function alert(){}";
                frame.ExecuteJavaScriptAsync(s);
                s = @"function onbeforeunload(){}";
                frame.ExecuteJavaScriptAsync(s);
            }
        }
        //json处理器
        private Task TaskParser(string eid, string res)
        {
            Task task = new Task();
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
                if (item.i == Constant.ITEN_Location)
                {
                    task.Location = item.v;
                }
            }
            task.EID = eid;
            // 下面这个字段处理有点麻烦，因为是来自另外一个页面，如果用户不提就不写了
            //task.Progress = ProgressLogParser(progressLogString);
            task.UserName = this.user.UserName;
            task.IsArched = "0";
            return task;
        }
        // 没用的方法，但不要删除
        private string ProgressLogParser(string progressLogString)
        {
            string result = "";
            MatchCollection matches = Regex.Matches(progressLogString, Regex.Escape(@"<TD class=ColumntableTD colSpan=7>") + @"[\s\S]*?" + Regex.Escape("</TD>"));
            foreach (Match match in matches)
            {
                result += match.Value.Replace("<TD class=ColumntableTD colSpan=7>", "").Replace("</TD>", "").Replace("&nbsp;", "").Trim() + "\r\n";
            }
            return result;
        }

    }
}