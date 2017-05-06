using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.DataAccess.Client;
using System.Data;

namespace Search_Engine
{
    public partial class Home : System.Web.UI.Page
    {
        List<string> searchKeywords;
        List<string> searchKeywordsSplited;
        List<string> stopWords;
        List<string> postionsList;
        List<string> frequenciesList;
        List<string> docIDsList;
        List<int> docsNumberList;
        List<int> docID;
        List<string> URLs;
        Dictionary<string, int> SoundexResultsOneWord;
        Dictionary<string, List<int>> termsMap;
        string connectionString = "Data source=orcl; User Id=scott; Password=tiger;";
        OracleConnection conn;
        Dictionary<char, char> indexes;
        List<List<string>> All_Keys;
        List<string> Intersections;
        bool ExactSearch;
        OracleCommand cmd;
        OracleDataReader dr;
        bool SoundexOneWord;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initiate Variables I need to use
            init();
            SearchResultsText.Visible = false;
            searchResults.Visible = false;
            ListBox1.Visible = false;
        }


        private void init()
        {
            stopWords = new string[] {"a", "about", "above", "after","again", "against", "all", "almost", "along", "am", "also","and","any","anyway", "anywhere", "are","aren't","as", "at",
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
                    "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever" }.ToList();
            indexes = new Dictionary<char, char>();
            Intersections = new List<string>();
            indexes.Add('A', '0');
            indexes.Add('E', '0');
            indexes.Add('O', '0');
            indexes.Add('U', '0');
            indexes.Add('I', '0');
            indexes.Add('H', '0');
            indexes.Add('W', '0');
            indexes.Add('Y', '0');
            indexes.Add('B', '1');
            indexes.Add('F', '1');
            indexes.Add('P', '1');
            indexes.Add('V', '1');
            indexes.Add('C', '2');
            indexes.Add('J', '2');
            indexes.Add('G', '2');
            indexes.Add('K', '2');
            indexes.Add('Q', '2');
            indexes.Add('S', '2');
            indexes.Add('X', '2');
            indexes.Add('Z', '2');
            indexes.Add('D', '3');
            indexes.Add('T', '3');
            indexes.Add('L', '4');
            indexes.Add('N', '5');
            indexes.Add('M', '5');
            indexes.Add('R', '6');
            conn = new OracleConnection(connectionString);
            conn.Open();
            termsMap = new Dictionary<string, List<int>>();
            postionsList = new List<string>();
            frequenciesList = new List<string>();
            docIDsList = new List<string>();
            docsNumberList = new List<int>();
            docID = new List<int>();
            All_Keys = new List<List<string>>();
            URLs = new List<string>();
            SoundexResultsOneWord = new Dictionary<string, int>();
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            ExactSearch = false;
            SoundexOneWord = false;
            SearchResultsText.Visible = false;
            searchResults.Visible = false;
            ListBox1.Visible = false;
            // Empty Query
            if (SearchWords.Text.Length == 0)
                RequiredFieldValidator2.Visible = true;
            else
            {
                // Handle UI
                RequiredFieldValidator2.Visible = false;
                // Radiobutton Selection 
                string selectedValue = RadioButtonList1.SelectedValue;
                if (selectedValue == "Soundex")
                {
                    searchKeywords = TokenLinguistics(SearchWords.Text, false, true);
                    if (searchKeywords.Count == 1)
                        SearchTermBySoundex();
                    else
                    {
                        /*SearchResultsText.Visible = true;
                        SearchResultsText.InnerText = "Did you Mean :";
                        ListBox1.Visible = true;*/

                        // SearchMultiTermsBySoundex
                    }
                }
                // K-Gram
                else if (selectedValue == "spelling correction")
                    SpellingCorrection();
                // Searching
                else
                    startSearch();
            }
        }
        private void startSearch()
        {
            SearchResultsText.Visible = true;
            SearchResultsText.InnerText = "Search Results : ";
            ListBox1.Visible = false;
            searchResults.Visible = true;
            // Know Search Type Multi word or exactSearch
            if (SearchWords.Text[0] == '\"' && SearchWords.Text[SearchWords.Text.Length - 1] == '\"')
                ExactSearch = true;
            // Apply Tokenization and linguistics algorithms .
            searchKeywords = TokenLinguistics(SearchWords.Text, true, true);
            // If Query One Word ... Ranking with Frequency
            if (searchKeywords.Count == 1)
                SearchOneWord(searchKeywords[0]);
            // Query Multi Word ... Ranking In Intersection Documents with Distance Between Words and other Frequences 
            else
                SearchMultiWord(searchKeywords);
        }
        private List<string> TokenLinguistics(string query, bool stemming, bool remstopwords)
        {
            searchKeywords = new List<string>();
            // split commas , dots and Remove any punctuation character from the word 
            searchKeywordsSplited = query.Split(new string[] {
                " ", ",","،","°","!", "%", "&","(", ")", "*","**", "*!", ";", "+=", "**=","+", "-", ".", "/","/!", "//","ً",
                    "{","}", "^", "-=","<","<=","<>","=","==",">",">=","?","@", "|=","*=","[","]","|","~","~="/*,"`"*/,":","&=","/=","\r\n","\r","\n","\\","\"","?","ـ","©","؟","ُ"
                    ,"ال","ا","آ","ٱ","إ","أ","ل","ك","ط","ظ","م","ء","ق","خ","ع","ف","ج","ح","ش","س","غ","ص","ذ","د","ز","ر","ء","ؤ","ّ",
                    "و","ض","ب","ت","ث","ن","ي","ئ","ى","ه","ة"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            // Remove Stop Words
            if (remstopwords)
            {
                for (int i = 0; i < searchKeywordsSplited.Count; i++)
                    if (!stopWords.Contains(searchKeywordsSplited[i]))
                        searchKeywords.Add(searchKeywordsSplited.ElementAt(i));
            }
            else
            {
                searchKeywords = searchKeywordsSplited;
            }
            // Apply Stemming
            if (stemming)
                searchKeywords = stemWord(searchKeywords);
            // Apply CaseFolding
            for (int i = 0; i < searchKeywords.Count; i++)
                searchKeywords[i] = searchKeywords[i].ToLower();
            return searchKeywords;
        }
        // Calc Stemming To List of Words
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
        // Searching Of One Word
        private void SearchOneWord(string Word)
        {
            // Get Word Details from Database (possitions,frequences,docIds).
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "getWordDetails";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = Word;
            cmd.Parameters.Add("documentDetails", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                URLs = new List<string>();
                // save docIds and frequencies in lists 
                string docIDs = dr.GetValue(4).ToString();
                docIDsList.AddRange(docIDs.Split(',').ToList());
                string frequencies = dr.GetValue(3).ToString();
                frequenciesList.AddRange(frequencies.Split(',').ToList());
                dr.Close();
                // Make Dictionary key is document_id and value is frequency
                Dictionary<string, int> onewordmap = new Dictionary<string, int>();
                for (int i = 0; i < docIDsList.Count; i++)
                    onewordmap[docIDsList[i]] = int.Parse(frequenciesList[i]);
                // Sort Keys (document_ids) by values (frequency) -> Descending
                Dictionary<string, int> dctTemp = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> pair in onewordmap.OrderByDescending(key => key.Value))
                    dctTemp.Add(pair.Key, pair.Value);
                // Get Urls with doc_ids from DB
                foreach (var id in dctTemp.Keys)
                {
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "getDocURL";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("docID", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = id;
                    cmd.Parameters.Add("documentDetails", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                        URLs.Add(dr.GetValue(0).ToString());
                    dr.Close();
                }
                if (SoundexOneWord)
                    for (int i = 0; i < URLs.Count; i++)
                    {
                        SoundexResultsOneWord[URLs.ElementAt(i)] = dctTemp.ElementAt(i).Value;
                    }
                else
                {
                    // Show URLS
                    searchResults.DataSource = URLs;
                    searchResults.DataBind();
                }
            }
            else
                SpellingCorrection();
        }
        private void SearchTermBySoundex()
        {
            // Intialization
            SoundexOneWord = true;
            // Handle Ui
            // Get Soundex Of One Word
            string soundex = Soundex(searchKeywords.ElementAt(0));
            // Get Terms from Soundex (DB)
            List<string> terms = GetTermsSoundex(soundex);
            // Rank Terms with EditDistance
            Dictionary<string, int> rankdic = RankingTermsSoundex(terms, searchKeywords.ElementAt(0));
            if (rankdic.Count > 0)
            {
                // Sort Dictionary 
                Dictionary<string, int> dctTemp = new Dictionary<string, int>();
                List<string> recomendationWords = new List<string>();
                foreach (KeyValuePair<string, int> pair in rankdic.OrderBy(key => key.Value))
                    dctTemp.Add(pair.Key, pair.Value);
                // Filter And Get List of Recommendation Values
                for (int i = 0; i < dctTemp.Count(); i++)
                    recomendationWords.Add(dctTemp.ElementAt(i).Key);
                foreach (string word in recomendationWords)
                    SearchOneWord(word);
                Dictionary<string, int> Urls = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> pair in SoundexResultsOneWord.OrderByDescending(key => key.Value))
                    Urls.Add(pair.Key, pair.Value);
                // Pop ListBox
                string results = "";
                for (int i = 0; i < recomendationWords.Count; i++)
                {
                    if (i == recomendationWords.Count - 1)
                        results += recomendationWords.ElementAt(i);
                    else
                        results += recomendationWords.ElementAt(i) + ", ";
                }
                SearchResultsText.InnerText = "Search Results Of : " + results;
                SearchResultsText.Visible = true;
                searchResults.Visible = true;
                searchResults.DataSource = Urls.Keys.ToList();
                searchResults.DataBind();
                RadioButtonList1.ClearSelection();
            }
            else
            {
                SearchResultsText.InnerText = "Your Soundex Search - " + SearchWords.Text + " - did not match any documents. You Should Use Spelling Correction";
            }
        }
        private void SearchMultiWord(List<string> Words)
        {
            bool Handle = true;
            // Get Details To each Word in query
            foreach (var word in Words)
            {
                cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "getWordDetails";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = word;
                cmd.Parameters.Add("documentDetails", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    // get Number Of document Has this word and save in list
                    int docsNumber = int.Parse(dr.GetValue(5).ToString());
                    docsNumberList.Add(docsNumber);
                    // get document_ids Has this word and save in list
                    string docIDs = dr.GetValue(4).ToString();
                    docIDsList.AddRange(docIDs.Split(',').ToList());
                    // save ids of document has word in one index in LIST
                    All_Keys.Add(docIDs.Split(',').ToList());
                    // get frequencies of this word in document and save in list
                    string frequencies = dr.GetValue(3).ToString();
                    frequenciesList.AddRange(frequencies.Split(',').ToList());
                    // get possitions of this word in document and save in list
                    string positions = dr.GetValue(2).ToString();
                    postionsList.AddRange(positions.Split(',').ToList());
                    dr.Close();
                }
                else
                {
                    SpellingCorrection();
                    Handle = false;
                }
            }
            if (Handle)
            {
                // get intersection of document_ids between query words
                List<string> ShowIntersect = All_Keys[0];
                for (int i = 0; i < All_Keys.Count - 1; i++)
                    ShowIntersect = ShowIntersect.Intersect(All_Keys[i + 1]).ToList();
                // build maping between (Word+id) -> possitions in dictionary
                int k = 0;
                int size = 0;
                int j = 0;
                foreach (var docNum in docsNumberList)
                {
                    size += docNum;
                    for (; k < size; k++)
                    {
                        List<string> pos = postionsList[k].Split('#').ToList();
                        List<int> positons = new List<int>();
                        pos.ForEach(p => positons.Add(int.Parse(p)));
                        termsMap[Words[j] + docIDsList[k]] = positons;
                    }
                    j++;
                }
                // Ranking Pages
                rankPages(Words, ShowIntersect);
            }
        }
        private void rankPages(List<string> Words, List<string> ids)
        {

            List<string> keys = new List<string>();
            List<double> disances = new List<double>();
            SortedDictionary<string, double> distancemap = new SortedDictionary<string, double>();
            // make keys merge between word and interscted ids
            foreach (var i in ids)
                foreach (var word in Words)
                    keys.Add(word + i);
            // Calc avg distances between term and following term
            // if order words is change and avg distance is neg rank this page decrease
            for (int i = 0; i < keys.Count - Words.Count + 1; i += Words.Count)
            {
                double docavg = 0;
                for (int j = i, k = 0; k < Words.Count - 1; j++, k++)
                    docavg += distance(termsMap[keys[j]], termsMap[keys[j + 1]]);
                docavg /= Words.Count - 1;
                if (double.IsNaN(docavg))
                    disances.Add(100000);
                else
                    disances.Add(docavg);
            }
            // maping between distances and document_ids
            for (int i = 0; i < ids.Count; i++)
                distancemap[ids[i]] = disances[i];
            // sort document_ids by values (distances)
            Dictionary<string, double> dctTemp = new Dictionary<string, double>();
            foreach (KeyValuePair<string, double> pair in distancemap.OrderBy(key => key.Value))
                dctTemp.Add(pair.Key, pair.Value);
            // filter ids if type of searching is exact and result of urls is documents has distance one only .
            if (ExactSearch && Words.Count > 1)
            {
                Dictionary<string, double> dctTemp2 = new Dictionary<string, double>();
                foreach (KeyValuePair<string, double> pair in dctTemp.Where(p => p.Value == 1))
                    dctTemp2.Add(pair.Key, pair.Value);
                dctTemp = dctTemp2;
            }
            else
            {
                // Continue Ranking in other documents didn't have all words 
                // calc frequency of words in this documents
                Dictionary<string, double> AllIdsFrecdocs = new Dictionary<string, double>();
                for (int i = 0; i < docIDsList.Count; i++)
                {
                    if (AllIdsFrecdocs.ContainsKey(docIDsList[i]))
                        AllIdsFrecdocs[docIDsList[i]] += double.Parse(frequenciesList[i]);
                    else
                        AllIdsFrecdocs[docIDsList[i]] = double.Parse(frequenciesList[i]);
                }
                // get ids that isn't intersect between words
                List<List<string>> DffDocsIds = new List<List<string>>();
                for (int i = 0; i < All_Keys.Count; i++)
                    DffDocsIds.Add(All_Keys[i].Except(ids).ToList());
                // get frequency of ids
                Dictionary<string, double> IdsFrecdocs = new Dictionary<string, double>();
                for (int i = 0; i < DffDocsIds.Count; i++)
                    for (int j = 0; j < DffDocsIds[i].Count; j++)
                        IdsFrecdocs[DffDocsIds[i].ElementAt(j)] = AllIdsFrecdocs[DffDocsIds[i].ElementAt(j)];
                // add sorted ranking ids by frequency with sorted ranking ids by distances
                foreach (KeyValuePair<string, double> pair in IdsFrecdocs.OrderByDescending(key => key.Value))
                    dctTemp.Add(pair.Key, pair.Value);
            }
            // get Url by ids
            foreach (var id in dctTemp.Keys)
            {
                OracleCommand cmd;
                cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "getDocURL";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("docID", OracleDbType.Int32, DBNull.Value, ParameterDirection.Input).Value = id;
                cmd.Parameters.Add("documentDetails", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                    URLs.Add(dr.GetValue(0).ToString());
                dr.Close();
            }
            if (dctTemp.Count == 0)
                SearchResultsText.InnerText = "Your search - " + SearchWords.Text + " - did not match any documents.";
            // show Urls 
            searchResults.DataSource = URLs;
            searchResults.DataBind();
        }
        // Get Four Chars is soundex for any term .
        private string Soundex(string term)
        {
            term = term.ToUpper();
            string soundex = "";
            soundex += term[0];
            for (int i = 1; i < term.Length; i++)
                soundex += indexes[term[i]];
            for (int i = 1; i < soundex.Length - 1; i++)
                if (soundex[i] == soundex[i + 1])
                    soundex = soundex.Remove(i, 1);
            soundex = soundex.Replace("0", string.Empty);
            if (soundex.Length < 4)
            {
                int temp = 4 - soundex.Length;
                for (int i = 0; i < temp; i++)
                    soundex += '0';
            }
            else if (soundex.Length > 4)
                soundex = soundex.Substring(0, 3);
            return soundex;
        }
        // Get Terms With Soundex
        private List<string> GetTermsSoundex(string soundex)
        {
            string terms = "";
            cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "getTermsSoundex";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("soun", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = soundex;
            cmd.Parameters.Add("termsofsoundex", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
            dr = cmd.ExecuteReader();
            if (dr.Read())
                terms = dr.GetValue(0).ToString();
            dr.Close();
            return terms.Split('#').ToList();
        }
        // Ranking Soundex By Edit_Distance
        private Dictionary<string, int> RankingTermsSoundex(List<string> terms, string query)
        {
            Dictionary<string, int> rankDic = new Dictionary<string, int>();
            foreach (string term in terms)
            {
                int dis = editDistance(term, query);
                if (dis <= 4)
                    rankDic[term] = dis;
            }
            return rankDic;
        }
        // Calc Average Distance between words in one document
        private double distance(List<int> pos1, List<int> pos2)
        {
            List<int> dist = new List<int>();
            int i = 0;
            double avgdist = 0;
            foreach (var p2 in pos2)
            {
                bool found = false;

                while (i < pos1.Count && pos1[i] < p2)
                {
                    i++;
                    found = true;
                }
                if (found)
                {
                    i--;
                    dist.Add(p2 - pos1[i]);
                    if (ExactSearch && (p2 - pos1[i]) == 1)
                        return 1;
                    i++;
                }
            }
            foreach (var dis in dist)
                avgdist += dis;
            return avgdist / dist.Count;
        }


        protected void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchWords.Text = ListBox1.SelectedValue.ToString();
            RadioButtonList1.ClearSelection();
            startSearch();
        }

        private void SpellingCorrection()
        {
            // Handle UI
            SearchResultsText.InnerText = "Did you Mean : ";
            // Tokenize Query without stemming and stop Removals
            List<string> searchKeyWords = TokenLinguistics(SearchWords.Text.ToString(), false, false);
            //  Get True Words From Query
            List<string> TrueWords = InInvertedIndex(searchKeyWords);
            // Difference between Right and Wrong words
            List<string> WrongWords = new List<string>();
            List<int> TrueIndexs = new List<int>();
            List<string> TrueTerms = new List<string>();
            for (int i = 0; i < searchKeyWords.Count(); i++)
            {
                if (!TrueWords.Contains(searchKeyWords[i]))
                    WrongWords.Add(searchKeyWords[i]);
                else
                {
                    TrueIndexs.Add(i);
                    TrueTerms.Add(searchKeywords.ElementAt(i));
                }
            }
            // Query hasn't any True words
            if (TrueWords.Count == 0 && WrongWords.Count == 0)
                WrongWords = searchKeyWords;
            // Make spelling correction
            if (WrongWords.Count > 0)
            {
                // Get All Grams to each word
                List<List<string>> Allgrams = new List<List<string>>();
                List<string> GramsTerm = new List<string>();
                List<List<List<string>>> AllTerms = new List<List<List<string>>>();
                foreach (string word in WrongWords)
                {
                    List<string> gramsWord = getBigrams(word);
                    foreach (string gr in gramsWord)
                        if (!gramsWord.Contains(gr))
                            gramsWord.Add(gr);
                    Allgrams.Add(gramsWord);
                }
                // Get All terms to each gram
                foreach (List<string> GramTerms in Allgrams)
                {
                    List<List<string>> terms = new List<List<string>>();
                    foreach (string gram in GramTerms)
                    {
                        cmd = new OracleCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = "GetTermsGram";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("gr", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = gram;
                        cmd.Parameters.Add("termsofsoundex", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            terms.Add(dr.GetValue(0).ToString().Split('#').ToList());
                            dr.Close();
                        }
                    }
                    AllTerms.Add(terms);
                }
                // Filter Terms by Semilarity Weight
                List<List<List<string>>> FilterdWordsSW = new List<List<List<string>>>();
                foreach (string term in WrongWords)
                {
                    List<List<string>> WordsWeights = new List<List<string>>();
                    foreach (List<List<string>> Listwords in AllTerms)
                    {
                        //Calculate smilarity weight between current word and input query
                        //If smilarity weight > 0.45
                        foreach (List<string> words in Listwords)
                        {
                            List<string> weights = calculateSimilarityWeight(words, term);
                            if (calculateSimilarityWeight(words, term).Count > 0)
                                WordsWeights.Add(weights);
                        }
                    }
                    FilterdWordsSW.Add(WordsWeights);
                }
                // Filter Terms by EditDistance
                List<Dictionary<string, int>> EaWoDicED = new List<Dictionary<string, int>>();
                int termIndex = 0;
                foreach (string term in WrongWords)
                {
                    Dictionary<string, int> FilterdWordsED = new Dictionary<string, int>();
                    foreach (List<string> fiterWordsInTerm in FilterdWordsSW.ElementAt(termIndex))
                    {
                        foreach (string word in fiterWordsInTerm)
                            FilterdWordsED[word + '#' + term] = editDistance(word, term);
                    }
                    termIndex++;
                    EaWoDicED.Add(FilterdWordsED);
                }
                // sort terms in dictionarys
                List<Dictionary<string, int>> dctTempList = new List<Dictionary<string, int>>();
                Dictionary<string, int> dctTemp = new Dictionary<string, int>();
                List<string> recomendationWords = new List<string>();
                for (int i = 0; i < EaWoDicED.Count; i++)
                {
                    dctTemp = new Dictionary<string, int>();
                    foreach (KeyValuePair<string, int> pair in EaWoDicED.ElementAt(i).OrderBy(key => key.Value))
                        dctTemp.Add(pair.Key, pair.Value);
                    dctTempList.Add(dctTemp);
                }
                // Get recomendation words 
                foreach (string word in WrongWords)
                    foreach (Dictionary<string, int> temp in dctTempList)
                    {
                        int counter = 0;
                        for (int i = 0; i < temp.Count(); i++)
                        {
                            string[] Merge = temp.ElementAt(i).Key.Split('#');
                            if (Merge[1] == word)
                            {
                                recomendationWords.Add(Merge[0]);
                                counter++;
                                if (counter == 2)
                                    break;
                            }
                        }
                    }
                // Put each two words in list to get combinations
                List<string> EaFilterTerm = new List<string>();
                List<string> TrueQuery;
                List<List<string>> lstMaster = new List<List<string>>();
                IEnumerable<string> lstRes = new List<string> { null };
                for (int i = 0; i < recomendationWords.Count(); i += 2)
                {
                    TrueQuery = new List<string>();
                    for (int j = 0; j < 2; j++)
                        TrueQuery.Add(recomendationWords.ElementAt(i + j));
                    lstMaster.Add(TrueQuery);
                }
                // Adding Stop words and Right Words
                if (TrueIndexs.Count > 0)
                {
                    int index = 0;
                    foreach (string word in TrueTerms)
                    {
                        List<string> right_word = new List<string>();
                        right_word.Add(word);
                        lstMaster.Insert(TrueIndexs.ElementAt(index), right_word);
                        index++;
                    }
                }
                // Handle Exact Search Query
                if (ExactSearch)
                {
                    List<string> HandleExactSearch = new List<string>();
                    List<string> HandleExactSearch2 = new List<string>();
                    HandleExactSearch.Add("\"");
                    HandleExactSearch2.Add("\"");
                    lstMaster.Insert(0, HandleExactSearch);
                    lstMaster.Add(HandleExactSearch2);
                }
                //Get All combinations between recomendation words
                foreach (var list in lstMaster)
                {
                    // cross join the current result with each member of the next list
                    lstRes = lstRes.SelectMany(o => list.Select(s => o + ' ' + s));
                }
                // Handle Ui and show Recomendations 
                ListBox1.Visible = true;
                searchResults.Visible = false;
                ListBox1.DataSource = lstRes;
                ListBox1.DataBind();
            }
            // If Query totally hasn't wrong words
            else if (WrongWords.Count == 0)
            {
                startSearch();
                RadioButtonList1.ClearSelection();
            }
        }
        private List<string> InInvertedIndex(List<string> queryWords)
        {
            // This Term in InvertedIndex Or Not
            List<string> TrueWords = new List<string>();
            foreach (string term in queryWords)
            {
                OracleCommand cmd;
                cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.CommandText = "GetTermDetailsBefStem";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("word", OracleDbType.Varchar2, DBNull.Value, ParameterDirection.Input).Value = term;
                cmd.Parameters.Add("documentDetails", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output);
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                    TrueWords.Add(term);
            }
            return TrueWords;
        }
        // get grams of one term
        // cat -> $c , ca , at , a$
        private List<string> getBigrams(string term)
        {
            string[] Bigrams = new string[term.Length + 1];
            Bigrams[0] = "$" + term[0].ToString();
            for (int i = 0; i < term.Length - 1; i++)
                Bigrams[i + 1] = term[i].ToString() + term[i + 1].ToString();
            Bigrams[term.Length] = term[term.Length - 1].ToString() + "$";
            return Bigrams.ToList();
        }
        // calculate sismilarityWeight between two terms 
        private List<string> calculateSimilarityWeight(List<string> dicTerm, string queryTerm)
        {
            List<string> FilterdWords = new List<string>();
            foreach (string word in dicTerm)
            {
                List<string> dicTermBigrams = getBigrams(word);
                List<string> queryTermBigrams = getBigrams(queryTerm);
                double commonBigrams = Convert.ToDouble(dicTermBigrams.Intersect(queryTermBigrams).Count());
                double similarityWeight = ((2.0 * commonBigrams) / (queryTermBigrams.Count + dicTermBigrams.Count));
                if (similarityWeight >= 0.45)
                    if (!FilterdWords.Contains(word))
                        FilterdWords.Add(word);
            }
            return FilterdWords;
        }
        // calculate editDistance between two terms (copy,replace,delete,insert)
        private int editDistance(string a, string b)
        {

            if (string.IsNullOrEmpty(a))
            {
                if (!string.IsNullOrEmpty(b))
                {
                    return b.Length;
                }
                return 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                if (!string.IsNullOrEmpty(a))
                {
                    return a.Length;
                }
                return 0;
            }

            int cost;
            int[,] d = new int[a.Length + 1, b.Length + 1];
            int min1;
            int min2;
            int min3;

            for (int i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (int i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (int i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (int j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }
    }
}