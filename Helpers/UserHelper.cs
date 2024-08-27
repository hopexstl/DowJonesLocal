using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using DJLocal.Models;
using System.Security.Principal;

namespace DJLocal.Helpers
{
    public class UserHelper
    {
        private readonly EdecApp_Trade_MonkeyEntities _db;

        public UserHelper(EdecApp_Trade_MonkeyEntities db)
        {
            _db = db;
        }

        public string GetUserId(IIdentity identity)
        {
            return identity.IsAuthenticated ? identity.GetUserId() : "Demo";
        }

        public string GetMentodIdForUser(string userId)
        {
            var selectedUser = _db.Selected_Users
                                 .Where(p => p.UserId == userId)
                                 .OrderByDescending(p => p.id)
                                 .FirstOrDefault();

            return selectedUser?.MandantID.ToString() ?? string.Empty;
        }
    }
}
