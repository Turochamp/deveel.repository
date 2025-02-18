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

using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a repository that is capable of segregating the
	/// data by the tenant that owns it.
	/// </summary>
    public interface IMultiTenantRepository<TEntity> : IRepository<TEntity> where TEntity : class {
		/// <summary>
		/// Gets the identifier of the tenant that owns the data
		/// </summary>
        string? TenantId { get; }
    }
}
