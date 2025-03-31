// src/PalletManagementSystem.Web/Controllers/BaseController.cs
using System.Threading.Tasks;
using System.Web.Mvc;
using PalletManagementSystem.Infrastructure.Identity;

namespace PalletManagementSystem.Web2.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUserContext UserContext;

        protected BaseController(IUserContext userContext)
        {
            UserContext = userContext ?? throw new System.ArgumentNullException(nameof(userContext));
        }

        protected string Username => UserContext.GetUsername();

        protected async Task<string> GetDisplayName()
        {
            return await UserContext.GetDisplayNameAsync();
        }
    }
}