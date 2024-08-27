using System.Collections.Generic;
using DJLocal.Models;
using DowJones.Models;

namespace DJLocal.Helpers
{
    public class DatabaseHelper
    {
        private readonly QueryHelper _queryHelper;
        private readonly LogicHelper _logicHelper;

        public DatabaseHelper()
        {
            _queryHelper = new QueryHelper();
            _logicHelper = new LogicHelper();
        }

        public void SaveRequestAndResponse(RiskCompSearchRequestModel requestModel, RiskCompSearchResponseModel response, string mentID)
        {
            int iterationNo = 1;

            foreach (var riskEntity in response.Data)
            {
                var parameters = _logicHelper.CreateSqlParameters(requestModel, response, mentID, iterationNo, riskEntity);
                _queryHelper.ExecuteSaveRequestAndResponseQuery(parameters);
                iterationNo++;
            }
        }
    }
}
