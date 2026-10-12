using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProyectoFinalMVC.Models;

    public class ListaPersonal
    {
      [Key]  
      public int IdListaPersonal{get;set;}

      [Required, MaxLength(100)]  
      public string Nombre{get;set;} = string.Empty; 
     
      public DateTime FechaCreacion {get;set;}= DateTime.UtcNow;



      public int DueñoId{get;set;}
      public Usuario Dueño{get;set;} = null!;
      
      public ICollection <TutorialGuardado>TutorialesGuardados{get;set;} = new List<TutorialGuardado>();

    }
