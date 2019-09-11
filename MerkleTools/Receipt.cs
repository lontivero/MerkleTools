using System.Collections.Generic;
using System.Linq;

namespace MerkleTools
{
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
}