# Contributing

Thanks for helping out. Bug reports, ideas and pull requests are all welcome.

## Reporting a bug or asking for a feature

Use the [issue forms](https://github.com/m51v5/HybridCryptographyAlgorithm/issues/new/choose). Never paste a real secret or private file; a short made-up text that shows the problem is enough.

For a security problem, see [SECURITY.md](SECURITY.md) instead of opening an issue.

## Making a change

The app is Windows-only, so you'll need Windows 10/11 and the .NET 8 SDK. The [development guide](docs/DEVELOPMENT.md) covers running from source, the checks and how the code is laid out.

1. Fork the repo and branch from `main`.
2. Make your change. For anything big, open an issue first so we can agree on the approach.
3. Run these, which must both pass:

   ```
   dotnet format HybridCryptographyAlgorithm.sln
   dotnet build HybridCryptographyAlgorithm.sln -c Release -warnaserror
   ```

4. Run the app and encrypt then decrypt a test file, to check the text comes back exactly.
5. Open a pull request saying what changed and why.

## Guidelines

- **Keep the algorithm readable.** This is a learning project, so clear code beats clever code. If a change makes it stronger, say so in the README and `SECURITY.md` too.
- **Keep the cipher format stable** within a version, so files encrypted with it still decrypt.
- **No new network access.** The update check is the only request the app makes.
- **Keep comments short:** one line that says why, not what.

By contributing, you agree that your work is released under the [MIT License](LICENSE). Everyone taking part is expected to follow the [code of conduct](https://github.com/m51v5/.github/blob/main/CODE_OF_CONDUCT.md).
