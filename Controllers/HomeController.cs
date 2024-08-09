using System.Web.Mvc;
using DowJones.Models;
using DowJones.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using DJLocal.Models;
using System;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Xml.Linq;

namespace DJLocal.Controllers
{
    public class HomeController : Controller
    {
        public dowjones_TrademonkeyEntities2 dj = new dowjones_TrademonkeyEntities2();
        private EdecApp_Trade_MonkeyEntities db = new EdecApp_Trade_MonkeyEntities();
        private readonly AuthService _authService;
        private readonly RiskComplianceApiService _riskComplianceApiService;

        public HomeController()
        {
            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new System.Uri(ConfigurationManager.AppSettings["BaseUrl:Auth"])
            };

            TokenRequestModel tokenRequestModel = new TokenRequestModel
            {
                client_id = ConfigurationManager.AppSettings["ClientId"],
                username = ConfigurationManager.AppSettings["Username"],
                password = ConfigurationManager.AppSettings["Password"],
                grant_type = ConfigurationManager.AppSettings["grant_type"],
                device = ConfigurationManager.AppSettings["device"],
                scope = ConfigurationManager.AppSettings["scope"],
                connection = ConfigurationManager.AppSettings["connection"]
            };

            _authService = new AuthService(httpClient, tokenRequestModel);
            _riskComplianceApiService = new RiskComplianceApiService(httpClient);
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Search(RiskCompSearchRequestModel requestModel)
        {
            if (ModelState.IsValid)
            {
                TokenResponseModel idTokenResponse = await _authService.GetIdTokenAsync();
                if (idTokenResponse != null && !string.IsNullOrEmpty(idTokenResponse.id_token))
                {
                    TokenResponseModel accessTokenResponse = await _authService.GetAccessTokenAsync(idTokenResponse.id_token);
                    if (accessTokenResponse != null && !string.IsNullOrEmpty(accessTokenResponse.access_token))
                    {
                        string accessToken = accessTokenResponse.access_token;
                        string jsonResponse = await _riskComplianceApiService.SearchRiskComplianceAsync(requestModel, accessToken);

                        RiskCompSearchResponseModel response = JsonConvert.DeserializeObject<RiskCompSearchResponseModel>(jsonResponse);

                        SaveRequestAndResponse(requestModel, response);

                        return View("SearchResponse", response);
                    }
                }
            }
            return Json(new { success = false, message = "Invalid request." });
        }



