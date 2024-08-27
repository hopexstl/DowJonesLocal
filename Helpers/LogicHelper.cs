using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DJLocal.Models;
using DowJones.Models;
using Newtonsoft.Json;

namespace DJLocal.Helpers
{
    public class LogicHelper
    {
        public string ConvertCountriesTerritoriesToXml(List<CountryTerritory> countriesTerritories)
        {
            if (countriesTerritories == null || !countriesTerritories.Any())
                return string.Empty;

            var xml = new StringBuilder();
            foreach (var country in countriesTerritories)
            {
                xml.Append("<country>");
                xml.Append($"<code>{country.Code ?? string.Empty}</code>");
                xml.Append($"<type>{country.Type ?? string.Empty}</type>");
                xml.Append($"<iso_alpha2>{country.IsoAlpha2 ?? string.Empty}</iso_alpha2>");
                xml.Append($"<iso_alpha3>{country.IsoAlpha3 ?? string.Empty}</iso_alpha3>");
                xml.Append("</country>");
            }
            return xml.ToString();
        }

        public string FormatDateOfBirth(List<DateOfBirth> dateOfBirthList)
        {
            StringBuilder formattedDateOfBirth = new StringBuilder();
            if (dateOfBirthList != null)
            {
                foreach (var dob in dateOfBirthList)
                {
                    formattedDateOfBirth.Append("|");
                    int day, month, year;
                    formattedDateOfBirth.Append(int.TryParse(dob.Day, out day) ? day + "." : ",");
                    formattedDateOfBirth.Append(int.TryParse(dob.Month, out month) ? month + "." : ",");
                    formattedDateOfBirth.Append(int.TryParse(dob.Year, out year) ? year.ToString() : ",");
                }
            }
            return formattedDateOfBirth.ToString();
        }

        public SqlParameter[] CreateSqlParameters(RiskCompSearchRequestModel requestModel, RiskCompSearchResponseModel response, string mentID, int iterationNo, RiskEntity riskEntity)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@MentodId", mentID ?? (object)DBNull.Value),
                new SqlParameter("@metacount", (object)response.Meta?.Count ?? DBNull.Value),
                new SqlParameter("@Iteraion", iterationNo),
                new SqlParameter("@PEid", riskEntity.Id ?? (object)DBNull.Value),
                new SqlParameter("@sType", riskEntity.Type ?? (object)DBNull.Value),
                new SqlParameter("@attrType", riskEntity.Attributes.Type ?? (object)DBNull.Value),
                new SqlParameter("@attrPrimary_name", riskEntity.Attributes.PrimaryName ?? (object)DBNull.Value),
                new SqlParameter("@attrTitle", riskEntity.Attributes.Title ?? (object)DBNull.Value),
                new SqlParameter("@attrCountryterritorycode", riskEntity.Attributes.CountryTerritoryCode ?? (object)DBNull.Value),
                new SqlParameter("@attrCountry_territory_name", riskEntity.Attributes.CountryTerritoryName ?? (object)DBNull.Value),
                new SqlParameter("@attrGender", riskEntity.Attributes.Gender ?? (object)DBNull.Value),
                new SqlParameter("@attrScore", riskEntity.Attributes.Score ?? (object)DBNull.Value),
                new SqlParameter("@attrIcon_hints", string.Join(",", riskEntity.Attributes.IconHints ?? new List<string>())),
                new SqlParameter("@atteCountries_territories", ConvertCountriesTerritoriesToXml(riskEntity.Attributes.CountriesTerritories)),
                new SqlParameter("@attrdate_of_birth", FormatDateOfBirth(riskEntity.Attributes.DateOfBirth)),
                new SqlParameter("@Searchoption", requestModel.Data.Attributes.Sort.ToLower() ?? (object)DBNull.Value),
                new SqlParameter("@SearchKeyword", requestModel.Data.Attributes.FilterGroupAnd.Filters.SearchKeyword.Text ?? (object)DBNull.Value),
                new SqlParameter("@SearchType", requestModel.Data.Type ?? (object)DBNull.Value),
                new SqlParameter("@SearchContentSet", string.Join(",", requestModel.Data.Attributes.FilterGroupAnd.Filters.Content_Set ?? new List<string>())),
                new SqlParameter("@SearchRecordType", string.Join(",", requestModel.Data.Attributes.FilterGroupAnd.Filters.Record_Types ?? new List<string>())),
                new SqlParameter("@Pincode", string.Empty),
                new SqlParameter("@SearchResult", JsonConvert.SerializeObject(response)),
                new SqlParameter("@SearchWay", "single"),
                new SqlParameter("@FoundStatus", "redlist"),
                new SqlParameter("@BatchName", "1"),
                new SqlParameter("@SearchRegin", DBNull.Value),
                new SqlParameter("@SearchUniqueID", DBNull.Value),
                new SqlParameter("@searchdate", DateTime.Now),
                new SqlParameter("@attraltername", riskEntity.Attributes.MatchedCriteria?.Name?.Name ?? string.Empty)
            };
        }
    }
}
