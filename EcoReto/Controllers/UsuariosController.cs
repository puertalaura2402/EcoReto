using EcoReto.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class UsuariosController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // ===== LISTAR USUARIOS =====
        public ActionResult Index()
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT u.IdUsuario, u.Usuario, u.Email, u.Rol,
                           COUNT(um.IdMision) AS MisionesCompletadas
                    FROM Usuarios u
                    LEFT JOIN UsuarioMisiones um ON u.IdUsuario = um.IdUsuario
                    GROUP BY u.IdUsuario, u.Usuario, u.Email, u.Rol
                    ORDER BY u.Usuario", con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    usuarios.Add(new Usuario
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        UsuarioNombre = dr["Usuario"].ToString(),
                        Email = dr["Email"].ToString(),
                        Rol = dr["Rol"].ToString()
                    });
                }
            }

            // ✨ Obtener insignias desbloqueadas para cada usuario
            InsigniaDAL insigniaDAL = new InsigniaDAL();
            Dictionary<int, List<Insignia>> insigniasPorUsuario = new Dictionary<int, List<Insignia>>();

            foreach (var usuario in usuarios)
            {
                var insigniasDesbloqueadas = insigniaDAL.ObtenerInsigniasConProgreso(usuario.IdUsuario)
                    .Where(i => i.EstaDesbloqueada)
                    .ToList();
                insigniasPorUsuario[usuario.IdUsuario] = insigniasDesbloqueadas;
            }

            ViewBag.InsigniasPorUsuario = insigniasPorUsuario;

            return View(usuarios);
        }

        // ===== VER ACTIVIDAD (MISIONES COMPLETADAS) =====
        public ActionResult Actividad(int id)
        {
            Usuario usuario = new Usuario();
            usuario.MisionesCompletadas = new List<Mision>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Obtener información del usuario
                SqlCommand cmdUsuario = new SqlCommand("SELECT * FROM Usuarios WHERE IdUsuario = @IdUsuario", con);
                cmdUsuario.Parameters.AddWithValue("@IdUsuario", id);
                SqlDataReader dr = cmdUsuario.ExecuteReader();
                if (dr.Read())
                {
                    usuario.IdUsuario = (int)dr["IdUsuario"];
                    usuario.UsuarioNombre = dr["Usuario"].ToString();
                    usuario.Email = dr["Email"].ToString();
                    usuario.Rol = dr["Rol"].ToString();
                }
                dr.Close();

                // Obtener misiones completadas
                SqlCommand cmdMisiones = new SqlCommand(@"
            SELECT m.Titulo, m.Descripcion, m.Puntos, m.IdCategoria
            FROM UsuarioMisiones um
            INNER JOIN Misiones m ON um.IdMision = m.IdMision
            WHERE um.IdUsuario = @IdUsuario", con);

                cmdMisiones.Parameters.AddWithValue("@IdUsuario", id);
                SqlDataReader drM = cmdMisiones.ExecuteReader();

                int totalPuntos = 0;

                while (drM.Read())
                {
                    int puntos = Convert.ToInt32(drM["Puntos"]);
                    totalPuntos += puntos;

                    usuario.MisionesCompletadas.Add(new Mision
                    {
                        Titulo = drM["Titulo"].ToString(),
                        Descripcion = drM["Descripcion"].ToString(),
                        CategoriaNombre = drM["IdCategoria"].ToString(),
                        Puntos = puntos
                    });
                }
                drM.Close();

                // ASIGNAR PUNTOS AL USUARIO
                usuario.PuntosTotales = totalPuntos;
            }

            return View(usuario);
        }



        // ===== ELIMINAR USUARIO =====
        public ActionResult Eliminar(int id)
        {
            Usuario u = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdUsuario, Usuario FROM Usuarios WHERE IdUsuario = @IdUsuario", con);
                cmd.Parameters.AddWithValue("@IdUsuario", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    u = new Usuario
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        UsuarioNombre = dr["Usuario"].ToString()
                    };
                }
            }
            return View(u);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfirmado(Usuario u)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario", con);
                cmd.Parameters.AddWithValue("@IdUsuario", u.IdUsuario);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["MensajeExito"] = "🗑️ Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}