# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v8.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [src\BankSimple.Api\BankSimple.Api.csproj](#srcbanksimpleapibanksimpleapicsproj)
  - [src\BankSimple.Application\BankSimple.Application.csproj](#srcbanksimpleapplicationbanksimpleapplicationcsproj)
  - [src\BankSimple.Domain\BankSimple.Domain.csproj](#srcbanksimpledomainbanksimpledomaincsproj)
  - [src\BankSimple.Infrastructure\BankSimple.Infrastructure.csproj](#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 4 | 0 require upgrade |
| Total NuGet Packages | 5 | All compatible |
| Total Code Files | 16 |  |
| Total Code Files with Incidents | 0 |  |
| Total Lines of Code | 138 |  |
| Total Number of Issues | 0 |  |
| Estimated LOC to modify | 0+ | at least 0,0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [src\BankSimple.Api\BankSimple.Api.csproj](#srcbanksimpleapibanksimpleapicsproj) | net8.0 | ✅ None | 0 | 0 |  | AspNetCore, Sdk Style = True |
| [src\BankSimple.Application\BankSimple.Application.csproj](#srcbanksimpleapplicationbanksimpleapplicationcsproj) | net8.0 | ✅ None | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\BankSimple.Domain\BankSimple.Domain.csproj](#srcbanksimpledomainbanksimpledomaincsproj) | net8.0 | ✅ None | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\BankSimple.Infrastructure\BankSimple.Infrastructure.csproj](#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj) | net8.0 | ✅ None | 0 | 0 |  | ClassLibrary, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 5 | 100,0% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 0 | 0,0% |
| ***Total NuGet Packages*** | ***5*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| Microsoft.AspNetCore.OpenApi | 8.0.22 |  | [BankSimple.Api.csproj](#srcbanksimpleapibanksimpleapicsproj) | ✅Compatible |
| Microsoft.EntityFrameworkCore | 8.0.0 |  | [BankSimple.Infrastructure.csproj](#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj) | ✅Compatible |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 |  | [BankSimple.Api.csproj](#srcbanksimpleapibanksimpleapicsproj)<br/>[BankSimple.Infrastructure.csproj](#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj) | ✅Compatible |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.0 |  | [BankSimple.Infrastructure.csproj](#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj) | ✅Compatible |
| Swashbuckle.AspNetCore | 6.6.2 |  | [BankSimple.Api.csproj](#srcbanksimpleapibanksimpleapicsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;BankSimple.Domain.csproj</b><br/><small>net8.0</small>"]
    P2["<b>📦&nbsp;BankSimple.Application.csproj</b><br/><small>net8.0</small>"]
    P3["<b>📦&nbsp;BankSimple.Infrastructure.csproj</b><br/><small>net8.0</small>"]
    P4["<b>📦&nbsp;BankSimple.Api.csproj</b><br/><small>net8.0</small>"]
    P2 --> P1
    P3 --> P2
    P3 --> P1
    P4 --> P2
    P4 --> P3
    P4 --> P1
    click P1 "#srcbanksimpledomainbanksimpledomaincsproj"
    click P2 "#srcbanksimpleapplicationbanksimpleapplicationcsproj"
    click P3 "#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj"
    click P4 "#srcbanksimpleapibanksimpleapicsproj"

```

## Project Details

<a id="srcbanksimpleapibanksimpleapicsproj"></a>
### src\BankSimple.Api\BankSimple.Api.csproj

#### Project Info

- **Current Target Framework:** net8.0✅
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 3
- **Dependants**: 0
- **Number of Files**: 7
- **Lines of Code**: 34
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["BankSimple.Api.csproj"]
        MAIN["<b>📦&nbsp;BankSimple.Api.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srcbanksimpleapibanksimpleapicsproj"
    end
    subgraph downstream["Dependencies (3"]
        P2["<b>📦&nbsp;BankSimple.Application.csproj</b><br/><small>net8.0</small>"]
        P3["<b>📦&nbsp;BankSimple.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        P1["<b>📦&nbsp;BankSimple.Domain.csproj</b><br/><small>net8.0</small>"]
        click P2 "#srcbanksimpleapplicationbanksimpleapplicationcsproj"
        click P3 "#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj"
        click P1 "#srcbanksimpledomainbanksimpledomaincsproj"
    end
    MAIN --> P2
    MAIN --> P3
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

<a id="srcbanksimpleapplicationbanksimpleapplicationcsproj"></a>
### src\BankSimple.Application\BankSimple.Application.csproj

#### Project Info

- **Current Target Framework:** net8.0✅
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 2
- **Number of Files**: 5
- **Lines of Code**: 6
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (2)"]
        P3["<b>📦&nbsp;BankSimple.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        P4["<b>📦&nbsp;BankSimple.Api.csproj</b><br/><small>net8.0</small>"]
        click P3 "#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj"
        click P4 "#srcbanksimpleapibanksimpleapicsproj"
    end
    subgraph current["BankSimple.Application.csproj"]
        MAIN["<b>📦&nbsp;BankSimple.Application.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srcbanksimpleapplicationbanksimpleapplicationcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;BankSimple.Domain.csproj</b><br/><small>net8.0</small>"]
        click P1 "#srcbanksimpledomainbanksimpledomaincsproj"
    end
    P3 --> MAIN
    P4 --> MAIN
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

<a id="srcbanksimpledomainbanksimpledomaincsproj"></a>
### src\BankSimple.Domain\BankSimple.Domain.csproj

#### Project Info

- **Current Target Framework:** net8.0✅
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 3
- **Number of Files**: 1
- **Lines of Code**: 6
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (3)"]
        P2["<b>📦&nbsp;BankSimple.Application.csproj</b><br/><small>net8.0</small>"]
        P3["<b>📦&nbsp;BankSimple.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        P4["<b>📦&nbsp;BankSimple.Api.csproj</b><br/><small>net8.0</small>"]
        click P2 "#srcbanksimpleapplicationbanksimpleapplicationcsproj"
        click P3 "#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj"
        click P4 "#srcbanksimpleapibanksimpleapicsproj"
    end
    subgraph current["BankSimple.Domain.csproj"]
        MAIN["<b>📦&nbsp;BankSimple.Domain.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srcbanksimpledomainbanksimpledomaincsproj"
    end
    P2 --> MAIN
    P3 --> MAIN
    P4 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

<a id="srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj"></a>
### src\BankSimple.Infrastructure\BankSimple.Infrastructure.csproj

#### Project Info

- **Current Target Framework:** net8.0✅
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 2
- **Dependants**: 1
- **Number of Files**: 5
- **Lines of Code**: 92
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P4["<b>📦&nbsp;BankSimple.Api.csproj</b><br/><small>net8.0</small>"]
        click P4 "#srcbanksimpleapibanksimpleapicsproj"
    end
    subgraph current["BankSimple.Infrastructure.csproj"]
        MAIN["<b>📦&nbsp;BankSimple.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srcbanksimpleinfrastructurebanksimpleinfrastructurecsproj"
    end
    subgraph downstream["Dependencies (2"]
        P2["<b>📦&nbsp;BankSimple.Application.csproj</b><br/><small>net8.0</small>"]
        P1["<b>📦&nbsp;BankSimple.Domain.csproj</b><br/><small>net8.0</small>"]
        click P2 "#srcbanksimpleapplicationbanksimpleapplicationcsproj"
        click P1 "#srcbanksimpledomainbanksimpledomaincsproj"
    end
    P4 --> MAIN
    MAIN --> P2
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 0 |  |
| ***Total APIs Analyzed*** | ***0*** |  |

