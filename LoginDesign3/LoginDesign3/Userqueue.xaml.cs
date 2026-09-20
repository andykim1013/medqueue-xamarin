using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Xamarin.Essentials;

namespace LoginDesign3
{
    public partial class Userqueue : ContentPage
    {
        private string doctorName;
        private string selectedDepartmentName;
        private int waitingTimePerPatient;

        public ObservableCollection<string> Patients { get; set; }
        private string selectedPatientKey;
        private string alertType;
        private int alertPosition;
        private int previousIndex = -1;

        // 원본(v1.3.2)에는 대기열(관리자/사용자)용 Firebase 프로젝트(queue-db)의 실제 AuthSecret/URL이 하드코딩되어 있었음.
        // 공개용 저장소에서는 AppConfig(로컬 전용, .gitignore 대상)에서 값을 읽어오도록 외부화했다.
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = AppConfig.QueueAuthSecret,
            BasePath = AppConfig.QueueDbUrl
        };

        IFirebaseClient client;


        public Userqueue(string departmentName, string doctorName)
        {
            InitializeComponent();

            this.selectedDepartmentName = departmentName;
            this.doctorName = doctorName;

            departmentLabel.Text = selectedDepartmentName;
            doctorLabel.Text = doctorName;

            Patients = new ObservableCollection<string>();
            client = new FireSharp.FirebaseClient(config);
            VerifyFirebaseConnection();
            SubscribeToFirebase();
            LoadDataForDoctor();
            LoadAlertSettings();
            LoadNotice();
            LoadWaitingTime();
        }

