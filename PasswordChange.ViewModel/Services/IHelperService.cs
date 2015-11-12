namespace PasswordChange.ViewModel.Services
{
    public interface IHelperService
    {
        System.Threading.Tasks.Task ChangePassword(string userName, string oldPassword, string newPassword);
        string GetDefaultUserName();
        System.Threading.Tasks.Task Sleep(int milliseconds);
        double GetRandomMultiplier();
        void ShowErrorDialog(string message, string title);
    }
}