﻿// Copyright 2023 Deveel AS
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

using MongoFramework;

namespace Deveel.Data {
	public sealed class MongoConnectionBuilder<TContext> : MongoConnectionBuilder where TContext : class, IMongoDbContext  {
		private readonly MongoConnectionBuilder builder;

		internal MongoConnectionBuilder(MongoConnectionBuilder builder) {
			this.builder = builder;
		}

		public override IMongoDbConnection Connection => new MongoDbConnection<TContext>(builder.Connection);
	}
}
