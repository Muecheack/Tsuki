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
using BotFace.Pages.Login;

namespace BotFace.Pages.Login
{
    /// <summary>
    /// Login_View.xaml 的交互逻辑
    /// </summary>
    public partial class Login_View : Page
    {
        public Login_View()
        {
            InitializeComponent();
            var login_VM = new Login_VM();
            this.DataContext = login_VM;
            login_VM.SetPasswordAction = () => PasswordInput.Password; // 拿密码
            login_VM.GotoRegister = () => this.NavigationService?.Navigate(new Register.Register_View()); // 导航到注册
        }
    }
}
