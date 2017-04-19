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
        private Queue<Dictionary<Int32,List<string>>> contentTokens;
        private Queue<String> EnglishContent;
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
            EnglishContent = new Queue<string>();
            specificContent = new Queue<string>();
            contentTokens = new Queue<Dictionary<Int32, List<string>>>();
            htmltotext = new HtmlToText();
            threads = new List<Thread>();
            numberOfDocuments = 8000;
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
            string defaultstring = "!DOCTYPE HTML PUBLIC";
            string defaultstring2 = "!DOCTYPE html PUBLIC";
            int count2 = 0;
            while (dr.Read())
            {
                var id = Convert.ToInt32(dr["ID"]);
                string dochtmlcontent = dr.GetString((1));

                if ((dochtmlcontent.Contains("lang=en") || dochtmlcontent.Contains("lang={{locale}}") || dochtmlcontent.Contains(defaultstring2) || dochtmlcontent.Contains(defaultstring)))
                {
                    EnglishContent.Enqueue(dochtmlcontent);
                    count++;
                }
                else
                {
                    count2++;
                    deletThisDocument(id);
                }
            }
            MessageBox.Show("Filtering Done");
        }
        private void deletThisDocument(Int32 id)
        {
            conn = new OracleConnection(connectionString);
            OracleCommand cmd;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "deletThisDocument";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("id", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = id;
            cmd.ExecuteNonQuery();
        }

        private void button2_Click(object sender, EventArgs e)
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

            while (dr.Read())
            {
                var id = Convert.ToInt32(dr["ID"]);
                Page page = new Page();
                page.Id = id;
                page.SpecificContent = dr.GetString(2);
                Thread newthread = new Thread(new ParameterizedThreadStart(tokenize));
                threads.Add(newthread);
                newthread.IsBackground = true;
                newthread.Start(page);
            }
            Thread thread = new Thread(new ThreadStart(hangleProcessingThreads));
            thread.IsBackground = true;
            thread.Start();
        }
        public void tokenize(Object obj)
        {
            Page page = (Page)obj;
            string[] tokens = page.SpecificContent.Split(new string[] {" ", ",","،","°","!", "%", "&","(", ")", "*","**", "*!", ";", "+=", "**=","+", "-", ".", "/","/!", "//","ً",
                    "{","}", "^", "-=","<","<=","<>","=","==",">",">=","?","@", "|=","*=","[","]","|","~","~="/*,"`"*/,":","&=","/=","\r\n","\r","\n","\\","\"","?","ـ","©","؟","ُ"
                    ,"ال","ا","آ","ٱ","إ","أ","ل","ك","ط","ظ","م","ء","ق","خ","ع","ف","ج","ح","ش","س","غ","ص","ذ","د","ز","ر","ء","ؤ","ّ",
                    "و","ض","ب","ت","ث","ن","ي","ئ","ى","ه","ة"}, StringSplitOptions.RemoveEmptyEntries);
            List<string> ListOfStrings = tokens.ToList();
            List<string> ListOfEnglishStrings = new List<string>();
            Dictionary<int, List<string>> temp = new Dictionary<int, List<string>>();
            for (int i = 0; i < ListOfStrings.Count; i++)
            {
                if (Regex.IsMatch(ListOfStrings[i], "^[a-zA-Z]*$"))
                {
                    
                    ListOfEnglishStrings.Add(ListOfStrings[i].ToLower());
                }
            }
            temp[page.Id] = ListOfEnglishStrings;
            contentTokens.Enqueue(temp);
        }

        private void hangleProcessingThreads()
        {
            while (threads.Count != 0 || contentTokens.Count > 0)
            {
                if (label1.InvokeRequired)
                    visitedCount.Invoke(new MethodInvoker(delegate
                    {
                        label1.Text = "ContentTokens : " + contentTokens.Count;
                    }));
                else
                    label1.Text = "ContentTokens: " + contentTokens.Count;
                for (int i = 0; i < threads.Count; i++)
                    if (!threads[i].IsAlive)
                        threads.Remove(threads[i]);
                if (contentTokens.Count > 0)
                {
                    CalculateFrequency(contentTokens.Dequeue());
                }
            }
        }
        private void CalculateFrequency(Dictionary<int, List<string>> index) {

        }
        /** Test program for demonstrating the Stemmer.  It reads text from a
		 * a list of files, stems each word, and writes the result to standard
		 * output. Note that the word stemmed is expected to be in lower case:
		 * forcing lower case must be done outside the Stemmer class.
		 * Usage: Stemmer file-name file-name ...
		 */
        public static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:  Stemmer <input file>");
                return;
            }
            char[] w = new char[501];
            Stemmer s = new Stemmer();
            for (int i = 0; i < args.Length; i++)
                try
                {
                    FileStream _in = new FileStream(args[i], FileMode.Open, FileAccess.Read);
                    try
                    {
                        while (true)
                        {
                            int ch = _in.ReadByte();
                            if (Char.IsLetter((char)ch))
                            {
                                int j = 0;
                                while (true)
                                {
                                    ch = Char.ToLower((char)ch);
                                    w[j] = (char)ch;
                                    if (j < 500)
                                        j++;
                                    ch = _in.ReadByte();
                                    if (!Char.IsLetter((char)ch))
                                    {
                                        /* to test add(char ch) */
                                        for (int c = 0; c < j; c++)
                                            s.add(w[c]);
                                        /* or, to test add(char[] w, int j) */
                                        /* s.add(w, j); */
                                        s.stem();

                                        String u;

                                        /* and now, to test toString() : */
                                        u = s.ToString();

                                        /* to test getResultBuffer(), getResultLength() : */
                                        /* u = new String(s.getResultBuffer(), 0, s.getResultLength()); */

                                        Console.Write(u);
                                        break;
                                    }
                                }
                            }
                            if (ch < 0)
                                break;
                            Console.Write((char)ch);
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("error reading " + args[i]);
                        break;
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("file " + args[i] + " not found");
                    break;
                }
        }
    }
}