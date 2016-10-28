using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MerkleTools
{
	public class MerkleTree
	{
		private readonly HashAlgorithm _hashAlgorithm;
		private readonly List<MerkleLeaf> _leave;
		private MerkleNodeBase _root;
		private bool _recalculate;

		public byte[] MerkleRootHash => Root?.Hash;

		internal MerkleNodeBase Root
		{
			get
			{
				if (_recalculate)
				{
					_root = MerkleNode.Build(_leave);
					_recalculate = false;
				}
				return _root;
			}
		}

		public MerkleTree()
			: this(SHA256.Create()){}

		public MerkleTree(HashAlgorithm hashAlgorithm)
		{
			_hashAlgorithm = hashAlgorithm;
			_leave = new List<MerkleLeaf>();
		}

		public void AddLeaf(byte[] data, bool mustHash=false)
		{
			var hash = mustHash ? _hashAlgorithm.ComputeHash(data) : data;
			_leave.Add(new MerkleLeaf(hash));
			_recalculate = true;
		}

		public void AddLeave(IEnumerable<byte[]> items, bool mustHash = false)
		{
			foreach (var item in items)
			{
				AddLeaf(item, mustHash);
			}
		}

		internal Proof GetProof(MerkleLeaf leaf)
		{
			var proof = new Proof(leaf.Hash, Root.Hash, _hashAlgorithm);
			var node = (MerkleNodeBase)leaf;
			while (node.Parent !=null)
			{
				if (node.Parent.Left == node)
				{
					proof.AddRight(node.Parent.Right.Hash);
				}
				else
				{
					proof.AddLeft(node.Parent.Left.Hash);
				}
				node = node.Parent;
			}
			return proof;
		}

		public Proof GetProof(int index)
		{
			return GetProof(_leave[index]);
		}

		public Proof GetProof(byte[] hash)
		{
			try
			{
				var leaf = _leave.Single(x => x.Hash.SequenceEqual(hash));
				return GetProof(leaf);
			}
			catch (InvalidOperationException e)
			{
				throw new InvalidOperationException("There is not single hash matching", e);
			}
		}

		public bool ValidateProof(Proof proof, byte[] hash)
		{
			return proof.Validate(hash, MerkleRootHash, _hashAlgorithm);
		}

		public int Levels => Root.Level;

		public static byte[] Melt(byte[] h1, byte[] h2, HashAlgorithm hashAlgorithm)
		{
			var buffer = new byte[h1.Length + h2.Length];
			Buffer.BlockCopy(h1, 0, buffer, 0, h1.Length);
			Buffer.BlockCopy(h2, 0, buffer, h1.Length, h2.Length);
			return hashAlgorithm.ComputeHash(buffer);
		}
	}

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

	public class Receipt
	{
		private readonly Proof _proof;
		public string Context { get; set; }
		public byte[] TargetHash { get; set; }
		public byte[] MerkleRoot { get; set; }
		public string Type { get; set; }

		public List<Anchor> _anchors;

		public Receipt(Proof proof)
		{
			_proof = proof;
			Context = "https://w3id.org/chainpoint/v2";
			TargetHash = proof.Target;
			MerkleRoot = proof.MerkleRoot;
			var hashAlgorithmName = proof.HashAlgorithm.GetType().DeclaringType.Name;
			Type = $"Chainpoint{hashAlgorithmName}v2";
			_anchors = new List<Anchor>();
		}

		public void AddBitcoinAnchor(string sourceId)
		{
			AddAnchor("BTCOpReturn", sourceId);
		}

		public void AddAnchor(string type, string sourceId)
		{
			_anchors.Add(new Anchor(type, sourceId));
		}

		public string ToJson()
		{
			var json = "{"
				+ $"\"@context\":\"{Context}\","
				+ $"\"type\":\"{Type}\","
				+ $"\"targetHash\":\"{HexEncoder.Encode(TargetHash)}\","
				+ $"\"merkleRoot\":\"{HexEncoder.Encode(MerkleRoot)}\","
				+ $"\"proof\":[";
			json = _proof.Aggregate(json, (current, p) => current + p.ToJson() + ",");
			json+= "],"
				+ "\"anchors\": [";
			json = _anchors.Aggregate(json, (current, a) => current + a.ToJson() + ",");
			json+= "]"
				+ "}";
			return json;
		}
	}


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

	public enum Branch
	{
		Left,
		Rigth
	}

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