using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProyectoFinalMVC.Models;

    public class Usuario
    {   
       [Key] 
       public int Id_usuario{get;set;}

       [Required, MaxLength(100)]
       public string NombreUsuario{get;set;}= string.Empty;

       [Required, MaxLength(100),EmailAddress]
       public string Email{get;set;}= string.Empty;

       [Required]
       public string HashContrasena{get;set;} = string.Empty;

       [Required, MaxLength(100)]
       public  string Rol{get;set;} = Roles.Usuario; 

       public DateTime FechaCreacion{get;set;} = DateTime.UtcNow;

       [MaxLength(500)] 
       public string? RutaFoto{get;set;}

       public ICollection <Tutorial> Tutoriales {get;set;} = new List<Tutorial>(); 
       public ICollection <Comentario> Comentarios{get;set;} = new List<Comentario>();
       public ICollection <Like> Likes{get;set;} = new List<Like>();
       public ICollection <LikeDeComentario> LikesDeComentarios {get;set;} = new List<LikeDeComentario>();
       public ICollection <ListaPersonal> ListaPersonales{get;set;} =new List<ListaPersonal>();

    }
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Usuario = "Usuario";
    }



    
