using Fundo.Applications.Domain.Common;
using Fundo.Applications.Domain.Entities;
using Fundo.Applications.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fundo.Applications.Infrastructure.Persistance
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IMediator _mediator;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator = null) 
            : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Loans> Loans { get; set; }
        public DbSet<LoanStates> LoanStates { get; set; }
        public DbSet<Payments> Payments { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
          
            UpdateEntitiesTimestamps();

           
            int result = await base.SaveChangesAsync(cancellationToken);

       
            if (_mediator == null) return result;

     
            await PublishDomainEvents(cancellationToken);

            return result;
        }

        private void UpdateEntitiesTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Entity<int> && 
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            var utcNow = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var entity = (Entity<int>)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = utcNow;
                }

                entity.UpdatedAt = utcNow;
            }
        }

        private async Task PublishDomainEvents(CancellationToken cancellationToken)
        {
        
            var entitiesWithEvents = ChangeTracker
                .Entries<Entity<int>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();


            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();
            
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

         
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Payments>();
            modelBuilder.Ignore<IDomainEvent>();
    
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId);
            });

            modelBuilder.Entity<Loans>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Applicant)
                      .WithMany()
                      .HasForeignKey(e => e.ApplicantId);
                
                entity.HasOne(e => e.Status)
                      .WithMany()
                      .HasForeignKey(e => e.StatusId);

               
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<LoanStates>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

    

            base.OnModelCreating(modelBuilder);
        }
    }
}
