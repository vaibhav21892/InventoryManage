using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace InvetoryManage.API
{
    public static class OwinExtensions
    {
        public static IApplicationBuilder UseOwinApp(
            this IApplicationBuilder aspNetCoreApp,
            Action<IAppBuilder> configuration)
        {
            return aspNetCoreApp.UseOwin(setup => setup(next =>
            {
                AppBuilder owinAppBuilder = new AppBuilder();

                IApplicationLifetime aspNetCoreLifetime =
                        (IApplicationLifetime)aspNetCoreApp.ApplicationServices.GetService
                        (typeof(IApplicationLifetime));

                AppProperties owinAppProperties = new AppProperties(owinAppBuilder.Properties);

                owinAppProperties.OnAppDisposing =
                       aspNetCoreLifetime?.ApplicationStopping ?? CancellationToken.None;

                owinAppProperties.DefaultApp = next;
                owinAppBuilder.Properties["host.AppName"] = "InventoryManage";
                configuration(owinAppBuilder);

                owinAppBuilder.UseCors(CorsOptions.AllowAll);
                OAuthAuthorizationServerOptions option = new OAuthAuthorizationServerOptions
                {
                    AuthorizeEndpointPath = new PathString("/OAuth/Authorize"),
                    TokenEndpointPath = new PathString("/OAuth/Token"),
                    ApplicationCanDisplayErrors = true,
                    //TokenEndpointPath = new PathString("/token"),
                    Provider = new ApplicationAuthProvider(),
                    AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                    AllowInsecureHttp = true
                };
                owinAppBuilder.UseOAuthAuthorizationServer(option);
                owinAppBuilder.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

                return owinAppBuilder.Build<Func<IDictionary<string, object>, Task>>();
            }));
        }
    }
}