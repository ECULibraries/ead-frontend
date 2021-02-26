using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using ead_frontend.Models;
using ead_frontend.Repositories;
using ead_frontend.Services;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using XmlWriterSettings = System.Xml.XmlWriterSettings;

namespace ead_frontend.Controllers
{
    public class AdminController : Controller
    {
        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult Index() => View(new AdminModel { Files = Directory.GetFiles(Server.MapPath("~/Uploads"), "*.xml") });

        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult Upload(string file)
        {
            // save + move
            var uploads = Server.MapPath("~/Uploads/" + file);
            var eadid = file.Split('_')[0];

            var guides = Server.MapPath("~/Guides/" + eadid);
            if (!Directory.Exists(guides))
            {
                Directory.CreateDirectory(guides);
            }

            var guideFile = guides + "/" + eadid + ".xml";
            System.IO.File.Delete(guideFile);
            System.IO.File.Delete(guideFile.Replace(".xml", "_preview.xml"));
            System.IO.File.Move(uploads, guideFile);

            var xdoc = new XmlDocument();
            xdoc.Load(guideFile);

            var xmgr = new XmlNamespaceManager(xdoc.NameTable);
            xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");

            InsertControlAccess(eadid, xdoc, xmgr);
            InsertDaos(eadid, xdoc, xmgr);

            // save
            using (new MemoryStream())
            {
                var settings = new XmlWriterSettings();
                settings.Encoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                settings.Indent = true;
                var xw = XmlWriter.Create(guideFile, settings);
                xdoc.Save(xw);
                xw.Close();
            }

            // index
            var sm = new SolrModel();
            sm.SetSolrIndexFields(eadid, guideFile);

            var solrWorker = SolrService<SolrModel>.GetSolrOperations();
            solrWorker.Add(sm);
            solrWorker.Commit();

            TempData["Message"] = $"File Published - <a href='{Url.Action("Index", "Guide", new {eadid})}'>View</a>";
            return RedirectToAction("Index");
        }

        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult Delete(string file)
        {
            // save + move
            var uploads = Server.MapPath("~/Uploads/" + file);
           
            System.IO.File.Delete(uploads);

            TempData["Message"] = "File Deleted";
            return RedirectToAction("Index");
        }

        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult Preview(string file)
        {
            var upload = Server.MapPath($"~/Uploads/{file}.xml");
            var eadid = file.Split('_')[0];
            var guides = Server.MapPath($"~/Guides/{eadid}");
            if (!Directory.Exists(guides))
            {
                Directory.CreateDirectory(guides);
            }
            var guideFile = guides + "/" + eadid + "_preview.xml";
            System.IO.File.Copy(upload, guideFile, true);

            var xdoc = new XmlDocument();
            xdoc.Load(guideFile);

            var xmgr = new XmlNamespaceManager(xdoc.NameTable);
            xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");

            InsertControlAccess(eadid, xdoc, xmgr);
            InsertDaos(eadid, xdoc, xmgr);

            // save
            using (new MemoryStream())
            {
                var settings = new XmlWriterSettings();
                settings.Encoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                settings.Indent = true;
                var xw = XmlWriter.Create(guideFile, settings);
                xdoc.Save(xw);
                xw.Close();
            }
            return RedirectToAction("Index", "Guide", new { eadid = eadid + "_preview" });
        }

