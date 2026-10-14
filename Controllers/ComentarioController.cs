
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalMVC.Controllers;

public class ComentarioController : Controller
{
    // 1. Listar los comentarios de un tutorial.
    // GET: Comentario/Index/5
    [HttpGet]
    public IActionResult Index(int idtutorial, int pagina = 1)
    {
        // TODO: Verificar que el tutorial exista.
        // TODO: Obtener los comentarios del tutorial.
        // TODO: Mostrar autor, fecha, cantidad de likes y respuestas.
        // TODO: Aplicar paginación del lado del servidor.

        return View();
    }

    // 2. Crear un comentario.
    // POST: Comentario/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(int idtutorial, string contenido)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Validar que el contenido no supere los 1000 caracteres.
        // TODO: Verificar que el tutorial exista.
        // TODO: Asignar autor y fecha desde el servidor.
        // TODO: Guardar el comentario.

        return RedirectToAction(nameof(Index), new { idtutorial });
    }

    // 3. Responder a un comentario.
    // POST: Comentario/Responder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Responder(
        int idtutorial,
        int comentarioPadreId,
        string contenido)
    {
        // TODO: Obtener el usuario autenticado.
        // TODO: Validar el contenido.
        // TODO: Comprobar que el comentario padre exista.
        // TODO: Verificar que pertenezca al mismo tutorial.
        // TODO: Asignar autor y fecha desde el servidor.
        // TODO: Guardar la respuesta.

        return RedirectToAction(nameof(Index), new { idtutorial });
    }

    // 4. Mostrar el formulario para editar un comentario.
    // GET: Comentario/Editar/5
    [HttpGet]
    public IActionResult Editar(int id)
    {
        // TODO: Buscar el comentario.
        // TODO: Permitir editarlo únicamente a su autor.

        return View();
    }

    // Procesar la edición del comentario.
    // POST: Comentario/Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, string contenido)
    {
        // TODO: Buscar el comentario.
        // TODO: Verificar que pertenezca al usuario autenticado.
        // TODO: Validar el contenido y guardar los cambios.

        return RedirectToAction(nameof(Index), "Home");
    }

    // 5. Mostrar la confirmación para eliminar un comentario.
    // GET: Comentario/Eliminar/5
    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        // TODO: Buscar el comentario.
        // TODO: Verificar si el usuario es su autor o administrador.

        return View();
    }

    // Procesar la eliminación del comentario.
    // POST: Comentario/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Eliminar")]
    public IActionResult ConfirmarEliminar(int id)
    {
        // TODO: Verificar los permisos del usuario.
        // TODO: Gestionar las respuestas y los likes asociados.
        // TODO: Eliminar el comentario según las reglas del proyecto.

        return RedirectToAction(nameof(Index), "Home");
    }
}
