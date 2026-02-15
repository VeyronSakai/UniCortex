# Unity Package Template

This is a template repository for creating Unity packages.

## Features

- Standard Unity package structure
- `.editorconfig` for consistent code styling
- GitHub workflows for CI/CD (lint, test, release)
- Unity meta files for all assets
- Sample folders structure

## Getting Started

1. Click "Use this template" to create a new repository
2. Update `package.json` with your package information:
   - `name`: Your package name (e.g., `com.yourcompany.packagename`)
   - `displayName`: Display name shown in Unity Package Manager
   - `version`: Package version (follow [Semantic Versioning](https://semver.org/))
   - `description`: Brief description of your package
   - `author.name`: Your name or organization
3. Update this README with your package documentation
4. Add your code to the appropriate folders:
   - `Runtime/`: Runtime scripts
   - `Editor/`: Editor-only scripts
   - `Tests/`: Unit tests
   - `Samples~/`: Sample scenes and assets (hidden from Unity)

## Package Structure

```
.
├── Editor/              # Editor scripts
├── Runtime/             # Runtime scripts
├── Tests/               # Tests
├── Samples~/            # Samples (hidden from Unity)
├── .editorconfig        # Code style configuration
├── .gitignore           # Git ignore rules
├── LICENSE.txt          # License file
├── README.md            # This file
└── package.json         # Package manifest
```

## Installation

Install this package via Unity Package Manager:

1. Open Package Manager in Unity
2. Click `+` button
3. Select "Add package from git URL"
4. Enter: `https://github.com/YOUR_USERNAME/YOUR_REPO.git`

## License

MIT License - see [LICENSE.txt](LICENSE.txt) for details
