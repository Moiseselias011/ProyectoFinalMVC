using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers.Api
{
    
    [Route("api/likes")]
    [ApiController]
    [Authorize]
    public class LikesApiController : ControllerBase
    {
        private readonly ILikesServicio _likes;

        public LikesApiController(ILikesServicio likes)
        {
            _likes = likes;
        }

        private int? ObtenerUsuarioId()
        {
            string? valorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
        }

     
        [HttpGet("tutoriales/{tutorialId}")]
        public async Task<IActionResult> ContarLikesTutorial(int tutorialId)
        {
            return Ok(new
            {
                IdTutorial = tutorialId,
                TotalLikes = await _likes.ContarLikesTutorialAsync(tutorialId)
            });
        }

    
        [HttpPost("tutoriales/{tutorialId}")]
        public async Task<IActionResult> LikeTutorial(int tutorialId)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            bool ok = await _likes.AgregarLikeTutorialAsync(usuarioId.Value, tutorialId);

            if (!ok) return NotFound(new { Mensaje = "El tutorial no existe." });

            return Ok(new
            {
                Mensaje = "Like agregado.",
                TotalLikes = await _likes.ContarLikesTutorialAsync(tutorialId)
            });
        }

    
        [HttpDelete("tutoriales/{tutorialId}")]
        public async Task<IActionResult> QuitarLikeTutorial(int tutorialId)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            await _likes.QuitarLikeTutorialAsync(usuarioId.Value, tutorialId);

            return Ok(new
            {
                Mensaje = "Like eliminado.",
                TotalLikes = await _likes.ContarLikesTutorialAsync(tutorialId)
            });
        }

        
        [HttpPost("comentarios/{comentarioId}")]
        public async Task<IActionResult> LikeComentario(int comentarioId)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            bool ok = await _likes.AgregarLikeComentarioAsync(usuarioId.Value, comentarioId);

            if (!ok) return NotFound(new { Mensaje = "El comentario no existe." });

            return Ok(new
            {
                Mensaje = "Like agregado.",
                TotalLikes = await _likes.ContarLikesComentarioAsync(comentarioId)
            });
        }


        [HttpDelete("comentarios/{comentarioId}")]
        public async Task<IActionResult> QuitarLikeComentario(int comentarioId)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            await _likes.QuitarLikeComentarioAsync(usuarioId.Value, comentarioId);

            return Ok(new
            {
                Mensaje = "Like eliminado.",
                TotalLikes = await _likes.ContarLikesComentarioAsync(comentarioId)
            });
        }
    }
}