using EcoReto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class NotificacionesController : Controller
    {
        NotificacionDAL dal = new NotificacionDAL();

        // Obtener notificaciones (JSON para AJAX)
        public ActionResult ObtenerNotificaciones()
        {
            if (Session["IdUsuario"] == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            var notificaciones = dal.ObtenerNotificaciones(idUsuario);
            int noLeidas = dal.ContarNotificacionesNoLeidas(idUsuario);

            return Json(new
            {
                success = true,
                notificaciones = notificaciones,
                noLeidas = noLeidas
            }, JsonRequestBehavior.AllowGet);
        }

        // Vista completa de notificaciones
        public ActionResult Index()
        {
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            var notificaciones = dal.ObtenerNotificaciones(idUsuario);
            ViewBag.NoLeidas = dal.ContarNotificacionesNoLeidas(idUsuario);

            return View(notificaciones);
        }

        // Marcar como leída
        [HttpPost]
        public ActionResult MarcarLeida(int id)
        {
            if (Session["IdUsuario"] == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            dal.MarcarComoLeida(id);
            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            int noLeidas = dal.ContarNotificacionesNoLeidas(idUsuario);

            return Json(new { success = true, noLeidas = noLeidas }, JsonRequestBehavior.AllowGet);
        }

        // Marcar todas como leídas
        [HttpPost]
        public ActionResult MarcarTodasLeidas()
        {
            if (Session["IdUsuario"] == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            dal.MarcarTodasComoLeidas(idUsuario);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        // Obtener contador de no leídas
        public ActionResult Contador()
        {
            if (Session["IdUsuario"] == null)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            int noLeidas = dal.ContarNotificacionesNoLeidas(idUsuario);

            return Json(new { count = noLeidas }, JsonRequestBehavior.AllowGet);
        }
    }
}
