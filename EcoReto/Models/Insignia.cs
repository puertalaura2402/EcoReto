using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EcoReto.Models
{
    public class Insignia
    {
        public int IdInsignia { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Ruta de Imagen")]
        public string ImagenPath { get; set; }

        [Display(Name = "Categoría")]
        public int? IdCategoria { get; set; }

        [Display(Name = "Nombre de Categoría")]
        public string NombreCategoria { get; set; }

        [Display(Name = "Tipo de Requisito")]
        public string TipoRequisito { get; set; } // 'MisionesPorCategoria', 'MisionesTotales'

        [Display(Name = "Cantidad Requerida")]
        public int CantidadRequisito { get; set; }

        // Propiedades adicionales para la vista
        public bool EstaDesbloqueada { get; set; }
        public int ProgresoActual { get; set; }
        public int ProgresoTotal { get; set; }
        public double PorcentajeProgreso { get; set; }
    }
}