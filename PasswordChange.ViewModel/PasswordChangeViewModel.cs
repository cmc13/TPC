using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using PasswordChange.ViewModel.Services;

namespace PasswordChange.ViewModel
{
    [Export]
    public class PasswordChangeViewModel : ViewModelBase
    {
        #region Command Definitions

        public RelayCommand GoCommand { get; private set; }

        #endregion

        #region Public Property Definitions

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
                GoCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
                GoCommand.RaiseCanExecuteChanged();
            }
        }

        private int _timesToChange = 30;
        public int TimesToChange
        {
            get { return _timesToChange; }
            set
            {
                _timesToChange = value;
                RaisePropertyChanged("TimesToChange");
            }
        }

        private decimal _delay = 1.0m;
        public decimal Delay
        {
            get { return _delay; }
            set
            {
                _delay = value;
                RaisePropertyChanged("Delay");
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        private string _busyStatus = "";
        public string BusyStatus
        {
            get { return _busyStatus; }
            set
            {
                _busyStatus = value;
                RaisePropertyChanged("BusyStatus");
            }
        }

        private int _timesChanged;
        public int TimesChanged
        {
            get { return _timesChanged; }
            set
            {
                _timesChanged = value;
                RaisePropertyChanged("TimesChanged");
            }
        }

        private bool _randomizeDelay;
        public bool RandomizeDelay
        {
            get { return _randomizeDelay; }
            set
            {
                _randomizeDelay = value;
                RaisePropertyChanged("RandomizeDelay");
            }
        }

        #endregion

        private readonly IHelperService _service;

        private static Logger Log
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }

        #region Constructor Definition

        [ImportingConstructor]
        public PasswordChangeViewModel(IHelperService helperService)
        {
            if (helperService == null)
                throw new ArgumentNullException("helperService");

            _service = helperService;

            _userName = _service.GetDefaultUserName();

            if (!_service.IsWhiteListedUserName(_userName))
                throw new InvalidOperationException("TPC failed to initialize...");

            GoCommand = new RelayCommand(ChangePassword,
                () => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password));
        }

        #endregion

        private async void ChangePassword()
        {
            string originalPassword = Password;
            string currentPassword = originalPassword;
            string newPassword = "";
            Random rand = new Random();

            try
            {
                TimesChanged = 0;
                BusyStatus = string.Format("{0}/{1}", TimesChanged, TimesToChange);
                IsBusy = true;

                int delay = Convert.ToInt32((double)Delay * 1000);
                for (int count = 0; count < TimesToChange; ++count)
                {
                    newPassword = originalPassword + count.ToString();
                    await _service.ChangePassword(UserName, currentPassword, newPassword);
                    currentPassword = newPassword;

                    TimesChanged = count + 1;
                    BusyStatus = string.Format("{0}/{1}", TimesChanged, TimesToChange);

                    if (RandomizeDelay)
                    {
                        double multiplier;
                        if (rand.NextDouble() < 0.5)
                            multiplier = (rand.NextDouble() / 2) + 0.5;
                        else // more than delay
                            multiplier = (rand.NextDouble() * 4) + 1;

                        delay = Convert.ToInt32((double)Delay * multiplier * 1000);
                    }

                    if (delay > 0)
                        await _service.Sleep(delay);
                }

                // Change back to original password
                newPassword = originalPassword;
                await _service.ChangePassword(UserName, currentPassword, newPassword);

                BusyStatus = "Done";
                IsBusy = false;
            }
            catch (Exception ex)
            {
                Log.ErrorException(string.Format("Unexpected exception thrown while trying to change password from {0} to {1}.",
                    currentPassword, newPassword), ex);
            }
            finally
            {
                Password = "";
            }
        }
    }
}
