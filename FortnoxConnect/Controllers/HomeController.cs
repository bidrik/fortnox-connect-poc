using System;
using System.Web.Mvc;

namespace FortnoxConnect.Controllers
{
    /// <summary>
    /// Home controller for the main pages
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Landing page with login button
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Success page after authentication
        /// </summary>
        public ActionResult Success()
        {
            var accessToken = Session["AccessToken"] as string;
            var refreshToken = Session["RefreshToken"] as string;
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Index");
            }

            ViewBag.HasToken = true;
            return View();
        }

        /// <summary>
        /// API test page to demonstrate calling Fortnox API
        /// </summary>
        public ActionResult ApiTest()
        {
            var accessToken = Session["AccessToken"] as string;
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
