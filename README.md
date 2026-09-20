# 메디큐 (MedQueue)

> 본 저장소는 당시 최종 프로젝트 소스인 **MedQueue.v1.3.2**를 기반으로 공개용으로 복원·정리한 아카이브입니다.

## 1. 프로젝트 소개

**메디큐(MedQueue)**는 대형병원 외래환자의 진료 대기 시간을 실시간으로 확인할 수 있는 **Xamarin.Forms 기반 모바일 앱**입니다. 건양대학교 의료IT공학과 학생 팀 "대기UP"이 2학년 1학기 학기설계(캡스톤) 과목으로 수행한 프로젝트입니다.

- **개발 기간**: 2024.03.04 ~ 2024.06.17 (약 3.5개월)
- **개발 방법론**: 폭포수(Waterfall) 모델
- **프로젝트 목적**: 대형/상급병원에서 예약 후에도 진료실 앞에서 오래 기다려야 하는 문제를 해결하기 위해, 진료실 앞 대기 모니터를 모바일 앱으로 옮겨 환자가 어디서든 자신의 대기 순번과 예상 대기 시간을 확인할 수 있도록 함
- **대상 사용자**: 대형병원 외래환자, 병원 대기열 관리 담당자(관리자)

> ⚠️ **이 프로젝트는 실제 임상 시스템이 아니며, 실제 병원에서 운영된 적이 없는 교육/학기설계용 프로젝트입니다.** 모든 진료과·의사·환자 데이터는 시연을 위한 가상의 예시 데이터입니다.

## 2. 실제 구현 기능

문서(요구사항분석서·설계서·단위/통합 시험결과서)와 실제 소스코드를 대조하여 확인된, 실제로 동작한 기능입니다.

- 회원가입 / 로그인 (아이디·비밀번호, 이름·전화번호·생년월일 입력, 환자번호 자동 발급)
- 진료과 / 담당의 검색 (이름 검색 + 진료과 필터)
- 실시간 대기열 정보 확인 (사용자 화면, 본인 위치 하이라이트)
- 예상 대기 시간 표시
- 대기 알림 설정 ("순서변동시" / "x번째시" 옵션)
- 관리자용 대기열 관리 (환자 추가/삭제, 진료 시간 편집, 공지사항 등록/삭제)
- 개인정보 수정 (재인증 후 전화번호/비밀번호 변경)
- 아이디 / 비밀번호 찾기 (이름, 전화번호, 주민등록번호 입력을 이용한 사용자 확인)

## 3. 미구현 기능 (계획 또는 향후 과제였으나 코드에는 없음)

아래 기능은 요구사항분석서 또는 최종발표 자료의 "향후 개발 계획"에는 언급되어 있으나, 실제 소스코드·화면·테스트 결과 어디에도 구현 흔적이 없어 **이번 복원 과정에서도 추가하지 않았습니다.**

- 병원 시설 검색 / 층별 평면도 표시
- 원격 진료·검사 접수
- 관리자용 웹페이지
- Push 알림 (앱 내부 알림 조건 판단 로직은 있으나, 실제 모바일 푸시 발송 연동은 없음)

## 4. 사용 기술

| 항목 | 내용 |
|---|---|
| 언어 | C# |
| 프레임워크 | Xamarin.Forms 5.0.0.2196 |
| IDE | Visual Studio 2022 |
| 대상 플랫폼 | Android (실제 테스트: Android 13, API 33), iOS, UWP (프로젝트 구조는 존재하나 Android 위주로 검증됨) |
| DB | Firebase Realtime Database (공식 Firebase.Database SDK + FireSharp 두 가지 클라이언트 병행 사용) |
| 주요 NuGet 패키지 | FirebaseDatabase.net 4.2.0, FireSharp 2.0.4, Xamarin.Essentials 1.7.0, Xamarin.Forms.PancakeView 2.3.0.759 |

## 5. Xamarin.Forms 프로젝트 구조 (원본 v1.3.2 그대로 보존)

