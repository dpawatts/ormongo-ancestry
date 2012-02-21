using System;
using System.Linq;

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

		public static IQueryable<T> BeforeRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return items.BeforeDepth(items.Depth + relativeDepth);
		}

		public static IQueryable<T> ToDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth <= depth);
		}

		public static IQueryable<T> ToRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return items.ToDepth(items.Depth + relativeDepth);
		}

		public static IQueryable<T> AtDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth == depth);
		}

		public static IQueryable<T> AtRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return items.AtDepth(items.Depth + relativeDepth);
		}

		public static IQueryable<T> FromDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth >= depth);
		}

		public static IQueryable<T> FromRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return items.FromDepth(items.Depth + relativeDepth);
		}

		public static IQueryable<T> AfterDepth<T>(this IQueryable<T> items, int depth)
			where T : AncestryDocument<T>
		{
			ValidateDepthCaching<T>();
			return items.Where(d => d.AncestryDepth > depth);
		}

		public static IQueryable<T> AfterRelativeDepth<T>(this IDepthQueryable<T> items, int relativeDepth)
			where T : AncestryDocument<T>
		{
			return items.AfterDepth(items.Depth + relativeDepth);
		}

		private static void ValidateDepthCaching<T>() 
			where T : AncestryDocument<T>
		{
			if (!AncestryDocument<T>.CacheDepth)
				throw new Exception("Depth queries are only available when depth caching is enabled.");
		}

		// scope :ancestors_of, lambda { |object| where(to_node(object).ancestor_conditions) }

		//public static IQueryable<T> AncestorsOf(T item)
		//{
		//    return Document<T>.Find(i => item.Ancestry.AncestorIDs.Contains(i.ID));
		//}
	}
}