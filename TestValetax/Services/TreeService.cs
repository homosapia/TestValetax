using TestValetax.DB.Entities;
using TestValetax.DB.Repositories.Interface;
using TestValetax.Exceptions;
using TestValetax.Model;
using TestValetax.Services.Interface;

namespace TestValetax.Services
{
    public class TreeService : ITreeService
    {
        private readonly ITreeNodeRepository _treeNodeRepository;
        private readonly ILogger<TreeService> _logger;

        public TreeService(
            ITreeNodeRepository treeNodeRepository,
            ILogger<TreeService> logger)
        {
            _treeNodeRepository = treeNodeRepository;
            _logger = logger;
        }

        public async Task<MNode> GetOrCreateTreeAsync(string treeName)
        {
            if (string.IsNullOrWhiteSpace(treeName))
            {
                throw new SecureException("Tree name cannot be empty");
            }

            var tree = await _treeNodeRepository.GetTreeAsync(treeName);

            if (tree != null)
            {
                _logger.LogInformation("Tree '{TreeName}' found", treeName);
                return MapToMNode(tree);
            }

            _logger.LogInformation("Tree '{TreeName}' not found, creating new one", treeName);

            var rootNode = await _treeNodeRepository.CreateRootNodeAsync(treeName, "Root");
            await _treeNodeRepository.SaveChangesAsync();

            var newTree = await _treeNodeRepository.GetTreeAsync(treeName);
            return MapToMNode(newTree!);
        }

        private MNode MapToMNode(TreeNode node)
        {
            var mNode = new MNode
            {
                Id = node.Id,
                Name = node.Name
            };

            if (node.Children != null && node.Children.Any())
            {
                foreach (var child in node.Children)
                {
                    mNode.Children.Add(MapToMNode(child));
                }
            }

            return mNode;
        }


        public async Task CreateNodeAsync(string treeName, long? parentNodeId, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new SecureException("Node name cannot be empty");

            var treeExists = await _treeNodeRepository.ExistsAsync(x => x.TreeName == treeName);
            if (!treeExists)
                throw new SecureException($"Tree '{treeName}' does not exist");

            if (parentNodeId.HasValue)
            {
                var parentNode = await _treeNodeRepository.GetByIdAsync(parentNodeId.Value);
                if (parentNode == null)
                    throw new SecureException($"Parent node with id {parentNodeId} not found");

                if (parentNode.TreeName != treeName)
                    throw new SecureException("Parent node belongs to different tree");

                var nameExists = await _treeNodeRepository.IsNameUniqueAmongSiblingsAsync(parentNodeId, nodeName, null);
                if (!nameExists)
                    throw new SecureException($"Node with name '{nodeName}' already exists at this level");
            }

            var node = new TreeNode
            {
                Name = nodeName,
                TreeName = treeName,
                ParentId = parentNodeId
            };

            await _treeNodeRepository.AddAsync(node);
            await _treeNodeRepository.SaveChangesAsync();
        }

        public async Task DeleteNodeAsync(long nodeId)
        {
            var node = await _treeNodeRepository.GetByIdAsync(nodeId);
            if (node == null)
                throw new SecureException($"Node with id {nodeId} not found");

            var descendants = await _treeNodeRepository.GetAllDescendantsAsync(nodeId);

            if (descendants.Any())
                _treeNodeRepository.RemoveRange(descendants);

            _treeNodeRepository.Remove(node);

            await _treeNodeRepository.SaveChangesAsync();
        }


        public async Task RenameNodeAsync(long nodeId, string newNodeName)
        {
            if (string.IsNullOrWhiteSpace(newNodeName))
                throw new SecureException("New node name cannot be empty");

            var node = await _treeNodeRepository.GetNodeWithParentAsync(nodeId);
            if (node == null)
                throw new SecureException($"Node with id {nodeId} not found");

            if (node.Name == newNodeName)
                return;

            var isNameUnique = await _treeNodeRepository.IsNameUniqueAmongSiblingsAsync(
                node.ParentId, newNodeName, nodeId);

            if (!isNameUnique)
                throw new SecureException($"Node with name '{newNodeName}' already exists at this level");

            node.Name = newNodeName;
            _treeNodeRepository.Update(node);
            await _treeNodeRepository.SaveChangesAsync();
        }
    }
}