        private async void VerifyFirebaseConnection()
        {
            try
            {
                var response = await client.GetAsync("TestConnection");
                if (response.Body != "null")
                {
                    await DisplayAlert("Success", "Firebase connection established!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Failure", $"Failed to connect to Firebase: {ex.Message}", "OK");
            }
        }

        private void SubscribeToFirebase()
        {
            var patientsPath = $"Department/{selectedDepartmentName}/Doctor/{doctorName}/Patients";
            var noticePath = $"Department/{selectedDepartmentName}/Doctor/{doctorName}/Notice";
            var waitingTimePath = $"Department/{selectedDepartmentName}/Doctor/{doctorName}/WaitingTime";

            client.OnAsync(patientsPath, changed: (sender, args, context) =>
            {
                Device.BeginInvokeOnMainThread(() => LoadDataForDoctor());
            });

            client.OnAsync(noticePath, changed: (sender, args, context) =>
            {
                Device.BeginInvokeOnMainThread(() => LoadNotice());
            });

            client.OnAsync(waitingTimePath, changed: (sender, args, context) =>
            {
                Device.BeginInvokeOnMainThread(() => LoadWaitingTime());
            });
        }

        private async void LoadDataForDoctor()
        {
            try
            {
                var response = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Patients");
                var patientsData = response.ResultAs<Dictionary<string, Dictionary<string, string>>>();
                if (patientsData != null)
                {
                    Patients.Clear();
                    foreach (var patient in patientsData.Values)
                    {
                        Patients.Add(patient["Name"]);
                    }
                    UpdateWaitingList();

                    string loggedInPatientNumber = Preferences.Get("PatientId", string.Empty);
                    if (!string.IsNullOrEmpty(loggedInPatientNumber))
                    {
                        var matchingPatient = patientsData.Values.FirstOrDefault(p => p["Number"] == loggedInPatientNumber);
                        if (matchingPatient != null)
                        {
                            int index = Patients.IndexOf(matchingPatient["Name"]);
                            if (index >= 0)
                            {
                                OnPatientLabelTapped(Waiting.Children[index], null, index);
                                OnPatientLabelTapped(Waiting.Children[index], null, index);
                                OnPatientLabelTapped(Waiting.Children[index], null, index);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load patients: {ex.Message}", "OK");
            }
        }

        private async void LoadWaitingTime()
        {
            try
            {
                var response = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/WaitingTime");
                string waitingTimeStr = response.ResultAs<string>();
                if (!string.IsNullOrEmpty(waitingTimeStr) && int.TryParse(waitingTimeStr, out int waitingTime))
                {
                    waitingTimePerPatient = waitingTime;
                }
                else
                {
                    waitingTimePerPatient = 0;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load waiting time: {ex.Message}", "OK");
            }
        }

        private void UpdateWaitingList()
        {
            Waiting.Children.Clear();
            for (int i = 0; i < Patients.Count; i++)
            {
                var label = new Label { Text = $"{i + 1}. {Patients[i]}", FontSize = 30, TextColor = Color.Black };
                var tapGestureRecognizer = new TapGestureRecognizer();
                int index = i;
                tapGestureRecognizer.Tapped += (s, e) => OnPatientLabelTapped(s, e, index);
                label.GestureRecognizers.Add(tapGestureRecognizer);

                if (Patients[i] == selectedPatientKey)
                {
                    label.BackgroundColor = Color.Orange;
                }

                Waiting.Children.Add(label);
            }

            CheckAlertCondition();
        }

        private void OnPatientLabelTapped(object sender, EventArgs e, int index)
        {
            foreach (var child in Waiting.Children)
            {
                if (child is Label label)
                {
                    label.BackgroundColor = Color.Default;
                }
            }

            if (sender is Label selectedLabel)
            {
                selectedLabel.BackgroundColor = Color.Orange;
                selectedPatientKey = Patients[index];
                previousIndex = index;

                int expectedWaitingTime = (index + 1) * waitingTimePerPatient;
                timerLabel.Text = $"{expectedWaitingTime} 분";
            }

            alertOptionsPicker.IsEnabled = true;
        }

        private void OnAlertOptionChanged(object sender, EventArgs e)
        {
            if (alertOptionsPicker.SelectedIndex == 1)
            {
                specificPositionEntry.IsEnabled = true;
                specificPositionEntry.Placeholder = "순서 입력";
            }
            else
            {
                specificPositionEntry.IsEnabled = false;
                specificPositionEntry.Text = string.Empty;
                specificPositionEntry.Placeholder = string.Empty;
            }
        }

        private async void OnSetAlertClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPatientKey))
            {
                await DisplayAlert("Warning", "알림을 받을 환자를 선택해 주세요", "OK");
                return;
            }

            if (alertOptionsPicker.SelectedIndex == 1 && string.IsNullOrEmpty(specificPositionEntry.Text))
            {
                await DisplayAlert("Warning", "알림을 받을 순서를 숫자로 입력해 주세요", "OK");
                return;
            }

            try
            {
                alertType = alertOptionsPicker.SelectedItem.ToString();
                alertPosition = alertOptionsPicker.SelectedIndex == 1 ? int.Parse(specificPositionEntry.Text) : 0;

                await client.SetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Alarm/PatientKey", selectedPatientKey);
                await client.SetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Alarm/AlertType", alertType);
                await client.SetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Alarm/AlertPosition", alertPosition.ToString());

                await DisplayAlert("Success", "알림 설정이 저장되었습니다.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to set alert: {ex.Message}", "OK");
            }
        }

        private async void LoadAlertSettings()
        {
            try
            {
                FirebaseResponse alarmResponse = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Alarm");
                var alarmData = alarmResponse.ResultAs<Dictionary<string, string>>();

                if (alarmData != null)
                {
                    selectedPatientKey = alarmData["PatientKey"];
                    alertType = alarmData["AlertType"];
                    alertPosition = int.Parse(alarmData["AlertPosition"]);

                    alertOptionsPicker.SelectedItem = alertType;
                    specificPositionEntry.Text = alertPosition.ToString();

                    await DisplayAlert("Info", $"최종 설정된 알림: 환자 - {selectedPatientKey}, 옵션 - {alertType}, 순서 - {alertPosition}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load alert settings: {ex.Message}", "OK");
            }
        }

        private async void ClearAlertSettings()
        {
            selectedPatientKey = null;
            alertType = null;
            alertPosition = 0;
            previousIndex = -1;

            alertOptionsPicker.SelectedIndex = -1;
            specificPositionEntry.Text = string.Empty;
            specificPositionEntry.IsEnabled = false;

            await client.DeleteAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Alarm");

            await DisplayAlert("Info", "알림 옵션 초기화됨", "OK");
        }

        private void CheckAlertCondition()
        {
            if (string.IsNullOrEmpty(selectedPatientKey) || string.IsNullOrEmpty(alertType))
            {
                return;
            }

            var patientIndex = Patients.IndexOf(selectedPatientKey);

            if (alertType == "순서변동시" && patientIndex != previousIndex)
            {
                previousIndex = patientIndex;
                DisplayAlert("Alert", $"순서가 {patientIndex + 1} 번째로 변경되었습니다", "OK");
            }
            else if (alertType == "x번째시" && patientIndex == alertPosition - 1)
            {
                DisplayAlert("Alert", $"순서가 {alertPosition} 번째로 변경되었습니다", "OK");
                ClearAlertSettings();
            }
        }

        private async void LoadNotice()
        {
            try
            {
                FirebaseResponse response = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Notice");
                string notice = response.ResultAs<string>();

                if (!string.IsNullOrEmpty(notice))
                {
                    noticeLabel.Text = notice;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load notice: {ex.Message}", "OK");
            }
        }

        private void OnClearAlertSettingsClicked(object sender, EventArgs e)
        {
            ClearAlertSettings();
        }

        private void OnSpecificPositionEntryFocused(object sender, FocusEventArgs e)
        {
            DisplayAlert("Info", "알림을 받을 순서를 숫자로 입력해 주세요", "OK");
        }
    }
}
