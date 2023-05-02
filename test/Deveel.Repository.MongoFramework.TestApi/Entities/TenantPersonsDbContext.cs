﻿using MongoFramework;

namespace Deveel.Data.Entities {
    public class TenantPersonsDbContext : MongoDbTenantContext {
        public TenantPersonsDbContext(IMongoDbTenantConnection<TenantPersonsDbContext> connection) 
            : base(connection, connection.TenantInfo.Id) {
        }

        protected override void OnConfigureMapping(MappingBuilder mappingBuilder) {
            mappingBuilder.Entity<PersonEntity>();
        }
    }
}
