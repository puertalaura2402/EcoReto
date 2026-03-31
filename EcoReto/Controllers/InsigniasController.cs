using EcoReto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class InsigniasController : Controller
    {

        InsigniaDAL dal = new InsigniaDAL();

        public ActionResult Index()
        {
            // Validar sesión
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            // Verificar y desbloquear insignias automáticamente
            dal.VerificarYDesbloquearInsignias(idUsuario);

            // Obtener insignias agrupadas por categoría
            var insigniasAgrupadas = dal.ObtenerInsigniasAgrupadasPorCategoria(idUsuario);

            return View(insigniasAgrupadas);
        }
    }
    
}
