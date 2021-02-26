using System.Collections.Generic;
using System.Web;
using System.Xml;
using Microsoft.Ajax.Utilities;

namespace ead_frontend.Models
{
    public class GuideModel
    {
        private XmlDocument _xdoc;
        private XmlNamespaceManager _xmgr;

        public string EadId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Date { get; set; }

        public string Creator { get; set; }
        public string PhysDesc { get; set; }
        public string Citation { get; set; }
        public string Repository { get; set; }
        public string Access { get; set; }

        public string Abstract { get; set; }
        public XmlNodeList BiogHist { get; set; }
        public XmlNodeList ScopeContent { get; set; }
        public XmlNodeList Accessions { get; set; }
        public XmlNodeList Acquisitions { get; set; }
        public XmlNodeList Processing { get; set; }
        public XmlNodeList Copyright { get; set; }
        public XmlNodeList RelatedMaterial { get; set; }
        public XmlNodeList SeparatedMaterial { get; set; }
        public XmlNode LangMaterial { get; set; }
        public List<OddModel> OddList { get; set; }
        public XmlNodeList Arrangement { get; set; }
        public List<Component> ContainerList { get; set; }

        public XmlNodeList CorpName { get; set; }
        public XmlNodeList GeogName { get; set; }
        public XmlNodeList PersName { get; set; }
        public XmlNodeList FamName { get; set; }
        public XmlNodeList Subject { get; set; }
        public XmlNodeList UniformTitle { get; set; }
        public XmlNodeList Meeting { get; set; }
        //public XmlNodeList Spatial { get; set; }

        public bool HasDigitizedContent { get; set; }
        public string DigitizedUrl { get; set; }
        public string DigitalCount { get; set; }

        public bool IsUmbrella { get; set; }
        public AsRepoId AsRepoId { get; set; }

        public bool ShowExpanded { get; set; }
        public bool IsRequest { get; set; }

        public GuideModel(string id)
        {
            _xdoc = new XmlDocument();
            _xdoc.Load(HttpContext.Current.Server.MapPath($"../../Guides/{id}/{id}.xml"));
            _xmgr = new XmlNamespaceManager(_xdoc.NameTable);
            _xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");
            Title = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:unittitle", _xmgr).InnerText;
        }

