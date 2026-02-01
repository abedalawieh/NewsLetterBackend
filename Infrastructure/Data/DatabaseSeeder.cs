using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Infrastructure.Data
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly NewsletterDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DatabaseSeeder(
            NewsletterDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();

            #region Roles Seeding

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Admin", Description = "Administrator" });
            
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new ApplicationRole { Name = "User", Description = "Standard User" });

            #endregion

            #region Admin User Seeding

            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@newsletter.com",
                    FirstName = "System",
                    LastName = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                    IsActive = true
                };
                await _userManager.CreateAsync(adminUser, "P@$$w0rd1234");
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }

            #endregion

            #region Metadata Lookups Seeding

            if (!await _context.LookupCategories.AnyAsync())
            {
                var catSubscriberType = LookupCategory.Create("SubscriberType", "Classification of subscribers", isSystem: true);
                var catCommMethod = LookupCategory.Create("CommunicationMethod", "Preferred way to reach subscribers", isSystem: true);
                var catInterest = LookupCategory.Create("Interest", "Topics of interest", isSystem: true);

                _context.LookupCategories.AddRange(catSubscriberType, catCommMethod, catInterest);
                await _context.SaveChangesAsync();

                _context.LookupItems.AddRange(
                    LookupItem.Create(catSubscriberType.Id, "HomeBuilder", "Home Builder", 1, isSystem: true),
                    LookupItem.Create(catSubscriberType.Id, "HomeBuyer", "Home Buyer", 2, isSystem: true),
                    
                    LookupItem.Create(catCommMethod.Id, "Email", "Email", 1, isSystem: true),
                    LookupItem.Create(catCommMethod.Id, "SMS", "SMS", 2, isSystem: true),
                    LookupItem.Create(catCommMethod.Id, "Phone", "Phone", 3, isSystem: true),
                    LookupItem.Create(catCommMethod.Id, "Post", "Post", 4, isSystem: true),

                    LookupItem.Create(catInterest.Id, "Houses", "Houses", 1, isSystem: true),
                    LookupItem.Create(catInterest.Id, "Apartments", "Apartments", 2, isSystem: true),
                    LookupItem.Create(catInterest.Id, "SharedOwnership", "Shared Ownership", 3, isSystem: true),
                    LookupItem.Create(catInterest.Id, "Rental", "Rental", 4, isSystem: true),
                    LookupItem.Create(catInterest.Id, "LandSourcing", "Land Sourcing", 5, isSystem: true)
                );
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update existing core items to be system items if they aren't already
                var systemCategoryNames = new[] { "SubscriberType", "CommunicationMethod", "Interest" };
                var systemCategories = await _context.LookupCategories
                    .IgnoreQueryFilters()
                    .Where(c => systemCategoryNames.Contains(c.Name))
                    .Include(c => c.Items)
                    .ToListAsync();

                foreach (var cat in systemCategories)
                {
                    cat.IsSystem = true;
                    foreach (var item in cat.Items)
                    {
                        item.IsSystem = true;
                    }
                }
                await _context.SaveChangesAsync();
            }

            #endregion
        }
    }
}
