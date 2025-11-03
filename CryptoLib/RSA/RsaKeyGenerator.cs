using System.Numerics;
using CryptoLib.Enums;
using CryptoLib.Interfaces;
using CryptoLib.Primality;
using CryptoLib.Primality.Implementations;
using CryptoLib.RSA.Models;
using System.Security.Cryptography;

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

        public RsaKeyPair GenerateKeyPair(BigInteger e)
        {
            if (e < 3 || e % 2 == 0)
            {
                throw new ArgumentException("Открытая экспонента 'e' должна быть нечетным числом больше 1.", nameof(e));
            }
            while (true)
            {
                // Генерируем два "сильных" простых числа p и q
                // BigInteger p = GenerateStrongPrime(_bitLength);
                BigInteger p = GeneratePrime(_bitLength);
                BigInteger q;
                do
                {
                    // q = GenerateStrongPrime(_bitLength);
                    q = GeneratePrime(_bitLength);
                } while (p == q);

                // Вычисляем модуль N и функцию Эйлера phi(N)
                BigInteger n = p * q;
                BigInteger phi = (p - 1) * (q - 1);

                while (_mathService.Gcd(e, phi) != 1)
                {
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
        
        /// <summary>
        /// Генерирует вероятно простое число заданной битности.
        /// </summary>
        private BigInteger GeneratePrime(int bits)
        {
            // Вероятность того, что случайное нечетное число N - простое, примерно 2/ln(N).
            // Для 1024 бит это означает, что нам нужно проверить в среднем ~355 кандидатов.
            while (true)
            {
                // 1. Создаем случайную последовательность байт нужной длины.
                int byteCount = (bits + 7) / 8;
                byte[] bytes = RandomNumberGenerator.GetBytes(byteCount);

                // 2. Формируем из нее число, гарантируя нужную битность и нечетность.
                // Устанавливаем старший бит, чтобы число имело длину ровно 'bits'.
                int lastByteBits = bits % 8;
                if (lastByteBits == 0) lastByteBits = 8;
                
                // Маска для установки старшего бита (например, 10000000)
                byte msbMask = (byte)(1 << (lastByteBits - 1));
                bytes[byteCount - 1] |= msbMask;
                
                // Маска для очистки лишних битов (например, 00011111)
                if(lastByteBits != 8)
                {
                    byte lsbMask = (byte)(0xFF >> (8 - lastByteBits));
                    bytes[byteCount - 1] &= lsbMask;
                }

                // Устанавливаем младший бит, чтобы число было нечетным.
                bytes[0] |= 1;

                var candidate = new BigInteger(bytes, isUnsigned: true);
                
                // 3. Проверяем кандидата на простоту.
                if (_primalityTest.IsPrime(candidate, _probability))
                {
                    return candidate;
                }
            }
        }
    }
}