        /// <summary>
        /// Populates model with data from EAD XML.
        /// </summary>
        /// <param name="id">Guide Identifier (EADID)</param>
        public GuideModel(string id, bool isRequest)
        {
            IsRequest = isRequest;

            if (id.EndsWith("e"))
            {
                ShowExpanded = true;
                id = id.TrimEnd('e');
            }

            EadId = id.ToUpper();

            if (EadId.StartsWith("0265") || EadId.StartsWith("0472") || EadId.StartsWith("0564") ||
                EadId.StartsWith("0677") || EadId.StartsWith("0691") || EadId.StartsWith("1096") ||
                EadId.StartsWith("1169")) IsUmbrella = true;

            if (EadId.ToLower().StartsWith("ua"))
            {
                AsRepoId = AsRepoId.UniversityArchives;
            }
            else if (EadId.ToLower().StartsWith("cd") || EadId.ToLower().StartsWith("ll"))
            {
                AsRepoId = AsRepoId.LaupusHistoryCollections;
            }
            else
            {
                AsRepoId = AsRepoId.ManuscriptCollection;
            }

            _xdoc = new XmlDocument();

            if (id.Contains("_preview"))
            {
                _xdoc.Load(HttpContext.Current.Server.MapPath($"../Guides/{EadId.Replace("_PREVIEW", "")}/{EadId}.xml"));
            }
            else if (id.Contains("_"))
            {
                _xdoc.Load(HttpContext.Current.Server.MapPath($"../Uploads/{id}.xml"));
            }
            else
            {
                var url = HttpContext.Current.Server.MapPath($"../Guides/{EadId}/{EadId}.xml");
                url = url.Replace("\\findingaids", "");
                _xdoc.Load(url);
            }

            _xmgr = new XmlNamespaceManager(_xdoc.NameTable);
            _xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");

            _xdoc.InnerXml = _xdoc.InnerXml.Replace("<emph render=\"italic\">", "<span class='font-italic'>")
                .Replace("<emph render=\"bolditalic\">", "<span class='font-italic font-weight-bold'>")
                .Replace("<emph render=\"bold\">", "<span class='font-weight-bold'>").Replace("</emph>", "</span>")
                .Replace("<extref", "<a").Replace("</extref>", "</a>");

            Title = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:unittitle", _xmgr).InnerText;
            Subtitle = _xdoc.SelectSingleNode("//ead:filedesc/ead:titlestmt/ead:subtitle", _xmgr).InnerText;
            //Date = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:unitdate", _xmgr).InnerText;
            var unitdates = _xdoc.SelectNodes("//ead:archdesc/ead:did/ead:unitdate", _xmgr);
            foreach (XmlNode date in unitdates)
            {
                Date += date.InnerText + "; ";
            }
            Date = Date.Trim().TrimEnd(';');

            var creators = _xdoc.SelectNodes("//ead:archdesc/ead:did/ead:origination[@label='creator']", _xmgr);
            for (var i = 0; i < creators.Count; i++)
            {
                Creator += creators[i].InnerText;
                if (i < creators.Count - 1) Creator += "; ";
            }
            //Creator = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:origination[@label='creator']", _xmgr)?.InnerText;

            var physNode1 = _xdoc.SelectNodes("//ead:archdesc/ead:did/ead:physdesc/ead:extent[@altrender='materialtype spaceoccupied']", _xmgr);
            var physNode3 = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:physdesc/ead:extent[@altrender='carrier']", _xmgr);

            foreach (XmlNode node in physNode1)
            {
                PhysDesc += node.InnerText + ", ";
            }

            PhysDesc = PhysDesc.Trim().TrimEnd(',');
            //PhysDesc = physNode1.InnerText;
            //if (physNode2 != null)
            //{
            //    PhysDesc += ", " + physNode2.InnerText;
            //}
            if (physNode3 != null)
            {
                PhysDesc += ", " + physNode3.InnerText;
            }

            var genreform = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:physdesc/ead:genreform", _xmgr);
            if (genreform != null)
            {
                PhysDesc += ", " + genreform.InnerText;
            }

            var citation = _xdoc.SelectSingleNode("//ead:archdesc/ead:prefercite/ead:p", _xmgr);
            if (citation != null)
            {
                Citation = citation.InnerText;
            }

            Repository = _xdoc.SelectSingleNode("//ead:publisher", _xmgr).InnerText;

            var access = _xdoc.SelectSingleNode("//ead:accessrestrict/ead:p", _xmgr);
            if (access != null)
            {
                Access = access.InnerText;
            }

            var abst = _xdoc.SelectSingleNode("//ead:abstract", _xmgr);
            if (abst != null)
            {
                Abstract = abst.InnerXml;
            }

            BiogHist = _xdoc.SelectNodes("//ead:bioghist/ead:p", _xmgr);
            ScopeContent = _xdoc.SelectNodes("//ead:archdesc/ead:scopecontent/ead:p", _xmgr);
            RelatedMaterial = _xdoc.SelectNodes("//ead:archdesc/ead:relatedmaterial/ead:p", _xmgr);
            SeparatedMaterial = _xdoc.SelectNodes("//ead:archdesc/ead:separatedmaterial/ead:p", _xmgr);
            Accessions = _xdoc.SelectNodes("//ead:archdesc/ead:acqinfo[ead:head = 'Accessions Information']/ead:p", _xmgr);
            Acquisitions = _xdoc.SelectNodes("//ead:archdesc/ead:acqinfo[ead:head = 'Acquisition Information']/ead:p", _xmgr);
            Processing = _xdoc.SelectNodes("//ead:archdesc/ead:processinfo/ead:p", _xmgr);
            Copyright = _xdoc.SelectNodes("//ead:archdesc/ead:userestrict/ead:p", _xmgr);
            CorpName = _xdoc.SelectNodes("//ead:controlaccess/ead:corpname", _xmgr);
            GeogName = _xdoc.SelectNodes("//ead:controlaccess/ead:geogname[not(@source)]", _xmgr);
            //Spatial = _xdoc.SelectNodes("//ead:controlaccess/ead:geogname[@source]", _xmgr);
            PersName = _xdoc.SelectNodes("//ead:controlaccess/ead:persname", _xmgr);
            FamName = _xdoc.SelectNodes("//ead:controlaccess/ead:famname", _xmgr);
            Subject = _xdoc.SelectNodes("//ead:controlaccess/ead:subject", _xmgr);
            UniformTitle = _xdoc.SelectNodes("//ead:controlaccess/ead:title", _xmgr);
            Meeting = _xdoc.SelectNodes("//ead:controlaccess/ead:function", _xmgr);
            LangMaterial = _xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:langmaterial[@id]", _xmgr);
            Arrangement = _xdoc.SelectNodes("//ead:archdesc/ead:arrangement/ead:p", _xmgr);

            var odds = _xdoc.SelectNodes("//ead:archdesc/ead:odd", _xmgr);
            if (odds != null && odds.Count > 0)
            {
                OddList = new List<OddModel>();
                foreach (XmlNode odd in odds)
                {
                    var newOdd = new OddModel();
                    newOdd.Head = odd.SelectSingleNode("ead:head", _xmgr).InnerText;
                    newOdd.Paragraphs = new List<string>();
                    foreach (XmlNode p in odd.SelectNodes("ead:p", _xmgr))
                    {
                        newOdd.Paragraphs.Add(p.InnerText);
                    }
                    OddList.Add(newOdd);
                }
            }

            var clist = _xdoc.SelectNodes("//ead:dsc/ead:c", _xmgr);

            if (clist != null)
            {
                ContainerList = new List<Component>();
                ContainerList = BuilComponents(clist);
            }

            var daos = _xdoc.SelectNodes("//ead:dao", _xmgr);
            if (daos.Count == 1)
            {
                HasDigitizedContent = true;
                DigitalCount = $" ({daos.Count} item)";
                DigitizedUrl = "http://digital.lib.ecu.edu/" + daos[0].Attributes["href"].Value;
            }
            else if(daos.Count > 1)
            {
                HasDigitizedContent = true;
                DigitalCount = $" ({daos.Count} items)";

                if (AsRepoId == AsRepoId.ManuscriptCollection)
                {
                    if (EadId.ToLower().StartsWith("oh") || EadId.ToLower().StartsWith("0701"))
                    {
                        DigitizedUrl = "http://digital.lib.ecu.edu/search.aspx?q=local_id:" + EadId.Replace("_preview", "").ToUpper() + "*";
                    }
                    else
                    {
                        DigitizedUrl = "http://digital.lib.ecu.edu/search.aspx?q=local_id:" + EadId.Replace("_preview", "") + "-*";
                    }
                    
                }
                else if (AsRepoId == AsRepoId.UniversityArchives)
                {
                    DigitizedUrl = "http://digital.lib.ecu.edu/search.aspx?q=local_id:" + EadId.Replace("_preview", "").Replace("ua", "UA").Replace("-", ".") + "*";
                }
                else if (AsRepoId == AsRepoId.LaupusHistoryCollections)
                {
                    DigitizedUrl = "http://digital.lib.ecu.edu/search.aspx?q=local_id:" + EadId.Replace("_preview", "").Replace("-", ".").ToUpper() + ".*";
                }
            }
        }

