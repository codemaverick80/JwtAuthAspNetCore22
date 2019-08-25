using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAspNetCore22.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region "Seed Data"
            //builder.Entity<IdentityRole>().HasData(
            //    new { Name="Admin", NormalizedName="ADMIN"},
            //    new { Name ="Customre", NormalizedName = "CUSTOMER" }
            //    );
            #endregion
        }

    }
}
