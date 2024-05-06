using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheHiveAzure.Extensions;
using TheHiveAzure.Helpers;
using TheHiveAzure.Models;
using TheHiveAzure.Services;

public class PerfilController : Controller
{

    private HelperCryptography helper;
    private ServiceApiTheHive service;

    public PerfilController(ServiceApiTheHive service)
    {
        this.service = service;
    }

    public async Task<IActionResult> VerPerfil(string otherUser)
    {
        Usuario user = await this.service.FindUsuario(otherUser);
        HttpContext.Session.SetObject("OtherUser", user);


        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");
        var otherUserr = HttpContext.Session.GetObject<Usuario>("OtherUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        var publicaciones = await this.service.GetPublicacionesUsuario(otherUserr.Username);

        int publicacionesCount = publicaciones.Count();

        var seguidosCount = await this.service.GetSeguidosCount(otherUserr.Username);
        var seguidoresCount = await this.service.GetSeguidoresCount(otherUserr.Username);

        
        ViewBag.PublicacionesCount = publicacionesCount;
        ViewBag.SeguidosCount = seguidosCount;
        ViewBag.SeguidoresCount = seguidoresCount;


        
        var likesPorPublicacion = new Dictionary<int, int>();
        foreach (var publicacion in publicaciones)
        {
            var likesCount = await this.service.GetLikesPublicacion(publicacion.IdPublicacion);
            likesPorPublicacion.Add(publicacion.IdPublicacion, likesCount);
            publicacion.Likeado = await this.service.IsLiked(publicacion.IdPublicacion, currentUser.Username);
        }

        ViewBag.LikesPorPublicacion = likesPorPublicacion;

        var isFollowing = await this.service.IsFollowing(otherUserr.Username);

        ViewBag.IsFollowing = isFollowing;



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


    [HttpPost]
    public async Task<IActionResult> Follow(string otherUser)
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        await this.service.Follow(otherUser);

        return RedirectToAction("VerPerfil", new { otherUser });
    }

    [HttpPost]
    public async Task<IActionResult> Unfollow(string otherUser)
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        await this.service.Unfollow(otherUser);

        return RedirectToAction("VerPerfil", new { otherUser });
    }


    [HttpPost]
    public IActionResult EliminarPublicacion(int idPublicacion)
    {
        var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Inicio");
        }

        this.service.DeletePublicacion(idPublicacion);

        return RedirectToAction("VerPerfil", new { otherUser = currentUser.Username });
    }


}






