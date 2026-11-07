# Development

How to run, check and change Hybrid Cryptography Algorithm, and how a release is made.

---

## Run from source

You need Windows 10/11 and the .NET 8 SDK. `global.json` picks the newest 8.0 SDK you have. Run from the repo root:

```
dotnet run --project src/HybridCryptographyAlgorithm
```

Or open `HybridCryptographyAlgorithm.sln` in Visual Studio 2022 and press F5. NuGet packages restore on the first build.

## Checks

These are what CI runs, and both must pass:

```
dotnet format HybridCryptographyAlgorithm.sln --verify-no-changes
dotnet build HybridCryptographyAlgorithm.sln -c Release -warnaserror
```

`dotnet format HybridCryptographyAlgorithm.sln` fixes formatting, following `.editorconfig`. There are no automated tests yet, so after a change to the algorithm, encrypt and decrypt a few test files: short text, exactly 16 bytes, several blocks, line breaks, and non-English letters. The decrypted text must match exactly.

## Build a release

```
.\scripts\build-release.ps1
```

This stops a running copy, publishes one self-contained exe to `dist\HybridCryptographyAlgorithm\`, and writes `THIRD_PARTY_LICENSES.txt` and `LICENSE` next to it. Add `-Zip <name>.zip` to zip the three files, as the release does. If PowerShell blocks scripts, run `powershell -ExecutionPolicy Bypass -File .\scripts\build-release.ps1`.

The exe is about 70 MB because it carries the .NET runtime, so it runs on any Windows 10/11 PC with nothing to install.

**Third-party licenses:** the script lists every package that ships in the exe, from `obj/project.assets.json`, plus the .NET runtime. It copies each package's license files, or writes the MIT text with its copyright line. A package under a license that isn't in `$allowed` at the top of the script stops the build until you've read that license and added it.

## Publish a release

Pushing to `main` runs CI when the commit changes what gets built: `src/`, the `.sln`, `global.json`, `.editorconfig` or `ci.yml` itself. Changes to anything else, like the README or docs, run nothing. Pushing a tag publishes a release. A tag adds a name to a commit and changes no code, so CI doesn't run again for it. Wait for CI before tagging.

Replace `1.1.0` with the new version.

1. Start from an up-to-date `main`, with nothing uncommitted:
   ```
   git checkout main
   git pull
   git status
   ```
2. Set `<Version>1.1.0</Version>` in `src/HybridCryptographyAlgorithm/HybridCryptographyAlgorithm.csproj`.
3. Commit and push:
   ```
   git add src/HybridCryptographyAlgorithm/HybridCryptographyAlgorithm.csproj
   git commit -m "chore: release v1.1.0"
   git push
   ```
4. Wait for CI to pass, in the Actions tab or with:
   ```
   gh run list --workflow ci.yml --limit 1
   ```
5. Tag that same commit and push the tag. Don't commit anything in between:
   ```
   git tag v1.1.0
   git push origin v1.1.0
   ```
6. Wait for the Release workflow. It runs `scripts/build-release.ps1`, zips the exe with `LICENSE` and `THIRD_PARTY_LICENSES.txt` as `hybrid-cryptography-algorithm-v1.1.0-windows-x64.zip`, and publishes a GitHub Release.
7. Write the notes. Generated notes only list merged pull requests, so they're nearly empty for commits pushed straight to `main`. Click **Edit** on the release, or run:
   ```
   gh release edit v1.1.0 --notes-file notes.md
   ```
8. Download the zip, unzip it into an empty folder, and run it. Open About: "Check for updates" should say "Up to date".

**If the Release workflow fails,** nothing is published. It stops when:

- the tagged commit isn't on `main`
- the tag and `<Version>` in the `.csproj` differ
- a shipped package's license isn't in `$allowed` in `scripts/build-release.ps1`

Remove the tag, push your fix to `main`, wait for CI if the fix started it, then repeat step 5:

```
git tag -d v1.1.0
git push origin --delete v1.1.0
```

---

## Source layout

```
src/HybridCryptographyAlgorithm/
  Algorithms/
    HybridCryptor.cs        splits text into 16-byte blocks, alternates AES and DES
    Original/AESCryptor.cs  AES-128, CBC, PKCS7
    Original/DESCryptor.cs  DES, CBC, PKCS7
  Utility/FileConvertor.cs  reads the picked file, writes through a save dialog
  MainWindow.xaml(.cs)      encrypt and decrypt forms, corner buttons, update check on launch
  AboutWindow.xaml(.cs)     About window
  AppLinks.cs               every address the app opens or reads, and the version
  UpdateChecker.cs          asks GitHub for the latest release
  Assets/                   app icon
scripts/build-release.ps1   release exe, licenses and zip
```

## Things to know

- **Cipher format:** UTF-8 text cut into 16-byte blocks; block 1 is AES, block 2 DES, and so on; each block is Base64, joined with `.`. The hybrid key is `Base64(DES key)` (12 characters) followed by `Base64(AES key)` (24 characters). Changing any of this breaks files encrypted with older versions.
- **Version:** `<Version>` in the `.csproj` is the only place to change it. The About window, the bug-report link and the update check all read it.
- **Update check:** it treats "no release yet" (404) as up to date, and any network error as "Couldn't reach GitHub", without a dialog.
