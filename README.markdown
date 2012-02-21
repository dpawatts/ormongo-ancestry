Ormongo-Ancestry
================

Ormongo-Ancestry is an extension of [Ormongo](https://github.com/roastedamoeba/ormongo) that allows the records of a 
.NET Ormongo model to be organised as a tree structure (or hierarchy). It uses a single, intuitively formatted database column, 
using a variation on the materialised path pattern. It exposes all the standard tree structure relations (ancestors, parent, root, 
children, siblings, descendants) and all of them can be fetched in a single query. Additional features are depth caching, 
depth constraints, ordering and different strategies for dealing with orphaned records.

## Installation

To apply Ormongo-Ancestry to any Ormongo model, follow these simple steps:

1. Install

  * TODO

2. Inherit from `AncestryDocument` or `OrderedAncestryDocument`.

        public class TreeNode : OrderedAncestryDocument<TreeNode>
        {
            public string Name { get; set; }
        }

Your model is now a tree!

## Organising records into a tree

You can use the `Parent` property to organise your records into a tree. If you have the ID of the record you want
to use as a parent and don't want to fetch it, you can also use `ParentID`. For example:

    TreeNode.Create(new TreeNode
    {
        Name = "Stinky",
        Parent = TreeNode.Create(new TreeNode { Name = "Squeeky" })
    });

or

    TreeNode.Create(new TreeNode
    {
        Name = "Stinky",
        ParentID = TreeNode.Create(new TreeNode { Name = "Squeeky" }).ID
    });

You can also create children through the `Children` relation on a node:

    node.Children.Create(new TreeNode { Name = "Stinky" });

## Navigating your tree

To navigate an Ancestry model, use the following methods on any instance / record:

    Parent                  Returns the parent of the record, null for a root node
    ParentID                Returns the ID of the parent of the record, null for a root node
    Root                    Returns the root of the tree the record is in, this for a root node
    RootID                  Returns the ID of the root of the tree the record is in
    IsRoot                  Returns true if the record is a root node, false otherwise
    AncestorIDs             Returns a list of ancestor IDs, starting with the root ID and ending with the parent ID
    Ancestors               Scopes the model on ancestors of the record
    AncestorsAndSelfIDs     Returns a list the path IDs, starting with the root ID and ending with the node's own ID
    AncestorsAndSelf        Scopes model on path records of the record
    Children                Scopes the model on children of the record
    ChildIDs                Returns a list of child IDs
    HasChildren             Returns true if the record has any children, false otherwise
    IsChildless             Returns true is the record has no childen, false otherwise
    Siblings                Scopes the model on siblings of the record, the record itself is not included
    SiblingIDs              Returns a list of sibling IDs
    SiblingsAndSelf         Scopes the model on siblings of the record, the record itself is included
    HasSiblings             Returns true if the record's parent has more than one child
    IsOnlyChild             Returns true if the record is the only child of its parent
    Descendants             Scopes the model on direct and indirect children of the record
    DescendantIDs           Returns a list of a descendant IDs
    DescendantsAndSelf      Scopes the model on descendants and itself
    DescendantsAndSelfIDs   Returns a list of all IDs in the record's descendants and itself
    Depth                   Return the depth of the node, root nodes are at depth 0

## Options for Ancestry

There are some configurable options, which can be set in a static constructor:

    public class TreeNode : OrderedAncestryDocument<TreeNode>
    {
        static TreeNode()
        {
            OrphanStrategy = OrphanStrategy.Destroy;
			CacheDepth = true;
        }
    }

The options are:

    OrphanStrategy  Instruct Ancestry what to do with children of a node that is destroyed:
                    Destroy    All children are destroyed as well (default)
                    Rootify    The children of the destroyed node become root nodes
                    Restrict   An exception is thrown if any children exist
    CacheDepth      Cache the depth of each node in the `AncestryDepth` field (default: false)

## Finders

Where possible, the navigation methods return `IQueryable` collections; this means additional ordering, 
conditions, limits, etc. can be applied and that the result can be either retrieved, counted or 
checked for existence. For example:

    node.Children.Where(n => n.Name == "Mary")
    node.Subtree.OrderByDescending(n => n.Name).Take(10)
    node.Descendants.Count()

For convenience, one `IQueryable` extension method is included:

    Roots()                 Root nodes

## Selecting nodes by depth

When depth caching is enabled (see "Options for Ancestry"), five more `IQueryable` extension methods can be used to select nodes on their depth:

    BeforeDepth(depth)     Return nodes that are less deep than depth (node.depth < depth)
    ToDepth(depth)         Return nodes up to a certain depth (node.depth <= depth)
    AtDepth(depth)         Return nodes that are at depth (node.depth == depth)
    FromDepth(depth)       Return nodes starting from a certain depth (node.depth >= depth)
    AfterDepth(depth)      Return nodes that are deeper than depth (node.depth > depth)

The depth finders are also available through calls to `Descendants`, `DescendantIDs`, `DescendantsAndSelf`, `DescendantsAndSelfIDs`, `AncestorsAndSelf` and `Ancestors`. 
In this case, depth values are interpreted relatively. Some examples:

    node.Subtree.ToRelativeDepth(2)              Subtree of node, to a depth of node.depth + 2 (self, children and grandchildren)
    node.Subtree.ToDepth(5)                      Subtree of node to an absolute depth of 5
    node.Descendants.AtRelativeDepth(2)          Descendant of node, at depth node.depth + 2 (grandchildren)
    node.Descendants.AtDepth(10)                 Descendants of node at an absolute depth of 10
    node.Ancestors.ToDepth(3)                    The oldest 4 ancestors of node (its root and 3 more)
    node.AncestorsAndSelf.FromRelativeDepth(-2)  The node's grandparent, parent and the node itself

    node.Ancestors.FromRelativeDepth(-6).ToRelativeDepth(-4)
    node.AncestorsAndSelf.FromDepth(3).ToDepth(4)
    node.Descendants.FromRelativeDepth(2).ToRelativeDepth(4)
    node.DescendantsAndSelf.FromDepth(10).ToDepth(12)

Please note that depth constraints cannot be passed to `AncestorIDs` and `AncestorAndSelfIDs`. The reason for this is that
both these relations can be fetched directly from the ancestry column without performing a database query. It would
require an entirely different method of applying the depth constraints which isn't worth the effort of implementing.

## Ordering

To enable ordering of tree nodes, inherit from `OrderedAncestryDocument`. This will add a `Position` field to your document
and provide additional utility methods:

    node.LowerSiblings
    node.HigherSiblings
    node.LowestSibling
    node.HighestSibling
    
    node.MoveUp()
    node.MoveDown()
    node.MoveToTop()
    node.MoveToBottom()
    node.MoveAbove(other)
    node.MoveBelow(other)
    
    node.AtTop
    node.AtBottom

Ormongo-Ancestry will manage the `Position` field automatically. If you delete a node, or move it to a different part of the tree,
its previous siblings will be moved up, if necessary. When you create a node and don't set its position, it will be assigned 
a default position.

## Tests

Ormongo-Ancestry includes a NUnit test suite consisting of about 60 tests.

## Internals

Ormongo-Ancestry stores a path from the root to the parent for every node. This is a variation on the materialised path database pattern. 
It allows any relation (siblings, descendants, etc.) to be fetched in a single query without the complicated algorithms and incomprehensibility 
associated with left and right values. Additionally, any inserts, deletes and updates only affect nodes within the affected node's own subtree.

The materialised path pattern requires Ormongo-Ancestry to use a `regexp` condition in order to fetch descendants. This should not be particularly 
slow; however, the condition never starts with a wildcard which allows the DBMS to use the column index. If you have any data on performance 
with a large number of records, please drop me line.

## Contact and copyright

It's a fork of [Mongoid-Ancestry](https://github.com/skyeagle/mongoid-ancestry) - which in turn is a port of 
[ancestry](https://github.com/stefankroes/ancestry) - but ported to C# and adapted to work with Ormongo.

The ordering functionality is ported from [Mongoid Tree](https://github.com/benedikt/mongoid-tree).

All thanks should goes to Stefan Kroes and Anton Orel for their great work.

Bug report? Faulty/incomplete documentation? Feature request? Please post an issue on [issues tracker](http://github.com/sitdap/ormongo-ancestry/issues).

Copyright (c) 2012 Sound in Theory Ltd, released under the MIT license