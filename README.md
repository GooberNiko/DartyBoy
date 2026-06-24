# DartyBoy

A 2D iOS game (Unity 6000.4.5f1, URP) — a mix of Flappy Bird and the Geometry Dash "Wave".

## Building an iOS IPA without a Mac

Unity only *exports* an Xcode project; turning that into an `.ipa` needs macOS + Xcode. So:
**Unity runs locally on your PC** (where your license already works) to produce the Xcode project →
the project is uploaded as a **GitHub Release asset** (it's ~1 GB, far too big for git — the
`UnityRuntime` binary alone is >100 MB) → a **GitHub Actions macOS runner** compiles it into an
**unsigned `.ipa`** → you re-sign it at install time with **Sideloadly** using a free Apple ID.

No Unity login is needed in the cloud (that's what broke the GameCI approach), and no Apple
Developer membership is needed.

### Each build (repo: `GooberNiko/DartyBoy`, release tag `ios-xcode-latest`)

1. **Export from Unity (your PC):** File → Build Profiles → **iOS** → **Build** → output folder
   `Lilgame\ios-xcode`. (IL2CPP, a few minutes. Needs the iOS Build Support module installed.)

2. **Tarball + upload as the release asset** (from the repo's parent folder):
   ```bash
   tar -czf ios-xcode.tar.gz -C Lilgame ios-xcode
   gh release upload ios-xcode-latest ios-xcode.tar.gz --clobber -R GooberNiko/DartyBoy
   ```
   (First time only: `gh release create ios-xcode-latest ios-xcode.tar.gz -R GooberNiko/DartyBoy -t "iOS Xcode export"`.)

3. **Run the build:**
   ```bash
   gh workflow run ios-build.yml -R GooberNiko/DartyBoy -f release_tag=ios-xcode-latest
   ```
   (Or Actions tab → Build iOS IPA (unsigned) → Run workflow.) Takes ~7–8 min.

4. **Download the IPA:** Actions → latest run → **`DartyBoy-ipa`** artifact → unzip →
   `DartyBoy-unsigned.ipa`. Or: `gh run download <run-id> -R GooberNiko/DartyBoy -n DartyBoy-ipa`.

### Install on your iPhone (Windows, no Mac)

5. **[Sideloadly](https://sideloadly.io/)** (or AltStore): plug in your iPhone, drag in
   `DartyBoy-unsigned.ipa`, enter your Apple ID → it re-signs and installs. On the phone:
   Settings → General → VPN & Device Management → trust your Apple ID.
   Free-Apple-ID apps expire after **7 days** (re-install to renew); max 3 sideloaded apps.

### Notes
- `ios-xcode/` and `*.tar.gz` are git-ignored on purpose; the export ships via the release asset.
- **Proper signing (optional):** with an Apple Developer account, switch the macOS job from
  `CODE_SIGNING_ALLOWED=NO` to a real `xcodebuild -exportArchive` (certificate + provisioning
  profile as secrets) to get a directly-installable signed IPA.
