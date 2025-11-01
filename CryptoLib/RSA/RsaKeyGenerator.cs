using System.Numerics;
using CryptoLib.Enums;
using CryptoLib.Interfaces;
using CryptoLib.Primality;
using CryptoLib.Primality.Implementations;
using CryptoLib.RSA.Models;

namespace CryptoLib.RSA
{
    public class RsaKeyGenerator
    {
        private readonly IPrimalityTest _primalityTest;
        private readonly ICryptoMathService _mathService;
        private readonly double _probability;
        private readonly int _bitLength;

        public RsaKeyGenerator(PrimalityTestType testType, double probability, int bitLength, ICryptoMathService mathService)
        {
            _probability = probability;
            _bitLength = bitLength;
            _mathService = mathService;
            
            // Фабрика для выбора реализации теста простоты
            _primalityTest = testType switch
            {
                PrimalityTestType.Fermat => new FermatTest(mathService),
                PrimalityTestType.SoloveyStrassen => new SoloveyStrassenTest(mathService),
                PrimalityTestType.MillerRabin => new MillerRabinTest(mathService),
                _ => throw new ArgumentException("Неизвестный тип теста простоты", nameof(testType)),
            };
        }

        public RsaKeyPair GenerateKeyPair()
        {
            while (true)
            {
                // Генерируем два "сильных" простых числа p и q
                BigInteger p = GenerateStrongPrime(_bitLength);
                BigInteger q;
                do {
                    q = GenerateStrongPrime(_bitLength);
                } while (p == q);

                // Вычисляем модуль N и функцию Эйлера phi(N)
                BigInteger n = p * q;
                BigInteger phi = (p - 1) * (q - 1);

                // Выбираем открытую экспоненту e
                BigInteger e = 65537;
                
                while (_mathService.Gcd(e, phi) != 1) {
                    e += 2; 
                }

                // Вычисляем секретную экспоненту d
                var (_, x, _) = _mathService.ExtendedGcd(e, phi);
                BigInteger d = (x % phi + phi) % phi;

                // Проверка безопасности (защита от атаки Винера)
                // d должно быть больше, чем N^(1/4)
                if (IsWienerAttackSafe(d, n))
                {
                    return new RsaKeyPair(new RsaPublicKey(e, n), new RsaPrivateKey(d, n));
                }
                
            }
        }
        
        // Генерация "сильного" простого числа для защиты от атаки Ферма
        private BigInteger GenerateStrongPrime(int bitLength)
        {
            while (true)
            {
                
                var candidate = PrimalityTestBase.GenerateRandomBigInteger(
                    BigInteger.Pow(2, bitLength - 1),
                    BigInteger.Pow(2, bitLength) - 1
                );
                if (_primalityTest.IsPrime(candidate, _probability))
                {
                
                    if (_primalityTest.IsPrime((candidate - 1) / 2, _probability))
                    {
                        return candidate;
                    }
                }
            }
        }
        
        // Проверка на уязвимость к атаке Винера
        private bool IsWienerAttackSafe(BigInteger d, BigInteger n)
        {
            // d должно быть > n^(1/4)
            // Приближенная проверка: битовая длина d должна быть > 1/4 битовой длины n
            return d.ToByteArray().Length > n.ToByteArray().Length / 4.0;
        }
    }
}