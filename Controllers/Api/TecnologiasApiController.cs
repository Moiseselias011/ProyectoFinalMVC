using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers.Api
{
    
    [Route("api/tecnologias")]
    [ApiController]
    public class TecnologiasApiController : ControllerBase
    {
        private readonly ITecnologiasServicio _tecnologias;

        public TecnologiasApiController(ITecnologiasServicio tecnologias)
        {
            _tecnologias = tecnologias;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await _tecnologias.ListarAsync());
        }

       
        [HttpGet("{idOrSlug}")]
        public async Task<IActionResult> Obtener(string idOrSlug)
        {
            var tecnologia = await _tecnologias.ObtenerPorIdOSlugAsync(idOrSlug);

            if (tecnologia is null) return NotFound(new { Mensaje = "La tecnología no existe." });

            return Ok(tecnologia);
        }
    }
}

