﻿using System.Linq;
using FluentMongo.Linq;
using NUnit.Framework;

namespace Ormongo.Ancestry.Tests
{
	[TestFixture]
	public class OrderedAncestryDocumentTests : AncestryTestsBase
	{
		[Test]
		public void CanGetLowerSiblings()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");
			childNode1.Position = 2;
			childNode1.Save();
			childNode2.Position = 1;
			childNode2.Save();
			childNode3.Position = 3;
			childNode3.Save();

			// Act.
			var result = childNode1.LowerSiblings.ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(childNode3.ID));
		}

		[Test]
		public void CanGetHigherSiblings()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");
			childNode1.Position = 2;
			childNode1.Save();
			childNode2.Position = 1;
			childNode2.Save();
			childNode3.Position = 3;
			childNode3.Save();

			// Act.
			var result = childNode1.HigherSiblings.QueryDump(Log).ToList();

			// Assert.
			Assert.That(result, Has.Count.EqualTo(1));
			Assert.That(result[0].ID, Is.EqualTo(childNode2.ID));
		}

		[Test]
		public void CanGetLowestSibling()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");
			childNode1.Position = 3;
			childNode1.Save();
			childNode2.Position = 1;
			childNode2.Save();
			childNode3.Position = 2;
			childNode3.Save();

			// Act.
			var result = childNode2.LowestSibling;

			// Assert.
			Assert.That(result.ID, Is.EqualTo(childNode1.ID));
		}

		[Test]
		public void CanGetHighestSibling()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");
			childNode1.Position = 2;
			childNode1.Save();
			childNode2.Position = 1;
			childNode2.Save();
			childNode3.Position = 3;
			childNode3.Save();

			// Act.
			var result = childNode1.HighestSibling;

			// Assert.
			Assert.That(result.ID, Is.EqualTo(childNode2.ID));
		}

		[Test]
		public void CanMoveToTop()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Act.
			childNode3.MoveToTop();

			// Assert.
			var children = rootNode.Children.QueryDump(Log).ToList();
			Assert.That(children[0].ID, Is.EqualTo(childNode3.ID));
			Assert.That(children[1].ID, Is.EqualTo(childNode1.ID));
			Assert.That(children[2].ID, Is.EqualTo(childNode2.ID));
		}

		[Test]
		public void MoveToHigherPositionShiftsOtherSiblingsDown()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");
			var childNode4 = CreateTreeNode(rootNode, "Child4");

			// Act.
			childNode3.MoveToPosition(1);

			// Assert.
			var children = rootNode.Children.ToList();
			Assert.That(children[0].ID, Is.EqualTo(childNode1.ID));
			Assert.That(children[0].Position, Is.EqualTo(0));
			Assert.That(children[1].ID, Is.EqualTo(childNode3.ID));
			Assert.That(children[1].Position, Is.EqualTo(1));
			Assert.That(children[2].ID, Is.EqualTo(childNode2.ID));
			Assert.That(children[2].Position, Is.EqualTo(2));
			Assert.That(children[3].ID, Is.EqualTo(childNode4.ID));
			Assert.That(children[3].Position, Is.EqualTo(3));
		}

		[Test]
		public void MoveToLowerPositionShiftsOtherSiblingsUp()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");
			var childNode4 = CreateTreeNode(rootNode, "Child4");

			// Act.
			childNode2.MoveToPosition(2);

			// Assert.
			var children = rootNode.Children.ToList();
			Assert.That(children[0].ID, Is.EqualTo(childNode1.ID));
			Assert.That(children[0].Position, Is.EqualTo(0));
			Assert.That(children[1].ID, Is.EqualTo(childNode3.ID));
			Assert.That(children[1].Position, Is.EqualTo(1));
			Assert.That(children[2].ID, Is.EqualTo(childNode2.ID));
			Assert.That(children[2].Position, Is.EqualTo(2));
			Assert.That(children[3].ID, Is.EqualTo(childNode4.ID));
			Assert.That(children[3].Position, Is.EqualTo(3));
		}

		#region Callbacks

		[Test]
		public void SetsDefaultPosition()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			
			// Act.
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Assert.
			Assert.That(childNode1.Position, Is.EqualTo(0));
			Assert.That(childNode2.Position, Is.EqualTo(1));
			Assert.That(childNode3.Position, Is.EqualTo(2));
		}

		[Test]
		public void SetsDefaultPositionWithNoSiblings()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");

			// Act.
			var childNode = CreateTreeNode(rootNode, "Child");

			// Assert.
			Assert.That(childNode.Position, Is.EqualTo(0));
		}

		[Test]
		public void LowerSiblingsAreMovedUpAfterDestroy()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Act.
			childNode1.Destroy();

			// Assert.
			Assert.That(TreeNode.Find(childNode2.ID).Position, Is.EqualTo(0));
			Assert.That(TreeNode.Find(childNode3.ID).Position, Is.EqualTo(1));
		}

		[Test]
		public void RepositionsFormerSiblings()
		{
			// Arrange.
			var rootNode1 = CreateTreeNode(null, "Root1");
			var childNode1 = CreateTreeNode(rootNode1, "Child1");
			var childNode2 = CreateTreeNode(rootNode1, "Child2");
			var childNode3 = CreateTreeNode(rootNode1, "Child3");
			var rootNode2 = CreateTreeNode(null, "Root2");

			// Act.
			childNode2.Parent = rootNode2;
			childNode2.Save();

			// Assert.
			Assert.That(TreeNode.Find(childNode1.ID).Position, Is.EqualTo(0));
			Assert.That(TreeNode.Find(childNode2.ID).Position, Is.EqualTo(0));
			Assert.That(TreeNode.Find(childNode3.ID).Position, Is.EqualTo(1));
		}

		[Test]
		public void SaveLoadAndSaveMaintainsPosition()
		{
			// Arrange.
			var rootNode = CreateTreeNode(null, "Root");
			var childNode1 = CreateTreeNode(rootNode, "Child1");
			var childNode2 = CreateTreeNode(rootNode, "Child2");
			var childNode3 = CreateTreeNode(rootNode, "Child3");

			// Act.
			var retrievedChildNode1 = TreeNode.Find(childNode1.ID);
			retrievedChildNode1.Save();
			retrievedChildNode1 = TreeNode.Find(childNode1.ID);

			// Assert.
			Assert.That(TreeNode.Find(retrievedChildNode1.ID).Position, Is.EqualTo(0));
		}

		#endregion
	}
}