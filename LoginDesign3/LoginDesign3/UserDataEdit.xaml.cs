using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LoginDesign3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserDataEdit : ContentPage
    {
        private FirebaseHelper _firebaseHelper = new FirebaseHelper();
        private User _loggedInUser;

        public UserDataEdit()
        {
            InitializeComponent();
            LoadUserData();
        }

        private async void LoadUserData()
        {
            string username = Preferences.Get("LoggedInUsername", string.Empty);

            if (!string.IsNullOrEmpty(username))
            {
                _loggedInUser = await _firebaseHelper.GetUserByUsername(username);
                if (_loggedInUser != null)
                {
                    usernameEntry.Text = _loggedInUser.Username;
                    nameLabel.Text = _loggedInUser.Name;
                    phoneEntry.Text = _loggedInUser.Phone;
                }
                else
                {
                    await DisplayAlert("Error", "사용자 정보를 불러올 수 없습니다.", "OK");
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await DisplayAlert("Error", "로그인된 사용자의 정보를 불러올 수 없습니다.", "OK");
                await Navigation.PopAsync();
            }
        }

        private async void ReLoginButton_Clicked(object sender, EventArgs e)
        {
            if (_loggedInUser == null)
            {
                await DisplayAlert("확인 실패", "로그인된 사용자의 정보를 불러올 수 없습니다.", "OK");
                return;
            }

            string username = usernameEntry.Text;
            string password = currentPasswordEntry.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("확인 실패", "아이디와 비밀번호를 입력해 주세요.", "OK");
                return;
            }

            var user = await _firebaseHelper.GetUserByUsernameAndPassword(username, password);
            if (user != null && user.Id == _loggedInUser.Id)
            {
                editSection.IsVisible = true;
                await DisplayAlert("확인 성공", "본인 확인에 성공하였습니다.", "OK");
            }
            else
            {
                await DisplayAlert("확인 실패", "아이디 또는 비밀번호가 잘못되었습니다.", "OK");
            }
        }

        private async void UpdateButton_Clicked(object sender, EventArgs e)
        {
            string newPassword = newPasswordEntry.Text;
            string confirmPassword = confirmPasswordEntry.Text;
            string phone = phoneEntry.Text;

            if (newPassword != confirmPassword)
            {
                await DisplayAlert("오류", "비밀번호가 일치하지 않습니다.", "OK");
                return;
            }

            if (!IsValidPassword(newPassword))
            {
                await DisplayAlert("오류", "비밀번호는 6~15자 이내이며 특수문자(!, @, #, $, %) 1개 이상을 포함해야합니다.", "OK");
                return;
            }

            if (!string.IsNullOrEmpty(phone) && await _firebaseHelper.IsPhoneExistsExcludingCurrentUser(phone, _loggedInUser.Id))
            {
                await DisplayAlert("오류", "이미 사용 중인 전화번호입니다.", "OK");
                return;
            }

            _loggedInUser.Password = newPassword;
            _loggedInUser.Phone = phone;

            await _firebaseHelper.UpdateUser(_loggedInUser);
            await DisplayAlert("성공", "회원 정보가 성공적으로 수정되었습니다.", "OK");
            await Navigation.PopAsync();
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6 || password.Length > 15)
                return false;

            string specialCharacters = "!@#$%";
            return password.Any(ch => specialCharacters.Contains(ch));
        }
    }
}
