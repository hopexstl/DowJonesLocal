using DJLocal.Helpers;
using DJLocal.Models;
using DowJones.Models;
using DowJones.Services;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DJLocal.Controllers
{
    public class HomeController : Controller
    {
        private readonly RiskComplianceApiService _riskComplianceApiService;
        private readonly AuthHelper _authHelper;
        private readonly DatabaseHelper _databaseHelper;
        private readonly UserHelper _userHelper;

        public HomeController()
        {
            HttpClient httpClient = new HttpClient();
            _riskComplianceApiService = new RiskComplianceApiService(httpClient);
            _authHelper = new AuthHelper(httpClient);
            _databaseHelper = new DatabaseHelper();
            _userHelper = new UserHelper(new EdecApp_Trade_MonkeyEntities());
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
                string accessToken = await _authHelper.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    ViewBag.Error = "Failed to retrieve access token.";
                    return View("Index");
                }

                string jsonResponse = await _riskComplianceApiService.SearchRiskComplianceAsync(requestModel, accessToken);
                RiskCompSearchResponseModel responseModel = JsonConvert.DeserializeObject<RiskCompSearchResponseModel>(jsonResponse);

                string userId = _userHelper.GetUserId(User.Identity);
                string mentID = _userHelper.GetMentodIdForUser(userId);

                _databaseHelper.SaveRequestAndResponse(requestModel, responseModel, mentID);

                return View("SearchResponse", responseModel);
            }

            return Json(new { success = false, message = "Invalid request." });
        }
    }
}
