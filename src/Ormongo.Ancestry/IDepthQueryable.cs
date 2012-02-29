using System.Linq;

namespace Ormongo.Ancestry
{
	public interface IDepthQueryable<out T> : IQueryable<T>
	{
		int Depth { get; }
	}
}