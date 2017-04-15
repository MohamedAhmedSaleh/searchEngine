using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Threading;
using System.Diagnostics;
using HtmlAgilityPack;

namespace IR
{
    public partial class Form1 : Form
    {
        private Queue<String> toVisit;//
        private Queue<String> visited;//urls//
        private Queue<String> content;//htmlcontent
        private Queue<String> specificContent; // specificContentFromhtml
        private Queue<String[]> contentTokens;
        HtmlToText htmltotext;
        int numberOfDocuments;
        string connectionString = "Data source=orcl; User Id=scott; Password=tiger;";
        OracleConnection conn;
        List<Thread> threads;
        Semaphore semaphore;

        public Form1()
        {
            InitializeComponent();
            semaphore = new Semaphore(1, 1);
            toVisit = new Queue<string>();
            visited = new Queue<string>();
            content = new Queue<string>();
            specificContent = new Queue<string>();
            contentTokens = new Queue<string[]>();
            htmltotext = new HtmlToText();
            numberOfDocuments = 10000;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            toVisit.Enqueue("https://en.wikipedia.org/wiki/Main_Page");
            toVisit.Enqueue("https://www.google.com");
            toVisit.Enqueue("https://maktoob.yahoo.com/?p=us");
            toVisit.Enqueue("http://www.msn.com");
            toVisit.Enqueue("https://www.techmeme.com/");
            toVisit.Enqueue("https://yts.ag/");
            toVisit.Enqueue("https://www.quora.com/");
        }

        private void crawl_Click(object sender, EventArgs e)
        {
            threads = new List<Thread>();
            int numberOfThreads = 100;
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread newthread = new Thread(new ThreadStart(crawler));
                newthread.IsBackground = true;
                threads.Add(newthread);
                newthread.Start();
            }
            Thread thread = new Thread(new ThreadStart(handleThreads));
            thread.IsBackground = true;
            thread.Start();

        }

        private void handleThreads()
        {
            while (threads.Count != 0)
            {
                if (visitedCount.InvokeRequired)
                    visitedCount.Invoke(new MethodInvoker(delegate
                    {
                        visitedCount.Text = "Visited Pages: " + visited.Count;
                    }));
                else
                    visitedCount.Text = "Visited Pages: " + visited.Count;

                for (int i = 0; i < threads.Count; i++)
                    if (!threads[i].IsAlive)
                        threads.Remove(threads[i]);
            }
            foreach (var strContent in content)
                GetSpecificContent(strContent);
            addCrawlerResultsToDatabase();
            MessageBox.Show("done");
        }

        private void crawler()
        {

            while (visited.Count <= numberOfDocuments)
            {

                if (toVisit.Count != 0)
                {
                    try
                    {
                        semaphore.WaitOne();
                        String strToVisit = toVisit.Dequeue();
                        bool released = false;
                        if (toVisit.Count > 1)
                        {
                            semaphore.Release();
                            released = true;
                        }
                        if (!visited.Contains(strToVisit))//to prevent duplicate
                        {
                            string temp = HTTPRequest(strToVisit);//call function to get html
                            if (!temp.Equals(""))
                            {
                                searchForLinks(temp);
                                if (!released)
                                    semaphore.Release();
                                content.Enqueue(temp);
                                visited.Enqueue(strToVisit);
                                String strContent = temp;
                            }
                        }
                        else
                            semaphore.Release();
                    }
                    catch (Exception ex)
                    {
                    }
                }


            }

        }

        public string HTTPRequest(String URL)
        {
            WebRequest myWebRequest;
            WebResponse myWebResponse;
            string rString = "";
            // Create a new 'WebRequest' object to the mentioned URL.
            Uri uri;
            if (!Uri.TryCreate(URL, UriKind.Absolute, out uri))
                return rString;

            try
            {
                myWebRequest = WebRequest.Create(URL);

                // The response object of 'WebRequest' is assigned to a WebResponse' variable.
                myWebResponse = myWebRequest.GetResponse();

                Stream streamResponse = myWebResponse.GetResponseStream();
                StreamReader reader = new StreamReader(streamResponse);
                rString = reader.ReadToEnd();

                streamResponse.Close();
                reader.Close();
                myWebResponse.Close();
            }
            catch (Exception ex)
            {
            }
            return rString;

        }
        public void searchForLinks(String content)
        {
            if (toVisit.Count < numberOfDocuments - visited.Count + 500)
            {
                var urlDictionary = new Dictionary<string, string>();

                Match match = Regex.Match(content, "(?i)<a .*?href=\"([^\"]+)\"[^>]*>(.*?)</a>");
                while (match.Success)
                {
                    string urlKey = match.Groups[1].Value;
                    string urlValue = Regex.Replace(match.Groups[2].Value, "(?i)<.*?>", string.Empty);
                    urlDictionary[urlKey] = urlValue;
                    match = match.NextMatch();
                }

                foreach (var item in urlDictionary)
                {
                    string href = item.Key;
                    string text = item.Value;
                    if (!string.IsNullOrEmpty(href))
                    {
                        string url = href.Replace("%3f", "?")
                            .Replace("%3d", "=")
                            .Replace("%2f", "/")
                            .Replace("&amp;", "&");

                        if (string.IsNullOrEmpty(url) || url.StartsWith("#")
                            || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                            || url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if ((url.Contains("http://") || url.Contains("https://")) && (!toVisit.Contains(url)))
                            toVisit.Enqueue(url);
                    }
                }
            }
        }

        private void GetSpecificContent(string htmlContent)
        {
            specificContent.Enqueue(htmltotext.ConvertHtml(htmlContent));
        }
        private void addCrawlerResultsToDatabase()
        {
            conn = new OracleConnection(connectionString);
            conn.Open();
            int count = 0;
            OracleCommand cmd;

            while (count < visited.Count - 1)
            {
                cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "INSERT_NEW_CRAWLER_RESULT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("page_url", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = visited.ElementAt(count);
                cmd.Parameters.Add("page_content", OracleDbType.NClob, DBNull.Value, ParameterDirection.Input).Value = content.ElementAt(count);
                cmd.Parameters.Add("page_specific_content", OracleDbType.NClob, DBNull.Value, ParameterDirection.Input).Value = specificContent.ElementAt(count);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
                count++;
            }
            conn.Close();
        }

        public void tokenize()
        {
            foreach (var item in specificContent)
            {
                string[] tokens = item.Split(new char[] { ' ', ',' });
                contentTokens.Enqueue(tokens);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            conn = new OracleConnection(connectionString);
            conn.Open();
            OracleCommand cmd;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "Get_All_documents";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("documents", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
            OracleDataReader dr = cmd.ExecuteReader();
            int count = 0;
            while (dr.Read())
            {
                var id = Convert.ToInt32(dr["ID"]);
                string dochtmlcontent = dr.GetString((1));
                if (dochtmlcontent.Contains("lang=en"))
                {
                    count++;
                }
                else {
                    //deletThisDocument(id);
                }
            }
            MessageBox.Show("done");
        }
        private void deletThisDocument(Int32 id) {
            OracleCommand c = new OracleCommand(connectionString);
            c.Connection = conn;
            c.CommandText = "Delete from crawler_results where id=:1";
            c.Parameters.Add("id", id);
            int r = c.ExecuteNonQuery();
        }
    }
}