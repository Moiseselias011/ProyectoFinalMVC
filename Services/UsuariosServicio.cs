using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class UsuariosServicio : IUsuariosServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public UsuariosServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LISTAR USUARIOS CON PAGINACION

    public async Task<List<Usuario>> ListarAsync(int pagina = 1, int tamañoPagina = 10)
    {
        if (pagina < 1) pagina = 1;

        return await _context.Usuario
            .AsNoTracking()
            .OrderBy(u => u.Id_usuario)
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarAsync()
    {
        return await _context.Usuario.CountAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuario
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id_usuario == id);
    }

    // 2. REGISTRO DE USUARIOS

    public async Task<(bool Ok, string? Error)> RegistrarAsync(string nombreUsuario, string email, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(contrasena))
        {
            return (false, "Todos los campos son obligatorios.");
        }

        bool emailExiste = await _context.Usuario.AnyAsync(u => u.Email == email);

        if (emailExiste)
        {
            return (false, "El email ya está registrado.");
        }

        string hashSeguro = BCrypt.Net.BCrypt.HashPassword(contrasena);

        var nuevoUsuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            Email = email,
            HashContrasena = hashSeguro,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Usuario.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 3. AUTENTICACION

    public async Task<Usuario?> AutenticarAsync(string email, string contrasena)
    {
        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null) return null;

        bool esContrasenaCorrecta = BCrypt.Net.BCrypt.Verify(contrasena, usuario.HashContrasena);

        return esContrasenaCorrecta ? usuario : null;
    }

    // 4. CAMBIAR ROL

    public async Task<(bool Ok, string? Error)> CambiarRolAsync(int id, string rol, int idAdministradorActual)
    {
        if (rol != Roles.Administrador && rol != Roles.Usuario)
        {
            return (false, "El rol seleccionado no es válido.");
        }

        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == id);

        if (usuario is null) return (false, "El usuario no existe.");

        if (usuario.Id_usuario == idAdministradorActual && rol != Roles.Administrador)
        {
            return (false, "No puedes quitarte el rol de Administrador a ti mismo.");
        }

        usuario.Rol = rol;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 7. ELIMINAR CUENTA Y CONTENIDOS RELACIONADOS

    public async Task<bool> EliminarAsync(int id, int idAdministradorActual)
    {
        if (id == idAdministradorActual) return false;

        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == id);

        if (usuario is null) return false;

        await using var transaccion = await _context.Database.BeginTransactionAsync();

        try
        {
 
            var idsTutorialesPropios = await _context.Tutorial
                .Where(t => t.Idusuario == id)
                .Select(t => t.Idtutorial)
                .ToListAsync();

 
            var comentariosPropios = await _context.Comentario
                .Where(c =>
                    c.Idusuario == id &&
                    !idsTutorialesPropios.Contains(c.idtutorial))
                .ToListAsync();

            var idsComentariosPropios = comentariosPropios
                .Select(c => c.Idcomentario)
                .ToList();

 
            var respuestas = await _context.Comentario
                .Where(c =>
                    c.ComentarioPadreId.HasValue &&
                    idsComentariosPropios.Contains(c.ComentarioPadreId.Value))
                .ToListAsync();

            foreach (var respuesta in respuestas)
            {
                respuesta.ComentarioPadreId = null;
            }

            await _context.SaveChangesAsync();

 
            var likesTutoriales = await _context.Like
                .Where(l => l.UsuarioId == id)
                .ToListAsync();

            _context.Like.RemoveRange(likesTutoriales);

            var likesComentarios = await _context.LikeDeComentario
                .Where(l => l.UsuarioId == id)
                .ToListAsync();

            _context.LikeDeComentario.RemoveRange(likesComentarios);

            await _context.SaveChangesAsync();

 
            _context.Comentario.RemoveRange(comentariosPropios);

            await _context.SaveChangesAsync();

 
            var listasPersonales = await _context.ListaPersonal
                .Where(l => l.DueñoId == id)
                .ToListAsync();

            _context.ListaPersonal.RemoveRange(listasPersonales);

            await _context.SaveChangesAsync();

 
            var tutorialesPropios = await _context.Tutorial
                .Where(t => t.Idusuario == id)
                .ToListAsync();

            _context.Tutorial.RemoveRange(tutorialesPropios);

            await _context.SaveChangesAsync();

 
            _context.Usuario.Remove(usuario);

            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();

            return true;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }
}

