using System.Collections.Generic;

namespace ead_frontend.Models
{
    /// <summary>
    /// Model for an ead:c node.
    /// </summary>
    public class Component
    {
        public string Level { get; set; }
        public string OtherLevel { get; set; }
        public string UnitId { get; set; }
        public string UnitTitleDate { get; set; }
        public string Container { get; set; }
        public string BoxLabel { get; set; }
        public List<DigitalModel> Daos { get; set; }
        public List<Component> Children { get; set; }
        public bool HasDigitizedContent { get; set; }
        public List<string> ScopeNotes { get; set; }
        public List<AccessRestrictModel> AccessRestrict { get; set; }
        public List<string> AcqInfo { get; set; }
        public List<string> ProcessInfo { get; set; }
        public AsRepoId AsRepoId { get; set; }
        public bool IsUmbrella { get; set; }
        public List<OddModel> Odds { get; set; }
        public OriginalsLoc OriginalsLoc { get; set; }
        public bool ShowExpanded { get; set; }
        public bool IsRequest { get; set; }
        public bool HasBoxParent { get; set; }
    }
}