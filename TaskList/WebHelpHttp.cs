using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Configuration;
using System.Web;
using TaskList;

namespace WebUtility
{
    public class WebHelpHttp
    {
        private CookieCollection cookies;

        public CookieCollection Cookies
        {
            get { return this.cookies; }
            set { this.cookies = value; }
        }

        public string Get(string url)
        {
            // *** Establish request by assigning Url

            string serverUrl = Constant.SERVER_ROOT;
            StringBuilder reqUrl = new StringBuilder(serverUrl);
            reqUrl.Append("/");
            reqUrl.Append(url);

            HttpWebRequest loHttp = (HttpWebRequest)WebRequest.Create(reqUrl.ToString());
            // *** Set any header related and operational properties
            loHttp.Timeout = 10000;  // 10 secs
            loHttp.UserAgent = "Code Sample Web Client";

            // *** reuse cookies if available
            loHttp.CookieContainer = new CookieContainer();

            if (cookies != null && cookies.Count > 0)
            {
                loHttp.CookieContainer.Add(cookies);

            }

            // *** Return the Response data

            HttpWebResponse loWebResponse = (HttpWebResponse)loHttp.GetResponse();

            // ** If the server returns any cookies
            // ** add 'em to our cookies collection
            if (loWebResponse.Cookies.Count > 0)
                if (cookies == null)
                {
                    cookies = loWebResponse.Cookies;
                }
                else
                {
                    // ** If we already have cookies update the list
                    foreach (Cookie oRespCookie in loWebResponse.Cookies)
                    {
                        bool bMatch = false;
                        foreach (Cookie oReqCookie in cookies)
                        {
                            if (oReqCookie.Name == oRespCookie.Name)
                            {
                                oReqCookie.Value = oRespCookie.Value;
                                bMatch = true;
                                break; // 
                            }
                        }
                        if (!bMatch)
                            cookies.Add(oRespCookie);
                    }
                }

            Encoding enc = Encoding.GetEncoding("gbk");
            if (loWebResponse.CharacterSet != null && loWebResponse.CharacterSet != "")
            {
                enc = Encoding.GetEncoding(loWebResponse.CharacterSet);
            }
            StreamReader loResponseStream =
                new StreamReader(loWebResponse.GetResponseStream(), enc);
            string text = loResponseStream.ReadToEnd();

            loResponseStream.Close();
            loWebResponse.Close();

            return text;
        }

        public string Post(string url, string args)
        {
            string serverUrl = Constant.SERVER_ROOT;
            StringBuilder reqUrl = new StringBuilder(serverUrl);
            reqUrl.Append("/");
            reqUrl.Append(url);
            HttpWebRequest loHttp = WebRequest.Create(reqUrl.ToString()) as HttpWebRequest;

            loHttp = loHttp as HttpWebRequest;
            // *** Set any header related and operational properties
            loHttp.Timeout = 20000;  // 20 secs
            loHttp.UserAgent = "Code Sample Web Client";
            loHttp.Method = "POST";
            loHttp.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            loHttp.KeepAlive = true;

            // *** reuse cookies if available
            loHttp.CookieContainer = new CookieContainer();
            if (cookies != null && cookies.Count > 0)
            {
                loHttp.CookieContainer.Add(cookies);
            }



            byte[] buffer = Encoding.UTF8.GetBytes(args);
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < buffer.Length; i++)
            //{
            //    if (buffer[i] < 128)
            //        sb.Append((char)buffer[i]);
            //    else
            //    {
            //        sb.Append("%" + buffer[i].ToString("X2"));

            //    }
            //}
            //buffer = Encoding.UTF8.GetBytes(sb.ToString());
            loHttp.ContentLength = buffer.Length;
            loHttp.GetRequestStream().Write(buffer, 0, buffer.Length);
            loHttp.AllowAutoRedirect = false;

            // *** Return the Response data
            HttpWebResponse loWebResponse;
            try
            {
                loWebResponse = (HttpWebResponse)loHttp.GetResponse();
            }
            catch
            {
                return "";
            }
            // ** If the server returns any cookies
            // ** add 'em to our cookies collection
            if (loWebResponse.Cookies.Count > 0)
                if (cookies == null)
                {
                    cookies = loWebResponse.Cookies;
                }
                else
                {
                    CookieCollection temp = new CookieCollection();
                    if (loWebResponse.Headers.ToString().Contains("_password="))
                    {
                        String headers = loWebResponse.Headers.ToString();
                        headers = headers.Substring(headers.IndexOf("_password=") + "_password=".Length);
                        headers = headers.Substring(0, headers.IndexOf(";"));
                        Cookie cook = new Cookie("_password", headers, "/arsys", "10.224.145.100");
                        temp.Add(cook);
                    }
                    temp.Add(loWebResponse.Cookies);
                    // ** If we already have cookies update the list
                    foreach (Cookie oRespCookie in temp)
                    {
                        bool bMatch = false;
                        foreach (Cookie oReqCookie in cookies)
                        {
                            if (oReqCookie.Name == oRespCookie.Name)
                            {
                                oReqCookie.Value = oRespCookie.Value;
                                bMatch = true;
                                break; // 
                            }
                        }
                        if (!bMatch)
                            cookies.Add(oRespCookie);
                    }

                }
            //Encoding enc = Encoding.GetEncoding("gbk"); // Windows-1252 or iso-
            Encoding enc = Encoding.UTF8;
            if (loWebResponse.CharacterSet != null && loWebResponse.CharacterSet != "")
            {
                enc = Encoding.GetEncoding(loWebResponse.CharacterSet);
            }
            StreamReader loResponseStream =
                new StreamReader(loWebResponse.GetResponseStream(), enc);

            string text = loResponseStream.ReadToEnd();

            loResponseStream.Close();
            loWebResponse.Close();
            return text;

        }

    }
}
