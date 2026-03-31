using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoReto.Models
{
    // ===== ENTIDAD PRINCIPAL (Usuarios en BD) =====
    // ===== ENTIDAD PRINCIPAL (Usuarios en BD) =====
    public class Usuario
    {
        public int PuntosTotales { get; set; }

        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Nombre de Usuario")]
        public string UsuarioNombre { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        // ❗️Quito [Required] para evitar error al registrar, ya que el rol se asigna automáticamente en el controlador.
        [Display(Name = "Rol")]
        public string Rol { get; set; }  // Usuario / Administrador

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime? FechaRegistro { get; set; }

        public List<Mision> MisionesCompletadas { get; set; } = new List<Mision>();
    }

    // ===== MODELO PARA LOGIN =====
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        // 🔁 Cambio de "TipoUsuario" → "Rol" para usar los mismos nombres que la BD y las vistas.
        [Required(ErrorMessage = "Debe seleccionar su rol")]
        [Display(Name = "Rol")]
        public string Rol { get; set; }  // Usuario o Administrador

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }

    // ===== MODELO PARA REGISTRO =====
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Nombre de Usuario")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contraseña { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Contraseña", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmarContraseña { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        // Relación con las misiones completadas
        public List<Mision> MisionesCompletadas { get; set; } = new List<Mision>();

    }

}
