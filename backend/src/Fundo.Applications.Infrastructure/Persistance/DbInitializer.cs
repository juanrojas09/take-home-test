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
                logger.LogInformation("Starting database initialization");
                
                // Usar MigrateAsync para aplicar migraciones pendientes
                await context.Database.MigrateAsync();
                
                await SeedDataAsync(context);
                
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database");
                throw;
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
                    new Roles("admin", "Administrador del sistema"),
                    new Roles("user", "Usuario regular del sistema")
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
                    new LoanStates("ACTIVE") { Id = (int)LoanStatusesEnum.ACTIVE },
                    new LoanStates("PAID") { Id = (int)LoanStatusesEnum.PAID }
                };
                
                await context.LoanStates.AddRangeAsync(loanStates);
                await context.SaveChangesAsync();
            }
        }
        
        private static async Task<List<Users>> SeedUsersAsync(ApplicationDbContext context)
        {
            var adminRoleId = await context.Roles.Where(r => r.Name == "admin").Select(r => r.Id).FirstOrDefaultAsync();
            var userRoleId = await context.Roles.Where(r => r.Name == "user").Select(r => r.Id).FirstOrDefaultAsync();
            
            // Utilizamos el método factory CreateNew para crear usuarios
            var users = new List<Users>
            {
                // Usuario Administrador
                Users.CreateNew(
                    "admin@fundo.com", 
                    BCrypt.Net.BCrypt.HashPassword("Admin123!"), 
                    "Admin", 
                    "User", 
                    adminRoleId
                ),
                
                // Usuario Normal 1
                Users.CreateNew(
                    "user1@fundo.com", 
                    BCrypt.Net.BCrypt.HashPassword("User123!"), 
                    "Normal", 
                    "User One", 
                    userRoleId
                ),
                
                // Usuario Normal 2
                Users.CreateNew(
                    "user2@fundo.com", 
                    BCrypt.Net.BCrypt.HashPassword("User123!"), 
                    "Regular", 
                    "User Two", 
                    userRoleId
                )
            };
            
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            
            return users;
        }
        
        private static async Task SeedLoansAsync(ApplicationDbContext context, List<Users> users)
        {
            // Generar 10 préstamos distribuidos entre los usuarios
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
            
            // Crear 4 préstamos para el usuario normal 2
            for (int i = 0; i < 4; i++)
            {
                var amount = random.Next(1000, 5000);
                loans.Add(Loans.CreateNew(amount, normalUser2.Id));
            }
            
            await context.Loans.AddRangeAsync(loans);
            await context.SaveChangesAsync();
            
            // Marcar algunos préstamos como pagados después de guardarlos
            var paidLoansCount = 3; // Marcaremos 3 préstamos como pagados
            var loanList = await context.Loans.ToListAsync();
            
            for (int i = 0; i < paidLoansCount && i < loanList.Count; i++)
            {
                var loanIndex = random.Next(loanList.Count);
                var loan = loanList[loanIndex];
                
                // Deducimos todo el saldo para marcar como pagado
                Loans.DeductCurrentBalance(loan, loan.CurrentBalance);
            }
            
            await context.SaveChangesAsync();
        }
    }
}