        private void SaveRequestAndResponse(RiskCompSearchRequestModel requestModel, RiskCompSearchResponseModel response)
        {
            System.IO.File.WriteAllText(Server.MapPath("~/App_Data/Request.txt"), JsonConvert.SerializeObject(requestModel));
            System.IO.File.WriteAllText(Server.MapPath("~/App_Data/Response.txt"), JsonConvert.SerializeObject(response));

            string connectionString = ConfigurationManager.ConnectionStrings["DowJonesConnection"].ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DowJonesConnection' is missing or empty.");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                int iteratioNo = 1;
                connection.Open();
                foreach (var riskEntity in response.Data)
                {
                    var query = @"
                INSERT INTO dowjones_Search_Result_Details (
                    MentodId, metacount, Iteraion, PEid, sType, attrType, attrPrimary_name, 
                    attrTitle, attrCountryterritorycode, attrCountry_territory_name, attrGender, 
                    attrScore, attrIcon_hints, atteCountries_territories, attrdate_of_birth, 
                    Searchoption, SearchKeyword, SearchType, SearchContentSet, SearchRecordType, 
                    Pincode, SearchResult, SearchWay, FoundStatus, BatchName, 
                    SearchRegin, SearchUniqueID, searchdate, attraltername
                ) VALUES (
                    @MentodId, @metacount, @Iteraion, @PEid, @sType, @attrType, @attrPrimary_name, 
                    @attrTitle, @attrCountryterritorycode, @attrCountry_territory_name, @attrGender, 
                    @attrScore, @attrIcon_hints, @atteCountries_territories, @attrdate_of_birth, 
                    @Searchoption, @SearchKeyword, @SearchType, @SearchContentSet, @SearchRecordType, 
                    @Pincode, @SearchResult, @SearchWay, @FoundStatus, @BatchName, 
                    @SearchRegin, @SearchUniqueID, @searchdate, @attraltername
                )";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MentodId", requestModel.Data.Attributes.FilterGroupAnd.Filters.SearchKeyword.Text ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@metacount", (object)response.Meta?.Count ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Iteraion", iteratioNo);
                        command.Parameters.AddWithValue("@PEid", riskEntity.Id ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@sType", riskEntity.Type ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrType", riskEntity.Attributes.Type ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrPrimary_name", riskEntity.Attributes.PrimaryName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrTitle", riskEntity.Attributes.Title ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrCountryterritorycode", riskEntity.Attributes.CountryTerritoryCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrCountry_territory_name", riskEntity.Attributes.CountryTerritoryName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrGender", riskEntity.Attributes.Gender ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrScore", riskEntity.Attributes.Score ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrIcon_hints", string.Join(",", riskEntity.Attributes.IconHints ?? new List<string>()));
                        command.Parameters.AddWithValue("@atteCountries_territories", JsonConvert.SerializeObject(riskEntity.Attributes.CountriesTerritories) ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@attrdate_of_birth", riskEntity.Attributes.DateOfBirth != null ? JsonConvert.SerializeObject(riskEntity.Attributes.DateOfBirth) : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Searchoption", requestModel.Data.Attributes.Sort ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SearchKeyword", requestModel.Data.Attributes.FilterGroupAnd.Filters.SearchKeyword.Text ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SearchType", requestModel.Data.Type ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@SearchContentSet", string.Join(",", requestModel.Data.Attributes.FilterGroupAnd.Filters.Content_Set ?? new List<string>()));
                        command.Parameters.AddWithValue("@SearchRecordType", string.Join(",", requestModel.Data.Attributes.FilterGroupAnd.Filters.Record_Types ?? new List<string>()));
                        command.Parameters.AddWithValue("@Pincode", DBNull.Value);
                        command.Parameters.AddWithValue("@SearchResult", DBNull.Value);
                        command.Parameters.AddWithValue("@SearchWay", "single");
                        command.Parameters.AddWithValue("@FoundStatus", "redlist");
                        command.Parameters.AddWithValue("@BatchName", "1");
                        command.Parameters.AddWithValue("@SearchRegin", DBNull.Value);
                        command.Parameters.AddWithValue("@SearchUniqueID", DBNull.Value);
                        command.Parameters.AddWithValue("@searchdate", DateTime.Now);
                        command.Parameters.AddWithValue("@attraltername", DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                    iteratioNo++;
                }
            }
        }
        [HttpPost]
        public async Task<JsonResult> Einzelpruefung_djsettings(string SearchSearchType, string searchoption, string nameSearchText, string nameSearchRegion, string nameSearchContentSet, string nameSearchRecordType)
        {
            string userId = GetUserId();
            var selectedusers = db.Selected_Users.Where(p => p.UserId == userId).ToList().LastOrDefault();
            var MandantID = selectedusers?.MandantID.ToString();

            if (MandantID == null)
            {
                return Json(new { success = false, message = "MandantID not found." });
            }

            try
            {
                var djs = dj.dowjones_Search_Config_Single_Search.Where(p => p.MentodId == MandantID).ToList().LastOrDefault();
                var config_xml = $"<searchoption>name</searchoption><nameSearchText>{nameSearchText}</nameSearchText><nameSearchSearchType>{SearchSearchType}</nameSearchSearchType><nameSearchRegion>{nameSearchRegion}</nameSearchRegion><nameSearchContentSet>{nameSearchContentSet}</nameSearchContentSet><nameSearchRecordType>{nameSearchRecordType}</nameSearchRecordType>";

                if (djs != null)
                {
                    djs.NameSearch = config_xml;
                    djs.updated_date = DateTime.Now;
                    djs.updated_by = selectedusers.Username;
                    djs.SearchType = SearchSearchType;
                    dj.Entry(djs).State = EntityState.Modified;
                }
                else
                {
                    dowjones_Search_Config_Single_Search new_dj = new dowjones_Search_Config_Single_Search
                    {
                        IDTypeSearch = "<PEIDSearchAMEArticleType dowjonesfield=\"ame_article_type\">all</PEIDSearchAMEArticleType><PEIDSearchAssPrimaryName dowjonesfield=\"include-associate-primary-name\">true</PEIDSearchAssPrimaryName>",
                        NameSearch = config_xml,
                        Config_Date = DateTime.Now,
                        SearchType = SearchSearchType,
                        created_by = selectedusers.Username,
                        MentodId = MandantID
                    };

                    dj.dowjones_Search_Config_Single_Search.Add(new_dj);
                }

                dj.SaveChanges();
                return Json(new { success = true, message = "Configuration saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string GetUserId()
        {
            return (User.Identity.IsAuthenticated ? User.Identity.GetUserId() : "Demo");
        }

        private void getLanguage()
        {
        }

        //public ActionResult Add_DJ_Config()
        //{
        //    return View();
        //}

        public ActionResult DJ_Config()
        {
            string userid = "123";
            var selected_users = db.Selected_Users.Where(p => p.UserId == userid).ToList().LastOrDefault();
            try
            {


                var result = dj.dowjones_Search_Config_Single_Search.ToList();
                var midlist = string.Join(",", result.Select(p => p.MentodId));
                var list_query = "select * from Admin_Mandant where id not in (" + midlist + ") and Plugin_Access like '%30%'";
                var getMandantlist = db.Admin_Mandant.SqlQuery(list_query).ToList();
                ViewBag.Mandantname = getMandantlist;
                return View(result);
            }
            catch (Exception e)
            {

            }
            return View();
        }
        public JsonResult Add_DJ_Config(string mandantid, string searchtype)
        {
            string userid = GetUserId();
            var selectedusers = db.Selected_Users.Where(p => p.UserId == userid).ToList().LastOrDefault();
            dowjones_Search_Config_Single_Search new_dj = new dowjones_Search_Config_Single_Search();
            if (string.IsNullOrEmpty(searchtype))
            {
                searchtype = "near";
            }
            var config_xml = "<searchoption>name</searchoption><nameSearchText>[searchtext]</nameSearchText><nameSearchSearchType>" + searchtype + "</nameSearchSearchType><nameSearchRegion>ANY</nameSearchRegion><nameSearchContentSet>Watchlist</nameSearchContentSet><nameSearchRecordType>Person,Entity</nameSearchRecordType>";
            var idtyprsearch = "<PEIDSearchAMEArticleType dowjonesfield=\"ame_article_type\">all</PEIDSearchAMEArticleType><PEIDSearchAssPrimaryName dowjonesfield=\"include-associate-primary-name\">true</PEIDSearchAssPrimaryName>";
            new_dj.IDTypeSearch = idtyprsearch;
            new_dj.NameSearch = config_xml;
            new_dj.Config_Date = DateTime.Now;
            new_dj.SearchType = searchtype;
            new_dj.created_by = selectedusers.Username;
            new_dj.updated_date = DateTime.Now;
            new_dj.updated_by = selectedusers.Username;
            new_dj.MentodId = mandantid;
            dj.dowjones_Search_Config_Single_Search.Add(new_dj);
            dj.SaveChanges();

            try
            {
                //var xmlpat = Server.MapPath("~/assets/userconfig.xml");
                //var xDoc = XDocument.Load(xmlpat);   // alternatively XDocuemnt.Parse

                //var xmlconfig = "<user mendid=\""+ mandantid + "\"><client_id>zuNaTUfYMxoi6IDZSDauM7jW3mc80EePznjoeMSZ</client_id><username>9TRA016100-svcaccount@dowjones.com</username><password>KR6imNXGGJ9f2ax0</password></user>";
                //xDoc.Root.Elements().First().AddFirst(xmlconfig);


                //xDoc.Save(xmlpat);

                XDocument xDocument = XDocument.Load(Server.MapPath("~/assets/userconfig.xml"));
                XElement root = xDocument.Element("users");
                IEnumerable<XElement> rows = root.Descendants("user");
                XElement firstRow = rows.First();

                firstRow.AddBeforeSelf(
                   new XElement("user", new XAttribute("mendid", mandantid),
                   new XElement("client_id", "zuNaTUfYMxoi6IDZSDauM7jW3mc80EePznjoeMSZ"),
                   new XElement("username", "9TRA016100-svcaccount@dowjones.com"), new XElement("password", "KR6imNXGGJ9f2ax0")));
                xDocument.Save(Server.MapPath("~/assets/userconfig.xml"));
            }
            catch (Exception e)
            {
                Error_Log er = new Error_Log();
                er.Doc_Type = "Add_DJ_Config ";
                er.Error_Messages = e.Message + "~" + e.InnerException;
                er.Status = true;
                er.Date_Created = DateTime.Now;
                // er.Mandant_Id = Mandantid.ToInt();
                db.Error_Log.Add(er);
                db.SaveChanges();
            }
            return Json("ok", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Delete_Dj_config(int id)
        {
            dowjones_Search_Config_Single_Search djs = dj.dowjones_Search_Config_Single_Search.Where(p => p.SearchID == id).ToList().LastOrDefault();
            if (djs != null)
            {
                dj.dowjones_Search_Config_Single_Search.Remove(djs);
                dj.SaveChanges();
            }
            return RedirectToAction("DJ_Config", "Admin");
        }

    }
}
