using System.Web.Mvc;

namespace PalletManagementSystem.Web.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all controllers
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Sets a success message to be displayed to the user via TempData
        /// </summary>
        /// <param name="message">The success message</param>
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Sets an error message to be displayed to the user via TempData
        /// </summary>
        /// <param name="message">The error message</param>
        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Sets an information message to be displayed to the user via TempData
        /// </summary>
        /// <param name="message">The information message</param>
        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        /// <summary>
        /// Sets a warning message to be displayed to the user via TempData
        /// </summary>
        /// <param name="message">The warning message</param>
        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        /// <summary>
        /// Prepares common view data for all views
        /// </summary>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Set common ViewBag properties
            ViewBag.Username = User?.Identity?.Name ?? "Unknown User";

            // Get division and platform from session
            ViewBag.Division = Session["CurrentDivision"] ?? "MA";
            ViewBag.Platform = Session["CurrentPlatform"] ?? "TEC1";

            // Get touch mode status
            ViewBag.TouchModeEnabled = Session["TouchModeEnabled"] != null && (bool)Session["TouchModeEnabled"];
        }

        /// <summary>
        /// Global error handling for controllers
        /// </summary>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            // Log the exception - in a real implementation this would use a proper logger
            System.Diagnostics.Debug.WriteLine($"Exception: {filterContext.Exception.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {filterContext.Exception.StackTrace}");

            // Set error message
            SetErrorMessage("An error occurred. Please try again or contact support if the problem persists.");

            // Return JSON response for AJAX requests
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = "An error occurred while processing your request." },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                // Redirect to Error page
                filterContext.Result = new RedirectResult("~/Home/Error");
            }

            filterContext.ExceptionHandled = true;
        }
    }
}