using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using HybridCryptographyAlgorithm.Algorithms;
using HybridCryptographyAlgorithm.Utility;

namespace HybridCryptographyAlgorithm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UpdateResult? lastUpdateCheck;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                ShowUpdateResult(await UpdateChecker.CheckAsync());
            };
        }

        // Marks the About button when a newer release exists.
        internal void ShowUpdateResult(UpdateResult result)
        {
            lastUpdateCheck = result;
            bool available = result.Status == UpdateStatus.Available;
            updateDot.Visibility = available ? Visibility.Visible : Visibility.Collapsed;
            btnAbout.ToolTip = available ? $"About: {result.LatestTag} is out" : "About";
        }

        private void About_Button_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow(this, lastUpdateCheck).ShowDialog();
        }

        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        enum enCryptoMethod
        {
            Encrypt,
            Decrypt,
        }

        // Choose text file
        private void PickTextFileOnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Select a .txt file"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;

                // Set the TextBox text to the selected file path
                TextBox? text = sender as TextBox;
                text!.Text = filePath;
            }
        }

        // Navigate between methods : Encrypt, Decrypt
        private void Encrypt_Button_Click(object sender, RoutedEventArgs e)
        {
            // Here
            if (ValidateInput(enCryptoMethod.Encrypt))
            {
                string filePath = txtFilePathForEncrypt.Text;
                string plainText = FileConvertor.TextFileToString(filePath);
                string cipher;
                string cipherFilePath;

                if (string.IsNullOrEmpty(plainText))
                {
                    MessageBox.Show("File is empty.", "Error!");
                    return;
                }

                byte[] aesKey = GenerateRandomByteArray(16);
                byte[] desKey = GenerateRandomByteArray(8);

                HybridCryptor cryptor = new HybridCryptor(desKey, aesKey);
                cipher = cryptor.Encrypt(plainText);

                cipherFilePath = FileConvertor.StringToTextFile(cipher);

                txtHybridKeyForDecrypt.Text = Convert.ToBase64String(cryptor.DesKey) + Convert.ToBase64String(cryptor.AesKey);
                txtFilePathForDecrypt.Text = cipherFilePath;
            }
            else
            {
                MessageBox.Show("Invalid inputs!");
            }
        }

        private void Decrypt_Button_Click(object sender, RoutedEventArgs e)
        {
            // Here
            if (ValidateInput(enCryptoMethod.Decrypt))
            {
                string filePath = txtFilePathForDecrypt.Text;
                string cipherText = FileConvertor.TextFileToString(filePath);
                string hybridKey = txtHybridKeyForDecrypt.Text;
                string plain;
                string cipherFilePath;

                if (string.IsNullOrEmpty(cipherText))
                {
                    MessageBox.Show("File is empty.", "Error!");
                    return;
                }

                try
                {
                    byte[] desKey = Convert.FromBase64String(hybridKey.Substring(0, 12));
                    byte[] aesKey = Convert.FromBase64String(hybridKey.Substring(12, 24));
                    HybridCryptor cryptor = new HybridCryptor(desKey, aesKey);

                    plain = cryptor.Decrypt(cipherText);
                }
                catch (Exception ex) when (ex is FormatException or ArgumentException or CryptographicException)
                {
                    MessageBox.Show("Wrong hybrid key, or the file isn't a cipher from this app.", "Error!");
                    return;
                }

                cipherFilePath = FileConvertor.StringToTextFile(plain);

                txtFilePathForEncrypt.Text = cipherFilePath;
            }
            else
            {
                MessageBox.Show("Invalid inputs!");
            }
        }

        private bool ValidateInput(enCryptoMethod method)
        {
            //
            return true;
        }

        static byte[] GenerateRandomByteArray(int size)
        {
            byte[] result = new byte[size];
            Random random = new Random();

            // Fill the byte array with random bytes
            random.NextBytes(result);

            return result;
        }

        // Drag the Window
        private void Drag_Window(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                // Allow window to be dragged
                this.DragMove();
            }
        }

    }
}
