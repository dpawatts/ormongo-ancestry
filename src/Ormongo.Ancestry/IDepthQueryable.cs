using System.Linq;

namespace Ormongo.Ancestry
{
	public interface IDepthQueryable<T> : IQueryable<T>
	{
		int Depth { get; }
	}
}