

namespace BlogApplication.Migrations.ApplicationDbContext
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<BlogApplication.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\ApplicationDbContext";
        }

        protected override void Seed(BlogApplication.Models.ApplicationDbContext context)
        {
            //This method will be called after migrating to the latest version.

            //You can use the DbSet<T>.AddOrUpdate() helper extension method
            //to avoid creating duplicate seed data.E.g.

            //context.People.AddOrUpdate(
            //  p => p.FullName,
            //  new Person { FullName = "Kenneth Villafuerte" },
            //  new Person { FullName = "Test Smith" },
            //  new Person { FullName = "Another Test" }
            //);


            //context.Roles.AddOrUpdate(r => r.Name,
            //    new IdentityRole { Name = "Admin" },
            //    new IdentityRole { Name = "Senior" },
            //    new IdentityRole { Name = "Moderator" },
            //    new IdentityRole { Name = "Member" },
            //    new IdentityRole { Name = "Junior" },
            //    new IdentityRole { Name = "Candidate" }
            //    );

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            string[] roleNames = { "Admin", "Member", "Candidate" };
            IdentityResult roleResult;
            foreach (var roleName in roleNames)
            {
                // We use RoleManager.RoleExists() method to check if role exists. If not create role by calling
                // RoleManager.Create() method. 
                //role is our level of access in this case
                if (!RoleManager.RoleExists(roleName))
                {
                    roleResult = RoleManager.Create(new IdentityRole(roleName));
                }
            }

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            UserManager.AddToRole("70c7a0c3-0bcf-4360-9267-1ee482d9ab88", "Admin");

        }
    }
}
