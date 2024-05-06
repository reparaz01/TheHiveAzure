using Microsoft.AspNetCore.Mvc;
using TheHiveAzure.Extensions;
using TheHiveAzure.Models;
using System.Collections.Generic;
using TheHiveAzure.Services;
using TheHiveAzure.Filters;

public class HomeController : Controller
{
    private ServiceApiTheHive service;

    public HomeController(ServiceApiTheHive service)
    {
        this.service = service; 
    }

    [AuthorizeUsuarios]
    public async Task<IActionResult> Index()
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        HttpContext.Session.Remove("OtherUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        var publicaciones = await this.service.GetPublicacionesExceptoUsuario(currentUser.Username);

        var likesPorPublicacion = new Dictionary<int, int>();
        foreach (var publicacion in publicaciones)
        {
            
            var likesCount = await this.service.GetLikesPublicacion(publicacion.IdPublicacion);
            likesPorPublicacion.Add(publicacion.IdPublicacion, likesCount);
            
            publicacion.Likeado = await this.service.IsLiked(publicacion.IdPublicacion, currentUser.Username);
        }

        
        ViewBag.LikesPorPublicacion = likesPorPublicacion;

        return View(publicaciones);
    }

    public async Task<IActionResult> Siguiendo()
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        HttpContext.Session.Remove("OtherUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        var publicaciones = await this.service.GetPublicacionesSeguidos(currentUser.Username);

        
        var likesPorPublicacion = new Dictionary<int, int>();
        foreach (var publicacion in publicaciones)
        {
            
            var likesCount = await this.service.GetLikesPublicacion(publicacion.IdPublicacion);
            likesPorPublicacion.Add(publicacion.IdPublicacion, likesCount);
            
            publicacion.Likeado = await this.service.IsLiked(publicacion.IdPublicacion, currentUser.Username);
        }

        
        ViewBag.LikesPorPublicacion = likesPorPublicacion;

        return View(publicaciones);
    }



    [HttpPost]
    public async Task<IActionResult> Like(int idPublicacion)
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        await this.service.Like(idPublicacion);

        ViewBag.LikesPorPublicacion = ViewBag.LikesPorPublicacion + 1;

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Dislike(int idPublicacion)
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        await this.service.Dislike(idPublicacion);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(int idPublicacion, bool isLiked)
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        if (currentUser == null)
        {
            return Unauthorized(); 
        }

        if (isLiked)
        {
            await this.service.Dislike(idPublicacion);
        }
        else
        {
            await this.service.Like(idPublicacion);
        }

        return Ok();
    }




}
