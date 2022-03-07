using AuthFilter.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HomeController.Controllers
{
    [CustomAuthenticationFilter]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UpdateInfo()
        {
            return View();
        }

        public ActionResult SearchInfo()
        {
            return View();
        }
        public ActionResult DeleteInfo()
        {
            return View();
        }
    }
}