        [HttpPost]
        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult FileUpload(HttpPostedFileBase[] files)
        {
            try
            {
                if (files[0] != null)
                {
                    foreach (var file in files)
                    {
                        var filename = Path.GetFileName(file.FileName);

                        if (filename.StartsWith("0265") || filename.StartsWith("0472") || filename.StartsWith("0564") ||
                            filename.StartsWith("0677") || filename.StartsWith("0691") || filename.StartsWith("1096") ||
                            filename.StartsWith("1169")) filename = filename.Insert(4, "-");

                            file.SaveAs(Server.MapPath("~/Uploads/") + filename.Replace(" ", "_").Replace("#", "").Replace(".", "-").Replace("-xml", ".xml"));
                    }
                }
            }
            catch
            {
                ViewBag.Message = "Error while uploading the files.";
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult Unindex(string eadid)
        {
            if (eadid == string.Empty)
            {
                TempData["Message"] = "You must supply an EADID to unindex.";
            }
            else
            {
                var guide = new SolrModel();
                guide.EadId = eadid;
                var solrWorker = SolrService<SolrModel>.GetSolrOperations();
                solrWorker.Delete(guide);
                solrWorker.Commit();

                TempData["Message"] = "Guide Unindexed";
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult MassIndexer()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Uploads"), "*.xml");
            
            var solrWorker = SolrService<SolrModel>.GetSolrOperations();
            foreach (var file in files)
            {
                var sm = new SolrModel();
                var split = Path.GetFileName(file).Split('-');
                var eadid = split.Length == 2 ? split[1].Replace(".xml", "") : split[1] + "-" + split[2].Replace(".xml", "");
                if (eadid.StartsWith("CD") || eadid.StartsWith("LL") || eadid.StartsWith("UA") || eadid.StartsWith("NCR")) eadid = eadid.Replace(".", "-");
                var guides = Server.MapPath("~/Guides/" + eadid);
                if (!Directory.Exists(guides))
                {
                    Directory.CreateDirectory(guides);
                }

                var guideFile = guides + "/" + eadid + ".xml";

                //
                System.IO.File.Copy(file, guideFile, true);
                
                var xdoc = new XmlDocument();
                xdoc.Load(guideFile);

                var xmgr = new XmlNamespaceManager(xdoc.NameTable);
                xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");

                InsertControlAccess(eadid, xdoc, xmgr);
                InsertDaos(eadid, xdoc, xmgr);

                using (new MemoryStream())
                {
                    var settings = new XmlWriterSettings();
                    settings.Encoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                    settings.Indent = true;
                    var xw = XmlWriter.Create(guideFile, settings);
                    xdoc.Save(xw);
                    xw.Close();
                }
                //

                try
                {
                    sm.SetSolrIndexFields(eadid, guideFile);
                    solrWorker.Add(sm);
                }
                catch (Exception e)
                {
                    TempData["Message"] += eadid + ",";
                }
                
            }
            solrWorker.Commit();

            return RedirectToAction("Index");
        }

        [Authorize]
        [ApiAuthorize("Administrators")]
        public ActionResult MassFinder()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Uploads"), "*.xml");

            foreach (var file in files)
            {
                var xdoc = new XmlDocument();
                xdoc.Load(file);

                var xmgr = new XmlNamespaceManager(xdoc.NameTable);
                xmgr.AddNamespace("ead", "urn:isbn:1-931666-22-9");

                var nodeToFind = xdoc.SelectNodes("//ead:odd", xmgr);
                if (nodeToFind != null && nodeToFind.Count > 1)
                {
                    var breakHere = true;
                }
            }
            
            return RedirectToAction("Index");
        }

        private void InsertControlAccess(string eadid, XmlDocument xdoc, XmlNamespaceManager xmgr)
        {
            // wipe existing controlaccess
            var ca = xdoc.SelectSingleNode("//ead:controlaccess", xmgr);
            if (ca == null)
            {
                var archdesc = xdoc.SelectSingleNode("//ead:archdesc", xmgr);
                ca = xdoc.CreateElement("controlaccess", "urn:isbn:1-931666-22-9");
                archdesc.AppendChild(ca);
            }
            else
            {
                ca.RemoveAll();
            }

            if(eadid.StartsWith("CD") || eadid.StartsWith("LL") || eadid.StartsWith("NCR")) eadid = eadid.Replace("-", ".");
            if (eadid.StartsWith("MC") && eadid.Length == 9) eadid = $"{eadid.Substring(0, 6)}-{eadid.Substring(6,3)}";
            if (eadid.StartsWith("OH") && eadid.Length == 9) eadid = $"{eadid.Substring(0, 6)}-{eadid.Substring(6, 3)}";
            // get AS id from EADID
            var AsRepo = new ArchivesSpaceRepo();
            var resource = AsRepo.GetAsResourceByEadId(eadid);

            // get subjects from authority
            var subjects = SearchSubjectByAsUri($"http://archivesspace.ecu.edu/resources/{resource.id}");

            // insert into XML
            if(subjects != null)
            {
                foreach (var subject in subjects.Docs)
                {
                    if (subject.type == "PersonalName")
                    {
                        var persname = xdoc.CreateElement("persname", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        persname.Attributes.Append(auth);
                        persname.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(persname);
                    }

                    if (subject.type == "FamilyName")
                    {
                        var famname = xdoc.CreateElement("famname", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        famname.Attributes.Append(auth);
                        famname.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(famname);
                    }

                    if (subject.type == "CorporateName")
                    {
                        var corpname = xdoc.CreateElement("corpname", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        corpname.Attributes.Append(auth);
                        corpname.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(corpname);
                    }

                    if (subject.type == "Topic")
                    {
                        var sub = xdoc.CreateElement("subject", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        sub.Attributes.Append(auth);
                        sub.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(sub);
                    }

                    if (subject.type == "Geographic")
                    {
                        var geo = xdoc.CreateElement("geogname", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        geo.Attributes.Append(auth);
                        geo.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(geo);
                    }

                    if (subject.type == "Meeting")
                    {
                        var function = xdoc.CreateElement("function", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        function.Attributes.Append(auth);
                        function.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(function);
                    }

                    if (subject.type == "UniformTitle")
                    {
                        var title = xdoc.CreateElement("title", "urn:isbn:1-931666-22-9");
                        var auth = xdoc.CreateAttribute("authfilenumber");
                        auth.Value = subject._id;
                        title.Attributes.Append(auth);
                        title.InnerText = subject.authoritativeLabel;
                        ca.AppendChild(title);
                    }
                }
            }

            //var spatials = SearchSpatialByAsUri($"http://archivesspace.ecu.edu/resources/{resource.id}");
            //if (spatials != null)
            //{
            //    foreach (var spatial in spatials.Docs)
            //    {
            //        var geo = xdoc.CreateElement("geogname", "urn:isbn:1-931666-22-9");
            //        var source = xdoc.CreateAttribute("source");
            //        source.Value = "wikidata";
            //        geo.Attributes.Append(source);
            //        var auth = xdoc.CreateAttribute("authfilenumber");
            //        auth.Value = spatial.externalAuthorityUri;
            //        geo.Attributes.Append(auth);
            //        geo.InnerText = spatial.authoritativeLabel;
            //        ca.AppendChild(geo);
            //    }
            //}
        }

        private CouchDocs SearchSubjectByAsUri(string uri)
        {
            var client = new RestClient("https://davenport.ecu.edu:6984/")
            {
                Authenticator = new HttpBasicAuthenticator("admin", "Cheese4$couch")
            };

            var request = new RestRequest($"subject_authority/_find", Method.POST, DataFormat.Json);
            request.AddParameter("application/json", "{\"selector\": {\"archivesSpaceRelations\":{\"$elemMatch\": {\"$eq\": \"" + uri + "\"} }}, " +
                                                     "\"limit\": 100, \"sort\": [{\"authoritativeLabel\": \"asc\"}] }", ParameterType.RequestBody);
            return JsonConvert.DeserializeObject<CouchDocs>(client.Execute(request).Content);
            //svm.Results.Docs.OrderBy(x => x.topic).OrderBy(x => x.geographic);
        }

        private CouchDocs SearchSpatialByAsUri(string uri)
        {
            var client = new RestClient("https://davenport.ecu.edu:6984/")
            {
                Authenticator = new HttpBasicAuthenticator("admin", "Cheese4$couch")
            };

            var request = new RestRequest($"spatial_authority/_find", Method.POST, DataFormat.Json);
            request.AddParameter("application/json", "{\"selector\": {\"archivesSpaceRelations\":{\"$elemMatch\": {\"$eq\": \"" + uri + "\"} }}, " +
                                                     "\"limit\": 100, \"sort\": [{\"authoritativeLabel\": \"asc\"}] }", ParameterType.RequestBody);
            return JsonConvert.DeserializeObject<CouchDocs>(client.Execute(request).Content);
            //svm.Results.Docs.OrderBy(x => x.topic).OrderBy(x => x.geographic);
        }

        private void InsertDaos(string eadid, XmlDocument xdoc, XmlNamespaceManager xmgr)
        {
            var daos = eadid.StartsWith("UA") ? MetsRepo.GetDigitalObjects(eadid.Replace("-", ".")) :
                eadid.StartsWith("CD") ? MetsRepo.GetDigitalObjects(eadid.Replace("-", ".")) :
                eadid.StartsWith("LL") ? MetsRepo.GetDigitalObjects(eadid.Replace("-", ".")) :
                eadid.StartsWith("NCR") ? MetsRepo.GetDigitalObjects(eadid.Replace("-", ".")) :
                MetsRepo.GetDigitalObjects(eadid);
            if (daos != null)
            {
                //var dsc = xdoc.SelectSingleNode("//ead:dsc", xmgr);
                
                //var componentId = string.Empty;
                foreach (var dao in daos)
                {
                    //var split = dao.localIdentifier.Split('.');
                    //componentId = eadid;
                    //for (var i = 1; i < split.Length; i++)
                    //{
                    //    switch (i)
                    //    {
                    //        case 1:
                    //            componentId += $"-b{split[i]}";
                    //            break;
                    //        case 2:
                    //            if (split[1].Contains("os"))  //8.os1.1 to 0008-bos1-i1 instead of 0008-bos1-f1
                    //            {
                    //                componentId += $"-i{split[i]}";
                    //            }
                    //            else
                    //            {
                    //                componentId += $"-f{split[i]}";
                    //            }
                                
                    //            break;
                    //        case 3:
                    //            componentId += $"-i{split[i]}";
                    //            break;
                    //    }
                    //}

                    //if (!dsc.HasChildNodes)
                    //{
                    //    var cnode = xdoc.CreateElement("c", "urn:isbn:1-931666-22-9");
                    //    var cid = xdoc.CreateAttribute("id");
                    //    cid.Value = "aspace_" + componentId;
                    //    var clevel = xdoc.CreateAttribute("level");
                    //    clevel.Value = "item";
                    //    cnode.Attributes.Append(cid);
                    //    cnode.Attributes.Append(clevel);
                    //    dsc.AppendChild(cnode);

                    //    var did = xdoc.CreateElement("did", "urn:isbn:1-931666-22-9");
                    //    var unitid = xdoc.CreateElement("unitid", "urn:isbn:1-931666-22-9");
                    //    unitid.InnerText = componentId;
                    //    var unittitle = xdoc.CreateElement("unittitle", "urn:isbn:1-931666-22-9");
                    //    unittitle.InnerText = dao.title;
                    //    did.AppendChild(unitid);
                    //    did.AppendChild(unittitle);
                    //    cnode.AppendChild(did);
                    //}

                    //var cont = xdoc.SelectSingleNode($"//ead:c[@id='aspace_{componentId}']", xmgr);
                    var cont = xdoc.SelectSingleNode($"//ead:c[ead:did/ead:unitid='{dao.localIdentifier}']", xmgr);

                    if (cont == null && dao.localIdentifier.Contains("-i"))
                    {
                        var noItemId = dao.localIdentifier.Substring(0, dao.localIdentifier.LastIndexOf('-'));
                        cont = xdoc.SelectSingleNode($"//ead:c[ead:did/ead:unitid='{noItemId}']", xmgr);
                    }

                    if (cont != null)
                    {
                        var daoNode = xdoc.CreateElement("dao", "urn:isbn:1-931666-22-9");
                        var daoID = xdoc.CreateAttribute("id");
                        daoID.Value = dao.localIdentifier;
                        daoNode.Attributes.Append(daoID);
                        var daoHref = xdoc.CreateAttribute("href");
                        daoHref.Value = dao.objectID.ToString();
                        daoNode.Attributes.Append(daoHref);
                        var daoTitle = xdoc.CreateAttribute("title");
                        daoTitle.Value = dao.title;
                        daoNode.Attributes.Append(daoTitle);
                        cont.AppendChild(daoNode);
                    }
                    else //if (dao.localIdentifier.StartsWith("OH"))
                    {
                        //var dsc = xdoc.SelectSingleNode("//ead:dsc", xmgr);

                        //var cnode = xdoc.CreateElement("c", "urn:isbn:1-931666-22-9");
                        //var cid = xdoc.CreateAttribute("id");
                        //cid.Value = "aspace_" + eadid;
                        //var clevel = xdoc.CreateAttribute("level");
                        //clevel.Value = "file";
                        //cnode.Attributes.Append(cid);
                        //cnode.Attributes.Append(clevel);

                        //var did = xdoc.CreateElement("did", "urn:isbn:1-931666-22-9");
                        //var unittitle = xdoc.CreateElement("unittitle", "urn:isbn:1-931666-22-9");
                        //var unitid = xdoc.CreateElement("unitid", "urn:isbn:1-931666-22-9");
                        //unitid.InnerText = eadid;
                        //unittitle.InnerText = "Digitized Material";
                        //did.AppendChild(unittitle);
                        //did.AppendChild(unitid);
                        //cnode.AppendChild(did);

                        var cnode = xdoc.SelectSingleNode("//ead:c[ead:did/ead:unittitle='Digitized Material']", xmgr);
                        if(cnode == null) cnode = xdoc.SelectSingleNode("//ead:c[ead:did/ead:unittitle='Digital Materials']", xmgr);
                        var daoNode = xdoc.CreateElement("dao", "urn:isbn:1-931666-22-9");
                        var daoID = xdoc.CreateAttribute("id");
                        daoID.Value = dao.objectID.ToString();
                        daoNode.Attributes.Append(daoID);
                        var daoTitle = xdoc.CreateAttribute("title");
                        daoTitle.Value = dao.title;
                        daoNode.Attributes.Append(daoTitle);
                        
                        var daoHref = xdoc.CreateAttribute("href");
                        daoHref.Value = dao.objectID.ToString();
                        daoNode.Attributes.Append(daoHref);

                        try
                        {
                            cnode.AppendChild(daoNode);
                        }
                        catch (Exception e)
                        {
                            
                        }


                        //dsc.AppendChild(cnode);
                    }
                }
            }
        }
    }
}