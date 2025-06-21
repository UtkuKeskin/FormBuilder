using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FormBuilder.Core.Entities;
using FormBuilder.Infrastructure.Data.Seed;

namespace FormBuilder.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Template> Templates { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TemplateTag> TemplateTags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<TemplateAccess> TemplateAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure entity relationships
            ConfigureTemplate(builder);
            ConfigureForm(builder);
            ConfigureLike(builder);
            ConfigureTemplateTag(builder);
            ConfigureTemplateAccess(builder);
            ConfigureIndexes(builder);
            
            // Seed data
            TopicSeeder.SeedTopics(builder);
            UserSeeder.SeedUsers(builder);
        }

        // configuration methods
        private void ConfigureTemplate(ModelBuilder builder)
        {
            builder.Entity<Template>()
                .HasOne(t => t.User)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.UserId);

            builder.Entity<Template>()
                .HasMany(t => t.Forms)
                .WithOne(f => f.Template)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureForm(ModelBuilder builder)
        {
            builder.Entity<Form>()
                .HasOne(f => f.User)
                .WithMany(u => u.Forms)
                .HasForeignKey(f => f.UserId);
        }

        private void ConfigureLike(ModelBuilder builder)
        {
            builder.Entity<Like>()
                .HasKey(l => new { l.TemplateId, l.UserId });
        }

        private void ConfigureTemplateTag(ModelBuilder builder)
        {
            builder.Entity<TemplateTag>()
                .HasKey(tt => new { tt.TemplateId, tt.TagId });

            builder.Entity<TemplateTag>()
                .HasOne(tt => tt.Template)
                .WithMany(t => t.TemplateTags)
                .HasForeignKey(tt => tt.TemplateId);

            builder.Entity<TemplateTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TemplateTags)
                .HasForeignKey(tt => tt.TagId);
        }

        private void ConfigureTemplateAccess(ModelBuilder builder)
        {
            builder.Entity<TemplateAccess>()
                .HasKey(ta => new { ta.TemplateId, ta.UserId });
        }

        private void ConfigureIndexes(ModelBuilder builder)
        {
            builder.Entity<Template>()
                .HasIndex(t => t.CreatedAt);

            builder.Entity<Form>()
                .HasIndex(f => f.FilledAt);

            builder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}
