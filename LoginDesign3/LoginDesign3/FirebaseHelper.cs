using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace LoginDesign3
{
    public class FirebaseHelper
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseHelper()
        {
            // 원본(v1.3.2)에는 회원가입/로그인용 Firebase 프로젝트(auth-db)의 실제 URL이 하드코딩되어 있었음.
            // 공개용 저장소에서는 AppConfig(로컬 전용, .gitignore 대상)에서 값을 읽어오도록 외부화했다.
            _firebaseClient = new FirebaseClient(AppConfig.AuthDbUrl);
        }

        public async Task<bool> IsPhoneExistsExcludingCurrentUser(string phone, string currentUserId)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            return users.Any(u => u.Object.Phone == phone && u.Object.Id != currentUserId);
        }


        public async Task AddUser(string id, string patientId, string name, string birthdate, string phone, string username, string password)
        {
            await _firebaseClient
                .Child("Users")
                .Child(id)
                .PutAsync(new User
                {
                    Id = id,
                    PatientId = patientId,
                    Name = name,
                    Birthdate = birthdate,
                    Phone = phone,
                    Username = username,
                    Password = password
                });
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            return users.Any(u => u.Object.Username == username);
        }

        public async Task<bool> IsPhoneExists(string phone)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            return users.Any(u => u.Object.Phone == phone);
        }

        public async Task<User> GetUserByUsernameAndPassword(string username, string password)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            return users.FirstOrDefault(u => u.Object.Username == username && u.Object.Password == password)?.Object;
        }

        public async Task<bool> IsPatientIdExists(string patientId)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            return users.Any(u => u.Object.PatientId == patientId);
        }

        public async Task<string> GenerateUniquePatientId()
        {
            string patientId;
            Random random = new Random();
            do
            {
                patientId = GeneratePatientId(random);
            } while (await IsPatientIdExists(patientId));

            return patientId;
        }

        private string GeneratePatientId(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";

            string alphaPart = new string(Enumerable.Repeat(chars, 2).Select(s => s[random.Next(s.Length)]).ToArray());
            string numberPart = new string(Enumerable.Repeat(numbers, 3).Select(s => s[random.Next(s.Length)]).ToArray());

            return alphaPart + numberPart;
        }

        public async Task<string> FindUserId(string name, string phone, string ssn)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            var user = users.FirstOrDefault(u => u.Object.Name == name && u.Object.Phone == phone && u.Object.Birthdate == ssn);

            return user?.Object.Username;
        }

        public async Task<string> FindUserPassword(string username, string name, string phone, string ssn)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            var user = users.FirstOrDefault(u => u.Object.Username == username && u.Object.Name == name && u.Object.Phone == phone && u.Object.Birthdate == ssn);

            return user?.Object.Password;
        }

        public async Task UpdatePassword(string username, string newPassword)
        {
            var user = await GetUserByUsername(username);
            if (user != null)
            {
                user.Password = newPassword;
                await _firebaseClient
                    .Child("Users")
                    .Child(user.Id)
                    .PutAsync(user);
            }
            else
            {
                throw new Exception("사용자를 찾을 수 없습니다.");
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var users = await _firebaseClient
                .Child("Users")
                .OnceAsync<User>();

            return users.FirstOrDefault(u => u.Object.Username == username)?.Object;
        }

        public async Task UpdateUser(User user)
        {
            await _firebaseClient
                .Child("Users")
                .Child(user.Id)
                .PutAsync(user);
        }
    }

    public class User
    {
        public string Id { get; set; }
        public string PatientId { get; set; }
        public string Name { get; set; }
        public string Birthdate { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
