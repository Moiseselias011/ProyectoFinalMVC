using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Controllers
{
    [Authorize(Roles = Roles.Administrador)]
    public class UsuarioController : Controller
    {
        private readonly ProyectoFinalMVCContext _context;

        public UsuarioController(ProyectoFinalMVCContext context)
        {
            _context = context;
        }

        // 1. LISTAR USUARIOS CON PAGINACIÓN
        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1)
        {
            if (pagina < 1)
                pagina = 1;

            const int tamañoPagina = 10;

            int totalUsuarios = await _context.Usuario.CountAsync();

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(totalUsuarios / (double)tamañoPagina)
            );

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var usuarios = await _context.Usuario
                .AsNoTracking()
                .OrderBy(u => u.Id_usuario)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(usuarios);
        }

        // 2. CONSULTAR INFORMACIÓN DE UN USUARIO
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id_usuario == id);

            if (usuario is null)
                return NotFound();

            return View(usuario);
        }

        // 3. MOSTRAR FORMULARIO PARA CAMBIAR EL ROL
        [HttpGet]
        public async Task<IActionResult> CambiarRol(int id)
        {
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id_usuario == id);

            if (usuario is null)
                return NotFound();

            return View(usuario);
        }

        // 4. CAMBIAR EL ROL DE UN USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(int id_usuario, string rol)
        {
            if (rol != Roles.Administrador && rol != Roles.Usuario)
            {
                ModelState.AddModelError(
                    "Rol",
                    "El rol seleccionado no es válido."
                );

                var usuarioError = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Id_usuario == id_usuario);

                if (usuarioError is null)
                    return NotFound();

                return View(usuarioError);
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id_usuario == id_usuario);

            if (usuario is null)
                return NotFound();

            var administradorActualId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuario.Id_usuario.ToString() == administradorActualId &&
                rol != Roles.Administrador)
            {
                ModelState.AddModelError(
                    "Rol",
                    "No puedes quitarte el rol de Administrador a ti mismo."
                );

                return View(usuario);
            }

            usuario.Rol = rol;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                $"Rol del usuario '{usuario.NombreUsuario}' actualizado a '{rol}' con éxito.";

            return RedirectToAction(nameof(Index));
        }

        // 5. MOSTRAR CONFIRMACIÓN PARA ELIMINAR UNA CUENTA
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id_usuario == id);

            if (usuario is null)
                return NotFound();

            var administradorActualId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (usuario.Id_usuario.ToString() == administradorActualId)
            {
                TempData["MensajeError"] =
                    "No puedes eliminar tu propia cuenta de administrador.";

                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // 6. CONFIRMAR Y EJECUTAR LA ELIMINACIÓN DE LA CUENTA
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Eliminar")]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var administradorActualId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (id.ToString() == administradorActualId)
            {
                TempData["MensajeError"] =
                    "No puedes eliminar tu propia cuenta de administrador.";

                return RedirectToAction(nameof(Index));
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id_usuario == id);

            if (usuario is null)
                return NotFound();

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

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
                        idsComentariosPropios.Contains(
                            c.ComentarioPadreId.Value))
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

                TempData["MensajeExito"] =
                    "La cuenta y sus contenidos relacionados fueron eliminados.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }
    }
}