using System;
using System.Collections.Generic;
using System.Linq;

namespace MerkleTools
{
	internal abstract class MerkleNodeBase : IMerkleNode
	{
		public MerkleNode Parent { get; internal set; }

		public IEnumerable<MerkleNode> Ancestors
		{
			get
			{
				var p = Parent;
				while (p!=null)
				{
					yield return p;
					p = p.Parent;
				}
			}
		}

		public MerkleNode Root => Ancestors.LastOrDefault();
		public abstract byte[] Hash { get; }
		public abstract int Level { get; }

		public string ToHex()
		{
			return HexEncoder.Encode(Hash, 0, Hash.Length);
		}
	}
}