using Microsoft.EntityFrameworkCore;
using TestValetax.DB.Entities;

namespace TestValetax.DB
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<TreeNode> TreeNodes { get; set; }
        public DbSet<ExceptionJournal> ExceptionJournals { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<TreeNode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.ParentId, e.Name })
                      .IsUnique()
                      .HasDatabaseName("IX_NodeName_UniquePerParent");

                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.TreeName);

                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.TreeName).IsRequired();
            });


            modelBuilder.Entity<ExceptionJournal>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EventId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Timestamp)
                      .IsRequired();

                entity.Property(e => e.QueryParams)
                      .HasColumnType("jsonb");

                entity.Property(e => e.BodyParams)
                      .HasColumnType("jsonb");

                entity.Property(e => e.StackTrace)
                      .IsRequired();

                entity.Property(e => e.ExceptionType)
                      .IsRequired();
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Token);
                entity.Property(e => e.ExpiresAt).IsRequired();
            });
        }
    }
}