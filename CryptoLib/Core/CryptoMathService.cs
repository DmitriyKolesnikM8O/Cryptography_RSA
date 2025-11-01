using CryptoLib.Interfaces;
using System.Numerics;

namespace CryptoLib.Core
{
    /// <summary>
    /// Реализация сервиса для выполнения базовых криптографических
    /// математических операций.
    /// </summary>
    public class CryptoMathService : ICryptoMathService
    {
        /// <summary>
        /// Вычисляет Наибольший Общий Делитель (НОД) двух целых чисел
        /// при помощи алгоритма Евклида.
        /// </summary>
        /// <param name="a">Первое целое число.</param>
        /// <param name="b">Второе целое число.</param>
        /// <returns>Наибольший общий делитель чисел a и b.</returns>
        public BigInteger Gcd(BigInteger a, BigInteger b)
        {
            a = BigInteger.Abs(a);
            b = BigInteger.Abs(b);

            while (b != 0)
            {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }

        /// <summary>
        /// Выполняет расширенный алгоритм Евклида для нахождения НОД
        /// двух чисел, а также коэффициентов x и y, удовлетворяющих
        /// соотношению Безу: a*x + b*y = НОД(a, b).
        /// </summary>
        /// <param name="a">Первое целое число.</param>
        /// <param name="b">Второе целое число.</param>
        /// <returns>Кортеж, содержащий (НОД, x, y).</returns>
        public (BigInteger Gcd, BigInteger X, BigInteger Y) ExtendedGcd(BigInteger a, BigInteger b)
        {
            // old_x соответствует a, x соответствует b
            BigInteger old_x = 1, x = 0;
            // old_y соответствует a, y соответствует b
            BigInteger old_y = 0, y = 1;

            while (b != 0)
            {
                var quotient = a / b;

                var temp_b = b;
                b = a % b;
                a = temp_b;

                var temp_x = x;
                x = old_x - quotient * x;
                old_x = temp_x;

                // И коэффициенты y.
                var temp_y = y;
                y = old_y - quotient * y;
                old_y = temp_y;
            }

            if (a < 0)
            {
                a = -a;
                old_x = -old_x;
                old_y = -old_y;
            }

            // a содержит НОД(a, b)
            // old_x является искомым коэффициентом x
            // old_y является искомым коэффициентом y
            return (a, old_x, old_y);
        }

        /// <summary>
        /// Выполняет операцию возведения в степень по модулю (value^exponent) mod modulus.
        /// Использует алгоритм бинарного возведения в степень для эффективности.
        /// </summary>
        /// <param name="value">Основание.</param>
        /// <param name="exponent">Показатель степени (должен быть неотрицательным).</param>
        /// <param name="modulus">Модуль (должен быть больше 0).</param>
        /// <returns>Результат операции (value^exponent) mod modulus.</returns>
        public BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus)
        {
            if (modulus <= 0)
                throw new ArgumentException("Модуль должен быть положительным числом.", nameof(modulus));

            if (exponent < 0)
                throw new ArgumentException("Показатель степени не может быть отрицательным.", nameof(exponent));

            if (modulus == 1)
                return 0;

            BigInteger result = 1;

            value %= modulus;

            while (exponent > 0)
            {
                if (exponent % 2 == 1)
                {
                    result = result * value % modulus;
                }
                exponent >>= 1;

                value = value * value % modulus;
            }

            return result;
        }

        /// <summary>
        /// Вычисляет символ Лежандра (a/p) с использованием критерия Эйлера.
        /// (a/p) ≡ a^((p-1)/2) (mod p)
        /// </summary>
        /// <param name="a">Целое число.</param>
        /// <param name="p">Нечетное простое число. Метод не выполняет проверку p на простоту,
        /// предполагая, что это гарантируется вызывающей стороной.</param>
        /// <returns>
        ///  1, если a является квадратичным вычетом по модулю p.
        /// -1, если a является квадратичным невычетом по модулю p.
        ///  0, если a кратно p.
        /// </returns>
        public int LegendreSymbol(BigInteger a, BigInteger p)
        {
            if (p < 3 || p % 2 == 0)
                throw new ArgumentException("Модуль p должен быть нечетным простым числом больше 2.", nameof(p));

            var exponent = (p - 1) / 2;

            var result = ModPow(a, exponent, p);

            if (a % p == 0)
            {
                return 0;
            }

            if (result == 1)
            {
                return 1;
            }

            return -1;
        }
        
        /// <summary>
        /// Вычисляет символ Якоби (a/n) с использованием свойств
        /// квадратичной взаимности без факторизации n.
        /// </summary>
        /// <param name="a">Целое число.</param>
        /// <param name="n">Положительное нечетное целое число.</param>
        /// <returns>
        ///  1 или -1, если НОД(a, n) = 1.
        ///  0, если НОД(a, n) > 1.
        /// </returns>
        public int JacobiSymbol(BigInteger a, BigInteger n)
        {
            if (n <= 0 || n % 2 == 0)
                throw new ArgumentException("n должно быть положительным нечетным числом.", nameof(n));
   
            if (n == 1) return 1;
            
            // Приводим 'a' к диапазону [0, n-1]
            a %= n;
            if (a < 0) a += n;
            if (a == 0) return 0;
            
            int j = 1;
            
            // 3. Обработка двоек в 'a'
            while (a % 2 == 0)
            {
                a /= 2;
                var n_mod_8 = n % 8;
                if (n_mod_8 == 3 || n_mod_8 == 5)
                {
                    j = -j;
                }
            }

            // 4. Закон квадратичной взаимности
            if ((a % 4 == 3) && (n % 4 == 3))
            {
                j = -j;
            }

            return j * JacobiSymbol(n % a, a);
        }
    }
}