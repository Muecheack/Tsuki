using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BotFace.Pages.Register
{
    internal partial class Register_VM : ObservableObject
    {
        // 由 View 注入：导航、获取密码、显示消息
        public Action GoToLogin { get; set; }
        public Func<string> SetPasswordRegister { get; set; }
        public Func<string> SetPasswordConfirm { get; set; }
        public Action<string> ShowMessageDialog { get; set; }

        [ObservableProperty]
        private string _nameRegister;
        [ObservableProperty]
        private string _passwordRegister;
        [ObservableProperty]
        private string _passwordConfirm;

        [RelayCommand]
        void Register()
        {
            // 从视图获取密码（如果视图注入了委托）
            PasswordRegister = SetPasswordRegister?.Invoke();
            PasswordConfirm = SetPasswordConfirm?.Invoke();

            // 校验流程，使用注入的 ShowMessageDialog 返回错误提示
            if (!NameContainsLetterOrChinese(NameRegister))
            {
                ShowMessageDialog?.Invoke("昵称必须是字母或汉字");
                return;
            }

            if (!NameLengthValid(NameRegister))
            {
                ShowMessageDialog?.Invoke("昵称长度不符合要求：最多12字节(utf-8)");
                return;
            }

            if (!NameHasNoDigits(NameRegister))
            {
                ShowMessageDialog?.Invoke("昵称不能包含数字");
                return;
            }

            if (!PasswordMeetsComplexity(PasswordRegister))
            {
                ShowMessageDialog?.Invoke("密码必须包含字母、数字和特殊字符");
                return;
            }

            if(!IsPasswordRight(PasswordRegister, PasswordConfirm))
            {
                ShowMessageDialog?.Invoke("这密码不对吧?");
                return;
            }

            // 注册通过，执行后续逻辑（这里简单显示并跳转到登录）
            // TODO: 到时候吧弹窗删了，直接导航到Home
            ShowMessageDialog?.Invoke("注册成功");
        }

        [RelayCommand]
        void GoLogin()
        {
            GoToLogin?.Invoke();
        }

        // 昵称必须含有字母/汉字
        public bool NameContainsLetterOrChinese(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            foreach (var c in name)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) return true;
                if (c >= '\u4E00' && c <= '\u9FFF') return true; // 常用汉字范围
            }
            return false;
        }

        // 昵称不能超过12个字母/6个汉字
        public bool NameLengthValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            int latin = 0, chinese = 0, others = 0;
            foreach (var c in name)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) latin++;
                else if (c >= '\u4E00' && c <= '\u9FFF') chinese++;
                else if (!char.IsWhiteSpace(c)) others++;
            }
            if (others > 0) return false; // 禁止除字母/汉字以外的字符
            return latin <= 12 && chinese <= 6 && (latin + chinese) > 0;
        }

        // 昵称不能含有数字
        public bool NameHasNoDigits(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            return !name.Any(char.IsDigit);
        }

        // 密码必须包含字母、数字、特殊字符
        public bool PasswordMeetsComplexity(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            bool hasLetter = false, hasDigit = false, hasSpecial = false;
            foreach (var c in password)
            {
                if (char.IsLetter(c)) hasLetter = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecial = true;
            }
            return hasLetter && hasDigit && hasSpecial;
        }
        
        // “确认密码”通过返回true 
        public bool IsPasswordRight(string password, string password2)
        {
            if (password != password2) return false;
            return true;
        }
    }
}
