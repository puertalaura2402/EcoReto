using EcoReto.Models;
using System;
using System.Web;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class AccountController : Controller
    {
        UsuarioDAL dal = new UsuarioDAL();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(Usuario user)
        {
            var usuarioValido = dal.VerificarLogin(user.UsuarioNombre, user.Contraseña);

            if (usuarioValido != null)
            {
                // Validar que el rol coincida con el que está en la base de datos
                if (usuarioValido.Rol.Equals(user.Rol, StringComparison.OrdinalIgnoreCase))
                {
                    // ✅ Guardar datos de sesión
                    Session["IdUsuario"] = usuarioValido.IdUsuario;
                    Session["Usuario"] = usuarioValido.UsuarioNombre;
                    Session["Rol"] = usuarioValido.Rol;

                    // Redirigir según el rol
                    if (usuarioValido.Rol == "Administrador")
                        return RedirectToAction("PanelAdmin", "Admin");
                    else
                        return RedirectToAction("Index", "Perfil"); // ✅ Redirige al perfil
                }
                else
                {
                    ViewBag.Error = "El rol seleccionado no coincide con el usuario.";
                    return View();
                }
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        // GET: Registro
        public ActionResult Register()
        {
            return View();
        }

        // POST: Registro
        [HttpPost]
        public ActionResult Register(Usuario user)
        {
            try
            {
                // Verificar si el usuario ya existe
                if (dal.UsuarioExistente(user.UsuarioNombre))
                {
                    ViewBag.Error = "El nombre de usuario ya existe.";
                    return View();
                }

                // Asignar valores por defecto
                user.Rol = "Usuario";
                user.FechaRegistro = DateTime.Now;

                // Insertar usuario en la base de datos
                dal.InsertarUsuario(user);

                // ✅ Mensaje de éxito
                TempData["MensajeExito"] = "✅ Cuenta creada correctamente.";

                // Redirigir al inicio de sesión
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al registrar el usuario: " + ex.Message;
                return View();
            }
        }

        // Cerrar sesión
        public ActionResult Logout()
        {
            // Elimina todas las variables de sesión del usuario
            Session.Clear();
            Session.Abandon();

            // Opcional: elimina las cookies de autenticación si las estás usando
            if (Request.Cookies[".ASPXAUTH"] != null)
            {
                var cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            return View("LogOut");

        }
    }
}
