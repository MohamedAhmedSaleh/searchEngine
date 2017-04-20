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
        private Queue<Dictionary<Int32, List<string>>> contentTokens;
        private Queue<String> EnglishContent;
        HtmlToText htmltotext;
        int numberOfDocuments;
        string connectionString = "Data source=orcl; User Id=scott; Password=tiger;";
        OracleConnection conn;
        List<Thread> threads;
        Semaphore semaphore;
        List<string> documentTerms;


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
            documentTerms = new List<string>();
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
                    HandlingModule1(contentTokens.Dequeue());
                }
            }
        }

        private void HandlingModule1(Dictionary<int, List<string>> index)
        {
            saveWordToDataBase(index);
            List<string> stemmers = stemWord(index.Values.ElementAt(0));
            Dictionary<string, int> frequences = Frequences(stemmers);
            Dictionary<string, string> positions = Positions(stemmers);
            OneDocumentInvindex doc = new OneDocumentInvindex(index.Keys.ElementAt(0), index.Values.ElementAt(0).Distinct().ToList(), frequences, positions);
            SaveInvertedIndex(StopWordsRemovals(doc));
        }
        private void saveWordToDataBase(Dictionary<int, List<string>> index)
        {
            conn = new OracleConnection(connectionString);
            conn.Open();
            OracleCommand cmd;
            foreach (KeyValuePair<int, List<string>> item in index)
            {
                int id = item.Key;
                List<string> distinct = item.Value.Distinct().ToList();
                for (int i = 0; i < distinct.Count; i++)
                {
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT_NEW_Term";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("documentId", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = id;
                    cmd.Parameters.Add("Term", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = distinct.ElementAt(i);
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                MessageBox.Show("Save Data Before Stemming Done !!");
            }
            conn.Dispose();
        }
        private List<String> stemWord(List<string> index)
        {
            List<String> stemming = new List<String>();
            for (int i=0;i<index.Count;i++)
            {
                char[] w = index.ElementAt(i).ToCharArray();
                int wLength = index.ElementAt(i).Length;
                Stemmer s = new Stemmer();
                s.add(w, wLength);
                s.stem();
                string stemmed = s.ToString();
                stemming.Add(stemmed);
            }
             return stemming;
        }
    private Dictionary<string, int> Frequences(List<string> term)
    {
        Dictionary<string, int> fre = new Dictionary<string, int>();
        foreach (var grp in term)
        {
            if (fre.ContainsKey(grp))
                fre[grp]++;
            else
                fre[grp] = 1;
        }
        return fre;
    }
    private Dictionary<string, string> Positions(List<string> term)
    {
        Dictionary<string, string> fre = new Dictionary<string, string>();
        int i = 0;
        foreach (var grp in term)
        {
            if (!(fre.ContainsKey(grp)))
                fre[grp] += (i.ToString());
            else if (fre.ContainsKey(grp))
                fre[grp] += '#' + (i.ToString());
            else if (fre.ContainsKey(grp) && i == term.Count - 1)
                fre[grp] += (i.ToString());
            i++;
        }
        return fre;
    }
    private OneDocumentInvindex StopWordsRemovals(OneDocumentInvindex doc)
    {
        OneDocumentInvindex DocumentUpdated;
        String temp = "";
        for (int i = 0; i < doc.Terms.Count; i++)
        {
            temp = doc.Terms[i] + " ";
        }
        string[] AfterWordsRemoval = temp.Split(new string[] {" ","a ", "about", "above", "above", "across", "after", "afterwards", "again", "against", "all", "almost", "alone", "along", "already", "also",
                    "anyway", "anywhere", "are", "around", "as", "at", "back", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "behind", "being", "below", "beside",
                    "besides", "between", "beyond", "bill", "both", "bottom", "but", "by", "call", "can", "cannot", "cant", "co", "con", "could", "couldnt", "cry", "de", "describe", "detail", "do", "done",
                    "down", "due", "during", "each", "eg", "eight", "either", "eleven", "else", "elsewhere", "empty", "enough", "etc", "even", "ever", "every", "everyone", "everything", "everywhere", "except",
                    "few", "fifteen", "fify", "fill", "find", "fire", "first", "five", "for", "former", "formerly", "forty", "found", "four", "from", "front", "full", "further", "get", "give", "go", "had", "has",
                    "hasnt", "have", "he", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "him", "himself", "his", "how", "however", "hundred", "ie", "if", "in", "inc", "indeed",
                    "interest", "into", "is", "it", "its", "itself", "keep", "last", "latter", "latterly", "least", "less", "ltd", "made", "many", "may", "me", "meanwhile", "might", "mill", "mine", "more", "moreover", "most",
                    "mostly", "move", "much", "must", "my","myself", "name", "namely", "neither", "never", "nevertheless", "next", "nine", "no", "nobody", "none", "noone", "nor", "not", "nothing", "now", "nowhere", "of", "off", "often",
                    "on", "once", "one", "only", "onto", "or", "other", "others", "otherwise", "our", "ours", "ourselves", "out", "over", "own", "part", "per", "perhaps", "please", "put", "rather", "re", "same", "see", "seem", "seemed",
                    "seeming", "seems", "serious", "several", "she","should", "show", "side", "since", "sincere", "six", "sixty", "so", "some", "somehow", "someone", "something", "sometime", "sometimes", "somewhere", "still", "such", "system",
                    "take", "ten", "than", "that", "the", "their","them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "thereupon", "these", "they", "thickv", "thin", "third", "this", "those", "though",
                    "three", "through", "throughout", "thru", "thus", "to", "together", "too", "top", "toward", "towards", "twelve", "twenty", "two", "un", "under", "until", "up", "upon", "us", "very", "via", "was", "we", "well", "were", "what",
                    "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever"}, StringSplitOptions.RemoveEmptyEntries);
        List<string> ListOfPureWords = AfterWordsRemoval.ToList();
        DocumentUpdated = new OneDocumentInvindex(doc.DocumentId, ListOfPureWords, doc.Frequences, doc.Positions);
        return DocumentUpdated;
    }
    private void SaveInvertedIndex(OneDocumentInvindex doc)
    {
        conn = new OracleConnection(connectionString);
        conn.Open();
        OracleCommand cmd;
        for (int i = 0; i < doc.Terms.Count(); i++)
        {
            string term = doc.Terms.ElementAt(i);
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "INSERT_NEW_TERM_INVERTEDINDEX";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("Word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = doc.Positions.Keys.ElementAt(i);
            cmd.Parameters.Add("freq", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = doc.Frequences[term];
            cmd.Parameters.Add("pos", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = doc.Positions[term];
            cmd.Parameters.Add("docID", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = doc.DocumentId;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        MessageBox.Show("Save Inverted Indext to One Document Done !!");
    }
}
}