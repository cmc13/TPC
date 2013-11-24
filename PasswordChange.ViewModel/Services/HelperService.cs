using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PasswordChange.ViewModel.Services
{
    [Export(typeof(IHelperService))]
    class HelperService : IHelperService
    {
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
    }
}
