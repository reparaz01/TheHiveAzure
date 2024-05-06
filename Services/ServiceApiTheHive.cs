using TheHiveAzure.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using TheHiveAzure.Helpers;
using ApiCoreTheHive.Models;

namespace TheHiveAzure.Services
{
    public class ServiceApiTheHive
    {
        private string UrlApiTheHive;
        private MediaTypeWithQualityHeaderValue Header;
        //OBJETO PARA RECUPERAR HttpContext Y EL User Y SU Claim
        private IHttpContextAccessor httpContextAccessor;

        public ServiceApiTheHive
            (IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.UrlApiTheHive =
                configuration.GetValue<string>("ApiUrls:ApiTheHiveLocal");
            this.Header =
                new MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/OAuth/Login";
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    Username = username,
                    Password = password
                };
                string jsonData = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(jsonData, Encoding.UTF8,
                    "application/json");
                HttpResponseMessage response = await
                    client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                    return default(T);
            }
        }

        //TENDREMOS UN METODO GENERICO QUE RECIBIRA EL REQUEST 
        //Y EL TOKEN
        private async Task<T> CallApiAsync<T> (string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }


        #region PUBLICACIONES

        public async Task<List<Publicacion>> GetPublicaciones()
        {
            string request = "api/Publicaciones/GetPublicaciones";

            List<Publicacion> publicaciones = await this.CallApiAsync<List<Publicacion>>(request);
            return publicaciones;
        }

        public async Task <List<Publicacion>> GetPublicacionesExceptoUsuario(string username)
        {
            string request = "api/Publicaciones/GetPublicacionesExceptoUsuario/" + username;

            List<Publicacion> publicaciones = await this.CallApiAsync<List<Publicacion>>(request);
            return publicaciones;
        }


        public async Task<List<Publicacion>> GetPublicacionesUsuario(string username)
        {
            string request = "api/Publicaciones/GetPublicacionesUsuario/" + username;
            return await CallApiAsync<List<Publicacion>>(request);
        }

        public async Task<List<Publicacion>> GetPublicacionesSeguidos(string username)
        {
            string request = "api/Publicaciones/GetPublicacionesSeguidos/" + username;
            return await CallApiAsync<List<Publicacion>>(request);
        }

        public async Task<int> GetNextPublicacionId()
        {
            string request = "api/Publicaciones/GetNextPublicacionId";

            return await CallApiAsync<int>(request);
        }

        public async Task AddPublicacion(Publicacion publicacion)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Publicaciones/AddPublicacion";
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                // Obtener el token del claim
                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    // Manejar el caso en el que no se encuentre el claim del token
                    // Aquí puedes lanzar una excepción, redirigir al usuario, etc.
                    return;
                }
                string token = tokenClaim.Value;

                // Agregar el token como encabezado de autorización
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                // Crear el objeto Publicacion
                Publicacion nuevaPublicacion = new Publicacion
                {
                    Texto = publicacion.Texto,
                    Imagen = publicacion.Imagen,
                    FotoPerfil = publicacion.FotoPerfil,
                    FechaPublicacion = publicacion.FechaPublicacion,
                    TipoPublicacion = publicacion.TipoPublicacion,
                    Likeado = false,
                    Username = publicacion.Username
                };

                // Serializar el objeto Publicacion a JSON
                string json = JsonConvert.SerializeObject(nuevaPublicacion);

                // Configurar el contenido de la solicitud
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Realizar la solicitud POST al servidor
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }



        public async Task DeletePublicacion(int idPublicacion)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Publicaciones/DeletePublicacion/" + idPublicacion;
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                // Obtener el token del claim
                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {

                    return;
                }
                string token = tokenClaim.Value;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = await client.DeleteAsync(request);
            }
        }

        #endregion

        #region LIKES
        public async Task<int> GetLikesPublicacion(int idPublicacion)
        {
            string request = "api/Likes/GetLikesPublicacion/" + idPublicacion;
            return await CallApiAsync<int>(request);
        }

        public async Task<bool> IsLiked(int idPublicacion, string username)
        {
            string request = "api/Likes/IsLiked/" + idPublicacion + "/" +  username;
            return await CallApiAsync<bool>(request);
        }

        public async Task Like(int idPublicacion)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Likes/Like/" + idPublicacion;
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    return;
                }
                string token = tokenClaim.Value;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = await client.PostAsync(request, null);
            }
        }

        public async Task Dislike(int idPublicacion)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Likes/Dislike/" + idPublicacion;
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    return;
                }
                string token = tokenClaim.Value;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = await client.DeleteAsync(request);
            }
        }

        #endregion

        #region SEGUIDORES

        public async Task<List<Usuario>> GetSeguidores(string username)
        {
            string request = "api/Seguidores/GetSeguidores/" + username;
            return await CallApiAsync<List<Usuario>>(request);
        }

        public async Task<List<Usuario>> GetSeguidos(string username)
        {
            string request = "api/Seguidores/GetSeguidos/" + username;
            return await CallApiAsync<List<Usuario>>(request);
        }

        public async Task<int> GetSeguidoresCount(string username)
        {
            string request = "api/Seguidores/GetSeguidoresCount/" + username;
            return await CallApiAsync<int>(request);
        }

        public async Task<int> GetSeguidosCount(string username)
        {
            string request = "api/Seguidores/GetSeguidosCount/" + username;
            return await CallApiAsync<int>(request);
        }

        public async Task<bool> IsFollowing(string username)
        {
            string request = "api/Seguidores/IsFollowing/" + username;
            return await CallApiAsync<bool>(request);
        }


        public async Task Follow(string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Seguidores/Follow/" + username;
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    return;
                }
                string token = tokenClaim.Value;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = await client.PostAsync(request, null);
            }
        }

        public async Task Unfollow(string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Seguidores/Unfollow/" + username;
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    return;
                }
                string token = tokenClaim.Value;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = await client.DeleteAsync(request);
            }
        }

        #endregion

        #region USUARIOS

        public async Task<List<Usuario>> GetUsuarios()
        {
            string request = "api/Usuarios/GetUsuarios";
            return await CallApiAsync<List<Usuario>>(request);
        }

        public async Task<Usuario> FindUsuario(string username)
        {
            string request = "api/Usuarios/FindUsuario/" + username;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                HttpResponseMessage response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Usuario usuario = await response.Content.ReadAsAsync<Usuario>();
                    return usuario;
                }
                else
                {
                    return null;
                }
            }
        }


        public async Task<bool> UsuarioExists(string username)
        {
            string request = "api/Usuarios/UsuarioExists/" + username;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                HttpResponseMessage response = await client.GetAsync(request);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> EmailExists(string email)
        {
            string request = "api/Usuarios/EmailExists/" + email;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                HttpResponseMessage response = await client.GetAsync(request);
                return response.IsSuccessStatusCode;
            }
        }


        public async Task AddUsuario(NuevoUsuario usuario)
        {
            string request = "api/Usuarios/AddUsuario";

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                // Crear el objeto NuevoUsuario
                var nuevoUsuario = new
                {
                    Username = usuario.Username,
                    Password = usuario.Password,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email
                };

                // Serializar el objeto NuevoUsuario a JSON
                string json = JsonConvert.SerializeObject(nuevoUsuario);

                // Configurar el contenido de la solicitud
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Realizar la solicitud POST al servidor
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }


        public async Task UpdateUsuario(Usuario usuario)
        {
            string request = "api/Usuarios/UpdateUsuario";
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    // Manejar el caso en el que no se encuentre el claim del token
                    // Aquí puedes lanzar una excepción, redirigir al usuario, etc.
                    return;
                }
                string token = tokenClaim.Value;

                // Agregar el token como encabezado de autorización
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                // Crear el objeto Usuario
                var usuarioObj = new
                {
                    Username = usuario.Username,
                    Nombre = usuario.Nombre,
                    Password = usuario.Password,
                    Salt = usuario.Salt,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    Descripcion = usuario.Descripcion,
                    FotoPerfil = usuario.FotoPerfil,
                    Rol = usuario.Rol
                };

                // Serializar el objeto Usuario a JSON
                string json = JsonConvert.SerializeObject(usuarioObj);

                // Configurar el contenido de la solicitud
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Realizar la solicitud PUT al servidor
                HttpResponseMessage response = await client.PutAsync(request, content);
            }
        }



        public async Task DeleteUsuario(string username)
        {
            string request = "api/Usuarios/DeleteUsuario/" + username;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiTheHive);
                client.DefaultRequestHeaders.Clear();

                var tokenClaim = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN");
                if (tokenClaim == null)
                {
                    return;
                }
                string token = tokenClaim.Value;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = await client.DeleteAsync(request);
            }
        }

        #endregion

        #region BUSCADOR 

        public async Task<List<Usuario>> BuscarUsuarios(string query)
        {
            string request = "api/Buscador/BuscarUsuarios/" + query;

            return await CallApiAsync<List<Usuario>>(request);
        }

        #endregion

    }
}