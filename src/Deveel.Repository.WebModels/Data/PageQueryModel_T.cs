﻿using System.Linq.Expressions;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
    public abstract class PageQueryModel<TEntity> : PageRequestModelBase {
		public PageQueryModel(int page, int size)
		: base(page, size) {
		}

		public PageQueryModel() {
		}

		[MinValue(1), FromQuery(Name = "p")]
		public override int? Page { get => base.Page; set => base.Page = value; }

		[MaxValue(150), FromQuery(Name = "c")]
		public override int? Size { get => base.Size; set => base.Size = value; }

		[FromQuery(Name = "s")]
		public virtual List<string>? SortBy { get; set; }

		public virtual string? GetPageParameterName() {
			var attr = GetType().GetProperty(nameof(Page))?.GetCustomAttribute<FromQueryAttribute>(false);
			return attr?.Name;
		}

		public virtual string? GetSizeParameterName() {
			var attr = GetType().GetProperty(nameof(Size))?.GetCustomAttribute<FromQueryAttribute>(false);
			return attr?.Name;
		}

		public virtual string? GetSortParameterName() {
			var attr = GetType().GetProperty(nameof(SortBy))?.GetCustomAttribute<FromQueryAttribute>(false);
			return attr?.Name;
		}

		protected override void GetRouteValues(IDictionary<string, object> routeValues) {
			var pageParam = GetPageParameterName();
			if (!string.IsNullOrWhiteSpace(pageParam) && Page != null)
				routeValues[pageParam] = Page.Value;

			var sizeParam = GetSizeParameterName();
			if (!string.IsNullOrWhiteSpace(sizeParam) && Size != null)
				routeValues[sizeParam] = Size;

			var sortParam = GetSortParameterName();
			if (!string.IsNullOrEmpty(sortParam) && SortBy != null)
				routeValues[sortParam] = SortBy.ToArray();

			base.GetRouteValues(routeValues);
		}

		protected override IEnumerable<IResultSort>? GetResultSort() => SortBy?.Select(QueryStringResultSort.ParseSort);

		public virtual void CopyTo(PageQueryModel<TEntity>? page) {
			if (page == null)
				return;

			var flags = BindingFlags.Instance | BindingFlags.Public;

			var properties = GetType().GetProperties(flags)
				.Where(x => Attribute.IsDefined(x, typeof(FromQueryAttribute)));

			foreach (var property in properties) {
				var otherProperty = page.GetType()
					.GetProperty(property.Name, property.PropertyType);

				if (otherProperty != null && otherProperty.CanWrite)
					otherProperty.SetValue(page, property.GetValue(this, null));
			}
		}

		public virtual RepositoryPageRequest<TResult> ToPageRequest<TResult>() where TResult : class {
			return new RepositoryPageRequest<TResult>(Page ?? 1, GetPageSize()) {
				Filter = QueryFilter.Combine(PageFilters()),
				ResultSorts = PageSort()
			};
		}

	}
}
