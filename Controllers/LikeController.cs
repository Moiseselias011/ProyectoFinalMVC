
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalMVC.Controllers;

public class LikeController : Controller
{
    // 1. Agregar un Me gusta a un tutorial.
    // POST: Like/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(int tutorialId)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Verificar que el tutorial exista.
        // TODO: Evitar que el usuario repita su Me gusta.
        // TODO: Registrar el Me gusta con la fecha del servidor.

        return RedirectToAction("Index", "Home");
    }

    // 2. Eliminar el Me gusta del usuario autenticado.
    // POST: Like/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int tutorialId)
    {
        // TODO: Buscar el Me gusta del usuario actual.
        // TODO: Eliminar únicamente el Me gusta propio.

        return RedirectToAction("Index", "Home");
    }
}
