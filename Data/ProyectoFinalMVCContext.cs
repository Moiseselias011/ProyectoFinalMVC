using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC
{
    public class ProyectoFinalMVCContext : DbContext
    {
        public ProyectoFinalMVCContext(DbContextOptions<ProyectoFinalMVCContext> options)
            : base(options)
        {
        }

        //  nombres singulares y plurales para compatibilidad
        public DbSet<Usuario> Usuario => Set<Usuario>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();

        public DbSet<Tutorial> Tutorial => Set<Tutorial>();
        public DbSet<Tutorial> Tutoriales => Set<Tutorial>();

        public DbSet<Tecnologia> Tecnologia => Set<Tecnologia>();
        public DbSet<Tecnologia> Tecnologias => Set<Tecnologia>();

        public DbSet<Comentario> Comentario => Set<Comentario>();
        public DbSet<Comentario> Comentarios => Set<Comentario>();

        public DbSet<ListaPersonal> ListaPersonal => Set<ListaPersonal>();
        public DbSet<ListaPersonal> ListasPersonales => Set<ListaPersonal>();

        public DbSet<Like> Like => Set<Like>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Like> likes => Set<Like>();

        public DbSet<LikeDeComentario> LikeDeComentario => Set<LikeDeComentario>();
        public DbSet<LikeDeComentario> LikesDeComentarios => Set<LikeDeComentario>();

        public DbSet<TutorialGuardado> TutorialGuardado => Set<TutorialGuardado>();
        public DbSet<TutorialGuardado> TutorialesGuardados => Set<TutorialGuardado>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // USUARIO
            mb.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            mb.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            mb.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasDefaultValue(Roles.Usuario);

            // TECNOLOGIA
            mb.Entity<Tecnologia>()
                .HasIndex(t => t.Slug)
                .IsUnique();

            mb.Entity<Tecnologia>()
                .HasIndex(t => t.Nombre)
                .IsUnique();

            mb.Entity<Tecnologia>()
                .Property(t => t.Dominio)
                .HasDefaultValue(Domains.Programacion);

            // TUTORIAL
            mb.Entity<Tutorial>()
                .HasIndex(t => t.TecnologiaId);

            mb.Entity<Tutorial>()
                .HasIndex(t => t.Idusuario);

            mb.Entity<Tutorial>()
                .HasIndex(t => t.FechaPublicacion);

            mb.Entity<Tutorial>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.Tutoriales)
                .HasForeignKey(t => t.Idusuario)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Tutorial>()
                .HasOne(t => t.Tecnologia)
                .WithMany(te => te.Tutoriales)
                .HasForeignKey(t => t.TecnologiaId)
                .OnDelete(DeleteBehavior.Restrict);

            // COMENTARIO
             mb.Entity<Comentario>()
                .HasIndex(c => c.idtutorial);

             mb.Entity<Comentario>()
               .HasIndex(c => c.ComentarioPadreId);

             mb.Entity<Comentario>()
               .HasOne(c => c.Tutorial)
               .WithMany(t => t.Comentarios)
               .HasForeignKey(c => c.idtutorial)
               .OnDelete(DeleteBehavior.Cascade);

             mb.Entity<Comentario>()
               .HasOne(c => c.Usuario)
               .WithMany(u => u.Comentarios)
               .HasForeignKey(c => c.Idusuario)
               .OnDelete(DeleteBehavior.Restrict);

             mb.Entity<Comentario>()
               .HasOne(c => c.ComentarioPadre)
               .WithMany(c => c.Respuestas)
               .HasForeignKey(c => c.ComentarioPadreId)
               .OnDelete(DeleteBehavior.NoAction);
            // LIKE DE TUTORIAL
            mb.Entity<Like>()
                .HasIndex(l => new { l.UsuarioId, l.TutorialId })
                .IsUnique();

            mb.Entity<Like>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Like>()
                .HasOne(l => l.Tutorial)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.TutorialId)
                .OnDelete(DeleteBehavior.Cascade);

            // LIKE DE COMENTARIO
            mb.Entity<LikeDeComentario>()
                .HasIndex(l => new { l.UsuarioId, l.ComentarioId })
                .IsUnique();

            mb.Entity<LikeDeComentario>()
                .HasOne(l => l.Usuario)
                .WithMany(u => u.LikesDeComentarios)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<LikeDeComentario>()
                .HasOne(l => l.Comentario)
                .WithMany(c => c.Likes)
                .HasForeignKey(l => l.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // LISTA PERSONAL
            mb.Entity<ListaPersonal>()
                .HasIndex(l => new { l.DueñoId, l.Nombre })
                .IsUnique();

            mb.Entity<ListaPersonal>()
                .HasOne(l => l.Dueño)
                .WithMany(u => u.ListaPersonales)
                .HasForeignKey(l => l.DueñoId)
                .OnDelete(DeleteBehavior.Restrict);

            // TUTORIAL GUARDADO
            mb.Entity<TutorialGuardado>()
                .HasIndex(tg => new { tg.IdListaPersonal, tg.TutorialId })
                .IsUnique();

            mb.Entity<TutorialGuardado>()
                .HasOne(tg => tg.ListaPersonal)
                .WithMany(l => l.TutorialesGuardados)
                .HasForeignKey(tg => tg.IdListaPersonal)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<TutorialGuardado>()
                .HasOne(tg => tg.Tutorial)
                .WithMany(t => t.TutorialesGuardados)
                .HasForeignKey(tg => tg.TutorialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}