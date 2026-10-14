
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinalMVC.Controllers;

public class ListaPersonalController : Controller
{
    // GET: ListaPersonal
    public IActionResult Index()
    {
        return View();
    }

    // GET: ListaPersonal/Detalle/5
    public IActionResult Detalle(int id)
    {
        return View();
    }

    // GET: ListaPersonal/Crear
    public IActionResult Crear()
    {
        return View();
    }

    // POST: ListaPersonal/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(string nombre)
    {
        return View();
    }

    // GET: ListaPersonal/Editar/5
    public IActionResult Editar(int id)
    {
        return View();
    }

    // POST: ListaPersonal/Editar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(int id, string nombre)
    {
        return View();
    }

    // GET: ListaPersonal/Eliminar/5
    public IActionResult Eliminar(int id)
    {
        return View();
    }

    // POST: ListaPersonal/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarEliminar(int id)
    {
        return View();
    }
}
