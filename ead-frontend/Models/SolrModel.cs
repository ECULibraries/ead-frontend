using System.Collections;
using System.Xml;
using SolrNet.Attributes;

namespace ead_frontend.Models
{
    public class SolrModel
    {
        [SolrUniqueKey("eadid")]
        public string EadId { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        [SolrField("title_sort")]
        public string TitleSort { get; set; }

        [SolrField("abstract")]
        public string Abstract { get; set; }

        [SolrField("physdesc")]
        public string PhysDesc { get; set; }

        //[SolrField("origination")]
        //public string Origination { get; set; }

        [SolrField("accessions")]
        public string Accessions { get; set; }

        [SolrField("acquisitions")]
        public string Acquisitions { get; set; }

        [SolrField("bionote")]
        public string BioNote { get; set; }

        [SolrField("scope")]
        public string Scope { get; set; }

        [SolrField("container")]
        public string Container { get; set; }

        [SolrField("repo")]
        public string Repository { get; set; }

        //[SolrField("terms")]
        //public ArrayList Terms { get; set; }

        [SolrField("persname")]
        public ArrayList Persname { get; set; }

        [SolrField("corpname")]
        public ArrayList Corpname { get; set; }

        [SolrField("famname")]
        public ArrayList Famname { get; set; }

        [SolrField("topic")]
        public ArrayList Topic { get; set; }

        [SolrField("geogname")]
        public ArrayList Geogname { get; set; }

        [SolrField("hasObjects")]
        public bool DigitizedItems { get; set; }


        public void SetSolrIndexFields(string eadid, string path)
        {
            EadId = eadid;

            var xdoc = new XmlDocument();
            xdoc.Load(path);

            var xmgr = new XmlNamespaceManager(xdoc.NameTable);
            xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");
            xmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");

            Title = xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:unittitle", xmgr).InnerText;
            TitleSort = xdoc.SelectSingleNode("//ead:eadheader/ead:filedesc/ead:titlestmt/ead:titleproper[@type='filing']", xmgr).InnerText;

            var abstractNode = xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:abstract", xmgr);
            if (abstractNode != null) Abstract = abstractNode.InnerText;
            //else Abstract = "EMPTYABSTRACTFIELD";

            var physDescNode = xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:physdesc/ead:genreform", xmgr);
            if (physDescNode != null) PhysDesc = physDescNode.InnerText;
            //else PhysDesc = "EMPTYPHYSDESCFIELD";

            //var origNode = xdoc.SelectSingleNode("//ead:archdesc/ead:did/ead:origination", xmgr);
            //if (origNode != null)
            //{
            //    Origination = null;
            //    foreach (XmlNode node in origNode.ChildNodes)
            //    {
            //        Origination += node.InnerText + " ";
            //    }
            //}
            //else Origination = "EMPTYORIGINATIONFIELD";

            var accessionNode = xdoc.SelectSingleNode("//ead:descgrp[@type='admininfo']/ead:acqinfo[ead:head = 'Accessions Information']", xmgr);
            if (accessionNode != null)
            {
                Accessions = null;
                foreach (XmlNode node in accessionNode.SelectNodes("ead:p", xmgr))
                {
                    Accessions += node.InnerText + " ";
                }
            }
            //else Accessions = "EMPTYACCESSIONSFIELD";

            var acquistionNode = xdoc.SelectSingleNode("//ead:descgrp[@type='admininfo']/ead:acqinfo[ead:head = 'Acquisition Information']", xmgr);
            if (acquistionNode != null)
            {
                Acquisitions = null;
                foreach (XmlNode node in acquistionNode.SelectNodes("ead:p", xmgr))
                {
                    Acquisitions += node.InnerText + " ";
                }
            }
            //else Acquisitions = "EMPTYACQUISITIONSFIELD";

            var bioNode = xdoc.SelectSingleNode("//ead:bioghist", xmgr);
            if (bioNode != null)
            {
                BioNote = null;
                foreach (XmlNode node in bioNode.SelectNodes("ead:p", xmgr))
                {
                    BioNote += node.InnerText + " ";
                }
            }
            //else BioNote = "EMPTYBIONOTEFIELD";

            var scopeNode = xdoc.SelectSingleNode("//ead:scopecontent[ead:head = 'Scope and Content']", xmgr);
            if (scopeNode != null)
            {
                Scope = null;
                foreach (XmlNode node in scopeNode.SelectNodes("ead:p", xmgr))
                {
                    Scope += node.InnerText + " ";
                }
            }
            //else Scope = "EMPTYSCOPEFIELD";

            
            var geoNode = xdoc.SelectNodes("//ead:controlaccess/ead:geogname", xmgr);
            if (geoNode.Count > 0)
            {
                Geogname = new ArrayList();
                foreach (XmlNode node in geoNode)
                {
                    Geogname.Add(node.InnerText);
                }
            }
            var persNode = xdoc.SelectNodes("//ead:controlaccess/ead:persname", xmgr);
            if (persNode.Count > 0)
            {
                Persname = new ArrayList();
                foreach (XmlNode node in persNode)
                {
                    Persname.Add(node.InnerText);
                }
            }
            var persNode2 = xdoc.SelectNodes("//ead:origination/ead:persname", xmgr);
            if (persNode2.Count > 0)
            {
                if(Persname == null) Persname = new ArrayList();
                foreach (XmlNode node in persNode2)
                {
                    Persname.Add(node.InnerText);
                }
            }
            var subNode = xdoc.SelectNodes("//ead:controlaccess/ead:subject", xmgr);
            if (subNode.Count > 0)
            {
                Topic = new ArrayList();
                foreach (XmlNode node in subNode)
                {
                    Topic.Add(node.InnerText);
                }
            }
            var corpNode = xdoc.SelectNodes("//ead:controlaccess/ead:corpname", xmgr);
            if (corpNode.Count > 0)
            {
                Corpname = new ArrayList();
                foreach (XmlNode node in corpNode)
                {
                    Corpname.Add(node.InnerText);
                }
            }
            var corpNode2 = xdoc.SelectNodes("//ead:origination/ead:corpname", xmgr);
            if (corpNode2.Count > 0)
            {
                if (Corpname == null) Corpname = new ArrayList();
                foreach (XmlNode node in corpNode2)
                {
                    Corpname.Add(node.InnerText);
                }
            }
            var famNode = xdoc.SelectNodes("//ead:origination/ead:famname", xmgr);
            if (famNode.Count > 0)
            {
                Famname = new ArrayList();
                foreach (XmlNode node in famNode)
                {
                    Famname.Add(node.InnerText);
                }
            }

            var containerNode = xdoc.SelectNodes("//ead:dsc", xmgr);

            if (containerNode.Count > 0)
            {
                foreach (XmlNode node in containerNode)
                {
                    Container = null;
                    foreach (XmlNode subnode in node.SelectNodes("//ead:unittitle", xmgr))
                    {
                        Container += subnode.InnerText + " ";
                    }
                }

                var daos = xdoc.SelectNodes("//ead:dao", xmgr);
                if (daos.Count > 0) DigitizedItems = true;
                else DigitizedItems = false;
            }
            //else Container = "EMPTYCONTAINERFIELD";


            if (eadid.StartsWith("0") || eadid.StartsWith("1")) Repository = "Manuscript Collection";
            else if (eadid.StartsWith("OH")) Repository = "Manuscript Collection"; //"Oral History Collection";
            else if (eadid.StartsWith("CR")) Repository = "Manuscript Collection"; //"Church Collection";
            else if (eadid.StartsWith("MB")) Repository = "Manuscript Collection"; //"Miscellaneous Broadside Collection";
            else if (eadid.StartsWith("MC")) Repository = "Manuscript Collection"; //"Map Collection";
            else if (eadid.StartsWith("MF")) Repository = "Manuscript Collection"; //"Microfilm Collection";
            else if (eadid.StartsWith("MG")) Repository = "Manuscript Collection"; //"Miscellaneous Genealogy";
            else if (eadid.StartsWith("MN")) Repository = "Manuscript Collection"; //"Miscellaneous Newspapers Collection";
            else if (eadid.StartsWith("MP")) Repository = "Manuscript Collection"; //"Miscellaneous Photographs Collection";
            else if (eadid.StartsWith("OH")) Repository = "Manuscript Collection"; //"Oral History Collection";
            else if (eadid.StartsWith("CD")) Repository = "Country Doctor Museum";
            else if (eadid.StartsWith("LL")) Repository = "Laupus Library";
            else if (eadid.StartsWith("UA")) Repository = "University Archives Collection";
            else if (eadid.StartsWith("NCR")) Repository = "North Carolina Collection";
        }
    }
}