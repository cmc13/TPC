using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using PasswordChange.ViewModel.Services;
using System;
using System.ComponentModel.Composition;
using System.Text;

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
        private bool randomizeDelay = false;
        private bool isBusy = false;
        private string busyStatus = "";
        private int timesChanged = 0;

        #endregion

        #region Constructor Definition

        [ImportingConstructor]
        public PasswordChangeViewModel(IHelperService helperService)
        {
            if (helperService == null)
                throw new ArgumentNullException(nameof(helperService));

            this.service = helperService;

            this.userName = this.service.GetDefaultUserName();

            this.GoCommand = new RelayCommand(ChangePassword,
                () => !string.IsNullOrWhiteSpace(this.UserName) && !string.IsNullOrWhiteSpace(this.Password));
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

        #region Private Function Definitions

        private async void ChangePassword()
        {
            string originalPassword = this.Password;
            string currentPassword = originalPassword;
            string newPassword = "";

            try
            {
                this.TimesChanged = 0;
                this.BusyStatus = string.Format("{0}/{1}", this.TimesChanged, this.TimesToChange);
                this.IsBusy = true;

                for (int count = 0; count < TimesToChange; ++count)
                {
                    newPassword = originalPassword + count.ToString();
                    await this.service.ChangePassword(this.UserName, currentPassword, newPassword);
                    currentPassword = newPassword;

                    this.TimesChanged = count + 1;
                    this.BusyStatus = string.Format("{0}/{1}", this.TimesChanged, this.TimesToChange);

                    int localDelay;
                    if (RandomizeDelay)
                        localDelay = Convert.ToInt32((double)this.Delay * this.service.GetRandomMultiplier() * 1000);
                    else
                        localDelay = Convert.ToInt32((double)Delay * 1000);

                    if (localDelay > 0)
                        await this.service.Sleep(localDelay);
                }

                // Change back to original password
                newPassword = originalPassword;
                await this.service.ChangePassword(this.UserName, currentPassword, newPassword);

                this.BusyStatus = "Done";
            }
            catch (AggregateException agEx)
            {
                for (var i = 0; i < agEx.InnerExceptions.Count; ++i)
                {
                    for (var ex = agEx.InnerExceptions[i]; ex != null; ex = ex.InnerException)
                        log.ErrorException($"[{i}]: Unexpected exception thrown while trying to change password from {currentPassword} to {newPassword}.", ex);
                }

                var messageBuilder = new StringBuilder();
                if (agEx.InnerExceptions.Count > 1)
                    messageBuilder.Append($"There were multipler exceptions while trying trying to change your password ({currentPassword} => {newPassword}).");
                else
                    messageBuilder.Append($"An exception occurred while trying to change your password ({currentPassword} => {newPassword}).");

                var fEx = agEx.InnerExceptions[0];
                messageBuilder.Append($" The message received was: {fEx.Message}. Check the log for additional details");

                service.ShowErrorDialog(messageBuilder.ToString(), "Exception");
            }
            catch (Exception ex)
            {
                for (var lEx = ex; lEx != null; lEx = lEx.InnerException)
                    log.ErrorException($"Unexpected exception thrown while trying to change password from {currentPassword} to {newPassword}.", lEx);
                service.ShowErrorDialog($"An exception occurred while trying to change your password ({currentPassword} => {newPassword}). The message received was: {ex.Message}. Check the log for additional details.",
                    "Exception");
            }
            finally
            {
                this.Password = "";
                this.IsBusy = false;
            }
        }

        #endregion
    }
}
