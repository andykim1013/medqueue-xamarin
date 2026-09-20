using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LoginDesign3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            UpdatePatientCard();
        }

        private async void btnLogin_Clicked(object sender, EventArgs e)
        {
            var loginPage = new LoginPage();
            loginPage.LoginSuccessful += OnLoginSuccessful;
            await Navigation.PushAsync(loginPage);
        }

        private async void btnSearch_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Search());
        }

        private void btnLogout_Clicked(object sender, EventArgs e)
        {
            Preferences.Clear();
            lblPatientName.Text = "이름: ";
            lblPatientId.Text = "환자번호: ";
            btnLogin.IsVisible = true;
            lblLogin.IsVisible = true;
            frameLogout.IsVisible = false; //// 수정
            frameOpt.IsVisible = false;   //// 수정 --> Ture에서 False로 수정
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdatePatientCard();
        }

        private void UpdatePatientCard()
        {
            string patientName = Preferences.Get("PatientName", string.Empty);
            string patientId = Preferences.Get("PatientId", string.Empty);
            bool isAdmin = Preferences.Get("IsAdmin", false);

            if (isAdmin)
            {
                lblPatientName.Text = "이름: 관리자";
                lblPatientId.Text = string.Empty;
                btnLogin.IsVisible = false;
                lblLogin.IsVisible = false;
                frameLogout.IsVisible = true; /////수정
                frameOpt.IsVisible = false;  // 관리자에게는 정보수정 버튼 숨기기
            }
            else if (!string.IsNullOrEmpty(patientName) && !string.IsNullOrEmpty(patientId))
            {
                lblPatientName.Text = $"이름: {patientName}";
                lblPatientId.Text = $"환자번호: {patientId}";
                btnLogin.IsVisible = false;
                lblLogin.IsVisible = false;
                frameLogout.IsVisible = true; /////수정
                frameOpt.IsVisible = true;  // 정보수정 버튼 보이기
            }
            else
            {
                lblPatientName.Text = "이름: ";
                lblPatientId.Text = "환자번호: ";
                btnLogin.IsVisible = true;
                lblLogin.IsVisible = true;
                frameLogout.IsVisible = false; /////수정
                frameOpt.IsVisible = false;  // 정보수정 버튼 숨기기
            }
        }

        private async void btnOpt_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserDataEdit());
        }

        private void OnLoginSuccessful(object sender, EventArgs e)
        {
            var loginPage = sender as LoginPage;
            if (loginPage != null)
            {
                var user = loginPage.LoggedInUser;
                if (user != null)
                {
                    Preferences.Set("LoggedInUsername", user.Username);
                    Preferences.Set("PatientName", user.Name);
                    Preferences.Set("PatientId", user.PatientId);
                }
            }
            UpdatePatientCard();
        }
    }
}
