namespace TestValetax.DB.Entities
{
    public class TreeNode
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string TreeName { get; set; }

        public long? ParentId { get; set; }
        public TreeNode Parent { get; set; }

        public ICollection<TreeNode> Children { get; set; }
    }
}
