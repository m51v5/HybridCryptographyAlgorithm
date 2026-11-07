# Security policy

## For learning only

This app shows how a hybrid cipher works. **Don't use it to protect real data.** It has known weaknesses, kept on purpose so the algorithm stays easy to follow:

- **DES is broken.** Its 56-bit key can be found by brute force, so every other block is weak.
- **Keys come from `System.Random`,** which isn't a secure random generator.
- **The IVs are fixed,** so the same text with the same key always gives the same cipher.
- **There's no integrity check.** A changed cipher file isn't detected; it just decrypts to wrong text or fails.
- **The hybrid key is shown as plain text** in the window. Anyone who sees or copies it can decrypt the file.

For real data, use a maintained tool built on AES-GCM or ChaCha20-Poly1305, such as 7-Zip's AES-256 archives or `age`.

## Reporting a vulnerability

A weakness listed above isn't a vulnerability; it's part of the lesson. For anything else, like a way to make the app run code or touch files you didn't pick, please don't open a public issue. Use [private vulnerability reporting](https://github.com/m51v5/HybridCryptographyAlgorithm/security/advisories/new) instead, with the steps to reproduce it and the version you're on.

Fixes go into the latest release only.

## What the app does with your files and network

- **Files:** it reads only the file you pick, and writes only where you choose in the save dialog. It keeps no history, settings or logs.
- **Keys:** a new key is made for each encryption and shown in the window. It's never saved.
- **Network:** only the update check. It runs a few seconds after the app opens, and again when you click "Check for updates" in About. It sends one request to `https://api.github.com/repos/m51v5/HybridCryptographyAlgorithm/releases/latest`, which tells GitHub your IP address and the app's version. Nothing you type, encrypt or decrypt is ever sent. It never downloads anything; if a new version is out, it offers to open the release page.
- **Links:** the About window's links open your browser.
