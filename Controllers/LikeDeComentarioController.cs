
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalMVC.Controllers;

public class LikeDeComentarioController : Controller
{
    // 1. Agregar un Me gusta a un comentario.
    // POST: LikeDeComentario/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(int comentarioId)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Verificar que el comentario exista.
        // TODO: Evitar que el usuario repita su Me gusta.
        // TODO: Registrar el Me gusta con la fecha del servidor.

        return RedirectToAction("Index", "Home");
    }

    // 2. Eliminar el Me gusta del usuario autenticado.
    // POST: LikeDeComentario/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int comentarioId)
    {
        // TODO: Buscar el Me gusta del usuario actual.
        // TODO: Eliminar únicamente el Me gusta propio.

        return RedirectToAction("Index", "Home");
    }
}
