using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Configuration;

namespace ead_frontend.App_Start
{
    public static class CentralAuthentication
    {
        public const string ApplicationCookie = "CentralAuthenticationType";
    }

    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CentralAuthentication.ApplicationCookie,
                CookieDomain = ConfigurationManager.AppSettings["CookieDomain"],
                LoginPath = new PathString("/Login"),
                Provider = new CookieAuthenticationProvider(),
                CookieName = "CentralAuthenticationCookie",
                CookieHttpOnly = true,
                ExpireTimeSpan = TimeSpan.FromHours(5)
            });

        }
    }
}