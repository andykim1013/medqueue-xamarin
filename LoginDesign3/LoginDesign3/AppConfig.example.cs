// ============================================================================
// MedQueue(메디큐) 공개용 저장소 — 로컬 설정 템플릿
// ============================================================================
// 이 파일은 실제 credential이 전혀 들어있지 않은 "예시" 파일입니다.
//
// 사용 방법:
//   1) 이 파일을 같은 폴더에 "AppConfig.cs" 라는 이름으로 복사하세요.
//   2) 복사한 AppConfig.cs 안의 빈 값들을 자신의 Firebase 프로젝트 정보로 채우세요.
//   3) AppConfig.cs는 .gitignore에 등록되어 있어 Git 저장소에 절대 커밋되지 않습니다.
//   4) 이 AppConfig.example.cs 파일 자체는 .csproj에서 컴파일 대상에서 제외되어 있으므로
//      AppConfig.cs를 만들지 않아도 프로젝트는 정상적으로 빌드됩니다(단, 관리자 로그인과
//      Firebase 연동 기능은 값이 없으면 동작하지 않습니다).
//
// 원본 v1.3.2에는 아래 값들이 소스코드에 실제 값으로 하드코딩되어 있었습니다.
// 이는 GitHub 공개 저장소로 옮기며 반드시 제거해야 하는 정보였기 때문에,
// 이 템플릿을 통해 "로컬에서만 채워 넣는" 방식으로 분리했습니다.
// ============================================================================

namespace LoginDesign3
{
    internal static class AppConfig
    {
        // ---- 관리자 로그인 (LoginPage.xaml.cs) ----
        // 비워두면 관리자 로그인은 항상 실패하며, 일반 사용자 로그인 로직만 동작합니다.
        public const string AdminUsername = "";
        public const string AdminPassword = "";

        // ---- Firebase: 회원가입/로그인용 DB (FirebaseHelper.cs, 공식 Firebase.Database SDK) ----
        // 예: "https://your-auth-project-default-rtdb.firebaseio.com/"
        public const string AuthDbUrl = "";

        // ---- Firebase: 검색(진료과/의사)용 DB (Search.xaml.cs, FireSharp) ----
        // 원본에서는 회원DB와 다른 별도의 Firebase 프로젝트를 사용했습니다(원본 구조 그대로 보존).
        public const string SearchDbUrl = "";
        public const string SearchAuthSecret = "";

        // ---- Firebase: 대기열(관리자/사용자 공용)용 DB (Queue.xaml.cs / Userqueue.xaml.cs, FireSharp) ----
        // 원본에서는 검색용 DB와도 다른 세 번째 Firebase 프로젝트를 사용했습니다(원본 구조 그대로 보존).
        public const string QueueDbUrl = "";
        public const string QueueAuthSecret = "";
    }
}
