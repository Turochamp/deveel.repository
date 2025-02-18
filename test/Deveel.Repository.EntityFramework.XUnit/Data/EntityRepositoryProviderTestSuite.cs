﻿using Bogus;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(SqlTenantConnectionCollection))]
    public class EntityRepositoryProviderTestSuite : RepositoryTestSuite<DbTenantPerson> {
        private readonly SqliteConnection sqliteConnection;

        public EntityRepositoryProviderTestSuite(SqlTestConnection sql, ITestOutputHelper outputHelper) : base(outputHelper) {
            this.sqliteConnection = sql.Connection;

			TenantInfo = new TenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test",
				Name = "Test Tenant",
				ConnectionString = sqliteConnection.ConnectionString
			};

			PersonFaker = new DbTenantPersonFaker(TenantInfo.Id);
        }

        protected override Faker<DbTenantPerson> PersonFaker { get; }

        protected TenantInfo TenantInfo { get; }

		protected string TenantId => TenantInfo.Id;

        protected IRepositoryProvider<DbTenantPerson> RepositoryProvider 
			=> Services.GetRequiredService<IRepositoryProvider<DbTenantPerson>>();

        protected override IRepository<DbTenantPerson> Repository => RepositoryProvider.GetRepository(TenantId);

		protected override IEnumerable<DbTenantPerson> NaturalOrder(IEnumerable<DbTenantPerson> source) => source.OrderBy(x => x.Id);

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(TenantInfo);
				});

			services.AddDbContext<DbContext, TestDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
				builder.UseSqlite(sqliteConnection);
				builder.LogTo(TestOutput!.WriteLine);
			})
				.AddRepositoryProvider<EntityRepositoryProvider<DbTenantPerson, TestDbContext>>();

			// AddRepository(services);

			base.ConfigureServices(services);
		}

		protected override async Task InitializeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<TestDbContext>>();
			using var dbContext = new TestDbContext(TenantInfo, options);

			await dbContext.Database.EnsureDeletedAsync();
			await dbContext.Database.EnsureCreatedAsync();

			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<TestDbContext>>();
			using var dbContext = new TestDbContext(TenantInfo, options);

			dbContext.People!.RemoveRange(dbContext.People);
			await dbContext.SaveChangesAsync(true);

			await dbContext.Database.EnsureDeletedAsync();
		}

		protected class TestDbContext : MultiTenantDbContext {
            public TestDbContext(ITenantInfo tenantInfo, DbContextOptions<TestDbContext> options) : base(tenantInfo, options) {
            }

			public DbSet<DbTenantPerson> People { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder) {
                modelBuilder.Entity<DbTenantPerson>()
                    .HasMany(x => x.Relationships)
                    .WithOne(x => x.Person)
                    .HasForeignKey(x => x.PersonId);

                modelBuilder.Entity<DbTenantPerson>()
                    .IsMultiTenant();

                modelBuilder.Entity<DbTenantPersonRelationship>();

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
