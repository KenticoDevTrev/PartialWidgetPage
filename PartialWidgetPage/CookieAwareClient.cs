using System;
using System.Linq;
using System.Net;
using System.Web;

namespace PartialWidgetPage
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer CookieContainer { get; set; }
        public Uri Uri { get; set; }
        public string RequestUserAgent { get; set; }

        private const string SessionIdCookie = "ASP.NET_SessionId";

        public CookieAwareWebClient() : this(new CookieContainer())
        {
        }

        /// <summary>
        /// Creates web client with cookies.
        /// </summary>
        /// <param name="cookies"></param>
        public CookieAwareWebClient(CookieContainer cookies)
        {
            CookieContainer = cookies;
        }

        /// <summary>
        /// Creates web client with the cookies of the given request.
        /// </summary>
        /// <param name="currentRequest">The current request to copy cookie from.</param>
        /// <param name="stripSession">Strips the ASP.NET_SessionId because by default sessions have a lock on them, so additional requests during the initial request will cause a timeout. Can override, however must make sure initial request has a read-only session attribute.</param>
        public CookieAwareWebClient(HttpRequest currentRequest, bool stripSession = true)
        {
            // copy cookies
            var cookieJar = new CookieContainer();

            // get list of cookies
            var cookieKeys = currentRequest.Cookies.AllKeys.ToList();
            if (stripSession)
                cookieKeys.RemoveAll(x => x.Equals(SessionIdCookie, StringComparison.InvariantCultureIgnoreCase));

            foreach (var cookieKey in cookieKeys)
            {
                if (!stripSession || !cookieKey.Equals(SessionIdCookie, StringComparison.InvariantCultureIgnoreCase))
                {
                    var cookie = currentRequest.Cookies[cookieKey];
                    if (cookie != null)
                    {
                        // CookieContainer requires a domain, so if it's empty, then use the current request's host/authority.
                        var domain = string.IsNullOrWhiteSpace(cookie.Domain) ? currentRequest.IsLocal ? currentRequest.Url.Authority : currentRequest.Url.Host : cookie.Domain;
                        if (!string.IsNullOrWhiteSpace(domain))
                            cookieJar.Add(new Cookie(cookieKey, cookie.Value, cookie.Path, domain));
                    }
                }
            }
            CookieContainer = cookieJar;
            RequestUserAgent = currentRequest.UserAgent;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            var httpRequest = (HttpWebRequest)request;
            if (httpRequest != null)
            {
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpRequest.CookieContainer = CookieContainer;
                httpRequest.UserAgent = RequestUserAgent;
            }
            return httpRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            var setCookieHeader = response?.Headers[HttpResponseHeader.SetCookie];

            // do something if needed to parse out the cookie
            if (setCookieHeader != null)
            {
                // get the cookies from the response 
                CookieContainer.SetCookies(response.ResponseUri, setCookieHeader);
            }

            return response;
        }
    }
}
