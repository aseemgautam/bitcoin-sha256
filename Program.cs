using System;
using System.Collections.Generic;
using System.Text;

namespace Bitcoin.Sha26
{
    class Program
    {
        static void Main(string[] args)
        {
            string block239711 =
                "020000000affed3fc96851d8c74391c2d9333168fe62165eb228bced7e000000000000004277b65e3bd527f0ceb5298bdb06b4aacbae8a4a808c2c8aa414c20f252db801130dae516461011a3aeb9bb8";
            string successNonce = "3aeb9bb8";
            byte[] work = Conversions.HexadecimalToByte(block239711);
            Sha256 sha256 = new Sha256();
            sha256.Initialize(work);
            Console.WriteLine(Conversions.ByteToHexadecimal(sha256.CalculateHash(Conversions.HexadecimalToByte(successNonce))));
            Console.ReadKey();
        }
    }
}
