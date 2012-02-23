namespace Ormongo.Ancestry.Tests
{
	public class TreeNode : OrderedAncestryDocument<TreeNode>
	{
		public string Name { get; set; }
	}

	public abstract class FileSystemNode : OrderedAncestryDocument<FileSystemNode>
	{
		public string Name { get; set; }
	}

	public class FolderNode : FileSystemNode
	{

	}

	public class FileNode : FileSystemNode
	{

	}
}