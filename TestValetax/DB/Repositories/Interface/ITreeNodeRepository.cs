using TestValetax.DB.Entities;

namespace TestValetax.DB.Repositories.Interface
{
    public interface ITreeNodeRepository : IRepository<TreeNode>
    {
        Task<TreeNode?> GetTreeAsync(string treeName);
        Task<bool> IsNameUniqueAmongSiblingsAsync(long? parentId, string name, long? excludeNodeId = null);
        Task<bool> HasChildrenAsync(long nodeId);
        Task<List<TreeNode>> GetAllDescendantsAsync(long nodeId);
        Task<TreeNode?> GetNodeWithParentAsync(long nodeId);
        Task<TreeNode?> GetNodeWithChildrenAsync(long nodeId);
        Task<TreeNode> CreateRootNodeAsync(string treeName, string nodeName);
    }
}
