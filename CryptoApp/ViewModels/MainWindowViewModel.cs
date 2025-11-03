using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CryptoLib.Enums;
using CryptoLib.Interfaces;
using CryptoLib.RSA;
using CryptoLib.RSA.Models;

namespace CryptoApp.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _selectedBitLength = 256;
        public List<int> AvailableBitLengths { get; } = new List<int> { 128, 256, 512, 1024 };

        [ObservableProperty]
        private PrimalityTestType _selectedTestType = PrimalityTestType.MillerRabin;
        public IEnumerable<PrimalityTestType> TestTypes => Enum.GetValues(typeof(PrimalityTestType)).Cast<PrimalityTestType>();
        
        // --- ВОЗВРАЩАЕМ К НЕ-NULLABLE СТРОКАМ, ЧТОБЫ НЕ БЫЛО ПРЕДУПРЕЖДЕНИЙ ---
        [ObservableProperty]
        private string _publicKeyE = "";
        [ObservableProperty]
        private string _publicKeyN = "";
        [ObservableProperty]
        private string _privateKeyD = "";

        [ObservableProperty]
        private string _messageToEncrypt = "123456789";
        [ObservableProperty]
        private string _encryptedMessage = "";
        [ObservableProperty]
        private string _decryptedMessage = "";
        
        [ObservableProperty]
        private string _statusMessage = "Готово к работе.";
        
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GenerateKeysCommand))]
        [NotifyCanExecuteChangedFor(nameof(EncryptCommand))]
        [NotifyCanExecuteChangedFor(nameof(DecryptCommand))]
        private bool _isBusy = false;

        private RsaKeyPair? _currentKeyPair;
        private const double Probability = 0.9999;

        [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
        private async Task GenerateKeysAsync()
        {
            IsBusy = true;
            StatusMessage = "Идет генерация ключей (это может занять время)...";
            ClearFields(); // Очищаем поля перед новой генерацией

            try
            {
                int bits = SelectedBitLength;

                await Task.Run(() =>
                {
                    // Создаем сервис прямо здесь
                    IRsaService rsaService = new RsaService(SelectedTestType, Probability, bits);
                    _currentKeyPair = rsaService.GenerateKeyPair();
                });

                if (_currentKeyPair != null)
                {
                    PublicKeyE = _currentKeyPair.PublicKey.E.ToString();
                    PublicKeyN = _currentKeyPair.PublicKey.N.ToString();
                    PrivateKeyD = _currentKeyPair.PrivateKey.D.ToString();
                    StatusMessage = "Ключи успешно сгенерированы!";
                }
                else
                {
                    StatusMessage = "Не удалось сгенерировать ключи.";
                }
                
                // --- УДАЛЯЕМ ДУБЛИРУЮЩИЙСЯ БЛОК ОТСЮДА ---
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при генерации ключей: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
        private void Encrypt()
        {
            if (_currentKeyPair == null)
            {
                StatusMessage = "Сначала сгенерируйте ключи.";
                return;
            }
            
            try
            {
                var rsaService = new RsaService(SelectedTestType, Probability, SelectedBitLength);
                var message = BigInteger.Parse(MessageToEncrypt);
                var encrypted = rsaService.Encrypt(message, _currentKeyPair.PublicKey);
                EncryptedMessage = encrypted.ToString();
                StatusMessage = "Сообщение успешно зашифровано.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка шифрования: {ex.Message}";
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
        private void Decrypt()
        {
            if (_currentKeyPair == null || string.IsNullOrEmpty(EncryptedMessage))
            {
                StatusMessage = "Сначала зашифруйте сообщение.";
                return;
            }

            try
            {
                var rsaService = new RsaService(SelectedTestType, Probability, SelectedBitLength);
                var encrypted = BigInteger.Parse(EncryptedMessage);
                var decrypted = rsaService.Decrypt(encrypted, _currentKeyPair.PrivateKey);
                DecryptedMessage = decrypted.ToString();
                StatusMessage = "Сообщение успешно расшифровано.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка дешифрования: {ex.Message}";
            }
        }
        
        private bool CanExecuteCommands() => !IsBusy;
        
        private void ClearFields() 
        { 
            _currentKeyPair = null; 
            PublicKeyE = ""; 
            PublicKeyN = ""; 
            PrivateKeyD = ""; 
            EncryptedMessage = ""; 
            DecryptedMessage = ""; 
        }
    }
}