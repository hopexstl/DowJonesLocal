using System;
using System.Configuration;
using System.Data.SqlClient;
using DJLocal.Models;

namespace DJLocal.Helpers
{
    public class QueryHelper
    {
        private readonly string _connectionString;

        public QueryHelper()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DowJonesConnection"].ConnectionString;

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DowJonesConnection' is missing or empty.");
            }
        }

        public void ExecuteSaveRequestAndResponseQuery(SqlParameter[] parameters)
        {
            string query = @"
                INSERT INTO dowjones_Search_Result_Details WITH (NOLOCK) (
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

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
