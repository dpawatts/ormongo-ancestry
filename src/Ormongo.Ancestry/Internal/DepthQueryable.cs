using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Ormongo.Ancestry.Internal
{
	internal class DepthQueryable<T> : IDepthQueryable<T>
	{
		private readonly IQueryable<T> _wrapped;
		private readonly int _depth;

		public DepthQueryable(IQueryable<T> wrapped, int depth)
		{
			_wrapped = wrapped;
			_depth = depth;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _wrapped.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Expression Expression
		{
			get { return _wrapped.Expression; }
		}

		public Type ElementType
		{
			get { return _wrapped.ElementType; }
		}

		public IQueryProvider Provider
		{
			get { return _wrapped.Provider; }
		}

		public int Depth
		{
			get { return _depth; }
		}
	}
}