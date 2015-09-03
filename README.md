# bitcoin-sha256
C# Bitcoin SHA256 implementation with MIDSTATE Optimization

Midstate Explained - 
[How can I calculate SHA256 Midstate?](http://crypto.stackexchange.com/questions/1862/how-can-i-calculate-the-sha-256-midstate)

## USAGE

1. Use `initialize(byte[] work)` method to setup the first block. 
2. `CalculateHash(byte[] nonce)` to calculate hash. 

### Example

Solve for [block 239711](https://blockchain.info/block/00000000000001272c7eb572d183c9b8da350b1835b78d3f56cc07c082d78a5c)
```
const string version = "02000000"; //v2
const string hashPrevBlock = "0affed3fc96851d8c74391c2d9333168fe62165eb228bced7e00000000000000";
const string merkleRoot = "4277b65e3bd527f0ceb5298bdb06b4aacbae8a4a808c2c8aa414c20f252db801";
const string time = "130dae51";
const string difficulty = "6461011a";
const string successNonce = "3aeb9bb8"; //3097226042

byte[] work = Conversions.HexadecimalToByte(version + hashPrevBlock + merkleRoot + time + difficulty + successNonce);

Sha256 sha256 = new Sha256();
sha256.Initialize(work);
string hash = Conversions.ByteToHexadecimal(sha256.CalculateHash(Conversions.HexadecimalToByte(successNonce)));
//hash equals 5c8ad782c007cc563f8db735180b35dab8c983d172b57e2c2701000000000000 = hash of block 239711
```
