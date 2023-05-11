﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;

namespace Deveel.Data {
    public class InMemoryRepository<TEntity> : 
		IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IMultiTenantRepository
		where TEntity : class {
		private readonly List<TEntity> entities;
		private readonly IEntityFieldMapper<TEntity>? fieldMapper;

		public InMemoryRepository(IEnumerable<TEntity>? list = null, IEntityFieldMapper<TEntity>? fieldMapper = null) {
			entities = list == null ? new List<TEntity>() : new List<TEntity>(list);
			this.fieldMapper = fieldMapper;
		}

		internal InMemoryRepository(string tenantId, IEnumerable<TEntity>? list = null, IEntityFieldMapper<TEntity>? fieldMapper = null)
			: this(list, fieldMapper) {
			TenantId = tenantId;
		}

		Type IRepository.EntityType => typeof(TEntity);

		IQueryable<TEntity> IQueryableRepository<TEntity>.AsQueryable() => entities.AsQueryable();

		public IReadOnlyList<TEntity> Entities => entities.AsReadOnly();

		string? IMultiTenantRepository.TenantId => TenantId;

		protected virtual string? TenantId { get; }

		private static TEntity Assert(object entity) {
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			if (!(entity is TEntity t))
				throw new ArgumentException($"The type '{entity.GetType()}' is not assignable from {typeof(TEntity)}", nameof(entity));

			return t;
		}

		public virtual string? GetEntityId(TEntity entity) {
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			if (entity.TryGetMemberValue("Id", out object? idValue))
				return null;

			string? id;

			if (idValue is string) {
				id = (string)idValue;
			} else {
				id = Convert.ToString(idValue, CultureInfo.InvariantCulture);
			}

			// TODO: try some other members?

			return id;
		}

		string? IRepository.GetEntityId(object entity) => GetEntityId(Assert(entity));

		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				return Task.FromResult(entities.AsQueryable().LongCount(lambda));
			} catch (Exception ex) {
				throw new RepositoryException("Could not count the entities", ex);
			}
		}

		public Task<string> CreateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var id = Guid.NewGuid().ToString();
				if (!entity.TrySetMemberValue("Id", id))
					throw new RepositoryException("Unable to set the ID of the entity");

				entities.Add(entity);

				return Task.FromResult(id);
			} catch (RepositoryException) {

				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not create the entity", ex);
			}
		}

		public Task<IList<string>> CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var result = new List<string>();

				foreach (var item in entities) {
					var id = Guid.NewGuid().ToString();
					if (!item.TrySetMemberValue("Id", id))
						throw new RepositoryException("Unable to set the ID of the entity");

					this.entities.Add(item);

					result.Add(id);
				}

				return Task.FromResult<IList<string>>(result);
			} catch (RepositoryException) {

				throw;
			} catch(Exception ex) {
				throw new RepositoryException("Could not add the entities to the repository", ex);
			}
		}

		Task<string> IRepository.CreateAsync(object entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository.CreateAsync(IEnumerable<object> entities, CancellationToken cancellationToken)
			=> CreateAsync(entities.Select(Assert), cancellationToken);

		public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				return Task.FromResult(entities.Remove(entity));
			} catch (RepositoryException) {

				throw;
			} catch(Exception ex) {
				throw new RepositoryException("Could not delete the entity", ex);
			}
		}

		Task<bool> IRepository.DeleteAsync(object entity, CancellationToken cancellationToken) 
			=> DeleteAsync(Assert(entity), cancellationToken);

		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.AsQueryable().Any(lambda);
				return Task.FromResult(result);
			} catch(Exception ex) {
				throw new RepositoryException("Could not check if any entities exist in the repository", ex);
			}
		}

		public Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.AsQueryable().Where(lambda).ToList();
				return Task.FromResult<IList<TEntity>>(result);
			} catch (Exception ex) {

				throw new RepositoryException("Error while trying to find all the entities in the repository matching the filter", ex);
			}
		}

		public Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.AsQueryable().FirstOrDefault(lambda);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Error while searching for any entities in the repository matching the filter", ex);
			}
		}

		public Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
			if (string.IsNullOrWhiteSpace(id)) 
				throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entity = entities.FirstOrDefault(x => x.TryGetMemberValue<string>("Id", out var entityId) && entityId == id);
				return Task.FromResult(entity);
			} catch (Exception ex) {
				throw new RepositoryException("Error while searching any entities with the given ID", ex);
			}
		}

		private Expression<Func<TEntity, object>> MapField(IFieldRef fieldRef) {
			if (fieldRef is ExpressionFieldRef<TEntity> expRef)
				return expRef.Expression;

			if (fieldRef is StringFieldRef fieldName)
				return MapField(fieldName.FieldName);

			throw new NotSupportedException();
		}

		protected virtual Expression<Func<TEntity, object>> MapField(string fieldName) {
			if (fieldMapper == null)
				throw new NotSupportedException("No field mapper was provided");

			return fieldMapper.Map(fieldName);
		}

		public Task<RepositoryPage<TEntity>> GetPageAsync(RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entitySet = entities.AsQueryable();
				if (request.Filter != null)
					entitySet = entitySet.Where(request.Filter);

				if (request.ResultSorts != null) {
					foreach (var sort in request.ResultSorts) {
						if (sort.Ascending) {
							entitySet = entitySet.OrderBy(MapField(sort.Field));
						} else {
							entitySet = entitySet.OrderByDescending(MapField(sort.Field));
						}
					}
				}

				var itemCount = entitySet.Count();
				var items = entitySet.Skip(request.Offset).Take(request.Size);

				var result = new RepositoryPage<TEntity>(request, itemCount,items);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to retrieve the page", ex) ;
			}
		}


		async Task<RepositoryPage> IPageableRepository.GetPageAsync(RepositoryPageRequest request, CancellationToken cancellationToken) {
			var pageRequest = new RepositoryPageRequest<TEntity>(request.Page, request.Size) {
				Filter = request.Filter?.AsLambda<TEntity>()
			};

			var result = await GetPageAsync(pageRequest, cancellationToken);

			return new RepositoryPage(request, result.TotalItems, result.Items?.Cast<object>());
		}
		
		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				if (!entity.TryGetMemberValue<string>("Id", out var entityId))
					return Task.FromResult(false);

				var oldIndex = entities.FindIndex(x => x.TryGetMemberValue<string>("Id", out var id) && id == entityId);
				if (oldIndex < 0)
					return Task.FromResult(false);

				entities[oldIndex] = entity;
				return Task.FromResult(true);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to update the entity", ex);
			}
		}
				
		Task<bool> IRepository.UpdateAsync(object entity, CancellationToken cancellationToken) 
			=> UpdateAsync(Assert(entity), cancellationToken);
				
		async Task<IList<object>> IFilterableRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
			return (await FindAllAsync(filter, cancellationToken)).Cast<object>().ToList();
		}
		
		async Task<object?> IFilterableRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await FindAsync(filter, cancellationToken);
		
		async Task<object?> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);
	}
}
