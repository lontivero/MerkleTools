namespace MerkleTools
{
    public class Anchor
	{
		public string AnchorType { get; set; }
		public string SourceId { get; set; }

		public Anchor(string anchorType, string sourceId)
		{
			AnchorType = anchorType;
			SourceId = sourceId;
		}

		public string ToJson()
		{
			return $"{{ \"type\": \"{AnchorType}\", \"sourceId\": \"{SourceId}\" }}";
		}
	}
}