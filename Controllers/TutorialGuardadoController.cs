
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalMVC.Controllers;

public class TutorialGuardadoController : Controller
{
    // 1. Listar los tutoriales guardados en una lista personal.
    // GET: TutorialGuardado/Index?IdListaPersonal=1
    [HttpGet]
    public IActionResult Index(int idListaPersonal, int pagina = 1)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Buscar la lista personal y verificar su propietario.
        // TODO: Obtener los tutoriales guardados en esa lista.
        // TODO: Mostrar título, imagen, autor y fecha.
        // TODO: Aplicar paginación del lado del servidor.

        return View();
    }

    // 2. Guardar un tutorial en una lista personal.
    // POST: TutorialGuardado/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(int idListaPersonal, int tutorialId)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Verificar que la lista pertenezca al usuario.
        // TODO: Verificar que el tutorial exista.
        // TODO: Evitar guardar dos veces el mismo tutorial en la lista.
        // TODO: Asignar la fecha desde el servidor.
        // TODO: Registrar la relación entre la lista y el tutorial.

        return RedirectToAction(nameof(Index), new { idListaPersonal });
    }

    // 3. Quitar un tutorial guardado de una lista.
    // POST: TutorialGuardado/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int idTutorialGuardado)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Buscar el tutorial guardado.
        // TODO: Verificar que la lista asociada pertenezca al usuario.
        // TODO: Eliminar únicamente la relación de guardado.
        // TODO: Conservar el tutorial original.

        return RedirectToAction("Index", "Home");
    }
}
