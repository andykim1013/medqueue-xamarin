using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LoginDesign3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private FirebaseHelper _firebaseHelper = new FirebaseHelper();
        // 원본(v1.3.2)에는 관리자 ID/PW가 여기에 하드코딩되어 있었음.
        // 공개용 저장소에서는 AppConfig(로컬 전용, .gitignore 대상)에서 값을 읽어오도록 외부화했다.
        // AppConfig 값이 비어있으면(로컬 설정 없음) 관리자 로그인은 항상 실패한다.
        private static readonly string AdminUsername = AppConfig.AdminUsername;
        private static readonly string AdminPassword = AppConfig.AdminPassword;

        public event EventHandler LoginSuccessful;

        public LoginPage()
        {
            InitializeComponent();
            ClearLoginState();  // 로그인 상태 초기화
        }

        private void ClearLoginState()
        {
            Preferences.Remove("LoggedInUsername");
            Preferences.Remove("PatientName");
            Preferences.Remove("PatientId");
            Preferences.Remove("IsAdmin");
        }

        public User LoggedInUser { get; private set; }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            string username = txtUserName.Text;
            string password = txtPassword.Text;

            if (!string.IsNullOrEmpty(AdminUsername) && !string.IsNullOrEmpty(AdminPassword)
                && username == AdminUsername && password == AdminPassword)
            {
                await DisplayAlert("로그인 성공", "관리자용 로그인 성공", "OK");
                Preferences.Set("PatientName", "관리자");
                Preferences.Set("PatientId", "");
                Preferences.Set("IsAdmin", true);  // 관리자 플래그 설정
                LoggedInUser = new User { Username = AdminUsername, Name = "관리자" };  // 관리자 정보 설정
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
                await Navigation.PopAsync();
                return;
            }

            var user = await _firebaseHelper.GetUserByUsernameAndPassword(username, password);

            if (user != null)
            {
                await DisplayAlert("로그인 성공", "환영합니다!", "OK");
                Preferences.Set("PatientName", user.Name);
                Preferences.Set("PatientId", user.PatientId);
                Preferences.Set("IsAdmin", false);  // 일반 사용자 플래그 설정
                LoggedInUser = user;  // 사용자 정보 설정
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("로그인 실패", "아이디 또는 비밀번호가 잘못되었습니다.", "OK");
            }
        }




        private async void JoinButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new JoinPage());
        }

        private async void ForgotIdNPasswordBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new IdPwRecoveryPage());
        }
    }
}
