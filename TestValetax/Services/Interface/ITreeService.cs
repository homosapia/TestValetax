using TestValetax.Model;

namespace TestValetax.Services.Interface
{
    public interface ITreeService
    {
        Task<MNode> GetOrCreateTreeAsync(string treeName);
        Task CreateNodeAsync(string treeName, long? parentNodeId, string nodeName);
        Task DeleteNodeAsync(long nodeId);
        Task RenameNodeAsync(long nodeId, string newNodeName);
    }
}
