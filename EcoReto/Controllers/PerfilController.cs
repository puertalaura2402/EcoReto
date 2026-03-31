using EcoReto.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class PerfilController : Controller
    {
        PerfilUsuarioDAL dal = new PerfilUsuarioDAL();

        public ActionResult Index(int? idCategoria)
        {
            // Validar sesión
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            // Cargar categorías para el filtro
            ViewBag.Categorias = dal.ObtenerCategorias();

            // Misiones disponibles filtradas o todas
            var misionesDisponibles = dal.ObtenerMisionesDisponibles(idCategoria, idUsuario);

            // Misiones completadas y puntaje
            var misionesCompletadas = dal.ObtenerMisionesCompletadas(idUsuario);
            var puntajeTotal = dal.ObtenerPuntajeTotal(idUsuario);

            // ✨ Cargar insignias del usuario
            InsigniaDAL insigniaDAL = new InsigniaDAL();
            insigniaDAL.VerificarYDesbloquearInsignias(idUsuario); // Verificar y desbloquear automáticamente
            var insigniasAgrupadas = insigniaDAL.ObtenerInsigniasAgrupadasPorCategoria(idUsuario);

            ViewBag.PuntajeTotal = puntajeTotal;
            ViewBag.MisionesCompletadas = misionesCompletadas;
            ViewBag.IdCategoriaSeleccionada = idCategoria;
            ViewBag.InsigniasAgrupadas = insigniasAgrupadas;

            return View(misionesDisponibles);
        }

        [HttpPost]
        public ActionResult CompletarMision(int id)
        {
            int idUsuario = (int)Session["IdUsuario"];
            dal.CompletarMision(idUsuario, id);

            // ✨ Verificar y desbloquear insignias automáticamente
            InsigniaDAL insigniaDAL = new InsigniaDAL();

            // Obtener insignias antes de desbloquear para detectar las nuevas
            var insigniasAntes = insigniaDAL.ObtenerInsigniasConProgreso(idUsuario);
            var idsDesbloqueadasAntes = insigniasAntes.Where(i => i.EstaDesbloqueada).Select(i => i.IdInsignia).ToList();

            // Desbloquear insignias
            insigniaDAL.VerificarYDesbloquearInsignias(idUsuario);

            // Obtener insignias después para detectar las nuevas
            var insigniasDespues = insigniaDAL.ObtenerInsigniasConProgreso(idUsuario);
            var nuevasInsignias = insigniasDespues.Where(i => i.EstaDesbloqueada && !idsDesbloqueadasAntes.Contains(i.IdInsignia)).ToList();

            // ✨ Crear notificaciones para nuevas insignias desbloqueadas
            if (nuevasInsignias != null && nuevasInsignias.Count > 0)
            {
                NotificacionDAL notificacionDAL = new NotificacionDAL();
                foreach (var insignia in nuevasInsignias)
                {
                    notificacionDAL.CrearNotificacion(
                        idUsuario,
                        "InsigniaDesbloqueada",
                        "¡Felicidades! Has desbloqueado una insignia",
                        "Has desbloqueado la insignia: " + insignia.Nombre,
                        insignia.IdInsignia
                    );
                }
                TempData["MensajeExito"] = "¡Misión completada con éxito! 🎉";
            }
            else
            {
                TempData["MensajeExito"] = "¡Misión completada con éxito!";
            }

            return RedirectToAction("Index");
        }

        public ActionResult Insignias()
        {
            return View();
        }
    }
}
