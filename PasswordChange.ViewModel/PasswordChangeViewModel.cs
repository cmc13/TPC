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
        #region Private Data Members

        private readonly IHelperService service;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string userName;
        private string password = "";
        private int timesToChange = 30;
        private decimal delay = 1.0m;
        private bool randomizeDelay;
        private bool isBusy = false;
        private string busyStatus = "";
        private int timesChanged;

        #endregion

        #region Constructor Definition

        [ImportingConstructor]
        public PasswordChangeViewModel(IHelperService helperService)
        {
            if (helperService == null)
                throw new ArgumentNullException("helperService");

            this.service = helperService;

            this.userName = this.service.GetDefaultUserName();

            this.GoCommand = new RelayCommand(ChangePassword,
                () => !string.IsNullOrEmpty(this.UserName) && !string.IsNullOrEmpty(this.Password));
        }

        #endregion

        #region Command Definitions

        public RelayCommand GoCommand { get; private set; }

        #endregion

        #region Public Property Definitions

        public string UserName
        {
            get { return this.userName; }
            set
            {
                if (this.userName != value)
                {
                    this.userName = value;
                    base.RaisePropertyChanged(() => this.UserName);
                    this.GoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                if (this.password != value)
                {
                    this.password = value;
                    base.RaisePropertyChanged(() => this.Password);
                    this.GoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public int TimesToChange
        {
            get { return this.timesToChange; }
            set
            {
                if (this.timesToChange != value)
                {
                    this.timesToChange = value;
                    base.RaisePropertyChanged(() => this.TimesToChange);
                }
            }
        }

        public decimal Delay
        {
            get { return this.delay; }
            set
            {
                if (this.delay != value)
                {
                    this.delay = value;
                    base.RaisePropertyChanged(() => this.Delay);
                }
            }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    base.RaisePropertyChanged(() => this.IsBusy);
                }
            }
        }

        public string BusyStatus
        {
            get { return this.busyStatus; }
            set
            {
                if (this.busyStatus != value)
                {
                    this.busyStatus = value;
                    base.RaisePropertyChanged(() => this.BusyStatus);
                }
            }
        }

        public int TimesChanged
        {
            get { return this.timesChanged; }
            set
            {
                if (this.timesToChange != value)
                {
                    this.timesChanged = value;
                    base.RaisePropertyChanged(() => this.TimesChanged);
                }
            }
        }

        public bool RandomizeDelay
        {
            get { return this.randomizeDelay; }
            set
            {
                if (this.randomizeDelay != value)
                {
                    this.randomizeDelay = value;
                    base.RaisePropertyChanged(() => this.RandomizeDelay);
                }
            }
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
                    await service.ChangePassword(UserName, currentPassword, newPassword);
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
                        await service.Sleep(delay);
                }

                // Change back to original password
                newPassword = originalPassword;
                await service.ChangePassword(UserName, currentPassword, newPassword);

                BusyStatus = "Done";
                IsBusy = false;
            }
            catch (Exception ex)
            {
                log.ErrorException(string.Format("Unexpected exception thrown while trying to change password from {0} to {1}.",
                    currentPassword, newPassword), ex);
            }
            finally
            {
                Password = "";
            }
        }
    }
}