        /// <summary>
        /// Recursive method for building a hierarchical list of components from an EAD container list.
        /// </summary>
        /// <param name="list">A list of ead:c nodes</param>
        /// <returns></returns>
        public List<Component> BuilComponents(XmlNodeList list)
        {
            var retVal = new List<Component>();

            foreach (XmlNode node in list)
            {
                var title = node.SelectSingleNode("ead:did/ead:unittitle", _xmgr)?.InnerText;

                if (title == "East Carolina vs Marshall")
                {
                    var t = "HER";
                }
                var date = node.SelectSingleNode("ead:did/ead:unitdate", _xmgr)?.InnerText;
                var unitid = node.SelectSingleNode("ead:did/ead:unitid", _xmgr)?.InnerText;

                if (date != null) title += ", " + date;

                var containerLabel = string.Empty;
                var boxLabel = string.Empty;
                var containers = node.SelectNodes("ead:did/ead:container", _xmgr);
                if (containers != null && containers.Count > 0)
                {
                    foreach (XmlNode cont in containers)
                    {
                        if (cont.Attributes["type"].InnerText == "box") boxLabel = cont.InnerText;
                        containerLabel += char.ToUpper(cont.Attributes["type"].InnerText[0]) + cont.Attributes["type"].InnerText.Substring(1) + " " + cont.InnerText + " ";
                    }
                }
                else
                {
                    containerLabel = unitid;
                }

                var scopeNode = node.SelectNodes("ead:scopecontent/ead:p", _xmgr);
                var scopeParagraphs = new List<string>();
                if (scopeNode != null && scopeNode.Count > 0)
                {
                    foreach (XmlNode para in scopeNode)
                    {
                        scopeParagraphs.Add(para.InnerText);
                    }
                }

                var accessRestrictNode = node.SelectNodes("ead:accessrestrict", _xmgr);
                var accessRestrictList = new List<AccessRestrictModel>();
                if (accessRestrictNode != null)
                {
                    foreach (XmlNode item in accessRestrictNode)
                    {
                        var newAr = new AccessRestrictModel();
                        newAr.Head = item.SelectSingleNode("ead:head", _xmgr).InnerText;
                        newAr.Paragraphs = new List<string>();
                        foreach (XmlNode p in item.SelectNodes("ead:p", _xmgr))
                        {
                            newAr.Paragraphs.Add(p.InnerText);
                        }
                        accessRestrictList.Add(newAr);
                    }
                }

                //var accessRestrictNode = node.SelectNodes("ead:accessrestrict/ead:p", _xmgr);
                //var accessRestrictParagraphs = new List<string>();
                //if (accessRestrictNode != null && accessRestrictNode.Count > 0)
                //{
                //    foreach (XmlNode para in accessRestrictNode)
                //    {
                //        accessRestrictParagraphs.Add(para.InnerText);
                //    }
                //}

                var acqInfoNode = node.SelectNodes("ead:acqinfo/ead:p", _xmgr);
                var acqInfoParagraphs = new List<string>();
                if (acqInfoNode != null && acqInfoNode.Count > 0)
                {
                    foreach (XmlNode para in acqInfoNode)
                    {
                        acqInfoParagraphs.Add(para.InnerText);
                    }
                }

                var processInfoNode = node.SelectNodes("ead:processinfo/ead:p", _xmgr);
                var processInfoParagraphs = new List<string>();
                if (processInfoNode != null && processInfoNode.Count > 0)
                {
                    foreach (XmlNode para in processInfoNode)
                    {
                        processInfoParagraphs.Add(para.InnerText);
                    }
                }

                var daos = node.SelectNodes("ead:dao", _xmgr);
                var dlist = new List<DigitalModel>();
                if (daos != null)
                {
                    foreach (XmlNode dao in daos)
                    {
                        dlist.Add(new DigitalModel{ objectID = int.Parse(dao.Attributes["href"].Value), title = dao.Attributes["title"].Value, localIdentifier = dao.Attributes["id"].Value});
                    }

                    if (containerLabel.IsNullOrWhiteSpace())
                    {
                        containerLabel = node.Attributes["id"].InnerText.Replace("aspace_", "");
                    }
                }

                var oddNode = node.SelectNodes("ead:odd", _xmgr);
                var newOddList = new List<OddModel>();
                if (oddNode != null)
                {
                    foreach(XmlNode item in oddNode)
                    {
                        var newOdd = new OddModel();
                        newOdd.Head = item.SelectSingleNode("ead:head", _xmgr).InnerText;
                        newOdd.Paragraphs = new List<string>();
                        foreach (XmlNode p in item.SelectNodes("ead:p", _xmgr))
                        {
                            newOdd.Paragraphs.Add(p.InnerText);
                        }
                        newOddList.Add(newOdd);
                    }
                }

                var origLocNode = node.SelectSingleNode("ead:originalsloc", _xmgr);
                OriginalsLoc newOrigLoc = null;
                if (origLocNode != null)
                {
                    newOrigLoc = new OriginalsLoc();
                    newOrigLoc.Head = origLocNode.SelectSingleNode("ead:head", _xmgr).InnerText;
                    newOrigLoc.Paragraphs = new List<string>();
                    foreach (XmlNode p in origLocNode.SelectNodes("ead:p", _xmgr))
                    {
                        newOrigLoc.Paragraphs.Add(p.InnerXml);
                    }
                }

                var newComponent = new Component
                {
                    Level = node.Attributes["level"]?.InnerText,
                    OtherLevel = node.Attributes["otherlevel"]?.InnerText,
                    UnitId = unitid,
                    UnitTitleDate = title,
                    Container = containerLabel,
                    BoxLabel = boxLabel,
                    Daos = dlist,
                    HasDigitizedContent = node.SelectNodes(".//ead:dao", _xmgr).Count > 0,
                    ScopeNotes = scopeParagraphs,
                    AccessRestrict = accessRestrictList,
                    AcqInfo = acqInfoParagraphs,
                    ProcessInfo = processInfoParagraphs,
                    AsRepoId = AsRepoId,
                    IsUmbrella = IsUmbrella,
                    Odds = newOddList,
                    OriginalsLoc = newOrigLoc,
                    ShowExpanded = ShowExpanded,
                    IsRequest = IsRequest
                };
                
                var kids = node.SelectNodes("ead:c", _xmgr);
                if (kids != null)
                {
                    newComponent.Children = BuilComponents(kids);
                    if (newComponent.Container.Contains("Box") && !newComponent.Container.Contains("Folder") && kids.Count > 0)
                    {
                        foreach (var child in newComponent.Children)
                        {
                            child.HasBoxParent = true;
                        }
                    }
                }

                retVal.Add(newComponent);
            }

            return retVal;
        }
    }
}