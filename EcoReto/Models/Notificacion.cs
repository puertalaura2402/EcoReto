using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EcoReto.Models
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public int IdUsuario { get; set; }

        [Display(Name = "Tipo")]
        public string Tipo { get; set; } // 'InsigniaDesbloqueada', 'NuevaMision'

        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Mensaje")]
        public string Mensaje { get; set; }

        [Display(Name = "ID Referencia")]
        public int? IdReferencia { get; set; }

        [Display(Name = "Leída")]
        public bool Leida { get; set; }

        [Display(Name = "Fecha")]
        public DateTime FechaCreacion { get; set; }

        // Propiedades adicionales para la vista
        public string ImagenPath { get; set; }
        public string Icono { get; set; }
    }
}