using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BotFace.Pages.Register
{
    /// <summary>
    /// Register_View.xaml 的交互逻辑
    /// </summary>
    public partial class Register_View : Page
    {
        public Register_View()
        {
            InitializeComponent();
            var vm = new Register_VM();
            DataContext = vm;
            vm.SetPasswordRegister = () => PasswordInput.Password;
            vm.SetPasswordConfirm = () => ConfirmPasswordInput.Password;
            vm.ShowMessageDialog = tip => MessageBox.Show(tip);
            vm.GoToLogin = () => this.NavigationService?.Navigate(new Login.Login_View());
        }
    }
}
