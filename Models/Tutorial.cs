using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProyectoFinalMVC.Models;

    public class Tutorial
    {   
        [Key]
        public int idtutorial{get;set;}

        [Required, MaxLength(200)]
        public string  Titulo{get;set;} = string.Empty;

        [Required, MaxLength(1000)]
        public string Descripcion {get;set;} = string.Empty;	

        [Required, MaxLength(200)]
        public string Url {get;set;} = string.Empty;

        [MaxLength(500)]
        public string UrlImagen {get;set;}	

        public DateTime  FechaPublicacion{get;set;} =  DateTime.UtcNow;	

        public int Idusuario {get;set;}
        public Usuario Usuario {get;set;}= null!;	

        
        public int TecnologiaId {get;set;}
        public Tecnologia Tecnologia {get;set;} = null!;	
        

        public ICollection<Comentario> Comentarios {get;set;} = new List<Comentario>();
        public ICollection<Like> Likes {get;set;} = new List<Like>();

        public ICollection<TutorialGuardado> TutorialesGuardados{get;set;} = new List<TutorialGuardado>();  

    }
