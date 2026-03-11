namespace TestValetax.Model
{
    public class MNode
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<MNode> Children { get; set; }
    }
}
