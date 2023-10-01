﻿using System;

using Xunit.Abstractions;

namespace Deveel.Data {
    public class CreateTenantEntityTests : EntityFrameworkRepositoryProviderTestBase {
        public CreateTenantEntityTests(SqlTestConnection testCollection, ITestOutputHelper outputHelper) 
            : base(testCollection, outputHelper) {
        }

        [Fact]
        public async Task Repository_CreateNewPerson() {
            var person = GeneratePerson();

            var id = await Repository.AddAsync(person);

            Assert.NotNull(id);
            Assert.NotEmpty(id);
            Assert.Equal(id, person.Id.ToString());
            Assert.Equal(TenantId, person.TenantId);
        }
    }
}
