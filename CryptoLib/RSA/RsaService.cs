using System.Numerics;
using CryptoLib.Core;
using CryptoLib.Enums;
using CryptoLib.Interfaces;
using CryptoLib.RSA.Models;

namespace CryptoLib.RSA
{
    public class RsaService : IRsaService
    {
        private readonly RsaKeyGenerator _keyGenerator;
        private readonly ICryptoMathService _mathService;

        /// <summary>
        /// Конструктор сервиса-обертки.
        /// Он принимает параметры и делегирует их вложенному сервису RsaKeyGenerator.
        /// </summary>
        public RsaService(PrimalityTestType testType, double probability, int bitLength)
        {
            _mathService = new CryptoMathService();
            _keyGenerator = new RsaKeyGenerator(testType, probability, bitLength, _mathService);
        }

        public RsaKeyPair GenerateKeyPair()
        {
            return _keyGenerator.GenerateKeyPair();
        }

        public BigInteger Encrypt(BigInteger message, RsaPublicKey key)
        {
            // C = M^e mod N
            return _mathService.ModPow(message, key.E, key.N);
        }

        public BigInteger Decrypt(BigInteger ciphertext, RsaPrivateKey key)
        {
            // M = C^d mod N
            return _mathService.ModPow(ciphertext, key.D, key.N);
        }
    }
}