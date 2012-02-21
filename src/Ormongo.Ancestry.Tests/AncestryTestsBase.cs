using NUnit.Framework;

namespace Ormongo.Ancestry.Tests
{
	public abstract class AncestryTestsBase : TestsBase
	{
		[SetUp]
		public virtual void SetUp()
		{
			TreeNode.OrphanStrategy = OrphanStrategy.Destroy;
			TreeNode.CacheDepth = false;
		}

		[TearDown]
		public virtual void TearDown()
		{
			TreeNode.Drop();
		}

		protected static TreeNode CreateTreeNode(TreeNode parent, string name)
		{
			return TreeNode.Create(new TreeNode
			{
				Parent = parent,
				Name = name
			});
		}
	}
}