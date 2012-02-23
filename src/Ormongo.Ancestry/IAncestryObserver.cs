namespace Ormongo.Ancestry
{
	public interface IAncestryObserver<T> : IObserver<T>
		where T : Document<T>
	{
		bool BeforeMove(T document, T newParent);
		void AfterMove(T document, T newParent);
	}
}