using System.Collections.Generic;

namespace ead_frontend.Models
{
    public class CouchDocs
    {
        public List<Doc> Docs { get; set; }
    }

    public class Doc
    {
        public string _id { get; set; }
        public string _rev { get; set; }

        public string authoritativeLabel { get; set; }
        public string type { get; set; }
        public string externalAuthorityUri { get; set; }
        //public List<string> familyNameCreator { get; set; }
        //public List<string> familyNameSource { get; set; }
        //public List<string> corporateNameCreator { get; set; }
        //public List<string> corporateNameSource { get; set; }
        //public List<string> personalNameCreator { get; set; }
        //public List<string> personalNameSource { get; set; }

        //public List<string> topic { get; set; }
        //public List<string> geographic { get; set; }
        //public List<string> meeting { get; set; }
        //public List<string> uniformTitle { get; set; }
        //public List<string> personalNameSubject { get; set; }
        //public List<string> familyNameSubject { get; set; }
        //public List<string> corporateNameSubject { get; set; }


    }
}