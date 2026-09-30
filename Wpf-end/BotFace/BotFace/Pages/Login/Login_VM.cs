using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace BotFace.Pages.Login
{
    public partial class Login_VM : ObservableObject
    {
        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userPassword;

        public Func<string> SetPasswordAction { get; set; }
        public Action GotoRegister { get; set; }

        // 名称为空返回 true
        bool VerifyName(string name) => string.IsNullOrWhiteSpace(name);
        // 密码为空返回 true
        bool VerifyPassword(string password) => string.IsNullOrWhiteSpace(password);
        // 不是空且长度小于4就返回false
        bool VerifyPasswordLength(string password) => password.Length < 4 && !string.IsNullOrWhiteSpace(password);

        [RelayCommand]
        void Login()
        {
            UserPassword = SetPasswordAction?.Invoke();
            if (VerifyName(UserName)) { MessageBox.Show("昵称没输!"); return; }
            if (VerifyPassword(UserPassword)) { MessageBox.Show("密码没输!"); return; }
            if (VerifyPasswordLength(UserPassword)) { MessageBox.Show("密码太短了!至少4个"); return; }
            MessageBox.Show($"欢迎!{UserName}");

        }

        [RelayCommand]
        void GoRegister()
        {
            GotoRegister?.Invoke();
        }
    }
}
