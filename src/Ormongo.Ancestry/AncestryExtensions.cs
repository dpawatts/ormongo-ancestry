using System;
using System.Linq;
using Ormongo.Ancestry.Internal;

namespace Ormongo.Ancestry
{
	public static class AncestryExtensions
	{
		public static IQueryable<T> Roots<T>(this IQueryable<T> items)
			where T : AncestryDocument<T>
		{
			return items.Where(d => d.Ancestry == null);
		}

		public static IQueryable<T> BeforeDepth<T>(this IQueryable<T> items, int depth) 
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth < depth);
		}

		public static IDepthQueryable<T> BeforeRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return new DepthQueryable<T>(items.BeforeDepth(items.Depth + relativeDepth), items.Depth);
		}

		public static IQueryable<T> ToDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth <= depth);
		}

		public static IDepthQueryable<T> ToRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return new DepthQueryable<T>(items.ToDepth(items.Depth + relativeDepth), items.Depth);
		}

		public static IQueryable<T> AtDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth == depth);
		}

		public static IDepthQueryable<T> AtRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return new DepthQueryable<T>(items.AtDepth(items.Depth + relativeDepth), items.Depth);
		}

		public static IQueryable<T> FromDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth >= depth);
		}

		public static IDepthQueryable<T> FromRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return new DepthQueryable<T>(items.FromDepth(items.Depth + relativeDepth), items.Depth);
		}

		public static IQueryable<T> AfterDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth > depth);
		}

		public static IDepthQueryable<T> AfterRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return new DepthQueryable<T>(items.AfterDepth(items.Depth + relativeDepth), items.Depth);
		}

		private static void ValidateDepthCaching<T>() 
			where T : AncestryDocument<T>
		{
			if (!AncestryDocument<T>.CacheDepth)
				throw new Exception("Depth queries are only available when depth caching is enabled.");
		}
	}
}