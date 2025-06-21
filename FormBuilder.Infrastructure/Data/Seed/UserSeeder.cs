using FormBuilder.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Data.Seed
{
    public static class UserSeeder
    {
        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = "admin-user-id",
                UserName = "admin@formbuilder.com",
                NormalizedUserName = "ADMIN@FORMBUILDER.COM",
                Email = "admin@formbuilder.com",
                NormalizedEmail = "ADMIN@FORMBUILDER.COM",
                EmailConfirmed = true,
                IsAdmin = true,
                Theme = "light",
                Language = "en",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Fixed date
                Version = 1,
                SecurityStamp = "INIT-SECURITY-STAMP", // Fixed Value
                ConcurrencyStamp = "INIT-CONCURRENCY"  // " " 
            };
            
            // Migration temp password
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "TempMigration123!");
            
            modelBuilder.Entity<User>().HasData(adminUser);
            
            // Admin role
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "admin-role-id",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "admin-role-stamp" // Fixed value
                },
                new IdentityRole
                {
                    Id = "user-role-id",
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "user-role-stamp" // " " 
                }
            );
            
            // Assign admin role to admin user
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUser.Id,
                    RoleId = "admin-role-id"
                }
            );
        }
    }
}