using System.Web.Mvc;
using System.Web.Mvc.Filters;
using PalletManagementSystem.Infrastructure.Identity;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces;

namespace PalletManagementSystem.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Add global error handling filter
            filters.Add(new HandleErrorAttribute());

            // Add authentication filter
            filters.Add(new AuthenticationFilter());

            // Add authorization filter
            filters.Add(new AuthorizationFilter());
        }

        /// <summary>
        /// Custom authentication filter to ensure Windows authentication
        /// </summary>
        public class AuthenticationFilter : IAuthenticationFilter
        {
            public void OnAuthentication(AuthenticationContext filterContext)
            {
                // Ensure user is authenticated via Windows Authentication
                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
            }

            public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
            {
                // Optional additional handling
            }
        }

        /// <summary>
        /// Custom authorization filter to check user roles and permissions
        /// </summary>
        public class AuthorizationFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationContext filterContext)
            {
                // Skip authorization for public/anonymous actions
                if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).Length > 0)
                {
                    return;
                }

                // Ensure user is in a valid role
                var userContext = new UserContext(
                    filterContext.HttpContext.GetService<IUserContextProvider>(),
                    filterContext.HttpContext.GetService<WindowsAuthenticationService>(),
                    filterContext.HttpContext.GetService<ILogger<UserContext>>()
                );

                var userRoles = userContext.GetRolesAsync().Result;

                // Check if user has appropriate roles
                bool isAuthorized = false;
                foreach (var role in userRoles)
                {
                    if (role == "Administrator" || role == "Editor" || role == "Viewer")
                    {
                        isAuthorized = true;
                        break;
                    }
                }

                if (!isAuthorized)
                {
                    filterContext.Result = new HttpForbiddenResult();
                }
            }
        }

        /// <summary>
        /// Custom forbidden result for unauthorized access
        /// </summary>
        public class HttpForbiddenResult : HttpStatusCodeResult
        {
            public HttpForbiddenResult() : base(403, "Forbidden")
            {
            }
        }
    }
}