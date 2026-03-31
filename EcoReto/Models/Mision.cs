using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EcoReto.Models
{
    public class Mision
    {
        public int IdMision { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Título de la Misión")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Los puntos son obligatorios")]
        [Range(1, 1000, ErrorMessage = "Debe asignar entre 1 y 1000 puntos")]
        [Display(Name = "Puntos")]
        public int Puntos { get; set; }

        // 🔽 NUEVAS PROPIEDADES 🔽

        [Display(Name = "Categoría")]
        public int IdCategoria { get; set; }

        // Esta no está en la base de datos, solo sirve para mostrar el nombre en la vista
        [Display(Name = "Nombre de la Categoría")]
        public string CategoriaNombre { get; set; }

    }
}