```
medqueue-xamarin/
├── LoginDesign3.sln
└── LoginDesign3/
    ├── LoginDesign3/            (공유 프로젝트, .NET Standard 2.0)
    │   ├── App.xaml(.cs)
    │   ├── MainPage.xaml(.cs)          — 홈 화면
    │   ├── LoginPage.xaml(.cs)         — 로그인 (관리자 로그인 분기 포함)
    │   ├── JoinPage.xaml(.cs)          — 회원가입
    │   ├── IdPwRecoveryPage.xaml(.cs)  — 아이디/비밀번호 찾기
    │   ├── UserDataEdit.xaml(.cs)      — 회원정보 수정
    │   ├── Search.xaml(.cs)            — 진료과/의사 검색
    │   ├── Queue.xaml(.cs)             — 관리자용 대기열 관리
    │   ├── Userqueue.xaml(.cs)         — 사용자용 대기열 확인
    │   ├── Department.cs               — Doctor/Department 모델
    │   ├── FirebaseHelper.cs           — 회원(로그인/가입) Firebase 연동
    │   └── AppConfig.example.cs        — [공개용 수정] 로컬 설정 템플릿 (아래 8번 참고)
    ├── LoginDesign3.Android/    (Android 헤드 프로젝트)
    ├── LoginDesign3.iOS/        (iOS 헤드 프로젝트 — ⚠ 원본 .sln에는 포함되어 있지 않음, 아래 "알려진 한계" 참고)
    └── LoginDesign3.UWP/        (UWP 헤드 프로젝트)
```

> 원본 프로토타입명이었던 `LoginDesign3`라는 솔루션/네임스페이스명은 프로젝트 진행 중 끝까지 리네이밍되지 않고 그대로 사용되었습니다. 당시 코드를 정확히 보존하기 위해 이 저장소에서도 `MedQueue`로 임의 변경하지 않았습니다.

## 6. 화면 구성 및 프로그램 흐름

```
MainPage (홈)
  ├─ 비로그인 상태: [로그인] 버튼만 노출
  ├─ 로그인 상태(일반 사용자): 환자 카드(이름/환자번호), [검색] [정보수정] [로그아웃]
  └─ 로그인 상태(관리자): "관리자" 표시, [검색] [로그아웃] (정보수정 버튼 숨김)
      │
      ├─ LoginPage → (신규가입 필요시) JoinPage → 가입 후 LoginPage 복귀
      │            → (계정 찾기) IdPwRecoveryPage
      │
      ├─ Search (진료과/의사 검색)
      │      ├─ 일반 사용자 선택 → Userqueue (실시간 대기열 구독, 알림설정, 예상대기시간)
      │      └─ 관리자 선택     → Queue (환자 추가/삭제, 진료시간·공지사항 편집)
      │
      └─ UserDataEdit (재인증 후 전화번호/비밀번호 변경)
```

## 7. Firebase 사용 구조 (원본 그대로 보존)

원본 프로젝트는 **역할별로 서로 다른 3개의 Firebase 프로젝트**를 사용했습니다. 이는 설계 실수로 보이지만, "당시 프로젝트를 있는 그대로 기록·재현"하는 것이 이 저장소의 목적이므로 이번 복원에서 하나로 통합하지 않았고, 각 역할의 URL/Secret만 로컬 설정으로 옮겼습니다.

| 역할 | 사용 위치 | 클라이언트 |
|---|---|---|
| 회원가입 / 로그인 (Users) | `FirebaseHelper.cs` | 공식 Firebase.Database SDK |
| 진료과 / 의사 검색 (Department/Doctor) | `Search.xaml.cs` | FireSharp |
| 대기열 관리 (Department/{과}/Doctor/{의사}/Patients, Alarm, Notice, WaitingTime) | `Queue.xaml.cs`, `Userqueue.xaml.cs` | FireSharp |

DB 스키마 예시는 저장소 루트의 [`DoctorsInfo.example.json`](DoctorsInfo.example.json) 참고 (⚠ 완전히 가상의 예시 데이터 — 아래 10번 참고).

