using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LoginDesign3
{
    public partial class Search : ContentPage
    {
        private IFirebaseClient _firebaseClient;
        public ObservableCollection<Department> Departments { get; set; }
        public ObservableCollection<Doctor> Doctors { get; set; }

        public Search()
        {
            InitializeComponent();

            Departments = new ObservableCollection<Department>();
            Doctors = new ObservableCollection<Doctor>();
            InitializeFirebase();
            LoadDataAsync();
        }

        private void InitializeFirebase()
        {
            // 원본(v1.3.2)에는 검색(진료과/의사)용 Firebase 프로젝트(search-db)의 실제 AuthSecret/URL이 하드코딩되어 있었음.
            // 공개용 저장소에서는 AppConfig(로컬 전용, .gitignore 대상)에서 값을 읽어오도록 외부화했다.
            var config = new FirebaseConfig
            {
                AuthSecret = AppConfig.SearchAuthSecret,
                BasePath = AppConfig.SearchDbUrl
            };
            _firebaseClient = new FireSharp.FirebaseClient(config);
        }

        private async void LoadDataAsync()
        {
            try
            {
                FirebaseResponse departmentsResponse = await _firebaseClient.GetAsync("Department");
                if (departmentsResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var departmentData = departmentsResponse.ResultAs<Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();
                    Device.BeginInvokeOnMainThread(() => {
                        Doctors.Clear(); // 기존 의사 데이터 클리어
                        Departments.Clear(); // 기존 부서 데이터 클리어

                        foreach (var deptEntry in departmentData)
                        {
                            var departmentName = deptEntry.Key;
                            var department = new Department
                            {
                                Name = departmentName,
                                Doctors = new Dictionary<string, Doctor>()
                            };

                            foreach (var docEntry in deptEntry.Value["Doctor"])
                            {
                                var doctor = new Doctor
                                {
                                    Name = docEntry.Key,
                                    DepartmentName = departmentName // 부서 이름 설정
                                };
                                department.Doctors.Add(docEntry.Key, doctor);
                                Doctors.Add(doctor); // 모든 의사 정보를 컬렉션에 추가
                            }

                            Departments.Add(department); // 부서를 Departments 컬렉션에 추가
                        }

                        // 이름으로 의사 목록 정렬
                        var sortedDoctors = Doctors.OrderBy(d => d.Name).ToList();
                        Doctors = new ObservableCollection<Doctor>(sortedDoctors);

                        MajorPicker.ItemsSource = Departments.Select(d => d.Name).ToList();
                        DoctorsListView.ItemsSource = Doctors; // 정렬된 목록으로 ListView 업데이트
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // SearchBar의 TextChanged이벤트가 작동하는 메소드
        private void DoctorsSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilteredDoctors();
        }

        // Picker의 SelectecIndexChanged이벤트가 작동하는 메소드
        private void MajorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFilteredDoctors();
        }

        // SearchBar와 Picker의 이벤트가 발생했을 때 이벤트의 조건에 맞게 ListView가 수정되는 메소드
        private void UpdateFilteredDoctors()
        {
            var searchText = DoctorsSearch.Text?.ToLower() ?? "";
            var selectedDepartment = MajorPicker.SelectedItem as string;

            var filteredDoctors = Doctors.Where(d =>
            {
                // 검색어에 맞는 의사만 필터링
                bool nameMatches = string.IsNullOrEmpty(searchText) || d.Name.ToLower().Contains(searchText);
                if (!nameMatches) return false; // 해당 의사가 없으면 false 반환

                // 선택된 부서가 없으면 모든 의사를 표시
                if (selectedDepartment == null) return true;

                // 선택된 부서를 찾고, 해당 부서에 의사가 존재하는지 확인
                var department = Departments.SingleOrDefault(dep => dep.Name == selectedDepartment);
                if (department == null) return false;  // 해당 부서가 없으면 false 반환

                return department.Doctors.ContainsKey(d.Name);
            }).ToList();

            DoctorsListView.ItemsSource = filteredDoctors;
        }

        // Resetbtn의 Clicked이벤트 메소드
        private void ResetBtn_Clicked(object sender, EventArgs e)
        {
            DoctorsListView.ItemsSource = Doctors;
            DoctorsSearch.Text = "";
            MajorPicker.SelectedIndex = -1; // 선택된 부서 초기화
        }

        private async void DoctorsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is Doctor selectedDoctor)
                {
                    var selectedDepartment = Departments.FirstOrDefault(d => d.Doctors.ContainsKey(selectedDoctor.Name));
                    if (selectedDepartment != null)
                    {
                        var loggedInUser = Preferences.Get("PatientName", string.Empty);
                        if (loggedInUser == "관리자")
                        {
                            await Navigation.PushAsync(new Queue(selectedDepartment.Name, selectedDoctor.Name));
                        }
                        else
                        {
                            await Navigation.PushAsync(new Userqueue(selectedDepartment.Name, selectedDoctor.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", ex.Message, "OK");
            }
        }
    }
}
