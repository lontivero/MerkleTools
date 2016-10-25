using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MerkleTools.Tests
{
	public class MerkleToolsTests
	{
		[Fact]
		public void EmptyTree()
		{
			var mt = new MerkleTree();
			Assert.Null(mt.Root);
		}

		[Fact]
		public void OneItem()
		{
			var hash = sha256("test");
			var mt = new MerkleTree();
			mt.AddLeaf(hash);
			Assert.Equal(hash, mt.MerkleRootHash);
		}

		[Fact]
		public void TwoItems()
		{
			var mt = new MerkleTree();
			mt.AddLeaf(sha256("test1"));
			mt.AddLeaf(sha256("test2"));
			Assert.Equal<string>("587b1fe3afa386ce7cf9e99cf6f3b7f6a78a3c1ca6a549bbd467c992e482dc56", HexEncoder.Encode(mt.MerkleRootHash));
		}

		[Fact]
		public void TenItems()
		{
			var mt = new MerkleTree();
			foreach (var i in Enumerable.Range(0, 10))
			{
				mt.AddLeaf(Encoding.ASCII.GetBytes($"test{i}"), true);
			}
			Assert.Equal(5, mt.Levels);

			var proof = mt.GetProof(0);
			Assert.Equal("right:1b4f0e9851971998e732078544c96b36c3d01cedf7caa332359d6f1d83567014", proof[0].ToString());
			Assert.Equal("right:22a4492ed2dae1d63b0bf80941a1f6b05766dd745c7726df977bd59091397543", proof[1].ToString());
			Assert.Equal("right:f6659f05233319b560780bf3a84f25a9e8e80f2e339b1a1f1bb6e9729a1649aa", proof[2].ToString());
			Assert.Equal("right:e99cb467490912a97964921a5e3936f8dcf000f7458ce35d5b42ec9aceb3a467", proof[3].ToString());

			proof = mt.GetProof(5);
			Assert.Equal("left:a4e624d686e03ed2767c0abd85c14426b0b1157d2ce81d27bb4fe4f6f01d688a" , proof[0].ToString());
			Assert.Equal("right:f822e1be49d3110650ff8de27e2f075fd1ff2bac900d122c5ab12a7939d5f7b2", proof[1].ToString());
			Assert.Equal("left:5f30252bcc93b3bf3abdab44cb06215fda83eb745d689735d2f17a0c1fd55bec" , proof[2].ToString());
			Assert.Equal("right:e99cb467490912a97964921a5e3936f8dcf000f7458ce35d5b42ec9aceb3a467", proof[3].ToString());

			proof = mt.GetProof(8);
			Assert.Equal("right:b4451034d3b6590060ce9484a28b88dd332a80a22ae8e39c9c5cb7357ab26c9f", proof[0].ToString());
			Assert.Equal("left:15a73ded06a3ae3f378ec09ce7c558dadeba84d2f3c76cede98c461bf64e422f" , proof[1].ToString());

			proof = mt.GetProof(9);
			Assert.Equal("left:1f9bfeb15fee8a10c4d0711c7eb0c083962123e1918e461b6a508e7146c189b2", proof[0].ToString());
			Assert.Equal("left:15a73ded06a3ae3f378ec09ce7c558dadeba84d2f3c76cede98c461bf64e422f", proof[1].ToString());
		}

		[Fact]
		public void TenItemsx()
		{
			var mt = new MerkleTree();
			foreach (var i in Enumerable.Range(0, 10))
			{
				mt.AddLeaf(sha256($"test{i}"));
			}
			Assert.Equal(5, mt.Levels);

			foreach (var i in Enumerable.Range(0, 10))
			{
				var valid = mt.ValidateProof(mt.GetProof(i), sha256($"test{i}"));
				Assert.True(valid);
			}

			foreach (var i in Enumerable.Range(0, 10))
			{
				var valid = mt.ValidateProof(mt.GetProof(i), sha256($"test{i + 1}"));
				Assert.False(valid);
			}
		}

		[Fact]
		public void Proof()
		{
			var mt = new MerkleTree();
			foreach (var i in Enumerable.Range(0, 10))
			{
				mt.AddLeaf(sha256($"test{i}"));
			}
			Assert.Equal(5, mt.Levels);

			foreach (var i in Enumerable.Range(0, 10))
			{
				var proof = mt.GetProof(i);
				var receipt = proof.ToReceipt();
				receipt.AddAnchor(new Anchor("BTCOpReturn", "ae125"));
				var jo = (JObject)JsonConvert.DeserializeObject(receipt.ToJson());
				Assert.Equal(receipt.Context, jo["@context"]);
				Assert.Equal(HexEncoder.Encode(receipt.TargetHash), (string)(jo["targetHash"]));
				Assert.Equal(HexEncoder.Encode(receipt.MerkleRoot), (string)(jo["merkleRoot"]));
				Assert.Equal(receipt.Type, jo["type"]);
				Assert.Equal("ChainpointSHA256v2", receipt.Type);
				var rproof = (JArray)jo["proof"];
				var j = 0;
				foreach (var p in proof)
				{
					var branch = (p.Branch == Branch.Left) ? "left" : "right";
					var rp = rproof[j++];
					Assert.Equal(HexEncoder.Encode(p.Hash), (string)(rp[branch]));
				}
			}
		}

		private static byte[] sha256(string text)
		{
			return SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(text));
		}
	}
}