## 8. Firebase 설정 방법 (실행/빌드에 필요한 로컬 설정)

이 저장소에는 **실제 Firebase credential이 전혀 포함되어 있지 않습니다.** ⚠️ **저장소를 clone한 직후에는 `AppConfig.cs`가 존재하지 않으므로 (정책상 의도된 상태입니다) 아래 절차를 먼저 완료해야 정상적으로 빌드·실행할 수 있습니다.**

1. Firebase 콘솔에서 본인 소유의 Firebase 프로젝트를 3개(또는 원본처럼 역할별로 분리하지 않고 1개로 통합해도 무방) 생성하고, Realtime Database를 활성화합니다.
2. `LoginDesign3/LoginDesign3/AppConfig.example.cs` 파일을 같은 폴더에 `AppConfig.cs`로 복사한 뒤, 파일 안의 빈 문자열들을 자신의 Firebase 프로젝트 URL / AuthSecret으로 채웁니다. (`AppConfig.cs`는 `.gitignore`에 등록되어 있어 커밋되지 않습니다.)
3. Android/UWP에서 Firebase 관련 기능(App ID 등)을 쓰려면 `LoginDesign3.Android/google-services.example.json`, `LoginDesign3.UWP/google-services.example.json`을 각각 `google-services.json`으로 복사하고 Firebase 콘솔에서 내려받은 실제 값으로 채웁니다. (이 파일들도 `.gitignore` 대상입니다.)
4. `AppConfig.cs`에 관리자 계정(`AdminUsername`/`AdminPassword`)을 채우면 로그인 화면에서 관리자 전용 로그인(하드코딩 분기)이 동작합니다. 값을 비워두면 해당 분기는 항상 실패하고 일반 Firebase 계정 로그인만 동작합니다.

## 9. 실행/빌드 환경 (문서상 검증 환경)

- Visual Studio 2022, Xamarin.Forms 5.0.0.2196
- 실제 단위/통합 시험은 **Android 13 (API 33)** 에뮬레이터/기기 기준으로 진행되었습니다 (시험 노트북: Samsung NT500R5T, i5 8세대, RAM 16GB).
- iOS/UWP 프로젝트 파일은 원본에 존재하여 그대로 보존했지만, 문서상 실제 빌드·시험이 이 두 플랫폼에서 이루어졌다는 근거는 없습니다.

## 10. 포함하지 않은 데이터 / 문서 / credential

다음은 개인정보 보호 및 출처 불명확성 때문에 **의도적으로 이 저장소에 포함하지 않았습니다** (원본 파일 자체는 삭제하지 않았으며, 원래 위치에 그대로 보존되어 있습니다):

- 팀원 이름, 학번, 전화번호, 개인 이메일, 개인 GitHub URL이 담긴 문서 (팀프로젝트편성서 등)
- 지도교수 관련 행정정보가 담긴 문서
- 회의록, 각종 HWP/PDF/PPTX 제출용 문서 전체
- 실제 병원/의사 명단으로 보이는 `DoctorsInfo.csv` / `DoctorsInfo.json` (출처 불명확 — 대신 완전히 가상의 `DoctorsInfo.example.json` 제공)
- MedQueue 실제 앱 녹화 영상 (화면에 이름/환자정보 노출 가능성 있어 별도 검토 후 선별 예정)
- 메디큐와 무관한 다른 프로젝트(IV 수액 모니터링 앱) 영상
- 실제 Firebase AuthSecret / API Key / 관리자 실제 ID·비밀번호

## 11. 알려진 한계 및 보안 주의사항

