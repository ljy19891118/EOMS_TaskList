using Microsoft.JScript;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebUtility;

namespace TaskList
{
    class Proxy
    {
        public User User { set; get; }
        private WebHelpHttp WebHelpHttp { set; get; }
        private Browser browser;

        public Proxy()
        {
            this.browser = new Browser();
            browser.Show();
            browser.Hide();
         }

        public bool Login(User user)
        {
            this.User = user;
            WebHelpHttp = new WebHelpHttp();
            WebHelpHttp.Get("ultrawf/UltraWF/share/login.jsp"); // 获取登录页和JSESSION
            string args = String.Format("username={0}&pwd={1}", user.UserName, Encode(user.Password));
            string sso = WebHelpHttp.Post("sso/servlet/LoginServlet", args);
            if (sso == "OKLOCAL")
            {
                string res = WebHelpHttp.Post("ultrawf/UltraWF/ManageConfig/UserLogin", args); // 登录
                return res == "";
            }
            else
            {
                return false;
            }

        }

        private string Encode(String pwd)
        {

            String cipher = "http://eoms.bmcc.com.cn";

            String result = "";
            if (pwd == null)
                pwd = "";
            pwd = GlobalObject.encodeURIComponent(pwd);

            char[] pwdArray = pwd.ToArray();
            char[] cipherArray = cipher.ToCharArray();
            for (int i = 0; i < pwdArray.Length; i++)
            {
                char cc = pwdArray[i];
                char dd = cipherArray[i % cipher.Length];
                int ee = cc + dd / 2;
                if (ee > 126)
                {
                    ee = ee - 94;
                }
                result += (char)ee;
            }
            return GlobalObject.encodeURIComponent(result);
        }

        public bool Logout()
        {
            WebHelpHttp.Get("ultrawf/UltraWF/ManageConfig/UserLogout");
            WebHelpHttp.Get("arsys/shared/logout.jsp");
            return true;
        }

        public ObservableCollection<string> FindTaskEIDList()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            string res = WebHelpHttp.Get(@"ultrawf/UltraWF/manageprocess/all_workFlow.jsp?type=NotAssigned&invalidSearch=no");
            int totalPage = TotalPageParser(res);
            for (int i = 0; i < totalPage; i++)
            {
                string resp = WebHelpHttp.Post(@"ultrawf/UltraWF/manageprocess/all_workFlow.jsp?type=NotAssigned&invalidSearch=no", String.Format(@"curpage={0}&pagecount={1}&orderby=BaseCreateDate&adsc=desc&isCommit=yes&invalidSearch=no&baseclassid=&colId=&typeindex=&groupSplit1=%3B&BaseCreatorFullName=&BaseCreatorFullNameCode=&BaseCreatorDep=&BaseCreatorDepCode=&BaseCreatorDepCode2=&createTimeFrom=&createTimeTo=&groupname=&groupnameCode=&Assginee=&AssgineeCode=&Dealer=&DealerCode=&closeTimeFrom=&closeTimeTo=&BaseStatus=&basesn=&overtime=&baseschemaName=&baseschemaNameCode=", i + 1, totalPage));
                EIDListParser(resp, list);
            }
            return list;
        }

