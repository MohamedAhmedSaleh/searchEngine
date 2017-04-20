using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR
{
    class OneDocumentInvindex
    {
        public int DocumentId;
        public Dictionary<string, int> Frequences;
        public Dictionary<string, string> Positions;
        public List<string> Terms;
        public OneDocumentInvindex(int docid,List<string> terms ,Dictionary<string, int> fre, Dictionary<string, string> pos)
        {
            DocumentId = docid;
            Terms = terms;
            Frequences = fre;
            Positions = pos;
        }
    }
}