- **평문 비밀번호 구조**: `FirebaseHelper.cs`와 `User` 모델은 비밀번호를 평문으로 Firebase에 저장하고 평문 비교로 로그인을 처리합니다. 이는 당시 구현의 실제 구조이며, 이번 복원 단계에서는 "원본 그대로 재현"이 목적이므로 해싱/암호화 등 새로운 인증 구조를 도입하지 않았습니다. **실제 사용자 데이터를 다루는 용도로는 절대 사용하지 마세요.**
- **주민등록번호 입력 구조**: 당시 원본 프로젝트에서는 회원가입(`JoinPage`) 및 아이디/비밀번호 찾기(`IdPwRecoveryPage`) 과정에 주민등록번호를 입력받는 필드가 포함되어 있었습니다. 이는 과거 프로젝트의 원본 설계를 그대로 보존한 것이며, 현재의 개인정보보호 및 보안 관점에서 실제 서비스에 그대로 적용하기에는 부적절한 구조입니다. **본 공개 저장소에는 실제 주민등록번호 데이터가 포함되어 있지 않습니다.**
- **Firebase DB 규칙 미확인**: 원본 앱은 클라이언트 측 검증 외에 별도의 서버 측 인증/권한 검증이 코드에 보이지 않습니다. 실제 Firebase 프로젝트의 Realtime Database 보안 규칙이 어떻게 설정되어 있었는지는 이 저장소만으로는 알 수 없습니다.
- **3분할 Firebase 프로젝트**: 회원 DB, 검색용 DB, 대기열 DB가 서로 다른 Firebase 프로젝트로 분리되어 있어(7번 참고), 정상 동작하려면 세 프로젝트에 동일한 `Department/{과}/Doctor/{의사}` 트리를 각각 시딩해야 합니다.
- **iOS 프로젝트가 .sln에 없음**: `LoginDesign3.sln`에는 UWP·Android·공유 프로젝트만 등록되어 있고 iOS 프로젝트는 포함되어 있지 않습니다(원본 zip 구조 그대로). iOS 소스 파일 자체는 보존되어 있으나, Visual Studio에서 솔루션을 열었을 때 iOS 프로젝트가 자동으로 로드되지는 않습니다.
- **빌드 미검증**: 이 저장소는 정적 구조 검증(XML/XAML 잘 정돈됨, C# 중괄호 균형 확인)만 완료했으며, 실제 Visual Studio/Xamarin SDK를 이용한 빌드 성공 여부는 확인하지 못했습니다.

## 12. 프로젝트 복원 방식 — 원본/공개용 수정/미구현 구분

```
[원본 보존]
v1.3.2 zip에 실제로 존재했던 코드/구조 그대로.
App.xaml(.cs), MainPage, LoginPage, JoinPage, IdPwRecoveryPage, UserDataEdit,
Department.cs, FirebaseHelper.cs(URL 제외 로직), Search/Queue/Userqueue(로직 전체),
XAML 화면 구성, 이미지 리소스, Android/iOS/UWP 헤드 프로젝트 구조, .sln 구성 그대로.

[공개용 수정]
GitHub 공개를 위해 최소한으로 변경한 부분만 해당:
  - LoginPage.xaml.cs: 하드코딩된 관리자 ID/PW를 AppConfig 참조로 교체 + 값이 없을 때
    안전하게 실패하도록 조건 추가
  - FirebaseHelper.cs / Search.xaml.cs / Queue.xaml.cs / Userqueue.xaml.cs:
    하드코딩된 Firebase URL/AuthSecret을 AppConfig 참조로 교체
  - LoginDesign3.csproj: AppConfig.example.cs를 빌드에서 제외하는 규칙 1줄 추가
  - google-services.json(Android/UWP) 제거, google-services.example.json으로 대체
  - DoctorsInfo.csv/json(원본, 실제 병원 데이터로 추정) 미포함, 대신 완전히 가상의
    DoctorsInfo.example.json 신규 작성 [신규 공개용 예시 데이터]
  - .gitignore, README.md 신규 작성
그 외 알고리즘, 화면 흐름, Firebase 데이터 구조, UI 동작은 전혀 변경하지 않았습니다.

[미구현]
요구사항/향후계획에는 있었지만 코드에 없던 기능 (3번 참고) — 이번 복원에서도 추가하지 않음.
```
