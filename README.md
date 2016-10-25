
# MerkleTools

MerkleTools is a .NET library to create of Merkle trees and output receipts in a format consistent with the [chainpoint](https://github.com/chainpoint) v2 standard. Also allows validation of a Chainpoint receipt.

This was developed in support of the [Blockchain Certificates](http://certificates.media.mit.edu/) project.


## How to use?
With nuget :
> **Install-Package MerkleTools** 

Go on the [nuget website](https://www.nuget.org/packages/MerkleTools/) for more information.

## Example

```c#
var tree = new MerkleTree();

tree.AddLeave(new[]{
	HexEncoder.Decode("e1566f09e0deea437826514431be6e4bdb4fe10aa54d75aecf0b4cdc1bc4320c"),
	HexEncoder.Decode("2f7f9092b2d6c5c17cfe2bcf33fc38a41f2e4d4485b198c2b1074bba067e7168"),
	HexEncoder.Decode("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"),
	HexEncoder.Decode("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")
});

var root = HexEncoder.Encode(tree.MerkleRootHash);
Console.WriteLine(root);

// 740c08b74d31bb77fd9806e4f6159d88dfd012acf8984bd41b4b4c9cbd7aa358

var proof = tree.GetProof(1);
Console.WriteLine(proof.ToJson());

/*
[{ 
	"left": "e1566f09e0deea437826514431be6e4bdb4fe10aa54d75aecf0b4cdc1bc4320c"
},{ 
	"right":"2dba5dbc339e7316aea2683faf839c1b7b1ee2313db792112588118df066aa35"
}]"
*/

var hash = HexEncoder.Decode("2f7f9092b2d6c5c17cfe2bcf33fc38a41f2e4d4485b198c2b1074bba067e7168");
var isValid = tree.ValidateProof(proof, hash);

Console.WriteLine(isValid); // true

var receipt = proof.ToReceipt();
receipt.AddBitcoinAnchor("780b4cdc16f09e0deebce156a434320c2654fe10aa54d75ae14431be6e4bdbcf");
Console.WriteLine(receipt.ToJson());

/*
{
   "@context":"https://w3id.org/chainpoint/v2",
   "type":"ChainpointSHA256v2",
   "targetHash":"2f7f9092b2d6c5c17cfe2bcf33fc38a41f2e4d4485b198c2b1074bba067e7168",
   "merkleRoot":"740c08b74d31bb77fd9806e4f6159d88dfd012acf8984bd41b4b4c9cbd7aa358",
   "proof":[
	  {
		 "left":"e1566f09e0deea437826514431be6e4bdb4fe10aa54d75aecf0b4cdc1bc4320c"
	  },
	  {
		 "right":"2dba5dbc339e7316aea2683faf839c1b7b1ee2313db792112588118df066aa35"
	  }
   ],
   "anchors":[
	  {
		 "type":"BTCOpReturn",
		 "sourceId":"780b4cdc16f09e0deebce156a434320c2654fe10aa54d75ae14431be6e4bdbcf"
	  }
   ]
}
*/
```

## Contact

Contact [info@O3-one.com](mailto:info@O3-one.com) with questions