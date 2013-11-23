using System;
namespace PasswordChange.ViewModel.Services
{
    public interface IHelperService
    {
        bool IsWhiteListedUserName(string userName);
        System.Threading.Tasks.Task ChangePassword(string userName, string oldPassword, string newPassword);
        string GetDefaultUserName();
        System.Threading.Tasks.Task Sleep(int milliseconds);
    }
}
