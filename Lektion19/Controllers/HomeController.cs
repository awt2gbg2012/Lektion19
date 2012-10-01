using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.ApplicationBlock;
using System.Configuration;
using DotNetOpenAuth.OAuth;
using System.Xml.XPath;
using System.Text;
using Newtonsoft.Json.Linq;

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
            var twitter = new WebConsumer(TwitterConsumer.ServiceDescription,
                                            this.TokenManager);
            // Is Twitter calling back with authorization?
            var accessTokenResponse = twitter.ProcessUserAuthorization();
            if (accessTokenResponse != null)
            {
                this.AccessToken = accessTokenResponse.AccessToken;
            }
            else if (this.AccessToken == null)
            {
                // If we don't yet have access, immediately request it.
                twitter.Channel.Send(twitter.PrepareRequestUserAuthorization());
            }

            return View();
        }

        public string GetTweets()
        {
            // Uppgift 6:
            var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, this.TokenManager);
            XPathDocument updates = new XPathDocument(TwitterConsumer.GetUpdates(twitter, this.AccessToken).CreateReader());
            XPathNavigator nav = updates.CreateNavigator();
            var parsedUpdates = from status in nav.Select("/statuses/status").OfType<XPathNavigator>()
                                where !status.SelectSingleNode("user/protected").ValueAsBoolean
                                select new
                                {
                                    User = status.SelectSingleNode("user/name").InnerXml,
                                    Status = status.SelectSingleNode("text").InnerXml,
                                };

            StringBuilder tableBuilder = new StringBuilder();
            tableBuilder.Append("<table><tr><td>Name</td><td>Update</td></tr>");

            foreach (var update in parsedUpdates)
            {
                tableBuilder.AppendFormat(
                    "<tr><td>{0}</td><td>{1}</td></tr>",
                    HttpUtility.HtmlEncode(update.User),
                    HttpUtility.HtmlEncode(update.Status));
            }
            tableBuilder.Append("</table>");
            return tableBuilder.ToString();
        }

        public string Tweet(string newstatus)
        {
            // Skapa tweets
            var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, this.TokenManager);
            string resultString;
            try
            {
                string updates = TwitterConsumer.Tweet(twitter,this.AccessToken,newstatus);
                JToken token = JObject.Parse(updates);
                resultString = string.Format("{0} - {1}",(string)token.SelectToken("user").SelectToken("screen_name"),(string)token.SelectToken("text"));
            }
            catch
            {
                resultString = "Error! Could not send new tweet.";
            }
            return resultString;
        }
    }
}
