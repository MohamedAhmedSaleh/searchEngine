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

namespace IR
{
    public partial class Form1 : Form
    {
        private List<String> toVisit;
        private List<String> visited;//urls
        private List<String> content;//htmlcontent
        int numberOfDocuments;
        string connectionString = "Data source=orcl; User Id=scott; Password=tiger;";
        OracleConnection conn;

        public Form1()
        {
            InitializeComponent();
            toVisit = new List<string>();
            visited = new List<string>();
            content = new List<string>();
            numberOfDocuments = 3000;
            toVisit.Add("https://www.google.com");
            toVisit.Add("https://en.wikipedia.org/wiki/Main_Page");
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        }

        private void crawl_Click(object sender, EventArgs e)
        {
            Thread newthread = new Thread(new ThreadStart(crawler));
            newthread.IsBackground = true;
            newthread.Start();
        }

        private void crawler()
        {
            int i = 0;

            while (visited.Count != numberOfDocuments)
            {

                if (toVisit.Count != 0)
                {
                    if (!visited.Contains(toVisit[i]))
                    {
                        string temp = HTTPRequest(toVisit[i]);
                        if (!temp.Equals(""))
                        {
                            content.Add(temp);
                            visited.Add(toVisit[i]);

                            string[] row = { "", toVisit[i], content[i] };
                            ListViewItem lvi = new ListViewItem(row);

                            if (listView1.InvokeRequired)
                                listView1.Invoke(new MethodInvoker(delegate
                                {
                                    listView1.Items.Add(lvi);
                                }));
                            else
                                listView1.Items.Add(lvi);

                            searchForLinks(content[i]);
                            i++;
                        }
                    }
                    toVisit.RemoveAt(i);

                }
                if (visitedCount.InvokeRequired)
                    visitedCount.Invoke(new MethodInvoker(delegate
                    {
                        visitedCount.Text = "Visited Pages: " + visited.Count;
                    }));
                else
                    visitedCount.Text = "Visited Pages: " + visited.Count;

            }
            addCrawlerResultsToDatabase();

        }

        public string HTTPRequest(String URL)
        {
            WebRequest myWebRequest;
            WebResponse myWebResponse;

            // Create a new 'WebRequest' object to the mentioned URL.
            Uri uri;
            if (!Uri.TryCreate(URL, UriKind.Absolute, out uri))
                return "";
            string rString;
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
                return "";
            }
            return rString;

        }
        public void searchForLinks(String content)
        {
            if (toVisit.Count < numberOfDocuments+500)
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
                            toVisit.Add(url);
                    }
                }
            }
        }

        private void addCrawlerResultsToDatabase()
        {
            conn = new OracleConnection(connectionString);
            conn.Open();
            int count = 0;
            OracleCommand cmd;

            while (count < visited.Count)
            {
                cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "INSERT_NEW_CRAWLER_RESULT";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("page_url", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = visited[count];
                cmd.Parameters.Add("page_content", OracleDbType.NClob, DBNull.Value, ParameterDirection.Input).Value = content[count];
                cmd.ExecuteNonQuery();
                count++;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}