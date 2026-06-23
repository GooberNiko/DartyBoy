# DartyBoy

A 2D iOS game (Unity 6000.4.5f1, URP) — a mix of Flappy Bird and the Geometry Dash "Wave".

## Building an iOS IPA without a Mac (GitHub Actions)

Unity only exports an **Xcode project**; turning it into an installable `.ipa` needs macOS + Xcode.
This repo's workflow (`.github/workflows/ios-build.yml`) does that in the cloud on GitHub's macOS
runners and produces an **unsigned `.ipa`**, which you then re-sign on your PC at install time with
**Sideloadly** or **AltStore** (using a free Apple ID — no Apple Developer membership needed).

### One-time setup

1. **Push this project to a new GitHub repo**
   ```bash
   git remote add origin https://github.com/<you>/DartyBoy.git
   git push -u origin main
   ```
   Tip: a **public** repo gets free Actions minutes. A private repo works too but macOS runners
   bill at 10x, so you'd burn through the free 2,000 min/month quickly.

2. **Add your Unity license as a repo secret** (Settings → Secrets and variables → Actions). GameCI
   needs a free Unity **Personal** license:
   - `UNITY_LICENSE` – the full contents of your local `.ulf` file. On Windows it lives at
     `C:\ProgramData\Unity\Unity_lic.ulf` (created when you activate a free Personal license in
     Unity Hub → Preferences → Licenses → Add). Copy/paste the whole file into the secret.
   - `UNITY_EMAIL` / `UNITY_PASSWORD` – your Unity account email/password. Usually optional for a
     Personal `.ulf`; add them only if a build fails complaining about activation.

   See <https://game.ci/docs/github/activation> for details.

### Each build

3. Go to the repo's **Actions** tab → **Build iOS IPA (unsigned)** → **Run workflow** (or just push
   to `main`). First run is slow (~15–25 min) while it downloads Unity; later runs are cached.
4. When it finishes, open the run and download the **`DartyBoy-ipa`** artifact → unzip →
   `DartyBoy-unsigned.ipa`.

### Install on your iPhone

5. On your Windows PC, use **[Sideloadly](https://sideloadly.io/)** (or AltStore):
   - Plug in your iPhone, open Sideloadly, drag in `DartyBoy-unsigned.ipa`.
   - Enter your Apple ID; it re-signs and installs.
   - On the phone: Settings → General → VPN & Device Management → trust your Apple ID.
   - Free Apple ID apps expire after **7 days** (re-install to renew) and are limited to 3 sideloaded apps.

### Notes / troubleshooting
- **Unity image not found:** Unity 6000.4.5f1 is recent; if GameCI hasn't published that exact
  Docker image yet, the `unity-export` job will fail with an image-not-found error. Either wait for
  GameCI to publish it, or temporarily set the project to the closest available 6000.4.x in
  `ProjectSettings/ProjectVersion.txt`.
- **Proper signing (optional):** if you ever get an Apple Developer account, the macOS job can be
  switched from `CODE_SIGNING_ALLOWED=NO` to a real `xcodebuild -exportArchive` with your
  certificate + provisioning profile stored as secrets, producing a directly-installable signed IPA.
