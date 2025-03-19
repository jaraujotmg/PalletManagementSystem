using System.Linq;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.Controllers
{
    /// <summary>
    /// Base controller for common functionality
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Sets a success message to be displayed to the user
        /// </summary>
        /// <param name="message">The success message</param>
        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        /// <summary>
        /// Sets an error message to be displayed to the user
        /// </summary>
        /// <param name="message">The error message</param>
        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Sets an information message to be displayed to the user
        /// </summary>
        /// <param name="message">The information message</param>
        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        /// <summary>
        /// Sets a warning message to be displayed to the user
        /// </summary>
        /// <param name="message">The warning message</param>
        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        /// <summary>
        /// Handles controller-level exceptions
        /// </summary>
        /// <param name="filterContext">The exception context</param>
        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            // Log the exception
            // In a real application, we would inject a logger and log the exception here

            // Set error message
            SetErrorMessage("An error occurred while processing your request. Please try again.");

            // Determine if we should return a JSON response
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
                // Redirect to error page
                filterContext.Result = new RedirectResult("~/Error");
            }

            filterContext.ExceptionHandled = true;
        }

        /// <summary>
        /// Checks if the model state is valid and returns a JSON response if not
        /// </summary>
        /// <returns>True if valid, false if errors were found and response sent</returns>
        protected bool ValidateModelStateForAjaxRequest()
        {
            if (ModelState.IsValid)
            {
                return true;
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            Response.StatusCode = 400;
            Response.TrySkipIisCustomErrors = true;

            var result = new JsonResult
            {
                Data = new { success = false, errors = errors },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            Response.Write(result);
            Response.End();

            return false;
        }
    }
}