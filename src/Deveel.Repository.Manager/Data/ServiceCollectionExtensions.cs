// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Deveel.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IServiceCollection"/> to provide methods
	/// to register the <see cref="EntityManager{TEntity}"/> service.
	/// </summary>
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Registers a <see cref="EntityManager{TEntity}"/> service
		/// in the given collection of services.
		/// </summary>
		/// <typeparam name="TManager">
		/// The type of the <see cref="EntityManager{TEntity}"/> to register.
		/// </typeparam>
		/// <param name="services">
		/// The collection of services to register the manager.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the manager.
		/// </param>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		/// <seealso cref="AddEntityManager(IServiceCollection, Type, ServiceLifetime)"/>
        public static IServiceCollection AddEntityManager<TManager>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TManager : class
            => AddEntityManager(services, typeof(TManager), lifetime);

		/// <summary>
		/// Adds and instance of <see cref="EntityManager{TEntity}"/> for the given
		/// entity type in the collection of services.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity to manage.
		/// </typeparam>
		/// <param name="services">
		/// The collection of services to register the manager.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the manager.
		/// </param>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		public static IServiceCollection AddManagerFor<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TEntity : class
			=> AddEntityManager(services, typeof(EntityManager<TEntity>), lifetime);

		/// <summary>
		/// Registers a <see cref="EntityManager{TEntity}"/> service
		/// in the given collection of services.
		/// </summary>
		/// <param name="services">
		/// The collection of services to register the manager.
		/// </param>
		/// <param name="managerType">
		/// The type of the <see cref="EntityManager{TEntity}"/> to register.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the manager.
		/// </param>
		/// <remarks>
		/// This method iterates through the base types of the given <paramref name="managerType"/>
		/// to find the <see cref="EntityManager{TEntity}"/> instances from
		/// which the service derives, and registers the service for each of them.
		/// </remarks>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when the given <paramref name="managerType"/> is not a concrete class,
		/// or if the type is not a valid <see cref="EntityManager{TEntity}"/>.
		/// </exception>
        public static IServiceCollection AddEntityManager(this IServiceCollection services, Type managerType, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
            ArgumentNullException.ThrowIfNull(managerType, nameof(managerType));

			if (!managerType.IsClass || managerType.IsAbstract)
				throw new ArgumentException($"The type {managerType} is not a concrete class", nameof(managerType));

			var baseType = managerType;
			while(baseType != null) {
				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(EntityManager<>)) {
					var entityType = baseType.GetGenericArguments()[0];
					var compareType = typeof(EntityManager<>).MakeGenericType(entityType);

					services.TryAdd(new ServiceDescriptor(compareType, managerType, lifetime));

					if (compareType != managerType)
						services.Add(new ServiceDescriptor(managerType, managerType, lifetime));

					break;
				}

				if (baseType == typeof(object))
					throw new ArgumentException($"The type {managerType} is not a valid manager type", nameof(managerType));

				baseType = baseType.BaseType;
			}

			return services;
        }

		/// <summary>
		/// Registers an entity validation service in the collection of services.
		/// </summary>
		/// <typeparam name="TValidator">
		/// The type of the entity validator to register.
		/// </typeparam>
		/// <param name="services">
		/// The collection of services to register the validator.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the validator.
		/// </param>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		/// <seealso cref="AddEntityValidator(IServiceCollection, Type, ServiceLifetime)"/>
		public static IServiceCollection AddEntityValidator<TValidator>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
			where TValidator : class => AddEntityValidator(services, typeof(TValidator), lifetime);

		/// <summary>
		/// Adds an entity validator service in the collection of services.
		/// </summary>
		/// <param name="services">
		/// The collection of services to register the validator.
		/// </param>
		/// <param name="validatorType"></param>
		/// <param name="lifetime"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static IServiceCollection AddEntityValidator(this IServiceCollection services, Type validatorType, ServiceLifetime lifetime = ServiceLifetime.Transient) {
			ArgumentNullException.ThrowIfNull(validatorType, nameof(validatorType));

			if (!validatorType.IsClass || validatorType.IsAbstract)
				throw new ArgumentException($"The type {validatorType} is not a concrete class", nameof(validatorType));

			var interfaceTypes = validatorType.GetInterfaces();
			foreach (var interfaceType in interfaceTypes) {
				if (interfaceType.GetGenericTypeDefinition() == typeof(IEntityValidator<>)) {
					var compareType = typeof(IEntityValidator<>).MakeGenericType(interfaceType.GetGenericArguments()[0]);

					services.TryAdd(new ServiceDescriptor(compareType, validatorType, lifetime));
				}
			}

			services.Add(new ServiceDescriptor(validatorType, validatorType, lifetime));

			return services;
		}
    }
}