using System.Linq;
using NUnit.Framework;

namespace Ormongo.Ancestry.Tests
{
	public class AncestryExtensionsTests : AncestryTestsBase
	{
		[Test]
		public void Roots()
		{
			// Arrange.
			var rootNode1 = CreateTreeNode(null, "Root1");
			var childNode = CreateTreeNode(rootNode1, "Child");
			CreateTreeNode(childNode, "GrandChild");
			var rootNode2 = CreateTreeNode(null, "Root2");

			// Act.
			var result = TreeNode.FindAll().Roots().ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(rootNode1.ID));
			Assert.That(result[1].ID, Is.EqualTo(rootNode2.ID));
		}

		[Test, ExpectedException]
		public void BeforeDepthIsNotAvailableWhenDepthCachingIsDisabled()
		{
			TreeNode.FindAll().BeforeDepth(0);
		}

		[Test, ExpectedException]
		public void ToDepthIsNotAvailableWhenDepthCachingIsDisabled()
		{
			TreeNode.FindAll().ToDepth(0);
		}

		[Test, ExpectedException]
		public void AtDepthIsNotAvailableWhenDepthCachingIsDisabled()
		{
			TreeNode.FindAll().AtDepth(0);
		}

		[Test, ExpectedException]
		public void FromDepthIsNotAvailableWhenDepthCachingIsDisabled()
		{
			TreeNode.FindAll().FromDepth(0);
		}

		[Test, ExpectedException]
		public void AfterDepthIsNotAvailableWhenDepthCachingIsDisabled()
		{
			TreeNode.FindAll().AfterDepth(0);
		}

		[Test]
		public void BeforeDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = TreeNode.FindAll().BeforeDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
		}

		[Test]
		public void BeforeRelativeDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");
			CreateTreeNode(grandChildNode, "GreatGrandChild");

			// Act.
			var result = childNode.Descendants.BeforeRelativeDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void ToDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = TreeNode.FindAll().ToDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(3));
			Assert.That(result[0].ID, Is.EqualTo(rootNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(childNode.ID));
			Assert.That(result[2].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void ToRelativeDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");
			var greatGrandChildNode = CreateTreeNode(grandChildNode, "GreatGrandChild");

			// Act.
			var result = childNode.Descendants.ToRelativeDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(grandChildNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(greatGrandChildNode.ID));
		}

		[Test]
		public void AtDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode1 = CreateTreeNode(childNode, "GrandChild1");
			var grandChildNode2 = CreateTreeNode(childNode, "GrandChild2");

			// Act.
			var result = TreeNode.FindAll().AtDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(grandChildNode1.ID));
			Assert.That(result[1].ID, Is.EqualTo(grandChildNode2.ID));
		}

		[Test]
		public void AtRelativeDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");
			var greatGrandChildNode1 = CreateTreeNode(grandChildNode, "GreatGrandChild1");
			var greatGrandChildNode2 = CreateTreeNode(grandChildNode, "GreatGrandChild2");

			// Act.
			var result = childNode.Descendants.AtRelativeDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(greatGrandChildNode1.ID));
			Assert.That(result[1].ID, Is.EqualTo(greatGrandChildNode2.ID));
		}

		[Test]
		public void FromDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = TreeNode.FindAll().FromDepth(1).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(childNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void FromRelativeDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");
			var greatGrandChildNode = CreateTreeNode(grandChildNode, "GreatGrandChild");

			// Act.
			var result = childNode.Descendants.FromRelativeDepth(1).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(grandChildNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(greatGrandChildNode.ID));
		}

		[Test]
		public void AfterDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");

			// Act.
			var result = TreeNode.FindAll().AfterDepth(1).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(grandChildNode.ID));
		}

		[Test]
		public void AfterRelativeDepth()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");
			var greatGrandChildNode = CreateTreeNode(grandChildNode, "GreatGrandChild");

			// Act.
			var result = childNode.Descendants.AfterRelativeDepth(1).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(greatGrandChildNode.ID));
		}

		[Test]
		public void RelativeDepthExtensionMethodsCanBeCombined()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");
			var childNode = CreateTreeNode(rootNode, "Child");
			var grandChildNode = CreateTreeNode(childNode, "GrandChild");
			var greatGrandChildNode = CreateTreeNode(grandChildNode, "GreatGrandChild");

			// Act.
			var result = childNode.Descendants.FromRelativeDepth(1).ToRelativeDepth(2).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result[0].ID, Is.EqualTo(grandChildNode.ID));
			Assert.That(result[1].ID, Is.EqualTo(greatGrandChildNode.ID));
		}

		[Test]
		public void AllRelativeDepthExtensionMethodsCanBeCombined()
		{
			// Arrange.
			TreeNode.CacheDepth = true;
			var rootNode = CreateTreeNode(null, "Root");

			// Act.
			var result = rootNode.Descendants
				.BeforeRelativeDepth(1)
				.ToRelativeDepth(1)
				.AtRelativeDepth(1)
				.FromRelativeDepth(1)
				.AfterRelativeDepth(2)
				.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(0));
		}
	}
}