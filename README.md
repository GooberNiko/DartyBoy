# DartyBoy

A 2D iOS game (Unity 6000.4.5f1, URP) — a mix of Flappy Bird and the Geometry Dash "Wave".

## Building an iOS IPA without a Mac

Unity only *exports* an Xcode project; turning that into an `.ipa` needs macOS + Xcode. We let
**Unity run locally on your PC** (where your license already works) to produce the Xcode project,
then **GitHub Actions' macOS runner** compiles it into an **unsigned `.ipa`**. You re-sign that IPA
at install time with **Sideloadly** (or AltStore) using a free Apple ID — no Apple Developer
membership, and no Unity login in the cloud (which is what broke the old GameCI approach).

### Each build

1. **Export the Xcode project from Unity (on your PC):**
   - Make sure the **iOS Build Support** module is installed (Unity Hub → your editor → Add modules).
   - In Unity: **File → Build Profiles** (or Build Settings) → select **iOS** → **Switch Platform** →
     **Build**, and choose the output folder **`ios-xcode`** inside this project
     (`...\UNTY TEST\Lilgame\ios-xcode`). Let it finish (it runs IL2CPP — a few minutes).

2. **Commit & push the export:**
   ```bash
   git add ios-xcode
   git commit -m "iOS Xcode export"
   git push
   ```
   The push automatically triggers the **Build iOS IPA (unsigned)** workflow.

3. **Download the IPA:** repo → **Actions** → latest run → download the **`DartyBoy-ipa`** artifact →
   unzip → `DartyBoy-unsigned.ipa`.

### Install on your iPhone (Windows, no Mac)

4. Use **[Sideloadly](https://sideloadly.io/)** (or AltStore):
   - Plug in your iPhone, open Sideloadly, drag in `DartyBoy-unsigned.ipa`.
   - Enter your Apple ID; it re-signs and installs.
   - On the phone: Settings → General → VPN & Device Management → trust your Apple ID.
   - Free-Apple-ID apps expire after **7 days** (just re-install to renew); max 3 sideloaded apps.

### Notes
- The committed `ios-xcode/` folder is large (it includes IL2CPP-generated C++). That's expected; it
  gets overwritten each export. Keeping the repo **public** keeps Actions minutes free.
- **Proper signing (optional):** with an Apple Developer account you can switch the macOS job from
  `CODE_SIGNING_ALLOWED=NO` to a real `xcodebuild -exportArchive` using your certificate +
  provisioning profile (stored as secrets) to get a directly-installable signed IPA.
