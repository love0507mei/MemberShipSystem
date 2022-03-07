using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace AuthFilter.Filters
{
    public class CustomAuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            if (string.IsNullOrEmpty(Convert.ToString(filterContext.HttpContext.Session["UserName"])))
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
            {
                string RedirectURL = "https://" + HttpContext.Current.Request.Url.Authority + "/Login/Login";

                filterContext.Result = new ContentResult()
                {
                    Content = "<script>alert('登入超時，請重新登入。');window.location.href='" + RedirectURL + "';</script>",
                    ContentType = "text/html"
                };
            }
        }
    }
}