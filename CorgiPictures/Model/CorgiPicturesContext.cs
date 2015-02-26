using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace CorgiPictures.Model
{
    public class CorgiPicturesContext : DbContext
    {
        public CorgiPicturesContext()
            : base("CorgiPicturesContext")
        {
        }

        public DbSet<Picture> Pictures { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
