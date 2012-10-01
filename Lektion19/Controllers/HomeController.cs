using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.ApplicationBlock;
using System.Configuration;

namespace Lektion19.Controllers
{
    public class HomeController : Controller
    {
        private string AccessToken
        {
            get { return (string)Session["TwitterAccessToken"]; }
            set { Session["TwitterAccessToken"] = value; }
        }
        private InMemoryTokenManager TokenManager
        {
            get
            {
                var tokenManager = (InMemoryTokenManager)HttpContext.Application["TwitterTokenManager"];
                if (tokenManager == null)
                {
                    string consumerKey = ConfigurationManager.AppSettings
                    ["twitterConsumerKey"];
                    string consumerSecret = ConfigurationManager.AppSettings
                    ["twitterConsumerSecret"];
                    if (!string.IsNullOrEmpty(consumerKey))
                    {
                        tokenManager = new InMemoryTokenManager(consumerKey,
                        consumerSecret);
                        HttpContext.Application["TwitterTokenManager"] =
                        tokenManager;
                    }
                }
                return tokenManager;
            }
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
