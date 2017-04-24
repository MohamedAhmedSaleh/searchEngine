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
        List<string> stop;

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
            conn = new OracleConnection(connectionString);
            stop  = new List<string>();
            conn.Open();
            numberOfDocuments = 15000;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            toVisit.Enqueue("https://en.wikipedia.org/wiki/Main_Page");
            toVisit.Enqueue("https://www.google.com");
            toVisit.Enqueue("https://maktoob.yahoo.com/?p=us");
            toVisit.Enqueue("http://www.msn.com");
            toVisit.Enqueue("https://www.techmeme.com/");
            toVisit.Enqueue("https://yts.ag/");
            toVisit.Enqueue("https://www.quora.com/");

            string[] StopWords = new string[] {"a", "about", "above", "after","again", "against", "all", "almost", "along", "am", "also","and","any","anyway", "anywhere", "are","aren't","as", "at",
                    "because", "be", "before", "becomes", "becoming", "been", "before", "beforehand", "behind", "being", "below", "beside","besides", "between", "beyond", "both", "bottom", "but", "by",
                    "can", "cannot", "cant","could", "couldnt",
                    "do", "describe", "detail", "do", "done","down", "due", "during",
                    "each", "eg", "eight", "either", "eleven", "else", "elsewhere", "empty", "enough", "etc", "even", "ever", "every", "everyone", "everything", "everywhere", "except",
                    "few", "fifteen", "fify", "fill", "find", "fire", "first", "five", "for", "former", "formerly", "forty", "found", "four", "from", "front", "full", "further", "get", "give", "go", "had", "has",
                    "hasnt", "have", "he", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "him", "himself", "his", "how", "however", "hundred", "ie", "if", "in", "inc", "indeed",
                    "interest", "into", "is", "it", "its", "itself", "keep", "last", "latter", "latterly", "least", "less", "ltd", "made", "many", "may", "me", "meanwhile", "might", "mill", "mine", "more", "moreover", "most",
                    "mostly", "move", "much", "must", "my","myself", "name", "namely", "neither", "never", "nevertheless", "next", "nine", "no", "nobody", "none", "noone", "nor", "not", "nothing", "now", "nowhere", "of", "off", "often",
                    "on", "once", "one", "only", "onto", "or", "other", "others", "otherwise", "our", "ours", "ourselves", "out", "over", "own", "part", "per", "perhaps", "please", "put", "rather", "re", "same", "see", "seem", "seemed",
                    "seeming", "seems", "serious", "several", "she","should", "show", "side", "since", "sincere", "six", "sixty", "so", "some", "somehow", "someone", "something", "sometime", "sometimes", "somewhere", "still", "such", "system",
                    "take", "ten", "than", "that", "the", "their","them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "thereupon", "these", "they", "thickv", "thin", "third", "this", "those", "though",
                    "three", "through", "throughout", "thru", "thus", "to", "together", "too", "top", "toward", "towards", "twelve", "twenty", "two", "un", "under", "until", "up", "upon", "us", "very", "via", "was", "we", "well", "were", "what",
                    "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever" };
            for (int i = 0; i < StopWords.Count(); i++)
            {
                stop.Add(StopWords[i]);
            }
        }

        private void crawl_Click(object sender, EventArgs e)
        {
            int numberOfThreads = 1000;
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
                        {
                            if (!released)
                                semaphore.Release();
                        }
                    }
                    catch (Exception ex)
                    {
                        ;
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
                try
                {
                    string dochtmlcontent = dr.GetString(1);

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
                catch
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
            conn.Open();
            OracleCommand cmd;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "deletThisDocument";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("id", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = id;
            cmd.ExecuteNonQuery();
            conn.Close();
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
            dr.Close();
            conn.Close();
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
                    Thread thread = new Thread(new ParameterizedThreadStart(HandlingModule1));
                    thread.IsBackground = true;
                    threads.Add(thread);
                    thread.Start(contentTokens.Dequeue());
                    //HandlingModule1(contentTokens.Dequeue());
                }
            }
            MessageBox.Show("Save Inverted Indext to One Document Done !!");
        }

        private void HandlingModule1(object obj)
        {
            Dictionary<int, List<string>> index = (Dictionary<int, List<string>>)obj;
            //Dictionary<int, List<string>> index = (Dictionary<int, List<string>>)obj;
            semaphore.WaitOne();
            saveWordToDataBase(index);
            semaphore.Release();
            List<string> stemmers = stemWord(index.Values.ElementAt(0));
            Dictionary<string, int> frequences = Frequences(stemmers);
            Dictionary<string, string> positions = Positions(stemmers);
            OneDocumentInvindex doc = new OneDocumentInvindex(index.Keys.ElementAt(0),stemmers.Distinct().ToList(), frequences, positions);
            semaphore.WaitOne();
            SaveInvertedIndex(StopWordsRemovals(doc));
            semaphore.Release();
        }

        private void saveWordToDataBase(Dictionary<int, List<string>> index)
        {
            OracleCommand cmd;
            foreach (KeyValuePair<int, List<string>> item in index)
            {
                int id = item.Key;
                List<string> distinct = item.Value.Distinct().ToList();
                conn.Open();
                for (int i = 0; i < distinct.Count; i++)
                {
                    string term = distinct.ElementAt(i);
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    if (TermInserted(term, "GetTermDetailsBefStem"))
                    {
                        cmd.CommandText = "UpdateTermBEFSTEMMING";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("Term", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = term;
                        cmd.Parameters.Add("documentId", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = id.ToString();
                    }
                    else
                    {
                        cmd.CommandText = "INSERTNEWTERMBEFSTEMMING";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("Term", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = term;
                        cmd.Parameters.Add("documentId", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = id.ToString();
                    }
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                conn.Close();
            }
        }
        private List<String> stemWord(List<string> index)
        {
            List<String> stemming = new List<String>();
            for (int i = 0; i < index.Count; i++)
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
                    fre[grp] = (i.ToString());
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
            List<string> documentTerms = doc.Terms;
            for (int i = 0; i < documentTerms.Count; i++) {
                if (stop.Contains(documentTerms[i])) {
                    documentTerms.RemoveAt(i);
                }
            }
            DocumentUpdated = new OneDocumentInvindex(doc.DocumentId, documentTerms, doc.Frequences, doc.Positions);
            return DocumentUpdated;
        }
        private void SaveInvertedIndex(OneDocumentInvindex doc)
        {
            for (int i = 0; i < doc.Terms.Count(); i++)
            {
                string term = doc.Terms.ElementAt(i);
                conn.Open();
                if (TermInserted(term, "GetTermDetails"))
                {
                    UpdateTerm(term, doc.Positions[term], doc.Frequences[term].ToString(), doc.DocumentId.ToString());
                }
                else
                {
                    InsertTerm(term, doc.Positions[term], doc.Frequences[term].ToString(), doc.DocumentId.ToString());
                }
                conn.Close();
            }
        }
        private void InsertTerm(string term,string possition,string frequences,string documentID) {
            OracleCommand cmd;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "INSERTNEWTERMINVERTEDINDEX";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("Word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = term;
            cmd.Parameters.Add("pos", OracleDbType.Clob, DBNull.Value, ParameterDirection.Input).Value =   possition;
            cmd.Parameters.Add("freq", OracleDbType.Clob, DBNull.Value, ParameterDirection.Input).Value =  frequences;
            cmd.Parameters.Add("docID", OracleDbType.Clob, DBNull.Value, ParameterDirection.Input).Value = documentID;
            cmd.Parameters.Add("docNumbers", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = 1;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateTerm(string term, string NewPos, string NewFreq, string NewDocIDS) {
            OracleCommand cmd;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "UpdateTermDetailsInvertedIndex";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("Word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = term;
            cmd.Parameters.Add("pos", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = NewPos;
            cmd.Parameters.Add("freq", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value =  NewFreq;
            cmd.Parameters.Add("docID", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = NewDocIDS;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool TermInserted(string term,string procedureName) {
            OracleCommand cmd;
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = term;
            cmd.Parameters.Add("documentDetails", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
            OracleDataReader dr = cmd.ExecuteReader();
            if (dr.HasRows)
            {
                dr.Close();
                return true;
            }
            else
            {
                dr.Close();
                return false;
            }
        }
    }
}