using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.Infrastructure.Persistance
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            try
            {
                if(context.Users.Any() && context.Loans.Any())
                {
                    logger.LogInformation("Database already initialized");
                    return;
                }
            }catch(Exception ex)
            {
                await context.Database.MigrateAsync();
                await SeedDataAsync(context);
                
                logger.LogInformation("Database initialization completed successfully");
            }
            
        }
        
        private static async Task SeedDataAsync(ApplicationDbContext context)
        {
   
            if (await context.Users.AnyAsync() || await context.Loans.AnyAsync())
            {
     
            }


            await SeedRolesAsync(context);
            

            await SeedLoanStatesAsync(context);
       
            var users = await SeedUsersAsync(context);
    
            await SeedLoansAsync(context, users);
            
            await context.SaveChangesAsync();
        }
        
        private static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Roles>
                {
                    new Roles("admin", "System administrator with full access"),
                    new Roles("user", "Default role for regular users")
                };
                
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }
        }
        
        private static async Task SeedLoanStatesAsync(ApplicationDbContext context)
        {
            if (!await context.LoanStates.AnyAsync())
            {
                var loanStates = new List<LoanStates>
                {
                    new LoanStates("ACTIVE") ,
                    new LoanStates("PAID") 
                };
                
                await context.LoanStates.AddRangeAsync(loanStates);
                await context.SaveChangesAsync();
            }
        }
        
        private static async Task<List<Users>> SeedUsersAsync(ApplicationDbContext context)
        {
            var adminRoleId = await context.Roles.Where(r => r.Name == "admin").Select(r => r.Id).FirstOrDefaultAsync();
            var userRoleId = await context.Roles.Where(r => r.Name == "user").Select(r => r.Id).FirstOrDefaultAsync();
            
        
            var users = new List<Users>
            {
     
                Users.CreateNew(
                    "admin@fundo.com", 
                    BCrypt.Net.BCrypt.HashPassword("Admin123!"), 
                    "Juan", 
                    "Roittman", 
                    adminRoleId
                ),
       
                Users.CreateNew(
                    "user1@fundo.com", 
                    BCrypt.Net.BCrypt.HashPassword("User123!"), 
                    "John", 
                    "Rojas", 
                    userRoleId
                ),
                
            
                Users.CreateNew(
                    "user2@fundo.com", 
                    BCrypt.Net.BCrypt.HashPassword("User123!"), 
                    "Pedro", 
                    "Perez", 
                    userRoleId
                )
            };
            
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            
            return users;
        }
        
        private static async Task SeedLoansAsync(ApplicationDbContext context, List<Users> users)
        {
       
            var random = new Random();
            var loans = new List<Loans>();
            
            // El administrador tendrá 2 préstamos
            var adminUser = users[0];
            

            var normalUser1 = users[1];
            var normalUser2 = users[2];
            

            for (int i = 0; i < 2; i++)
            {
                var amount = random.Next(1000, 10000);
                loans.Add(Loans.CreateNew(amount, adminUser.Id));
            }
            

            for (int i = 0; i < 4; i++)
            {
                var amount = random.Next(1000, 5000);
                loans.Add(Loans.CreateNew(amount, normalUser1.Id));
            }
            
      
            for (int i = 0; i < 4; i++)
            {
                var amount = random.Next(1000, 5000);
                loans.Add(Loans.CreateNew(amount, normalUser2.Id));
            }
            
            await context.Loans.AddRangeAsync(loans);
            await context.SaveChangesAsync();
            

            var paidLoansCount = 3; 
            var loanList = await context.Loans.ToListAsync();
            
            for (int i = 0; i < paidLoansCount && i < loanList.Count; i++)
            {
                var loanIndex = random.Next(loanList.Count);
                var loan = loanList[loanIndex];
                
         
                if (loan.StatusId != (int)LoanStatusesEnum.PAID && loan.CurrentBalance > 0)
                {
                  
                    Loans.DeductCurrentBalance(loan, loan.CurrentBalance);
                }
            }
            
            await context.SaveChangesAsync();
        }
    }
}
