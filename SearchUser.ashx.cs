using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gafware.Modules.Reservations
{
    /// <summary>
    /// Summary description for SearchUser
    /// </summary>
    public class SearchUser : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string term = (context.Request.QueryString["term"] ?? (context.Request.QueryString["q"] ?? String.Empty));
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            IList<UserInfo> users = (new UserController()).GetUsersBasicSearch(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentSettings().PortalId, 0, 100, "DisplayName", true, "DisplayName", term);
            if(users != null)
            {
                bool first = true;
                foreach (UserInfo user in users)
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }
                    sb.Append(String.Format("\"{0}\"", user.DisplayName.Replace("\"", "&quot;")));
                    first = false;
                }
            }
            sb.Append("]");
            context.Response.Write(sb.ToString());
            context.Response.ContentType = "application/json";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}