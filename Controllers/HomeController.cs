using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers;

public class HomeController : Controller
{
    private readonly ITutorialesServicio _tutoriales;
    private readonly ITecnologiasServicio _tecnologias;
    private const int TamañoPagina = 10;

    public HomeController(ITutorialesServicio tutoriales, ITecnologiasServicio tecnologias)
    {
        _tutoriales = tutoriales;
        _tecnologias = tecnologias;
    }

    
    public async Task<IActionResult> Index(int pagina = 1, string? buscar = null, int? tecnologiaId = null)
    {
        if (pagina < 1) pagina = 1;

        int total = await _tutoriales.ContarAsync(buscar, tecnologiaId);

        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamañoPagina));

        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var tutoriales = await _tutoriales.ListarAsync(pagina, TamañoPagina, buscar, tecnologiaId);

        ViewBag.PaginaActual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalResultados = total;
        ViewBag.Tecnologias = await _tecnologias.ListarAsync();
        ViewBag.Buscar = buscar;
        ViewBag.TecnologiaId = tecnologiaId;

        return View(tutoriales);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
