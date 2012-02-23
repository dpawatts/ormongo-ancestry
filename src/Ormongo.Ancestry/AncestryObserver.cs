namespace Ormongo.Ancestry
{
	public class AncestryObserver<T> : Observer<T>, IAncestryObserver<T> 
		where T : Document<T>
	{
		public virtual bool BeforeMove(T document, T newParent)
		{
			return true;
		}

		public virtual void AfterMove(T document, T newParent)
		{
			
		}
	}
}