using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProyectoFinalMVC.Models;

    public class Comentario
    {    
        [Key]
        public int Idcomentario {get;set;}

        [Required,MaxLength(1000)] 
        public string Contenido{get;set;} = string.Empty;

        public DateTime FechaCreacion{get;set;} = DateTime.UtcNow;

        public int idtutorial{get;set;}
        public Tutorial Tutorial {get;set;} = null!;

        public int Idusuario{get;set;}
        public Usuario Usuario{get;set;} = null!;

        public int? ComentarioPadreId{get;set;}
        public Comentario? ComentarioPadre {get;set;} 


        public ICollection<Comentario>Respuestas{get;set;} = new List<Comentario>();  
        public ICollection<LikeDeComentario> Likes {get;set;} = new List<LikeDeComentario>(); 

    }
