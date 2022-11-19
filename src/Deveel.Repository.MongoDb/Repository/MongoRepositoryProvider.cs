﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

using Deveel.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deveel.Data
{
    public class MongoRepositoryProvider<TEntity> : MongoStoreProvider<TEntity>, IRepositoryProvider<TEntity> where TEntity : class, IEntity
    {
        public MongoRepositoryProvider(
            IOptions<MongoDbOptions> baseOptions,
            IDocumentFieldMapper<TEntity>? fieldMapper = null,
            ITenantConnectionProvider? connectionProvider = null,
            ICollectionKeyProvider? collectionNameProvider = null,
            ILoggerFactory? loggerFactory = null)
            : base(baseOptions, connectionProvider, collectionNameProvider, loggerFactory)
        {
            FieldMapper = fieldMapper;
        }

        protected IDocumentFieldMapper<TEntity>? FieldMapper { get; }

        /// <inheritdoc />
        IRepository<TEntity> IRepositoryProvider<TEntity>.GetRepository(string tenantId)
        {
            return (MongoRepository<TEntity>)GetStore(tenantId);
        }

        IRepository IRepositoryProvider.GetRepository(string tenantId) => (MongoRepository<TEntity>)GetStore(tenantId);

        public MongoRepository<TEntity> GetRepository(string tenantId)
            => (MongoRepository<TEntity>)GetStore(tenantId);

        protected override MongoStore<TEntity> CreateStore(IOptions<MongoDbStoreOptions<TEntity>> options, ILogger<MongoStore<TEntity>> logger)
            => new MongoRepository<TEntity>(options, FieldMapper, logger);
    }
}
