using EcoReto.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace EcoReto.Controllers
{
    public class MisionesController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // ===== LISTAR MISIONES (con filtro por categoría) =====
        public ActionResult Index(int? idCategoria)
        {
            List<Mision> misiones = new List<Mision>();
            List<Categoria> categorias = new List<Categoria>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Obtener categorías
                SqlCommand cmdCat = new SqlCommand("SELECT IdCategoria, NombreCategoria FROM Categorias", con);
                SqlDataReader drCat = cmdCat.ExecuteReader();
                while (drCat.Read())
                {
                    categorias.Add(new Categoria
                    {
                        IdCategoria = Convert.ToInt32(drCat["IdCategoria"]),
                        NombreCategoria = drCat["NombreCategoria"].ToString()
                    });
                }
                drCat.Close();

                // Obtener misiones (con filtro si hay categoría)
                string query = "SELECT M.IdMision, M.Titulo, M.Descripcion, M.Puntos, C.NombreCategoria " +
                               "FROM Misiones M INNER JOIN Categorias C ON M.IdCategoria = C.IdCategoria";

                if (idCategoria.HasValue)
                    query += " WHERE M.IdCategoria = @IdCategoria";

                SqlCommand cmd = new SqlCommand(query, con);
                if (idCategoria.HasValue)
                    cmd.Parameters.AddWithValue("@IdCategoria", idCategoria.Value);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    misiones.Add(new Mision
                    {
                        IdMision = Convert.ToInt32(dr["IdMision"]),
                        Titulo = dr["Titulo"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        Puntos = Convert.ToInt32(dr["Puntos"]),
                        CategoriaNombre = dr["NombreCategoria"].ToString()
                    });
                }
            }

            ViewBag.Categorias = new SelectList(categorias, "IdCategoria", "NombreCategoria");
            return View(misiones);
        }

        // ===== CREAR MISION =====
        public ActionResult Crear()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdCategoria, NombreCategoria FROM Categorias", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                List<Categoria> categorias = new List<Categoria>();
                while (dr.Read())
                {
                    categorias.Add(new Categoria
                    {
                        IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                        NombreCategoria = dr["NombreCategoria"].ToString()
                    });
                }
                ViewBag.Categorias = new SelectList(categorias, "IdCategoria", "NombreCategoria");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Crear(Mision m)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Misiones (Titulo, Descripcion, Puntos, IdCategoria) VALUES (@Titulo, @Descripcion, @Puntos, @IdCategoria)", con);

                    cmd.Parameters.AddWithValue("@Titulo", m.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", m.Descripcion);
                    cmd.Parameters.AddWithValue("@Puntos", m.Puntos);
                    cmd.Parameters.AddWithValue("@IdCategoria", m.IdCategoria);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["MensajeExito"] = "✅ Misión creada correctamente.";

                // ✨ Crear notificación para todos los usuarios sobre la nueva misión
                NotificacionDAL notificacionDAL = new NotificacionDAL();
                using (SqlConnection con2 = new SqlConnection(connectionString))
                {
                    con2.Open();
                    SqlCommand cmdUsuarios = new SqlCommand("SELECT IdUsuario FROM Usuarios WHERE Rol = 'Usuario'", con2);
                    SqlDataReader drUsuarios = cmdUsuarios.ExecuteReader();
                    while (drUsuarios.Read())
                    {
                        int idUsuario = Convert.ToInt32(drUsuarios["IdUsuario"]);
                        notificacionDAL.CrearNotificacion(
                            idUsuario,
                            "NuevaMision",
                            "¡Hay una nueva misión disponible!",
                            "Una nueva misión ha sido agregada: " + m.Titulo,
                            null
                        );
                    }
                    drUsuarios.Close();
                }

                return RedirectToAction("Index");
            }
            return View(m);
        }

        // ===== EDITAR MISION =====
        public ActionResult Editar(int id)
        {
            Mision m = new Mision();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Misiones WHERE IdMision = @IdMision", con);
                cmd.Parameters.AddWithValue("@IdMision", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    m.IdMision = id;
                    m.Titulo = dr["Titulo"].ToString();
                    m.Descripcion = dr["Descripcion"].ToString();
                    m.Puntos = Convert.ToInt32(dr["Puntos"]);
                    m.IdCategoria = Convert.ToInt32(dr["IdCategoria"]);
                }
            }

            // Categorías para el combo
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdCategoria, NombreCategoria FROM Categorias", con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                List<Categoria> categorias = new List<Categoria>();
                while (dr.Read())
                {
                    categorias.Add(new Categoria
                    {
                        IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                        NombreCategoria = dr["NombreCategoria"].ToString()
                    });
                }
                ViewBag.Categorias = new SelectList(categorias, "IdCategoria", "NombreCategoria", m.IdCategoria);
            }

            return View(m);
        }

        [HttpPost]
        public ActionResult Editar(Mision m)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Misiones SET Titulo = @Titulo, Descripcion = @Descripcion, Puntos = @Puntos, IdCategoria = @IdCategoria WHERE IdMision = @IdMision", con);
                cmd.Parameters.AddWithValue("@IdMision", m.IdMision);
                cmd.Parameters.AddWithValue("@Titulo", m.Titulo);
                cmd.Parameters.AddWithValue("@Descripcion", m.Descripcion);
                cmd.Parameters.AddWithValue("@Puntos", m.Puntos);
                cmd.Parameters.AddWithValue("@IdCategoria", m.IdCategoria);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            TempData["MensajeExito"] = "✅ Misión actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // ===== ELIMINAR MISION =====
        // GET: Misiones/Eliminar/5
        public ActionResult Eliminar(int id)
        {
            Mision m = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdMision, Titulo FROM Misiones WHERE IdMision = @IdMision", con);
                cmd.Parameters.AddWithValue("@IdMision", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    m = new Mision
                    {
                        IdMision = Convert.ToInt32(dr["IdMision"]),
                        Titulo = dr["Titulo"].ToString()
                    };
                }
            }

            if (m == null)
            {
                TempData["MensajeError"] = "No se encontró la misión seleccionada.";
                return RedirectToAction("Index");
            }

            return View(m);
        }


        // POST: Misiones/EliminarConfirmado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfirmado(Mision m)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Misiones WHERE IdMision = @IdMision", con);
                    cmd.Parameters.AddWithValue("@IdMision", m.IdMision);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["MensajeExito"] = "🗑️ Misión eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Ocurrió un error al eliminar la misión: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}
