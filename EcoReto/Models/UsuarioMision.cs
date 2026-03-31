using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcoReto.Models
{
    public class UsuarioMision
    {
        public int IdUsuario { get; set; }
        public int IdMision { get; set; }

        // Navegación opcional
        public Usuario Usuario { get; set; }
        public Mision Mision { get; set; }
    }
}