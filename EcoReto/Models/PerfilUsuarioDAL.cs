using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;


namespace EcoReto.Models
{
    public class PerfilUsuarioDAL
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // ================================
        // 1. Obtener todas las CATEGORÍAS
        // ================================
        public List<Categoria> ObtenerCategorias()
        {
            var lista = new List<Categoria>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT IdCategoria, NombreCategoria FROM Categorias";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Categoria
                    {
                        IdCategoria = Convert.ToInt32(dr["IdCategoria"]),
                        NombreCategoria = dr["NombreCategoria"].ToString()
                    });
                }
            }
            return lista;
        }

        // =======================================================
        // 2. Obtener misiones DISPONIBLES filtradas por categoría
        // =======================================================
        public List<Mision> ObtenerMisionesDisponibles(int? idCategoria, int idUsuario)
        {
            var lista = new List<Mision>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                        SELECT M.IdMision, M.Titulo, M.Descripcion, M.Puntos, C.NombreCategoria AS CategoriaNombre
                        FROM Misiones M
                        INNER JOIN Categorias C ON M.IdCategoria = C.IdCategoria
                        WHERE 
                            (@idCategoria IS NULL OR M.IdCategoria = @idCategoria)
                            AND M.IdMision NOT IN (SELECT IdMision FROM UsuarioMisiones WHERE IdUsuario = @idUsuario)
                       ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idCategoria", (object)idCategoria ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Mision
                    {
                        IdMision = Convert.ToInt32(dr["IdMision"]),
                        Titulo = dr["Titulo"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        Puntos = Convert.ToInt32(dr["Puntos"]),
                        CategoriaNombre = dr["CategoriaNombre"].ToString()
                    });
                }
            }

            return lista;
        }



        // ===========================================
        // 3. Obtener MISIONES COMPLETADAS por usuario
        // ===========================================
        public List<Mision> ObtenerMisionesCompletadas(int idUsuario)
        {
            var lista = new List<Mision>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT M.Titulo, M.Descripcion, M.Puntos, C.NombreCategoria AS CategoriaNombre
                    FROM UsuarioMisiones UM
                    INNER JOIN Misiones M ON UM.IdMision = M.IdMision
                    INNER JOIN Categorias C ON M.IdCategoria = C.IdCategoria
                    WHERE UM.IdUsuario = @idUsuario
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Mision
                    {
                        Titulo = dr["Titulo"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        Puntos = Convert.ToInt32(dr["Puntos"]),
                        CategoriaNombre = dr["CategoriaNombre"].ToString()
                    });
                }
            }

            return lista;
        }


        // ===========================================
        // 4. Obtener PUNTAJE TOTAL del usuario
        // ===========================================
        public int ObtenerPuntajeTotal(int idUsuario)
        {
            int total = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT SUM(M.Puntos) 
                    FROM UsuarioMisiones UM
                    INNER JOIN Misiones M ON UM.IdMision = M.IdMision
                    WHERE UM.IdUsuario = @idUsuario
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                    total = Convert.ToInt32(result);
            }

            return total;
        }


        // ===========================================
        // 5. COMPLETAR MISIÓN (insert en UsuarioMision)
        // ===========================================
        public void CompletarMision(int idUsuario, int idMision)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    IF NOT EXISTS (SELECT 1 FROM UsuarioMisiones WHERE IdUsuario = @idUsuario AND IdMision = @idMision)
                    BEGIN
                        INSERT INTO UsuarioMisiones (IdUsuario, IdMision)
                        VALUES (@idUsuario, @idMision)
                    END
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@idMision", idMision);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
