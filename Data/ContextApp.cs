using Microsoft.EntityFrameworkCore;
using TheHiveAzure.Models;

namespace TheHiveAzure.Data
{
    public class ContextApp : DbContext
    {
        public ContextApp(DbContextOptions<ContextApp> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Publicacion> Publicaciones { get; set; }
        public DbSet<Seguidores> Seguidores { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Like>().HasKey(l => new { l.IdPublicacion, l.Username });
        }


    }
}