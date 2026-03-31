using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class AdminController : Controller
    {
        // Seguridad: Solo el rol Administrador puede acceder
        private bool EsAdministrador()
        {
            return Session["Rol"] != null && Session["Rol"].ToString() == "Administrador";
        }

        public ActionResult PanelAdmin()
        {
            if (!EsAdministrador())
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
