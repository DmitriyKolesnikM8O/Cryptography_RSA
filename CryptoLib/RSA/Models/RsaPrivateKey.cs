using System.Numerics;

namespace CryptoLib.RSA.Models
{
    public record RsaPrivateKey(BigInteger D, BigInteger N);
}