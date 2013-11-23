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

        public bool IsWhiteListedUserName(string userName)
        {
            var whitelistedUserNames = new List<string>();

            string encryptedKey = ConfigurationManager.AppSettings["key"];
            byte decryptKey = Convert.ToByte(base85Decode(encryptedKey));
            string encryptedBase64EncodedUserList = ConfigurationManager.AppSettings["wl_users"];
            string base64EncodedUserList = DecryptString(encryptedBase64EncodedUserList, decryptKey);
            byte[] decoded = Convert.FromBase64String(base64EncodedUserList);
            string users = System.Text.Encoding.ASCII.GetString(decoded);
            if (!string.IsNullOrWhiteSpace(users))
                whitelistedUserNames.AddRange(users.Split(';'));

            return whitelistedUserNames.Any(s => s.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
        }

        public string DecryptString(string encryptedString, byte key)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in encryptedString)
                sb.Append((char)(c ^ key));

            return sb.ToString();
        }

        private static string base85Decode(string encodedString)
        {
            string prefixMark = "<~";
            string suffixMark = "~>";
            int _asciiOffset = 33;
            uint _tuple = 0;
            uint[] pow85 = { 85 * 85 * 85 * 85, 85 * 85 * 85, 85 * 85, 85, 1 };
            byte[] _encodedBlock = new byte[5];

		    // strip prefix and suffix if present
            if (encodedString.StartsWith(prefixMark))
                encodedString = encodedString.Substring(prefixMark.Length);
            if (encodedString.EndsWith(suffixMark))
                encodedString = encodedString.Substring(0, encodedString.Length - suffixMark.Length);

		    MemoryStream ms = new MemoryStream();
		    int count = 0;
		    bool processChar = false;

            byte[] _decodedBlock = new byte[4];
            foreach (char c in encodedString)
		    {
			    switch (c)
			    {
				    case 'z':
					    if (count != 0)
					    {
						    throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
					    }
					    _decodedBlock[0] = 0;
					    _decodedBlock[1] = 0;
					    _decodedBlock[2] = 0;
					    _decodedBlock[3] = 0;
					    ms.Write(_decodedBlock, 0, _decodedBlock.Length);
					    processChar = false;
					    break;
				    case '\n': case '\r': case '\t': case '\0': case '\f': case '\b': 
					    processChar = false;
					    break;
				    default:
					    if (c < '!' || c > 'u')
					    {
						    throw new Exception("Bad character '" + c + "' found. ASCII85 only allows characters '!' to 'u'.");
					    }
					    processChar = true;
					    break;
			    }

			    if (processChar)
			    {
				    _tuple += ((uint)(c - _asciiOffset) * pow85[count]);
				    count++;
				    if (count == _encodedBlock.Length)
				    {                    
					    DecodeBlock(_decodedBlock, _tuple, _decodedBlock.Length);
					    ms.Write(_decodedBlock, 0, _decodedBlock.Length);
					    _tuple = 0;
					    count = 0;
				    }                            
			    }
		    }

		    // if we have some bytes left over at the end..
		    if (count != 0)
		    {
			    if (count == 1) 
			    {
				    throw new Exception("The last block of ASCII85 data cannot be a single byte.");
			    }
			    count--;
			    _tuple += pow85[count];
			    DecodeBlock(_decodedBlock, _tuple, count);
			    for (int i = 0; i < count; i++)
			    {
				    ms.WriteByte(_decodedBlock[i]);
			    }
		    }

            return Encoding.ASCII.GetString(ms.ToArray());
        }

        private static void DecodeBlock(byte[] _decodedBlock, uint _tuple, int bytes)
        {
            for (int i = 0; i < bytes; i++)
            {
                _decodedBlock[i] = (byte)(_tuple >> 24 - (i * 8));
            }
        }
    }
}
