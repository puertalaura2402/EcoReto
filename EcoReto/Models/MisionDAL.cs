using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace EcoReto.Models
{
    public class MisionDAL
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // Insertar misión
        public void InsertarMision(Mision mision)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertarMision", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Titulo", mision.Titulo);
                cmd.Parameters.AddWithValue("@Descripcion", mision.Descripcion);
                cmd.Parameters.AddWithValue("@Puntos", mision.Puntos);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Listar misiones
        public List<Mision> ListarMisiones()
        {
            List<Mision> lista = new List<Mision>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ListarMisiones", con);
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new Mision
                    {
                        IdMision = Convert.ToInt32(dr["IdMision"]),
                        Titulo = dr["Titulo"].ToString(),
                        Descripcion = dr["Descripcion"].ToString(),
                        Puntos = Convert.ToInt32(dr["Puntos"])
                    });
                }
            }
            return lista;
        }

        // Actualizar misión
        public void ActualizarMision(Mision mision)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ActualizarMision", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdMision", mision.IdMision);
                cmd.Parameters.AddWithValue("@Titulo", mision.Titulo);
                cmd.Parameters.AddWithValue("@Descripcion", mision.Descripcion);
                cmd.Parameters.AddWithValue("@Puntos", mision.Puntos);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Eliminar misión
        public void EliminarMision(int idMision)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_EliminarMision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMision", idMision);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}