        public Task FindTask(string eid)
        {
            try
            {
                string res2 = WebHelpHttp.Get(String.Format("ultrawf/UltraWF/manageprocess/BaseInfoView.jsp?baseschema=WF:BMCC_EOMS_ITDealFault&baseid={0}", eid));

                string res1 = WebHelpHttp.Get(String.Format("arsys/BackChannel/?param=100%2FGetEntry%2F12%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault0%2F19%2FARRoot147236736612415%2F{0}2%2F0%2F1%2F1", eid));

                if (res1.Contains("当前"))
                {
                    // 不知道如何复写暂时不管了
                    string temp = WebHelpHttp.Get("arsys/BackChannel/?param=15/SetOverride/1/1");
                    temp = WebHelpHttp.Get("arsys/BackChannel/?param=15/SetOverride/1/1");
                    temp = WebHelpHttp.Get("arsys/BackChannel/?param=15/SetOverride/1/1");
                    temp = WebHelpHttp.Get("arsys/BackChannel/?param=15/SetOverride/1/1");
                    temp = WebHelpHttp.Get("arsys/BackChannel/?param=15/SetOverride/1/1");
                    temp = WebHelpHttp.Get(String.Format("arsys/BackChannel/?param=187%2FGetTableEntryList%2F12%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault18%2FDefault%20Admin%20View4%2F102012%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault0%2F1%2F01%2F02%2F0%2F31%2F4%5C1%5C1%5C1%5C2%5C4%5C15%5C{0}%5C2%2F0%2F2%2F0%2F2%2F0%2F", eid));
                    temp = WebHelpHttp.Get("arsys/BackChannel/?param=15/SetOverride/1/1");
                    temp = WebHelpHttp.Get(String.Format("arsys/BackChannel/?param=187%2FGetTableEntryList%2F12%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault18%2FDefault%20Admin%20View4%2F102012%2Fbmcc-eoms-0524%2FWF%3ABMCC_EOMS_ITDealFault0%2F1%2F01%2F02%2F0%2F31%2F4%5C1%5C1%5C1%5C2%5C4%5C15%5C{0}%5C2%2F0%2F2%2F0%2F2%2F0%2F", eid));

                    return null;
                }

                return TaskParser(eid, res1, res2);
            }catch(Exception ex)
            {
                return null;
            }
        }

        private Task TaskParser(string eid, string res1, string res2)
        {
            Task task = new Task();
            List<Item> items = JSON.parse<List<Item>>(res1.Substring("this.result=".Length));
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
            task.Progress = ProgressLogParser(res2);
            task.UserName = User.UserName;
            task.IsArched = "0";
            return task;
        }

        private int EIDListParser(string res, ObservableCollection<string> list)
        {
            int count = 0;
            MatchCollection matches = Regex.Matches(res, Regex.Escape("?eid=") + ".*" + Regex.Escape("&processid"));
            foreach (Match match in matches)
            {
                string eid = match.Value.Replace("?eid=", "").Replace("&processid", "");
                if (!list.Contains(eid))
                {
                    list.Add(eid);
                    count++;
                }
            }
            return count;
        }

        private int TotalPageParser(string res)
        {
            int result = 1;
            Match match = Regex.Match(res, Regex.Escape(@"<td width=""32px"" align=""left"">/") + ".*" + Regex.Escape("</td>"));
            string temp = match.Value.Replace(@"<td width=""32px"" align=""left"">/", "");
            temp = temp.Replace("</td>", "");
            if(temp == "")
            {
                return 0;
            }
            result = Int32.Parse(temp);
            return result;
        }

        private string ProgressLogParser(string res)
        {
            string result = "";
            MatchCollection matches = Regex.Matches(res, Regex.Escape(@"<TD class=ColumntableTD colSpan=7>") + @"[\s\S]*?" + Regex.Escape("</TD>"));
            foreach (Match match in matches)
            {
                result += match.Value.Replace("<TD class=ColumntableTD colSpan=7>", "").Replace("</TD>", "").Replace("&nbsp;", "").Trim() + "\r\n";
            }
            return result;
        }

        public bool DealTask(Task task)
        {
           if(browser == null)
            {

                browser = new Browser();
                browser.Show();
                browser.Hide();
            }
            if (!browser.IsBusy)
            {
                browser.Deal(task);
                return true;
            }
            return false;
        }

        public bool TaskIsFinished()
        {

            return !browser.IsBusy;
        }

        public bool Done()
        {
            return this.browser.Finish();
        }

        public void ShowBrowser()
        {
            this.browser.Show();
        }
        public void HideBrowser()
        {
            this.browser.Hide();
        }
        public void CloseBrowser()
        {
            if(this.browser != null)
                this.browser.Close();
        }
        public void ResetBrowser()
        {
            this.browser.Close();
            this.browser = null;
            this.browser = new Browser();
            browser.Show();
            browser.Hide();
        }
    }
}
