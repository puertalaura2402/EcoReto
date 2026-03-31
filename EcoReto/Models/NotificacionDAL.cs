using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace EcoReto.Models
{
    public class NotificacionDAL
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // ============================================
        // Crear notificación
        // ============================================
        public void CrearNotificacion(int idUsuario, string tipo, string titulo, string mensaje, int? idReferencia = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    INSERT INTO Notificaciones (IdUsuario, Tipo, Titulo, Mensaje, IdReferencia, Leida)
                    VALUES (@IdUsuario, @Tipo, @Titulo, @Mensaje, @IdReferencia, 0)
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@Tipo", tipo);
                cmd.Parameters.AddWithValue("@Titulo", titulo);
                cmd.Parameters.AddWithValue("@Mensaje", mensaje);
                cmd.Parameters.AddWithValue("@IdReferencia", idReferencia ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        // ============================================
        // Obtener notificaciones del usuario (no leídas primero)
        // ============================================
        public List<Notificacion> ObtenerNotificaciones(int idUsuario)
        {
            var notificaciones = new List<Notificacion>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT IdNotificacion, Tipo, Titulo, Mensaje, IdReferencia, Leida, FechaCreacion
                    FROM Notificaciones
                    WHERE IdUsuario = @IdUsuario
                    ORDER BY Leida ASC, FechaCreacion DESC
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                SqlDataReader dr = cmd.ExecuteReader();
                var idsInsignias = new List<int>();

                // Primero leer todas las notificaciones
                while (dr.Read())
                {
                    var notificacion = new Notificacion
                    {
                        IdNotificacion = Convert.ToInt32(dr["IdNotificacion"]),
                        IdUsuario = idUsuario,
                        Tipo = dr["Tipo"].ToString(),
                        Titulo = dr["Titulo"].ToString(),
                        Mensaje = dr["Mensaje"].ToString(),
                        IdReferencia = dr["IdReferencia"] != DBNull.Value ? (int?)Convert.ToInt32(dr["IdReferencia"]) : null,
                        Leida = Convert.ToBoolean(dr["Leida"]),
                        FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"])
                    };

                    // Agregar icono según el tipo
                    if (notificacion.Tipo == "InsigniaDesbloqueada")
                    {
                        notificacion.Icono = "🏅";
                        if (notificacion.IdReferencia.HasValue)
                        {
                            idsInsignias.Add(notificacion.IdReferencia.Value);
                        }
                    }
                    else if (notificacion.Tipo == "NuevaMision")
                    {
                        notificacion.Icono = "✨";
                    }

                    notificaciones.Add(notificacion);
                }
                dr.Close();

                // Obtener imágenes de insignias si hay
                if (idsInsignias.Count > 0)
                {
                    var imagenesInsignias = ObtenerImagenesInsignias(idsInsignias, conn);
                    foreach (var notificacion in notificaciones)
                    {
                        if (notificacion.Tipo == "InsigniaDesbloqueada" && notificacion.IdReferencia.HasValue)
                        {
                            if (imagenesInsignias.ContainsKey(notificacion.IdReferencia.Value))
                            {
                                notificacion.ImagenPath = imagenesInsignias[notificacion.IdReferencia.Value];
                            }
                        }
                    }
                }
            }

            return notificaciones;
        }

        // ============================================
        // Obtener imágenes de insignias
        // ============================================
        private Dictionary<int, string> ObtenerImagenesInsignias(List<int> idsInsignias, SqlConnection conn)
        {
            var imagenes = new Dictionary<int, string>();
            if (idsInsignias.Count == 0) return imagenes;

            string ids = string.Join(",", idsInsignias);
            string query = $"SELECT IdInsignia, ImagenPath FROM Insignias WHERE IdInsignia IN ({ids})";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                int id = Convert.ToInt32(dr["IdInsignia"]);
                string imagenPath = dr["ImagenPath"] != DBNull.Value ? dr["ImagenPath"].ToString() : null;
                imagenes[id] = imagenPath;
            }
            dr.Close();

            return imagenes;
        }

        // ============================================
        // Contar notificaciones no leídas
        // ============================================
        public int ContarNotificacionesNoLeidas(int idUsuario)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Notificaciones WHERE IdUsuario = @IdUsuario AND Leida = 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ============================================
        // Marcar notificación como leída
        // ============================================
        public void MarcarComoLeida(int idNotificacion)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Notificaciones SET Leida = 1 WHERE IdNotificacion = @IdNotificacion";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdNotificacion", idNotificacion);
                cmd.ExecuteNonQuery();
            }
        }

        // ============================================
        // Marcar todas las notificaciones como leídas
        // ============================================
        public void MarcarTodasComoLeidas(int idUsuario)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Notificaciones SET Leida = 1 WHERE IdUsuario = @IdUsuario AND Leida = 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }
    }
}