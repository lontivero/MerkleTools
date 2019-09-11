namespace MerkleTools
{
    public class ProofItem
	{
		public ProofItem(Branch branch, byte[] hash)
		{
			Branch = branch;
			Hash = hash;
		}

		public Branch Branch { get; }
		public byte[] Hash { get; }

		public override string ToString()
		{
			var branch = Branch == Branch.Left ? "left" : "right";
			var encodedHash = HexEncoder.Encode(Hash);
			return $"{branch}:{encodedHash}";
		}
		public object ToJson()
		{
			var branch = Branch == Branch.Left ? "left" : "right";
			var encodedHash = HexEncoder.Encode(Hash);
			return $"{{ \"{branch}\":\"{encodedHash}\"}}";
		}
	}
}