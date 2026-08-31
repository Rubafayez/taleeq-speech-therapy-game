<h1 align="center">Taleeq — طليق</h1>

<p align="center">
  <b>An interactive mobile game that helps children improve speech fluency,
  built around established stuttering therapy techniques.</b>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity_6-000000?style=flat-square&logo=unity&logoColor=white">
  <img src="https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=dotnet&logoColor=white">
  <img src="https://img.shields.io/badge/Firebase-FFCA28?style=flat-square&logo=firebase&logoColor=black">
  <img src="https://img.shields.io/badge/iOS_·_Android-lightgrey?style=flat-square">
  <img src="https://img.shields.io/badge/Arabic_RTL-success?style=flat-square">
</p>

---

## The problem

Speech therapy for children who stutter works, but it depends on repetition — and repetition in a
clinical room is where children lose interest. Sessions are also expensive, infrequent, and
disconnected from the situations where speech actually breaks down: ordering food, answering a
question, talking to a stranger.

**Taleeq** ("fluent" in Arabic) moves that practice into a game. Children rehearse real-life
scenarios in a world they want to return to, and parents get a clear view of what happened in
each session without having to be in the room.

---

## Therapeutic approach

The game is not a generic set of speaking exercises. It is structured around two recognised
therapy techniques:

**Psychodrama** — children practise speech through role-play in scenarios drawn from daily life
rather than repeating isolated words. Rehearsing the situation, not just the sound, is what makes
the fluency transfer outside the session.

**Smith Accent Breathing** — a rhythm-and-breath method where speech is paced against controlled
breathing patterns. The game builds the timing into play, so the child follows the rhythm without
having to think about it as an exercise.

---

## Features

### 🎮 For the child
- Scenario-based gameplay mapped to real-life speaking situations
- Breathing and rhythm exercises woven into game mechanics
- A customisable avatar and personal profile
- Progression through a game map that unlocks as skills develop
- Full Arabic interface with right-to-left text rendering

### 👨‍👩‍👧 For the parent
- A family dashboard covering every child profile in the household
- Detailed session reports showing what was practised and how it went
- **Audio recordings** of each session — parents hear the actual progress rather than reading a
  summary of it
- Progress tracked over time across sessions
- Calendar view of practice history

### 🔐 Access control
- Firebase Authentication for parent accounts
- Multi-step registration for parent and child profiles
- **PIN protection** on the parent area so a child cannot reach reports or settings — with PIN
  setup, change, and reset flows
- Security questions for account recovery

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Engine** | Unity 6 (6000.3.5f1) |
| **Language** | C# |
| **Authentication** | Firebase Authentication |
| **Database** | Cloud Firestore |
| **Storage** | Firebase Storage — session audio recordings |
| **Arabic text** | RTLTMPro — right-to-left rendering for TextMesh Pro |
| **UI** | Unity UI (uGUI) |
| **Platforms** | iOS · Android |

---

## Project Structure

```
Assets/
├── T-Scripts/                    Authentication and registration
│   ├── FirebaseInitializer.cs    Firebase startup
│   ├── FirebaseManager.cs        Auth, Firestore, and Storage access
│   ├── RegisterManager.cs        Parent registration flow
│   ├── ParentRegisterStep1.cs    Multi-step registration
│   ├── RegistrationState.cs      State carried across registration scenes
│   ├── childReg.cs               Child profile creation
│   ├── calanderSystem.cs         Session calendar
│   └── TogglePasswordView.cs     Password visibility control
│
├── scripts/                      Profiles, security, and dashboard
│   ├── FamilyDashboardManager.cs Parent dashboard across child profiles
│   ├── AvatarManager.cs          Avatar customisation
│   ├── PinSetupManager.cs        PIN creation
│   ├── ProfilePINController.cs   PIN entry and verification
│   ├── UpdateUserPIN.cs          PIN change
│   ├── VerifySecurityAnswer.cs   Account recovery
│   ├── PINAutoMove.cs            PIN input field behaviour
│   ├── createCardSystem.cs       Profile cards
│   └── imageSystem.cs            Image handling and upload
│
├── Scenes/
│   ├── login.unity                        Sign in
│   ├── Register 2.unity                   Parent registration
│   ├── ChildReg.unity                     Child profile creation
│   ├── WhoUare.unity                      Profile selection
│   ├── PIN.unity                          PIN entry
│   ├── PIN in profile.unity               PIN within a profile
│   ├── createPINinReg / editPIN.unity     PIN setup and change
│   ├── Confirm identity in PIN change     Identity confirmation
│   └── map.unity                          Game world
│
├── Sprites/ · UI-Image/          Game art and interface assets
├── Font/                         Arabic typefaces
├── RTLTMPro/                     Right-to-left text rendering
└── prefab/                       Reusable prefabs
```

---

## Getting Started

### Prerequisites

- **Unity 6000.3.5f1** or newer
- A Firebase project with Authentication, Firestore, and Storage enabled
- Xcode for iOS builds, Android SDK for Android builds

### Setup

**1. Restore the Firebase SDK**

The Firebase Unity SDK and its native binaries are not committed — they add roughly 220 MB and are
restored rather than versioned. Download the
[Firebase Unity SDK](https://firebase.google.com/download/unity) and import:

- `FirebaseAuth.unitypackage`
- `FirebaseFirestore.unitypackage`
- `FirebaseStorage.unitypackage`

**2. Connect your Firebase project**

Place your own configuration files in `Assets/`:

- `google-services.json` for Android
- `GoogleService-Info.plist` for iOS

**3. Restore third-party art packages**

Several asset-store packages are excluded for the same reason. Re-import them through the Package
Manager, or replace the missing references with your own art.

**4. Open and build**

Open the project in Unity, load `Assets/Scenes/login.unity`, and press Play. For device builds use
**File → Build Settings**, select iOS or Android, and build as usual.

---

## Localisation

The interface is entirely in Arabic and renders right-to-left throughout, using RTLTMPro over
TextMesh Pro so Arabic letterforms connect and shape correctly — something Unity's default text
rendering does not handle.

---

## Repository Notes

Firebase native libraries, purchased asset packs, and Unity's `Library/` folder are excluded from
version control. Any API keys that appeared in the source have been replaced with placeholders.
Supply your own Firebase configuration as described above.
