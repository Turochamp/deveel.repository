﻿using System;

namespace Deveel.Data {
	public class DeleteEntityTests : InMemoryRepositoryTestBase {
		private readonly IList<Person> people;

		public DeleteEntityTests() {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(IRepository repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Memory_DeleteExisting() {
			var entity = people[^1];

			var result = await InMemoryRepository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = people[^1];

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = people[^1];

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.True(result);
		}


		[Fact]
		public async Task Memory_DeleteNotExisting() {
			var entity = new Person { Id = Guid.NewGuid().ToString() };

			var result = await InMemoryRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new Person { Id = Guid.NewGuid().ToString() };

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new Person { Id = Guid.NewGuid().ToString() };

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Memory_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await InMemoryRepository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await Repository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await FacadeRepository.RemoveByIdAsync(id);

			Assert.True(result);
		}

		[Fact]
		public async Task Memory_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await InMemoryRepository.RemoveByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await Repository.RemoveByIdAsync(id);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_NotExisting() {
			var id = Guid.NewGuid().ToString();

			var result = await FacadeRepository.RemoveByIdAsync(id);

			Assert.False(result);
		}
	}
}
