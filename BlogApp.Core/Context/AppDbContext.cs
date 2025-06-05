using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BlogApp.Core.Context
{
    public interface IAppDbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync();
        DatabaseFacade GetDatabaseFacade();
    }

    public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tag>()
                   .HasIndex(t => t.Name)
                   .IsUnique();

            builder.Entity<BlogTag>()
                   .HasIndex(bt => new { bt.BlogId, bt.TagId })
                   .IsUnique();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
        public DatabaseFacade GetDatabaseFacade()
        {
            var connectionString = this.Database.GetDbConnection().ConnectionString;
            var canConnect = this.Database.CanConnect();

            return this.Database;
        }
    }
}
