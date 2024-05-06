using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheHiveAzure.Models;
using TheHiveAzure.Extensions;
using System.Text;
using TheHiveAzure.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TheHiveAzure.Services;
using ApiCoreTheHive.Models;

namespace TheHiveAzure.Controllers
{
    public class InicioController : Controller
    {
        private HelperCryptography helper;
        private ServiceApiTheHive service;

        public InicioController(ServiceApiTheHive service)
        {
            this.service = service;
        }

        public async Task<IActionResult> LoginAsync()
        {
            await HttpContext.SignOutAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {


            string token = await this.service.GetTokenAsync(username, password);

            if (token == null)
            {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("TOKEN", token);
                ClaimsIdentity identity =
                    new ClaimsIdentity
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , ClaimTypes.Name, ClaimTypes.Role);
                //ALMACENAMOS EL NOMBRE DE USUARIO (BONITO)
                identity.AddClaim
                    (new Claim(ClaimTypes.Name, username));
                //ALMACENAMOS EL ID DEL USUARIO
                identity.AddClaim
    (new Claim(ClaimTypes.NameIdentifier, password));
                identity.AddClaim
                    (new Claim("TOKEN", token));
                ClaimsPrincipal userPrincipal =
                    new ClaimsPrincipal(identity);
                //DAMOS DE ALTA AL USUARIO INDICANDO QUE 
                //ESTARA VALIDADO DURANTE 30 MINUTOS
                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , userPrincipal, new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });
                Usuario user = await this.service.FindUsuario(username);
                HttpContext.Session.SetObject("CurrentUser", user);

                return RedirectToAction("Index", "Home");
            }


               

        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(string username, string nombre, string password, string confirmPassword, string email)
        {
            if (await this.service.UsuarioExists(username))
            {
                ViewData["Mensaje"] = "El nombre de usuario ya está en uso.";
                return View();
            }

            if (!string.IsNullOrEmpty(email) && await this.service.EmailExists(email))
            {
                ViewData["Mensaje"] = "Este correo electrónico ya está en uso.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden.";
                return View();
            }

            NuevoUsuario nuevoUsuario = new NuevoUsuario();

            nuevoUsuario.Username = username;
            nuevoUsuario.Password = password;
            nuevoUsuario.Email = email; 
            nuevoUsuario.Nombre = nombre;

            await this.service.AddUsuario(nuevoUsuario);


            Usuario user =await this.service.FindUsuario(username);
            HttpContext.Session.SetObject("CurrentUser", user);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("CurrentUser");
            return RedirectToAction("Login");
        }
    }
}
