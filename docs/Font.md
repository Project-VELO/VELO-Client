# 폰트 기준

## 어떤 화면에 무엇을 쓰나

| 화면 | 폰트 |
| --- | --- |
| 스토리 감상 (대사 본문·화자 이름·중앙 문구) | **NotoSerifKR-SemiBold** |
| 스토리 선택 화면의 챕터·에피소드 제목 | **NotoSerifKR-SemiBold** |
| 사무실 캐릭터 대사·이름 | **NotoSerifKR-SemiBold** (스토리와 같은 서체·굵기로 맞춤) |
| 그 외 전부 | **NotoSansKR-Regular** |

스토리 본문을 SemiBold(600)로 올린 것은 Regular가 "매우 가녀리다"는 지적 때문입니다(PR #159).
Medium(500)은 Regular와 차이가 미묘해 채택하지 않았습니다. `NotoSerifKR-Regular SDF`와
`NotoSerifKR-Medium SDF`는 에셋으로 남아 있지만 현재 어떤 프리팹도 참조하지 않습니다.

Sans 계열에서 굵게가 필요한 곳은 프리팹에서 Bold를 지정하고, `NotoSansKR-Regular SDF`의 굵기 표
700 칸에 물린 `NotoSansKR-Bold SDF`가 실제 글리프를 그립니다.

## 폴백은 일본어 한 벌만 겁니다

한국어 SDF마다 **같은 서체·같은 굵기의 일본어 SDF 하나**만 폴백으로 겁니다
(`NotoSerifKR-SemiBold SDF` → `NotoSerifJP-SemiBold SDF`, `NotoSansKR-Regular SDF` → `NotoSansJP-Regular SDF`).
일본어 문장이 나올 때 그 글자만 여기서 나오게 하기 위해서이고, 굵기를 맞추는 것은 Regular 폴백을 두면
한 문장 안에서 일본어 구간만 얇게 튀기 때문입니다. 그 외의 폴백은 걸지 않습니다. 글자에 따라 서체가 섞여 나옵니다.

폴백 없이 되는 근거입니다. 프로젝트가 실제로 그리는 글자 **1,003종**(대사·화자 이름·마스터 데이터·프리팹과 씬에 박아 둔 문자열)을 모아 각 폰트의 글리프 목록과 대조했습니다.

| 폰트 | 글리프 수 | 빠진 글자 |
| --- | --- | --- |
| NotoSansKR-Regular | 23,174 | **없음** (U+200B 제외) |
| NotoSerifKR-SemiBold | 23,124 | **없음** (U+200B 제외) |
| GowunBatang-Bold | 12,494 | **16자** — 書 影 謠 庫 怪 談 幻 押 留 古 無 名 架 異 界 恨 |

U+200B는 제로 폭 공백입니다. 눈에 보이지 않고, 라이브 에디터 씬에만 3번 나옵니다.

**고운바탕에 NotoSerifKR이 폴백으로 걸려 있는 것은 위 한자 16자 때문입니다.** 대부분 스토리 지문의
한자 병기(怪談, 謠, 靈요書庫 등)에서 나옵니다. NotoSansKR로 바꾸면 이 폴백이 필요 없어집니다.

## SDF 에셋 설정

`VELO ▸ Font ▸ NotoSansKR SDF 만들기` 로 만듭니다. 값은 `NotoSerifKR-SemiBold SDF`와 같습니다.

| 항목 | 값 |
| --- | --- |
| 샘플링 크기 | 28 |
| 여백(Padding) | 5 |
| 렌더 모드 | SDFAA |
| 아틀라스 | 1024 × 1024, 멀티 아틀라스 켬 |
| 채우기 모드 | **Dynamic** |
| ClearDynamicDataOnBuild | 켬 |
| 폴백 | 같은 굵기의 일본어 SDF 한 벌 |

**Dynamic을 쓰는 이유** — 정적으로 구우면 한글 글자 수 때문에 에셋 하나가 40MB에 육박합니다.
고운바탕 두 벌이 그렇게 들어와 있어 저장소에서 79MB를 차지합니다. 동적은 6.7KB입니다.

**ClearDynamicDataOnBuild를 켜는 이유** — 꺼 두면 에디터에서 띄워 본 글자가 아틀라스에 남습니다.
사람마다 다른 내용이 형상 관리에 잡혀, 손대지 않은 폰트 에셋이 계속 변경으로 올라옵니다.

## 남아 있는 고운바탕

교체 뒤에도 고운바탕 네 파일(TTF 2, SDF 2, 합 95MB)은 지우지 않고 둡니다.
