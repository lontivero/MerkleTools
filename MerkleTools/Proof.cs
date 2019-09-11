using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MerkleTools
{
    public class Proof : IEnumerable<ProofItem>
	{
		public HashAlgorithm HashAlgorithm { get; }
		public byte[] Target { get; }
		public byte[] MerkleRoot { get; }
		private readonly List<ProofItem> _proofItems = new List<ProofItem>();

		public ProofItem this[int i] => _proofItems[i];

		public Proof(byte[] target, byte[] merkleRoot, HashAlgorithm hashAlgorithm)
		{
			HashAlgorithm = hashAlgorithm;
			Target = target;
			MerkleRoot = merkleRoot;
		}

		public void AddLeft(byte[] hash)
		{
			_proofItems.Add(new ProofItem(Branch.Left, hash));	
		}
		public void AddRight(byte[] hash)
		{
			_proofItems.Add(new ProofItem(Branch.Rigth, hash));
		}

		public bool Validate()
		{
			return Validate(Target, MerkleRoot, HashAlgorithm);
		}

		public bool Validate(byte[] hash, byte[] root, HashAlgorithm hashAlgorithm)
		{
			var proofHash = hash;
			foreach (var x in this)
			{
				if (x.Branch == Branch.Left)
				{
					proofHash = MerkleTree.Melt(x.Hash, proofHash, hashAlgorithm);
				}
				else if (x.Branch == Branch.Rigth)
				{
					proofHash = MerkleTree.Melt(proofHash, x.Hash, hashAlgorithm);
				}
				else
				{
					return false;
				}
			}

			return proofHash.SequenceEqual(root);
		}

		public IEnumerator<ProofItem> GetEnumerator()
		{
			return _proofItems.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Receipt ToReceipt()
		{
			return new Receipt(this);
		}

		public string ToJson()
		{
			return $"[{string.Join(",", this.Select(x=>x.ToJson()))}]";
		}
	}
}