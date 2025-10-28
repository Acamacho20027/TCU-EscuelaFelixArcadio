using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "Usuario bloqueado")]
        public bool EstaBloqueado { get; set; }
        
        [Display(Name = "Email confirmado")]
        public bool EmailConfirmed { get; set; }
        
        [Display(Name = "Fecha de registro")]
        public DateTime? FechaRegistro { get; set; }
    }
    
    public class CrearUsuarioViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos 6 caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
        
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "Rol inicial")]
        public string RolInicial { get; set; }
    }
    
    public class EditarUsuarioViewModel
    {
        public string Id { get; set; }
        
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; }
    }
    
    public class CambiarPasswordViewModel
    {
        public string UserId { get; set; }
        
        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos 6 caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaPassword { get; set; }
        
        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; }
    }
}

