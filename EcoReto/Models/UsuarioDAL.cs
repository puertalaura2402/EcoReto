using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EcoReto.Models
{
    public class UsuarioDAL
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MiConexion"].ConnectionString;

        // Registrar usuario
        public void InsertarUsuario(Usuario user)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Insertar usuario directamente (ID autogenerado)
                    using (SqlCommand cmd = new SqlCommand("sp_InsertarUsuario", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Usuario", user.UsuarioNombre);
                        cmd.Parameters.AddWithValue("@Contraseña", user.Contraseña);
                        cmd.Parameters.AddWithValue("@Rol", string.IsNullOrEmpty(user.Rol) ? "Usuario" : user.Rol);
                        cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar usuario: " + ex.Message);
            }
        }

        // Verificar login
        public Usuario VerificarLogin(string usuario, string contraseña)
        {
            Usuario user = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ValidarUsuario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                cmd.Parameters.AddWithValue("@Contraseña", contraseña);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    user = new Usuario
                    {
                        IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                        UsuarioNombre = dr["Usuario"].ToString(),
                        Contraseña = dr["Contraseña"].ToString(),
                        Rol = dr["Rol"].ToString(),
                        Email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : null
                    };
                }
            }
            return user;
        }

        // Verificar si el usuario ya existe
        public bool UsuarioExistente(string usuario)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_VerificarUsuarioExistente", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                con.Open();
                int existe = Convert.ToInt32(cmd.ExecuteScalar());
                return existe > 0;
            }
        }
    }
}
