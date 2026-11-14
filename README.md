# [개인 포트폴리오] 니케 필드 프로토타입 — Night City Event Field

> **비공식 개인 포트폴리오입니다.** 원작 게임·IP의 공식 개발·서비스·배포와 무관한 프로토타입입니다.

승리의 여신: 니케 × 사이버펑크 엣지러너를 소재로 작성한 콜라보 이벤트 필드 기획을 Unity로 구현한 개인 포트폴리오용 프로토타입입니다. 이 저장소는 **2026-10-29 최종 동결본**의 에디터용 소스와 Windows·WebGL 실행 빌드를 제공합니다.

## 시연 영상

### 필드

https://github.com/user-attachments/assets/f4bc13e5-f9e8-43e1-9651-eeee7d05e48f

### NET RUNNER

https://github.com/user-attachments/assets/3bbf8520-d5f2-42a0-9843-55d9d84aa380

## 플레이 및 다운로드

| 용도 | 링크 |
| --- | --- |
| PC 브라우저에서 플레이 | [Night City Event Field](https://nikke-edgerunners-event-field-prototype.tndudss78.chatgpt.site/) |
| Windows 실행 파일 다운로드 | [Windows 최종 동결본 ZIP](https://github.com/tndudss/portfolio-nikke-field-prototype/releases/download/final-2026-10-29/NIKKE_Field_Windows_Final_2026-10-29.zip) |
| 웹 빌드 다운로드 | [WebGL 최종 동결본 ZIP](https://github.com/tndudss/portfolio-nikke-field-prototype/releases/download/final-2026-10-29/NIKKE_Field_WebGL_Final_2026-10-29.zip) |
| Unity 에디터용 소스 | 이 저장소를 클론하거나 **Code → Download ZIP** |

바탕화면의 초기 창모드 빌드 이후, 스토리 색상·안티앨리어싱·메인타워 LED 개선을 반영한 최종본을 기준으로 합니다.

### Windows에서 실행

1. 릴리스의 **Assets**에서 `NIKKE_Field_Windows_Final_2026-10-29.zip`을 다운로드합니다.
2. ZIP 전체를 새 폴더에 압축 해제합니다.
3. `NightCityEventField.exe`를 실행합니다.

Unity 에디터 설치는 필요하지 않습니다. `NightCityEventField_Data/`, `MonoBleedingEdge/`, `D3D12/` 및 DLL·라이선스 파일을 EXE와 함께 유지하세요.

### 웹 빌드 실행

웹 빌드 ZIP에는 `index.html`, `Build/`, `StreamingAssets/`, `TemplateData/`와 로컬 서버 실행 파일이 들어 있습니다. 전체 압축을 해제하고 파일 구조를 유지하세요. HTML을 더블 클릭하는 방식으로는 실행되지 않습니다.

Windows 로컬 미리보기는 `Start-WebGL.cmd`를 실행하면 서버와 브라우저가 열립니다. 플레이하는 동안 서버 창을 열어 두세요. 기본 주소는 `http://127.0.0.1:18765/`이며, 사용 중이면 다른 포트가 선택되므로 서버 창에 표시된 주소를 이용합니다. 웹에 배포할 때는 압축을 푼 전체 폴더를 정적 웹 서버에서 제공합니다.

## Unity 에디터에서 열기

- 제작 버전: **Unity 6000.6.6f1**
- 렌더 파이프라인: **URP 17.6.0**
- 시작 씬: `Assets/Scenes/EventField.unity`

1. Unity Hub에서 Unity 6000.6.6f1을 설치합니다.
2. 저장소를 클론하거나 ZIP을 내려받아 압축을 해제합니다.
3. Unity Hub의 **Add → Add project from disk**에서 프로젝트 폴더를 선택합니다.
   - **Windows용:** 이 저장소의 루트 폴더
   - **WebGL용:** `WebGL_Project/` 폴더
4. 패키지 복원과 에셋 임포트가 끝나면 시작 씬을 열고 Play를 실행합니다.

WebGL 빌드를 만들려면 해당 Unity 버전의 WebGL Build Support 모듈이 필요합니다. 첫 실행에는 패키지 복원을 위한 인터넷 연결이 필요할 수 있습니다.

## 포함 파일

- `Assets/`: 필드 스크립트, 씬, 셰이더, 텍스처, 폰트, StreamingAssets 및 `.meta` 파일
- `Packages/`: Windows 프로젝트의 패키지 목록과 버전 잠금 파일
- `ProjectSettings/`: Windows 프로젝트 및 빌드 설정
- `Tools/`: NetRunner 웹뷰 보조 프로그램의 소스와 의존 파일
- `WebGL_Project/`: 최종 동결본의 독립된 WebGL Unity 프로젝트

`Library/`, `Temp/`, `Logs/`, `UserSettings/` 등의 캐시와 개인 설정은 버전 관리에서 제외합니다. 실행 빌드는 GitHub Releases에서 배포합니다.

## 조작 방법

- **WASD / 방향키:** 이동
- **바닥 클릭:** 클릭한 지점으로 이동
- **비콘 근처 E / 화면 버튼:** 콘텐츠 화면 열기
- **뒤로가기 / Esc:** 필드로 복귀
- **타워 포탈에서 대기:** 로비 입장 / 야외 복귀

## 최종 동결본 변경 사항

- 스토리 1부는 분홍색, 2부는 보라색으로 메뉴·비콘·전환 화면의 색상 통일
- 4배 MSAA 설정 및 메인타워 LED 선 조정
- 사용하지 않는 미니게임 객체 생성 제거

이 저장소는 보관된 최종본을 배포하며, 이번 업로드 과정에서 게임을 다시 빌드하거나 성능을 새로 측정하지 않았습니다.
