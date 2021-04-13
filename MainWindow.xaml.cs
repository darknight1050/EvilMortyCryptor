using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EvilMortyCryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        enum CryptorState
        {
            ENCRYPT, DECRYPT
        }

        private DataEncryptor _dataEncryptor;
        private DataDecryptor _dataDecryptor;

        private CryptorState _state;
        private CryptorState State { 
            get { return _state; } 
            set 
            {
                _state = value;
                if (_state == CryptorState.ENCRYPT)
                {
                    if (_encryptStateButtonBorder != null)
                    {
                        _encryptStateButtonBorder.Background = _selectedStateBrush;
                        _encryptStateButtonBorder.BorderBrush = _selectedStateBrush;
                    }
                    if(_decryptStateButtonBorder != null)
                    {
                        _decryptStateButtonBorder.Background = _notSelectedStateBrush;
                        _decryptStateButtonBorder.BorderBrush = _notSelectedStateBrush;
                    }
                }
                if (_state == CryptorState.DECRYPT)
                {
                    if (_encryptStateButtonBorder != null)
                    {
                        _encryptStateButtonBorder.Background = _notSelectedStateBrush;
                        _encryptStateButtonBorder.BorderBrush = _notSelectedStateBrush;
                    }
                    if (_decryptStateButtonBorder != null)
                    {
                        _decryptStateButtonBorder.Background = _selectedStateBrush;
                        _decryptStateButtonBorder.BorderBrush = _selectedStateBrush;
                    }
                }
                if (_dataEncryptor.HasPublicKey() || _state != CryptorState.ENCRYPT)
                {
                    KeyRow.Height = new GridLength(0);
                }
                else
                {
                    KeyRow.Height = new GridLength(52);
                }
            }
        }

        private Brush _selectedStateBrush;
        private Brush _notSelectedStateBrush;
        private Brush _hoverBrush;

        private Border _encryptStateButtonBorder;
        private Border _decryptStateButtonBorder;

        public MainWindow()
        {
            _dataEncryptor = new DataEncryptor();
            _dataDecryptor = new DataDecryptor();
            _selectedStateBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27ab50"));
            _notSelectedStateBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40444b"));
            _hoverBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#787b82"));
            InitializeComponent();
            
        }

        private async void ShowCopiedPopup()
        {
            CopiedPopup.IsOpen = true;
            await Task.Delay(500);
            CopiedPopup.IsOpen = false;
        }
        
        private void DataChanged()
        {
            if (OutputTextBox == null)
                return;
            try
            {
                if (string.IsNullOrWhiteSpace(InputTextBox.Text))
                {
                    OutputTextBox.Text = "";
                }
                else
                {
                    if (_state == CryptorState.ENCRYPT)
                    {
                        OutputTextBox.Text = _dataEncryptor.EncryptToBase64(InputTextBox.Text);
                    }
                    if (_state == CryptorState.DECRYPT)
                    {
                        OutputTextBox.Text = _dataDecryptor.DecryptFromBase64(InputTextBox.Text);
                    }
                }
            }
            catch(Exception e)
            {
                OutputTextBox.Text = "Error";
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataChanged();
        }

        private void OutputTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(OutputTextBox.Text);
            ShowCopiedPopup();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EncryptStateButtonBorder_Loaded(object sender, RoutedEventArgs e)
        {
            _encryptStateButtonBorder = (Border)sender;
            State = CryptorState.ENCRYPT;
        }

        private void EncryptStateButton_Click(object sender, RoutedEventArgs e)
        {
            State = CryptorState.ENCRYPT;
            DataChanged();
        }

        private void EncryptStateButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if(State != CryptorState.ENCRYPT)
            {
                _encryptStateButtonBorder.Background = _hoverBrush;
                _encryptStateButtonBorder.BorderBrush = _hoverBrush;
            }
        }

        private void EncryptStateButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (State != CryptorState.ENCRYPT)
            {
                _encryptStateButtonBorder.Background = _notSelectedStateBrush;
                _encryptStateButtonBorder.BorderBrush = _notSelectedStateBrush;
            }
        }

        private void DecryptStateButtonBorder_Loaded(object sender, RoutedEventArgs e)
        {
            _decryptStateButtonBorder = (Border)sender;
        }

        private void DecryptStateButton_Click(object sender, RoutedEventArgs e)
        {
            State = CryptorState.DECRYPT;
            DataChanged();
        }

        private void DecryptStateButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (State != CryptorState.DECRYPT)
            {
                _decryptStateButtonBorder.Background = _hoverBrush;
                _decryptStateButtonBorder.BorderBrush = _hoverBrush;
            }
        }

        private void DecryptStateButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (State != CryptorState.DECRYPT)
            {
                _decryptStateButtonBorder.Background = _notSelectedStateBrush;
                _decryptStateButtonBorder.BorderBrush = _notSelectedStateBrush;
            }
        }

        private void CopyKeyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_dataDecryptor.GetPublicKey());
            ShowCopiedPopup();
        }

        private void PasteKeyButton_Click(object sender, RoutedEventArgs e)
        {
            KeyTextBox.Text = Clipboard.GetText();
        }

        private void KeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _dataEncryptor.SetPublicKey(KeyTextBox.Text);
            }
            catch
            {

            }
            DataChanged();
        }
    }
}
