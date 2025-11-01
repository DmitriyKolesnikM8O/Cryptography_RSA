using System.Numerics;

namespace CryptoLib.RSA.Models
{
    public record RsaPublicKey(BigInteger E, BigInteger N);
}