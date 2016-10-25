using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MerkleTools
{
	internal class MerkleNode : MerkleNodeBase
	{
		public static MerkleNodeBase Build(IEnumerable<MerkleNodeBase> nodes )
		{
			var merkleLeaves = nodes as MerkleLeaf[] ?? nodes.ToArray();
			if (merkleLeaves.Length == 1) return merkleLeaves[0];

			var nodeList = new List<MerkleNodeBase>();

			for (var i = 0; i < merkleLeaves.Length; i += 2)
			{
				nodeList.Add(i + 1 <= merkleLeaves.Length - 1
					? new MerkleNode(merkleLeaves[i], merkleLeaves[i + 1])
					: merkleLeaves[i]);
			}
			return Build(nodeList);
		}

		public MerkleNodeBase Left { get; }
		public MerkleNodeBase Right { get; }

		public MerkleNode(MerkleNodeBase left, MerkleNodeBase right)
		{
			Left = left;
			Right = right;
			if (Left != null) Left.Parent = this;
			if (Right != null) Right.Parent = this;
		}

		public override byte[] Hash
		{
			get
			{
				byte[] hash;
				if (Left != null && Right == null)
				{
					hash = Left.Hash;
				}
				else if (Left == null && Right != null)
				{
					hash = Right.Hash;
				}
				else
				{
					hash = SHA256.Create().ComputeHash(Left.Hash.Concat(Right.Hash).ToArray());
				}
				return hash;
			}
		}

		public override int Level => 1+ Math.Max(Left?.Level ?? 0, Right?.Level ?? 0);
	}
}