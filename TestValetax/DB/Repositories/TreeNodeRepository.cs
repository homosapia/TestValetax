using Microsoft.EntityFrameworkCore;
using TestValetax.DB.Entities;
using TestValetax.DB.Repositories.Interface;

namespace TestValetax.DB.Repositories
{
    public class TreeNodeRepository : Repository<TreeNode>, ITreeNodeRepository
    {
        public TreeNodeRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<TreeNode?> GetTreeAsync(string treeName)
        {
            // Получаем корневые узлы дерева (без parent)
            var rootNodes = await _dbSet
                .Where(n => n.TreeName == treeName && n.ParentId == null)
                .Include(n => n.Children)
                    .ThenInclude(c => c.Children) // Рекурсивно загружаем все уровни
                .ToListAsync();

            return rootNodes.FirstOrDefault(); // Предполагаем один корень
        }

        public async Task<bool> IsNameUniqueAmongSiblingsAsync(
            long? parentId,
            string name,
            long? excludeNodeId = null)
        {
            var query = _dbSet.Where(n => n.ParentId == parentId && n.Name == name);

            if (excludeNodeId.HasValue)
                query = query.Where(n => n.Id != excludeNodeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<bool> HasChildrenAsync(long nodeId)
        {
            return await _dbSet.AnyAsync(n => n.ParentId == nodeId);
        }

        public async Task<List<TreeNode>> GetAllDescendantsAsync(long nodeId)
        {
            var descendants = new List<TreeNode>();
            await GetDescendantsRecursive(nodeId, descendants);
            return descendants;
        }

        private async Task GetDescendantsRecursive(long parentId, List<TreeNode> result)
        {
            var children = await _dbSet.Where(n => n.ParentId == parentId).ToListAsync();

            foreach (var child in children)
            {
                result.Add(child);
                await GetDescendantsRecursive(child.Id, result);
            }
        }

        public async Task<TreeNode?> GetNodeWithParentAsync(long nodeId)
        {
            return await _dbSet
                .Include(n => n.Parent)
                .FirstOrDefaultAsync(n => n.Id == nodeId);
        }

        public async Task<TreeNode?> GetNodeWithChildrenAsync(long nodeId)
        {
            return await _dbSet
                .Include(n => n.Children)
                .FirstOrDefaultAsync(n => n.Id == nodeId);
        }

        public async Task<TreeNode> CreateRootNodeAsync(string treeName, string nodeName)
        {
            var rootNode = new TreeNode
            {
                Name = nodeName,
                TreeName = treeName,
                ParentId = null,
                Children = new List<TreeNode>()
            };

            await _dbSet.AddAsync(rootNode);
            return rootNode;
        }
    }
}
