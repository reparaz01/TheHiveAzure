using Microsoft.AspNetCore.Mvc;
using TheHiveAzure.Extensions;
using TheHiveAzure.Helpers;
using TheHiveAzure.Models;
using TheHiveAzure.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TheHiveAzure.Controllers
{
    public class EditarPerfilController : Controller
    {
        private HelperPathProvider helper;
        private ServiceApiTheHive service;

        public EditarPerfilController(HelperPathProvider helper, ServiceApiTheHive service)
        {
            this.helper = helper;
            this.service = service;

        }



        public async Task<IActionResult> EditarPerfil()
        {

            var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

            HttpContext.Session.Remove("OtherUser");

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Inicio");
            }

            Usuario user = await this.service.FindUsuario(currentUser.Username);


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditarPerfil(Usuario usuario, IFormFile FotoPerfil)
        {

            var currentUser = HttpContext.Session.GetObject<Usuario>("CurrentUser");

            string nuevaFotoPerfil = null;


            if (FotoPerfil != null && FotoPerfil.Length > 0)
            {

                string fileName = "img" + currentUser.Username + ".jpeg";

                string path = this.helper.MapPath(fileName, Folders.Usuarios);
                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    await FotoPerfil.CopyToAsync(stream);
                }

                nuevaFotoPerfil = fileName;
            }


            Usuario perfilActualizado = new Usuario
            {
                Username = currentUser.Username,
                Nombre = usuario.Nombre,
                Password = currentUser.Password,
                Salt = currentUser.Salt,
                Email = usuario.Email,
                Descripcion = usuario.Descripcion ?? "",
                Telefono = usuario.Telefono ?? "",
                Rol = currentUser.Rol,
                FotoPerfil = !string.IsNullOrEmpty(nuevaFotoPerfil) ? nuevaFotoPerfil : currentUser.FotoPerfil
            };

            await this.service.UpdateUsuario(perfilActualizado);


            var username = currentUser.Username;

            HttpContext.Session.Remove("CurrentUser");
            Usuario user = await this.service.FindUsuario(username);
            HttpContext.Session.SetObject("CurrentUser", user);

            return RedirectToAction("Index", "Home");
        }

    }
}
