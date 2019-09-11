using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

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
}