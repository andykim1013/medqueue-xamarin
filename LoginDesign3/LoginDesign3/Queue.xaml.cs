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
    public partial class Queue : ContentPage
    {
        private string doctorName;
        private string selectedDepartmentName;

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

        public Queue(string departmentName, string doctorName)
        {
            InitializeComponent();
            this.selectedDepartmentName = departmentName;
            this.doctorName = doctorName;

            // 선택된 부서와 교수의 이름을 Entry에 설정
            departmentEntry.Text = selectedDepartmentName;
            doctorEntry.Text = doctorName;

            Patients = new ObservableCollection<string>();
            client = new FireSharp.FirebaseClient(config);
            VerifyFirebaseConnection();
            LoadDataForDoctor();
            LoadAlertSettings();
            LoadNotice();
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
                else
                {
                    //await DisplayAlert("Failure", "Failed to connect to Firebase: Received null response.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Failure", $"Failed to connect to Firebase: {ex.Message}", "OK");
            }
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

                    // 로그인한 사용자의 환자번호와 일치하는 환자를 선택
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



        private async void OnAddPatientClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(PatientPlus.Text) && !string.IsNullOrEmpty(PatientNumberPlus.Text))
            {
                try
                {
                    var newPatient = new
                    {
                        Name = PatientPlus.Text,
                        Number = PatientNumberPlus.Text
                    };
                    PushResponse response = await client.PushAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Patients", newPatient);

                    Patients.Add(PatientPlus.Text);
                    UpdateWaitingList();
                    PatientPlus.Text = string.Empty;
                    PatientNumberPlus.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to add patient: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "Please enter both patient name and number.", "OK");
            }
        }



        private async void OnRemovePatientClicked(object sender, EventArgs e)
        {
            if (Patients.Count > 0)
            {
                try
                {
                    var response = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Patients");
                    var patientsData = response.ResultAs<Dictionary<string, Dictionary<string, string>>>();
                    if (patientsData != null && patientsData.Count > 0)
                    {
                        var firstPatientKey = patientsData.FirstOrDefault(kvp => kvp.Value["Name"] == Patients[0]).Key;
                        await client.DeleteAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Patients/" + firstPatientKey);

                        if (Patients[0] == selectedPatientKey)
                        {
                            ClearAlertSettings();
                        }

                        Patients.RemoveAt(0);
                        UpdateWaitingList();
                    }
                    else
                    {
                        await DisplayAlert("Info", "No patients to remove.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to remove patient: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Info", "No patients to remove.", "OK");
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
            }

            alertOptionsPicker.IsEnabled = true;
        }

        private void OnAlertOptionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPatientKey))
            {
                DisplayAlert("Warning", "알림을 받을 환자를 선택해 주세요", "OK");
                alertOptionsPicker.SelectedIndex = -1;
                return;
            }

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

                    var patientIndex = Patients.IndexOf(selectedPatientKey);
                    if (patientIndex >= 0)
                    {
                        OnPatientLabelTapped(Waiting.Children[patientIndex], null, patientIndex);
                    }

                    alertOptionsPicker.SelectedItem = alertType;
                    specificPositionEntry.Text = alertPosition.ToString();

                    await DisplayAlert("Info", $"최종 설정된 알림: 환자 - {selectedPatientKey}, 옵션 - {alertType}, 순서 - {alertPosition}", "OK");
                }
                else
                {
                    //await DisplayAlert("Info", "No alert settings found.", "OK");
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

            foreach (var child in Waiting.Children)
            {
                if (child is Label label)
                {
                    label.BackgroundColor = Color.Default;
                }
            }

            await client.DeleteAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Alarm");

            await DisplayAlert("Info", "알림 옵션 초기화됨", "OK");
        }

        private async void LoadDepartment()
        {
            try
            {
                FirebaseResponse response = await client.GetAsync($"Department/{selectedDepartmentName}");
                string department = response.ResultAs<string>();

                if (!string.IsNullOrEmpty(department))
                {
                    departmentEntry.Text = department;
                    await DisplayAlert("Info", $"최종 저장한 진료과: {department}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load department: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateDepartmentClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(departmentEntry.Text))
            {
                try
                {
                    await client.SetAsync($"Department/{selectedDepartmentName}", departmentEntry.Text);
                    await DisplayAlert("Success", "Department updated successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to update department: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "Please enter a department.", "OK");
            }
        }

        private async void LoadDoctor()
        {
            try
            {
                FirebaseResponse response = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}");
                string doctor = response.ResultAs<string>();

                if (!string.IsNullOrEmpty(doctor))
                {
                    doctorEntry.Text = doctor;
                    await DisplayAlert("Info", $"최종 저장한 진료의/검사실: {doctor}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load doctor: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateDoctorClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(doctorEntry.Text))
            {
                try
                {
                    await client.SetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}", doctorEntry.Text);
                    await DisplayAlert("Success", "Doctor updated successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to update doctor: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "Please enter a doctor.", "OK");
            }
        }

        private async void LoadWaitingTime()
        {
            try
            {
                FirebaseResponse response = await client.GetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/WaitingTime");
                string waitingTime = response.ResultAs<string>();

                if (!string.IsNullOrEmpty(waitingTime))
                {
                    timerLabel.Text = FormatTime(waitingTime);
                    await DisplayAlert("Info", $"최종 저장한 대기 시간: {waitingTime}분", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load waiting time: {ex.Message}", "OK");
            }
        }

        private async void OnUpdateTimePerPatientClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(timePerPatientEntry.Text) && int.TryParse(timePerPatientEntry.Text, out int waitingTime))
            {
                try
                {
                    await client.SetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/WaitingTime", waitingTime.ToString());
                    timerLabel.Text = FormatTime(waitingTime.ToString());
                    await DisplayAlert("Success", "Waiting time updated successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to update waiting time: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "Please enter a valid waiting time in minutes.", "OK");
            }
        }

        private string FormatTime(string time)
        {
            if (int.TryParse(time, out int minutes))
            {
                return $"{minutes:D2}:00";
            }
            return "00:00";
        }

        private void OnTimeEntryFocused(object sender, FocusEventArgs e)
        {
            DisplayAlert("Info", "숫자의 단위는 분(min)으로 입력해야 합니다.", "OK");
        }

        private void OnSpecificPositionEntryFocused(object sender, FocusEventArgs e)
        {
            DisplayAlert("Info", "알림을 받을 순서를 숫자로 입력해 주세요", "OK");
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

        private async void OnUpdateNoticeClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(noticeEditor.Text))
            {
                try
                {
                    await client.SetAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Notice", noticeEditor.Text);
                    await DisplayAlert("Success", "공지사항이 저장되었습니다.", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to update notice: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "공지사항을 입력해 주세요.", "OK");
            }
        }

        private async void OnDeleteNoticeClicked(object sender, EventArgs e)
        {
            try
            {
                await client.DeleteAsync($"Department/{selectedDepartmentName}/Doctor/{doctorName}/Notice");
                noticeEditor.Text = string.Empty;
                await DisplayAlert("Success", "공지사항이 삭제되었습니다.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete notice: {ex.Message}", "OK");
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
                    noticeEditor.Text = notice;
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


        public class Patient
        {
            public string Name { get; set; }
            public string PatientId { get; set; }
        }

    }
}
