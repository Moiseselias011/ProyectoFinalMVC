using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoFinalMVC.Controllers
{
    public class TecnologiaController : Controller
    {
        private readonly ProyectoFinalMVCContext _context;

        public TecnologiaController(ProyectoFinalMVCContext context)
        {
            _context = context;
        }

        
        // 1. LISTAR TECNOLOGÍAS (Index)
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            var tecnologias = await _context.Tecnologia.ToListAsync();
            
            return View(tecnologias);
        }

        
        // 2. CONSULTAR UNA TECNOLOGIA (Detalle)
        
        [HttpGet]
        [Route("Tecnologia/Detalle/{idOrSlug}")]
        public async Task<IActionResult> Detalle(string idOrSlug, int pagina = 1)
        {
            if (pagina < 1) pagina = 1;
            int tamañoPagina = 10; 

           
            bool esId = int.TryParse(idOrSlug, out int idTecnologia);

            
            var tecnologia = await _context.Tecnologia
                .FirstOrDefaultAsync(t => (esId && t.IdTecnologia == idTecnologia) || (!esId && t.Slug == idOrSlug));

            
            if (tecnologia == null)
            {
                return NotFound();
            }

           
            var queryTutoriales = _context.Tutorial
                .Include(t => t.Usuario) 
                .Where(t => t.TecnologiaId == tecnologia.IdTecnologia);

            int totalTutoriales = await queryTutoriales.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalTutoriales / (double)tamañoPagina);

            var tutorialesAsociados = await queryTutoriales
                .OrderByDescending(t => t.FechaPublicacion)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

           
            ViewBag.Tutoriales = tutorialesAsociados;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;
            ViewBag.IdOrSlug = idOrSlug;

            
            return View(tecnologia);
        }

       
        // 3. CREAR TECNOLOGÍA (Crear)
       
        
        [Authorize(Roles = Roles.Administrador)]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Tecnologia tecnologia)
        {
            
            var slugExiste = await _context.Tecnologia.AnyAsync(t => t.Slug == tecnologia.Slug);
            if (slugExiste)
            {
                ModelState.AddModelError("Slug", "El Slug ingresado ya se encuentra registrado.");
            }

            
            if (!string.IsNullOrEmpty(tecnologia.Dominio) && !Domains.EsValido(tecnologia.Dominio))
            {
                ModelState.AddModelError("Dominio", "El dominio provisto no cumple con los formatos válidos.");
            }

            
            if (!ModelState.IsValid)
            {
                return View(tecnologia);
            }

           
            _context.Tecnologia.Add(tecnologia);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La tecnología '{tecnologia.Nombre}' fue creada y registrada con éxito.";
            return RedirectToAction("Index");
        }

        
        // 4. EDITAR TECNOLOGÍA (Editar)
        
        [Authorize(Roles = Roles.Administrador)]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var tecnologia = await _context.Tecnologia.FirstOrDefaultAsync(t => t.IdTecnologia == id);
            
            
            if (tecnologia == null)
            {
                return NotFound();
            }

            return View(tecnologia);
        }

        [Authorize(Roles = Roles.Administrador)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Tecnologia tecnologiaModificada)
        {
            if (id != tecnologiaModificada.IdTecnologia) return NotFound();

            var tecnologiaBase = await _context.Tecnologia.FirstOrDefaultAsync(t => t.IdTecnologia == id);
            if (tecnologiaBase == null) return NotFound();

            
            var slugDuplicado = await _context.Tecnologia
                .AnyAsync(t => t.Slug == tecnologiaModificada.Slug && t.IdTecnologia != id);
            
            if (slugDuplicado)
            {
                ModelState.AddModelError("Slug", "Este Slug ya está siendo utilizado por otra tecnología.");
            }

            
            if (!string.IsNullOrEmpty(tecnologiaModificada.Dominio) && !Domains.EsValido(tecnologiaModificada.Dominio))
            {
                ModelState.AddModelError("Dominio", "El dominio provisto no cumple con los formatos válidos.");
            }

            
            if (!ModelState.IsValid)
            {
                return View(tecnologiaModificada);
            }

            
            tecnologiaBase.Nombre = tecnologiaModificada.Nombre;
            tecnologiaBase.Slug = tecnologiaModificada.Slug;
            tecnologiaBase.Dominio = tecnologiaModificada.Dominio;

            
            _context.Tecnologia.Update(tecnologiaBase);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"La tecnología '{tecnologiaBase.Nombre}' se actualizó correctamente.";
            return RedirectToAction("Index");
        }
    }
}