
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ProyectoFinalMVC.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ProyectoFinalMVC
{
    public class TutorialController : Controller
    {
        private readonly ProyectoFinalMVCContext _context;
        private readonly ITutorialesService _tutorialesService;

        public TutorialController(
            ProyectoFinalMVCContext context,
            ITutorialesService tutorialesService)
        {
            _context = context;
            _tutorialesService = tutorialesService;
        }

        // 1. LISTAR TUTORIALES (Index)

        [HttpGet]
        public async Task<IActionResult> Index(
            string buscarTitulo,
            int? IdTecnologia,
            int pagina = 1)
        {
            if (pagina < 1)
                pagina = 1;

            int tamañoPagina = 10;

            var query = _context.Tutorial
                .Include(t => t.Usuario)
                .Include(t => t.Tecnologia)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscarTitulo))
            {
                query = query.Where(
                    t => t.Titulo.Contains(buscarTitulo));
            }

            if (IdTecnologia.HasValue)
            {
                query = query.Where(
                    t => t.TecnologiaId == IdTecnologia.Value);
            }

            int totalTutoriales = await query.CountAsync();

            int totalPaginas = (int)Math.Ceiling(
                totalTutoriales / (double)tamañoPagina);

            var tutoriales = await query
                .OrderBy(t => t.idtutorial)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0
                ? 1
                : totalPaginas;

            ViewBag.buscarTitulo = buscarTitulo;
            ViewBag.IdTecnologia = IdTecnologia;

            ViewBag.Tecnologias = await _context.Tecnologia
                .ToListAsync();

            return View(tutoriales);
        }

        // 2. CONSULTAR UN TUTORIAL (Detalle)

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var tutorial = await _context.Tutorial
                .Include(t => t.Usuario)
                .Include(t => t.Tecnologia)
                .Include(t => t.Comentarios)
                .Include(t => t.Likes)
                .FirstOrDefaultAsync(t => t.idtutorial == id);

            if (tutorial == null)
            {
                return NotFound();
            }

            return View(tutorial);
        }

        // 3. MOSTRAR FORMULARIO PARA PUBLICAR (Crear)

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Tecnologias = await _context.Tecnologia
                .ToListAsync();

            return View();
        }

        // 4. PUBLICAR TUTORIAL (Crear)

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Tutorial tutorial)
        {
            ModelState.Remove(nameof(tutorial.Usuario));
            ModelState.Remove(nameof(tutorial.Tecnologia));

            var usuarioIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(usuarioIdClaim) ||
                !int.TryParse(usuarioIdClaim, out int idUsuario))
            {
                return Challenge();
            }

            var tecnologiaExiste = await _context.Tecnologia
                .AnyAsync(te =>
                    te.IdTecnologia == tutorial.TecnologiaId);

            if (!tecnologiaExiste)
            {
                ModelState.AddModelError(
                    nameof(tutorial.TecnologiaId),
                    "La tecnología seleccionada no es válida.");
            }

            string miniatura = ExtraerMiniaturaYouTube(tutorial.Url);

            if (string.IsNullOrEmpty(miniatura))
            {
                ModelState.AddModelError(
                    nameof(tutorial.Url),
                    "La URL debe corresponder a un enlace válido de YouTube.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Tecnologias = await _context.Tecnologia
                    .ToListAsync();

                return View(tutorial);
            }

            tutorial.Idusuario = idUsuario;
            tutorial.UrlImagen = miniatura;
            tutorial.FechaPublicacion = DateTime.UtcNow;

            _context.Tutorial.Add(tutorial);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "¡Tutorial publicado con éxito!";

            return RedirectToAction(nameof(Index));
        }

        private static string ExtraerMiniaturaYouTube(string url)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                uri.Scheme != Uri.UriSchemeHttps)
            {
                return string.Empty;
            }

            string? videoId = null;

            if (uri.Host.Equals(
                    "www.youtube.com",
                    StringComparison.OrdinalIgnoreCase) ||
                uri.Host.Equals(
                    "youtube.com",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (uri.AbsolutePath == "/watch")
                {
                    videoId = Microsoft.AspNetCore.WebUtilities
                        .QueryHelpers.ParseQuery(uri.Query)["v"]
                        .ToString();
                }
                else if (uri.AbsolutePath.StartsWith(
                    "/embed/",
                    StringComparison.Ordinal))
                {
                    videoId = uri.AbsolutePath["/embed/".Length..];
                }
                else if (uri.AbsolutePath.StartsWith(
                    "/shorts/",
                    StringComparison.Ordinal))
                {
                    videoId = uri.AbsolutePath["/shorts/".Length..];
                }
            }
            else if (uri.Host.Equals(
                "youtu.be",
                StringComparison.OrdinalIgnoreCase))
            {
                videoId = uri.AbsolutePath.Trim('/');
            }

            if (string.IsNullOrEmpty(videoId) ||
                !Regex.IsMatch(videoId, @"^[a-zA-Z0-9_-]{11}$"))
            {
                return string.Empty;
            }

            return $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
        }
    }
}
