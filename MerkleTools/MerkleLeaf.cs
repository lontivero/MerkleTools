namespace MerkleTools
{
	internal class MerkleLeaf : MerkleNodeBase
	{
		public MerkleLeaf(byte[] hash)
		{
			Hash = hash;
		}

		public override byte[] Hash { get; }
		public override int Level => 0;
	}
}
