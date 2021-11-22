using JankiBusiness.Abstraction;
using JankiBusiness.Services;
using System;

namespace JankiBusiness.ViewModels.Web
{
    public class LoginPageViewModel : PageViewModel
    {
        private string username;

        public string Username
        {
            get => username;
            set => Set(ref username, value);
        }

        private string password;

        public string Password
        {
            get => password;
            set => Set(ref password, value);
        }

        public GenericCommand Login { get; }

        public GenericCommand Register { get; }

        public LoginPageViewModel(JankiWebClient client, INavigationService navigation)
        {
            Login = new GenericDelegateCommand(async p =>
            {
                client.SetBearerToken((await client.Login(username, password)).access_token);
                Username = "";
                Password = "";
                navigation.NavigateToVM(typeof(SyncPageViewModel), null);
            });

            Register = new GenericDelegateCommand(async p =>
            {
                if (await client.Register(Username, Password))
                {
                    await Login.ExecuteAsync(null);
                }
            });
        }
    }
}