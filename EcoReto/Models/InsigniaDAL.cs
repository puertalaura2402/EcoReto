using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EcoReto.Models
{
    public class InsigniaDAL
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // ============================================
        // Obtener todas las insignias con progreso del usuario
        // ============================================
        public List<Insignia> ObtenerInsigniasConProgreso(int idUsuario)
        {
            var insignias = new List<Insignia>();

            // Primero obtener todas las insignias (sin progreso)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        I.IdInsignia,
                        I.Nombre,
                        I.Descripcion,
                        I.ImagenPath,
                        I.IdCategoria,
                        C.NombreCategoria,
                        I.TipoRequisito,
                        I.CantidadRequisito,
                        CASE WHEN UI.IdInsignia IS NOT NULL THEN 1 ELSE 0 END AS EstaDesbloqueada
                    FROM Insignias I
                    LEFT JOIN Categorias C ON I.IdCategoria = C.IdCategoria
                    LEFT JOIN UsuarioInsignias UI ON I.IdInsignia = UI.IdInsignia AND UI.IdUsuario = @IdUsuario
                    ORDER BY I.IdCategoria, I.CantidadRequisito";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var insignia = new Insignia
                            {
                                IdInsignia = Convert.ToInt32(dr["IdInsignia"]),
                                Nombre = dr["Nombre"].ToString(),
                                Descripcion = dr["Descripcion"].ToString(),
                                ImagenPath = dr["ImagenPath"].ToString(),
                                IdCategoria = dr["IdCategoria"] != DBNull.Value ? (int?)Convert.ToInt32(dr["IdCategoria"]) : null,
                                NombreCategoria = dr["NombreCategoria"] != DBNull.Value ? dr["NombreCategoria"].ToString() : "Generales",
                                TipoRequisito = dr["TipoRequisito"].ToString(),
                                CantidadRequisito = Convert.ToInt32(dr["CantidadRequisito"]),
                                EstaDesbloqueada = Convert.ToInt32(dr["EstaDesbloqueada"]) == 1
                            };
                            insignias.Add(insignia);
                        }
                    }
                }
            }

            // Ahora calcular el progreso para cada insignia (con conexiones separadas)
            foreach (var insignia in insignias)
            {
                CalcularProgreso(insignia, idUsuario);
            }

            return insignias;
        }

        // ============================================
        // Calcular progreso de una insignia
        // ============================================
        private void CalcularProgreso(Insignia insignia, int idUsuario)
        {
            insignia.ProgresoTotal = insignia.CantidadRequisito;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                switch (insignia.TipoRequisito)
                {
                    case "MisionesPorCategoria":
                        if (insignia.IdCategoria.HasValue)
                        {
                            using (SqlCommand cmd = new SqlCommand("sp_ObtenerProgresoPorCategoria", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                                cmd.Parameters.AddWithValue("@IdCategoria", insignia.IdCategoria.Value);
                                var result = cmd.ExecuteScalar();
                                insignia.ProgresoActual = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                            }
                        }
                        break;

                    case "MisionesTotales":
                        using (SqlCommand cmd = new SqlCommand("sp_ObtenerTotalMisionesCompletadas", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                            var result = cmd.ExecuteScalar();
                            insignia.ProgresoActual = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                        break;

                }
            }

            // Calcular porcentaje
            if (insignia.ProgresoTotal > 0)
            {
                insignia.PorcentajeProgreso = Math.Min(100, (double)insignia.ProgresoActual / insignia.ProgresoTotal * 100);
            }
            else
            {
                insignia.PorcentajeProgreso = 0;
            }

            // Limitar progreso al total requerido
            if (insignia.ProgresoActual > insignia.ProgresoTotal)
            {
                insignia.ProgresoActual = insignia.ProgresoTotal;
            }
        }

        // ============================================
        // Desbloquear insignia
        // ============================================
        public void DesbloquearInsignia(int idUsuario, int idInsignia)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_DesbloquearInsignia", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@IdInsignia", idInsignia);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ============================================
        // Verificar y desbloquear insignias automáticamente
        // ============================================
        public void VerificarYDesbloquearInsignias(int idUsuario)
        {
            var insignias = ObtenerInsigniasConProgreso(idUsuario);

            foreach (var insignia in insignias)
            {
                // Solo desbloquear si cumple el requisito y aún no está desbloqueada
                if (!insignia.EstaDesbloqueada && insignia.ProgresoActual >= insignia.ProgresoTotal)
                {
                    DesbloquearInsignia(idUsuario, insignia.IdInsignia);
                }
            }
        }

        // ============================================
        // Obtener insignias por categoría (para agrupar en la vista)
        // ============================================
        public Dictionary<string, List<Insignia>> ObtenerInsigniasAgrupadasPorCategoria(int idUsuario)
        {
            var insignias = ObtenerInsigniasConProgreso(idUsuario);
            var agrupadas = new Dictionary<string, List<Insignia>>();

            foreach (var insignia in insignias)
            {
                string categoria = string.IsNullOrEmpty(insignia.NombreCategoria) ? "Generales" : insignia.NombreCategoria;

                if (!agrupadas.ContainsKey(categoria))
                {
                    agrupadas[categoria] = new List<Insignia>();
                }

                agrupadas[categoria].Add(insignia);
            }

            return agrupadas;
        }
    }
}

