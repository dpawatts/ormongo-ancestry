using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Ormongo.Ancestry.Internal;

namespace Ormongo.Ancestry
{
	public abstract class AncestryDocument<T> : Document<T>
		where T : AncestryDocument<T>
	{
		#region Static

		/// <summary>
		/// Configures what to do with children of a node that is destroyed. Defaults to Destroy.
		/// </summary>
		public static OrphanStrategy OrphanStrategy { get; set; }

		/// <summary>
		/// Cache the depth of each node in the ExtraData.AncestryDepth field. Defaults to false.
		/// </summary>
		public static bool CacheDepth { get; set; }

		#endregion

		#region Instance

		#region Persisted

		public string Ancestry { get; set; }
		public int AncestryDepth { get; set; }

		#endregion

		/// <summary>
		/// The ancestry value for this record's children
		/// </summary>
		internal string ChildAncestry
		{
			get
			{
				// New records cannot have children
				if (IsNewRecord)
					throw new InvalidOperationException("No child ancestry for new record. Save record before performing tree operations.");

				string originalAncestryValue = GetOriginalValue(d => d.Ancestry);
				return (string.IsNullOrEmpty(originalAncestryValue))
					? ID.ToString()
					: String.Format("{0}/{1}", originalAncestryValue, ID);
			}
		}

		#region Ancestors

		public IEnumerable<ObjectId> AncestorIDs
		{
			get { return (string.IsNullOrEmpty(Ancestry)) ? new List<ObjectId>() : Ancestry.Split('/').Select(ObjectId.Parse); }
		}

		public IDepthQueryable<T> Ancestors
		{
			get { return new DepthQueryable<T>(Where(d => AncestorIDs.Contains(d.ID)), Depth); }
		}

		public IEnumerable<ObjectId> AncestorsAndSelfIDs
		{
			get { return AncestorIDs.Union(new[] { ID }); }
		}

		public IDepthQueryable<T> AncestorsAndSelf
		{
			get { return new DepthQueryable<T>(Where(d => AncestorsAndSelfIDs.Contains(d.ID)), Depth); }
		}

		public int Depth
		{
			get { return AncestorIDs.Count(); }
		}

		#endregion

		#region Parent

		[BsonIgnore]
		public T Parent
		{
			get { return (ParentID == ObjectId.Empty) ? null : Find(ParentID); }
			set
			{
				if (!OnBeforeMove(value))
					return;

				Ancestry = (value == null) ? null : value.ChildAncestry;

				OnAfterMove(value);
			}
		}

		[BsonIgnore]
		public ObjectId ParentID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.Last() : ObjectId.Empty; }
			set { Parent = (value == ObjectId.Empty) ? null : Find(value); }
		}

		#endregion

		#region Root

		public ObjectId RootID
		{
			get { return AncestorIDs.Any() ? AncestorIDs.First() : ID; }
		}

		public T Root
		{
			get { return (RootID == ID) ? (T) this : Find(RootID); }
		}

		public bool IsRoot
		{
			get { return string.IsNullOrEmpty(Ancestry); }
		}

		#endregion

		#region Children

		public Relation<T> Children
		{
			get
			{
				return new Relation<T>(
					Where(d => d.Ancestry == ChildAncestry),
					d => d.Ancestry = ChildAncestry);
			}
		}

		public IEnumerable<ObjectId> ChildIDs
		{
			get { return Children.Select(d => d.ID); }
		}

		public bool HasChildren
		{
			get { return Children.Any(); }
		}

		public bool IsChildless
		{
			get { return !HasChildren; }
		}

		#endregion

		#region Siblings

		public IQueryable<T> SiblingsAndSelf
		{
			get { return Where(d => d.Ancestry == Ancestry); }
		}

		public IQueryable<T> Siblings
		{
			get { return SiblingsAndSelf.Where(d => d.ID != ID); }
		}

		public IEnumerable<ObjectId> SiblingIDs
		{
			get { return Siblings.Select(d => d.ID); }
		}

		public bool HasSiblings
		{
			get { return Siblings.Any(); }
		}

		public bool IsOnlyChild
		{
			get { return !HasSiblings; }
		}

		/// <summary>
		/// Is this document a sibling of the other document?
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSiblingOf(T other)
		{
			return ParentID == other.ParentID;
		}

		#endregion

		#region Descendants

		public IDepthQueryable<T> DescendantsAndSelf
		{
			get { return new DepthQueryable<T>(Where(d => d.ID == ID || d.Ancestry.StartsWith(ChildAncestry) || d.Ancestry == ChildAncestry), Depth); }
		}

		public IEnumerable<ObjectId> DescendantsAndSelfIDs
		{
			get { return DescendantsAndSelf.Select(d => d.ID); }
		}

		public IDepthQueryable<T> Descendants
		{
			get { return new DepthQueryable<T>(Where(d => d.Ancestry.StartsWith(ChildAncestry) || d.Ancestry == ChildAncestry), Depth); }
		}

		public IEnumerable<ObjectId> DescendantIDs
		{
			get { return Descendants.Select(d => d.ID); }
		}

		#endregion

		#region Callbacks

		protected virtual bool OnBeforeMove(T newParent)
		{
			return ExecuteCancellableObservers<IAncestryObserver<T>>(o => o.BeforeMove((T) this, newParent));
		}

		protected virtual void OnAfterMove(T newParent)
		{
			ExecuteObservers<IAncestryObserver<T>>(o => o.AfterMove((T) this, newParent));
		}

		protected override bool OnBeforeSave()
		{
			UpdateDescendantsWithNewAncestry();
			if (CacheDepth)
				AncestryDepth = Depth;
			return base.OnBeforeSave();
		}

		protected override bool OnBeforeDestroy()
		{
			ApplyOrphanStrategy();
			return base.OnBeforeDestroy();
		}

		private bool _disableAncestryCallbacks;
		private void WithoutAncestryCallbacks(Action callback)
		{
			_disableAncestryCallbacks = true;
			callback();
			_disableAncestryCallbacks = false;
		}

		private void UpdateDescendantsWithNewAncestry()
		{
			// Skip this if callbacks are disabled.
			if (_disableAncestryCallbacks)
				return;

			// Skip this if it's a new record or ancestry wasn't updated.
			if (IsNewRecord || !HasValueChanged(d => d.Ancestry))
				return;

			// For each descendant...
			foreach (var descendant in Descendants)
			{
				// Replace old ancestry with new ancestry.
				var copyOfDescendant = descendant;
				descendant.WithoutAncestryCallbacks(() =>
				{
					string forReplace = (String.IsNullOrEmpty(Ancestry))
						? ID.ToString()
						: String.Format("{0}/{1}", Ancestry, ID);
					string newAncestry = Regex.Replace(copyOfDescendant.Ancestry, "^" + ChildAncestry, forReplace);
					copyOfDescendant.Ancestry = newAncestry;
					copyOfDescendant.Save();
				});
			}
		}

		private void ApplyOrphanStrategy()
		{
			// Skip this if callbacks are disabled.
			if (_disableAncestryCallbacks)
				return;

			// Skip this if it's a new record.
			if (IsNewRecord)
				return;

			switch (OrphanStrategy)
			{
				case OrphanStrategy.Destroy:
					foreach (var descendant in Descendants)
						descendant.WithoutAncestryCallbacks(descendant.Destroy);
					break;
				case OrphanStrategy.Rootify:
					foreach (var descendant in Descendants)
					{
						var copyOfDescendant = descendant;
						descendant.WithoutAncestryCallbacks(() =>
						{
							string val = null;
							if (copyOfDescendant.Ancestry != ChildAncestry)
								val = Regex.Replace(copyOfDescendant.Ancestry, "^" + ChildAncestry + "/", string.Empty);
							copyOfDescendant.Ancestry = val;
							copyOfDescendant.Save();
						});
					}
					break;
				case OrphanStrategy.Restrict:
					if (HasChildren)
						throw new InvalidOperationException("Cannot delete record because it has descendants");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

		}

		#endregion

		#endregion
	}
}