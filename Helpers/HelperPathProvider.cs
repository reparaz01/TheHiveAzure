using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace TheHiveAzure.Helpers
{
    public enum Folders { Publicaciones = 0, Usuarios = 1 , TheHive = 2}
    public class HelperPathProvider
    {
        //NECESITAMOS ACCEDER AL SISTEMA DE ARCHIVOS DEL WEB SERVER (wwwroot) 
        private IServer server;
        private readonly IWebHostEnvironment hostEnvironment;
        public HelperPathProvider(IWebHostEnvironment hostEnvironment, IServer server)
        {
            this.server = server;
              this.hostEnvironment = hostEnvironment;
        }

        public string GetFolderPath(Folders folder)
        {
            string carpeta = "";
            if (folder == Folders.Publicaciones)
            {
                carpeta = "publicaciones";
            }
            else if (folder == Folders.Usuarios)
            {
                carpeta = "usuarios";
            }
            else if (folder == Folders.TheHive)
            {
                carpeta = "thehive";
            }

            return carpeta;
        }
        public string MapUrlPath(string fileName, Folders folder)
        {
            string carpeta = this.GetFolderPath(folder);
            var addresses = server.Features.Get<IServerAddressesFeature>().Addresses;
            string serverUrl = addresses.FirstOrDefault();
            string urlPath = serverUrl + "/" + carpeta + "/" + fileName;
            return urlPath;
        }

        public string MapUrlServerPath()
        {
            var addresses = server.Features.Get<IServerAddressesFeature>().Addresses;
            string serverUrl = addresses.FirstOrDefault();
            return serverUrl;
        }



        public string MapPath(string fileName, Folders folder)
        {
            string carpeta = this.GetFolderPath(folder);
            string rootPath = this.hostEnvironment.WebRootPath;
            string path = Path.Combine(rootPath, carpeta, fileName);
            return path;
        }
    }
}