using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LoginDesign3;
// 아이디 비밀번호 찾는 화면
namespace LoginDesign3
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IdPwRecoveryPage : ContentPage
    {
        private FirebaseHelper _firebaseHelper;
        private bool _isPasswordChangeRequested = false;

        public IdPwRecoveryPage()
        {
            InitializeComponent();
            _firebaseHelper = new FirebaseHelper();
        }

  

        private async void FindIdButton_Clicked(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string phone = txtPhone.Text;
            string ssn = txtSSN.Text;

            string userId = await _firebaseHelper.FindUserId(name, phone, ssn);
            if (userId != null)
            {
                resultLabel.Text = $"아이디: {userId}";
            }
            else
            {
                resultLabel.Text = "입력한 정보와 일치하는 아이디를 찾을 수 없습니다.";
            }
        }

        

        private async void FindPwButton_Clicked(object sender, EventArgs e)
        {
            string username = txtId.Text;
            string name = txtName.Text;
            string phone = txtPhone.Text;
            string ssn = txtSSN.Text;

            string password = await _firebaseHelper.FindUserPassword(username, name, phone, ssn);
            if (password != null)
            {
                _isPasswordChangeRequested = await DisplayAlert("비밀번호 변경", "비밀번호를 변경하시겠습니까?", "예", "아니오");
                if (_isPasswordChangeRequested)
                {
                    ChangePasswordLayout();
                }
                else
                {
                    resultLabel.Text = "입력한 정보와 일치하는 비밀번호를 찾을 수 없습니다.";
                }
            }
            else
            {
                resultLabel.Text = "입력한 정보와 일치하는 비밀번호를 찾을 수 없습니다.";
            }
        }

        private void ChangePasswordLayout()
        {
            //
        }
    }
}
