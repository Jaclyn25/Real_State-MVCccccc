namespace RealState_Platform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Global query filter for soft delete
            builder.Entity<Property>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<PropertyImage>().HasQueryFilter(pi => !pi.IsDeleted);
            builder.Entity<Inquiry>().HasQueryFilter(i => !i.IsDeleted);
            builder.Entity<Favorite>().HasQueryFilter(f => !f.IsDeleted);
            builder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
