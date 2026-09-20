using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LoginDesign3;

namespace LoginDesign3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JoinPage : ContentPage
    {
        private FirebaseHelper _firebaseHelper = new FirebaseHelper();

        public JoinPage()
        {
            InitializeComponent();
        }

        // 회원가입 페이지의 코드
        private async void JoinPageButton_Clicked(object sender, EventArgs e)
        {
            string id = Guid.NewGuid().ToString();
            string name = nameEntry.Text;
            string birthdate = birthdateEntry.Text;
            string phone = phoneEntry.Text;
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;

            if (!IsValidUsername(username))
            {
                await DisplayAlert("가입 실패", "아이디는 6~15자 이내로 작성해주세요", "OK");
                return;
            }

            if (!IsValidPassword(password))
            {
                await DisplayAlert("가입 실패", "비밀번호는 6~15자 이내이며 특수문자(!, @, #, $, %) 1개 이상을 포함해야합니다.", "OK");
                return;
            }

            if (await _firebaseHelper.IsPhoneExists(phone))
            {
                await DisplayAlert("가입 실패", "이미 가입된 전화번호입니다.", "OK");
                return;
            }

            string patinetId = await _firebaseHelper.GenerateUniquePatientId();

            await _firebaseHelper.AddUser(id, patinetId, name, birthdate, phone, username, password);
            await DisplayAlert("가입 완료", $"성공적으로 가입되었습니다. 환자번호 : {patinetId}", "OK");

            // 회원가입 처리 등을 수행한 후
            // 로그인 페이지로 이동
            await Navigation.PushAsync(new LoginPage());
            // 현재 페이지(회원가입 페이지)를 스택에서 제거
            Navigation.RemovePage(this);
        }

        private async void CheckUsernameButton_Clicked(object sender, EventArgs e)
        {
            string username = usernameEntry.Text;

            if (string.IsNullOrEmpty(username) || username.Length < 6 || username.Length > 15)
            {
                await DisplayAlert("확인 실패", "아이디는 6~15자 이내로 작성해주세요", "OK");
                return;
            }

            bool exists = await _firebaseHelper.IsUsernameExists(username);

            if (exists)
            {
                await DisplayAlert("확인 실패", "이미 사용 중인 아이디입니다. 다른 아이디를 입력해주세요.", "OK");
            }
            else
            {
                await DisplayAlert("확인 성공", "사용 가능한 아이디입니다.", "OK");
            }
        }

        private bool IsValidUsername(string username)
        {
            return !string.IsNullOrEmpty(username) && username.Length >= 6 && username.Length <= 15;
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
