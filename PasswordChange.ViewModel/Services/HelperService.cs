using System;
using System.ComponentModel.Composition;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

namespace PasswordChange.ViewModel.Services
{
    [Export(typeof(IHelperService))]
    class HelperService : IHelperService
    {
        private static readonly Random rand = new Random();

        public string GetDefaultUserName()
        {
            return Environment.UserName;
        }

        public async Task ChangePassword(string userName, string oldPassword, string newPassword)
        {
            await Task.Factory.StartNew(() =>
                {
                    using (var context = new PrincipalContext(ContextType.Domain))
                    using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                    {
                        user.ChangePassword(oldPassword, newPassword);
                    }
                });
        }

        public async Task Sleep(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        public double GetRandomMultiplier()
        {
            double multiplier;
            if (rand.NextDouble() < 0.5)
                multiplier = (rand.NextDouble() / 2) + 0.5;
            else // more than delay
                multiplier = (rand.NextDouble() * 4) + 1;
            return multiplier;
        }
    }
}
