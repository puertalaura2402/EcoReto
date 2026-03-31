using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Usuario"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
