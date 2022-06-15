using System;
using System.IO;
using System.Net;
using System.Text;

public static class WebUtils
{
    
        public static bool GetRequest(out HttpWebResponse response, string url, CookieContainer cookieContainer)
        {
            response = null;

            try
            {
                HttpWebRequest request = CreateWebRequestObject(url, cookieContainer);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if(response != null) response.Close();
                return false;
            }

            return true;
        }
    
        public static bool GetBasicRequest(out HttpWebResponse response, string url)
        {
            response = null;

            try
            {
                HttpWebRequest request = CreateBasicWebRequestObject(url);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if(response != null) response.Close();
                return false;
            }

            return true;
        }

        public static bool OfficeFootballPoolLoginPostRequest(out HttpWebResponse response,
            string url,
            CookieContainer cookieContainer,
            string username,
            string password)
        {
            response = null;

            try
            {
                HttpWebRequest request = CreateWebRequestObject(url, cookieContainer);
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string body = @$"login=1&username={username}&password={password}&useSavedPwd=1&yes=1&login=&gotopage=index.cfm&poolid=&entrykey=&suppressAlerts=0";
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if(response != null) response.Close();
                return false;
            }

            return true;
        }

        private static HttpWebRequest CreateWebRequestObject(string url, CookieContainer cookieContainer)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookieContainer;
            request.KeepAlive = true;
            request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");
            request.Headers.Add("sec-ch-ua", @"""Microsoft Edge"";v=""95"", ""Chromium"";v=""95"", "";Not A Brand"";v=""99""");
            request.Headers.Add("sec-ch-ua-mobile", @"?0");
            request.Headers.Add("sec-ch-ua-platform", @"""Windows""");
            request.Headers.Add("Upgrade-Insecure-Requests", @"1");
            // request.Headers.Add("Origin", @"https://www.officefootballpool.com");
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4613.0 Safari/537.36 Edg/95.0.1000.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            request.Headers.Add("Sec-Fetch-Site", @"same-origin");
            request.Headers.Add("Sec-Fetch-Mode", @"navigate");
            request.Headers.Add("Sec-Fetch-User", @"?1");
            request.Headers.Add("Sec-Fetch-Dest", @"document");
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            return request;
        }
        private static HttpWebRequest CreateBasicWebRequestObject(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            return request;
        }
}