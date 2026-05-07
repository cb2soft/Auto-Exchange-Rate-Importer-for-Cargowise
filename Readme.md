<div align="center">

# 💱 Exchange Rate Importer

**Automated daily exchange rate fetcher & CargoWise One importer for multi-country logistics operations.**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![CargoWise](https://img.shields.io/badge/Integration-CargoWise%20One-FF6600)](https://www.wisetechglobal.com/cargowise/)

---

*A .NET 8 console application that fetches live buy/sell exchange rates from central banks and financial institutions across 5 countries, then automatically imports them into CargoWise One via the Native XML API.*

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Supported Countries](#-supported-countries)
- [Architecture](#-architecture)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Usage](#-usage)
- [Scheduling](#-scheduling-with-windows-task-scheduler)
- [Logging](#-logging)
- [Troubleshooting](#-troubleshooting)
- [Project Structure](#-project-structure)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🔍 Overview

In freight forwarding and logistics, accurate and up-to-date exchange rates are critical for billing, quoting, and financial reporting. This tool eliminates the manual process of looking up and entering rates by:

1. **Fetching** live exchange rates from official central bank and financial institution APIs
2. **Transforming** rates into CargoWise-compatible Native XML messages
3. **Importing** both BUY and SELL rates into CargoWise One via HTTP POST
4. **Logging** every transaction with detailed status tracking

The program supports dual-server targeting (Test / Production) and handles business-day logic automatically (e.g., skipping weekends for Turkey's TCMB).

---

## 🌍 Supported Countries

| Country | Data Source | Currencies | CW Company Code | Method |
|:---:|:---:|:---:|:---:|:---:|
| 🇹🇷 **Turkey** | [TCMB](https://www.tcmb.gov.tr) (Central Bank) | EUR, GBP, USD | `TR1` | XML Feed |
| 🇮🇳 **India** | [Axis Bank](https://www.axisbank.com) | ~17 currencies | `IND` | HTML Scraping |
| 🇦🇪 **UAE** | [Global Exchange Rates API](https://globalexchangerates.org) (CBAE) | EUR, GBP, JPY, SAR, SGD, USD | `AE4` | REST API (XML) |
| 🇦🇺 **Australia** | [NAB Bank](https://www.nab.com.au) | ~50 currencies (IMT rates) | `AU1` | GraphQL API (OAuth2) |
| 🇻🇳 **Vietnam** | [Vietcombank](https://www.vietcombank.com.vn) | USD, EUR, GBP | `VN1` | REST API (JSON) |

---

## 🏗 Architecture

```
┌──────────────────────────────────────────────────────────┐
│                  Exchange Rate Sources                    │
│  ┌───────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌─────────────┐  │
│  │ TCMB  │ │ Axis │ │ CBAE │ │ NAB  │ │ Vietcombank │  │
│  │ (XML) │ │(HTML)│ │(XML) │ │(GQL) │ │   (JSON)    │  │
│  └───┬───┘ └──┬───┘ └──┬───┘ └──┬───┘ └──────┬──────┘  │
└──────┼────────┼────────┼────────┼─────────────┼──────────┘
       │        │        │        │             │
       ▼        ▼        ▼        ▼             ▼
┌──────────────────────────────────────────────────────────┐
│              Exchange Rate Importer (.NET 8)              │
│                                                          │
│  ┌─────────────────┐    ┌──────────────────────────┐     │
│  │ Rate Collectors  │───▶│  Native XML Transformer  │     │
│  │ (per country)    │    │  (CargoWise schema)      │     │
│  └─────────────────┘    └───────────┬──────────────┘     │
│                                     │                    │
│  ┌─────────────────┐    ┌───────────▼──────────────┐     │
│  │   File Logger   │◀───│     HttpXmlClient        │     │
│  │ (TextFiles/)    │    │  (Basic Auth + GZip)     │     │
│  └─────────────────┘    └───────────┬──────────────┘     │
└─────────────────────────────────────┼────────────────────┘
                                      │
                                      ▼
                          ┌───────────────────────┐
                          │   CargoWise One API    │
                          │   (TST or PRD Server)  │
                          └───────────────────────┘
```

---

## ✅ Prerequisites

- [**.NET 8.0 Runtime**](https://dotnet.microsoft.com/download/dotnet/8.0) or SDK
- **Windows OS** (for Task Scheduler integration)
- **CargoWise One** API access (endpoint URL + credentials)
- **Global Exchange Rates API Key** (required for UAE only)

---

## 📦 Installation

### Clone the Repository

```bash
git clone https://github.com/yourusername/Turkey-ExchangeRateImporter.git
cd Turkey-ExchangeRateImporter
```

### Build

```bash
dotnet build --configuration Release
```

### Publish (Self-Contained Executable)

```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained
```

---

## ⚙ Configuration

All sensitive configuration is managed through **Windows Environment Variables** for security.

### Required Environment Variables

| Variable | Description | Required For |
|:---|:---|:---:|
| `Cargowise_URI_TST` | CargoWise **Test** server API endpoint | All (TST) |
| `Cargowise_User_TST` | CargoWise **Test** username | All (TST) |
| `Cargowise_Password_TST` | CargoWise **Test** password | All (TST) |
| `Cargowise_URI_PRD` | CargoWise **Production** server API endpoint | All (PRD) |
| `Cargowise_User_PRD` | CargoWise **Production** username | All (PRD) |
| `Cargowise_Password_PRD` | CargoWise **Production** password | All (PRD) |
| `Global_Exchange_Rates_API_Key` | Global Exchange Rates subscription key | UAE only |

### Setting Environment Variables

```powershell
# PowerShell (Run as Administrator)
[System.Environment]::SetEnvironmentVariable("Cargowise_URI_PRD", "https://your-cw-endpoint.com/...", "Machine")
[System.Environment]::SetEnvironmentVariable("Cargowise_User_PRD", "your_username", "Machine")
[System.Environment]::SetEnvironmentVariable("Cargowise_Password_PRD", "your_password", "Machine")
```

> ⚠️ **Important:** A system restart may be required for environment variable changes to take effect.

---

## 🚀 Usage

### Command-Line Syntax

```
Turkey-ExchangeRateImporter.exe <Country> <Server>
```

| Parameter | Description | Valid Values |
|:---|:---|:---|
| `Country` | Target country for rate collection | `Turkey`, `India`, `UAE`, `Australia`, `Vietnam` |
| `Server` | CargoWise target environment | `TST` (Test), `PRD` (Production) |

### Examples

```bash
# Fetch Turkey rates and import to Test server
Turkey-ExchangeRateImporter.exe Turkey TST

# Fetch India rates and import to Production server
Turkey-ExchangeRateImporter.exe India PRD

# Fetch UAE rates and import to Production server
Turkey-ExchangeRateImporter.exe UAE PRD

# Fetch Australia rates and import to Production server
Turkey-ExchangeRateImporter.exe Australia PRD

# Fetch Vietnam rates and import to Production server
Turkey-ExchangeRateImporter.exe Vietnam PRD
```

---

## ⏰ Scheduling with Windows Task Scheduler

For automated daily execution, create a scheduled task for each country.

### Recommended Schedule

| Country | Recommended Time | Reason |
|:---:|:---:|:---|
| 🇹🇷 Turkey | **15:30 local** | TCMB publishes rates around 15:30 |
| 🇮🇳 India | **12:00 IST** | Axis Bank updates mid-morning |
| 🇦🇪 UAE | **10:00 GST** | Central Bank rates available by morning |
| 🇦🇺 Australia | **09:00 AEST** | NAB updates early morning |
| 🇻🇳 Vietnam | **10:00 ICT** | Vietcombank updates by morning |

### Task Scheduler Configuration

1. Open **Task Scheduler** (`taskschd.msc`)
2. Click **Create Task**
3. Configure:
   - **General → Name:** `ExchangeRate_Turkey_PRD`
   - **General → Security:** *Run whether user is logged on or not*
   - **Trigger:** Daily at the recommended time
   - **Action:**
     - **Program:** `C:\path\to\Turkey-ExchangeRateImporter.exe`
     - **Arguments:** `Turkey PRD`
     - **Start in:** `C:\path\to\`
   - **Settings → Stop task if it runs longer than:** `30 minutes`

---

## 📄 Logging

Every execution generates detailed log files for audit and troubleshooting.

- **Location:** `<program directory>/TextFiles/`
- **Naming Convention:** `ImportLog_<Country>_<yyyyMMdd_HHmm>.txt`

### Log Format

```
15:30:05 - EUR BUY: 1 Insert
15:30:06 - EUR SEL: 1 Insert
15:30:07 - GBP BUY: 1 Update
15:30:08 - GBP SEL: 1 Update
15:30:09 - USD BUY: 1 Insert
15:30:10 - USD SEL: 1 Insert
```

### Status Codes

| Status | Meaning |
|:---|:---|
| `1 Insert` | ✅ New exchange rate record created |
| `1 Update` | ✅ Existing exchange rate updated |
| `NO UPDATES` | ⚠️ No changes made — may require review |
| `Other/Error` | ❌ Error occurred — check the Details field |

> **Note:** For Australia, the raw NAB API response is also saved as `TextFiles/FX_NAB_raw_<datetime>.json`.

---

## 🔧 Troubleshooting

| Issue | Possible Cause | Solution |
|:---|:---|:---|
| Program does not start | .NET 8 Runtime missing | Run `dotnet --version` to verify installation |
| `Invalid server parameter` | Wrong 2nd argument | Use `TST` or `PRD` (case-insensitive) |
| `Usage:` message appears | Missing arguments | Provide both: `Turkey PRD` |
| Turkey rates unavailable | TCMB connection issue | Check internet access to `tcmb.gov.tr` |
| UAE rates unavailable | Missing/invalid API key | Verify `Global_Exchange_Rates_API_Key` env var |
| CargoWise import fails | Bad credentials or endpoint | Verify `Cargowise_URI_*` and credential env vars |
| Log file not created | Permission issue | Ensure write permissions in program directory |
| No rates on weekends (Turkey) | Expected behavior | TCMB doesn't publish weekend rates; next business day is used |
| India scraping fails | Website layout changed | Axis Bank HTML structure may have been updated — code update needed |

---

## 📁 Project Structure

```
Turkey-ExchangeRateImporter/
├── Turkey-ExchangeRateImporter.sln        # Solution file
└── Turkey-ExchangeRateImporter/
    ├── Program.cs                         # Main application logic
    │                                      #   - Rate collectors (per country)
    │                                      #   - XML message builders
    │                                      #   - HTTP POST & response parsing
    │                                      #   - File logging
    ├── HttpXmlClient.cs                   # HTTP client for CargoWise XML API
    │                                      #   - Basic authentication
    │                                      #   - GZip response support
    ├── UniversalResponse.cs               # CargoWise Universal Response model
    │                                      #   - XML deserialization classes
    ├── Turkey-ExchangeRateImporter.csproj  # Project configuration (.NET 8)
    ├── SOP.md                             # Standard Operating Procedure (Turkish)
    ├── SOP_EN.md                          # Standard Operating Procedure (English)
    └── Test.bat                           # Quick test script
```

---

## 🤝 Contributing

1. **Fork** the repository
2. Create a **feature branch** (`git checkout -b feature/new-country`)
3. **Commit** your changes (`git commit -m "Add support for new country"`)
4. **Push** to the branch (`git push origin feature/new-country`)
5. Open a **Pull Request**

### Adding a New Country

To add support for a new country, you need to:

1. Add exchange rate arrays (`exchangeType`, `BuyRates`, `SellRates`) for the new country
2. Implement an `ExchangeCollector<Country>()` method to fetch rates from the source
3. Implement a `SendData<Country>()` method to build and send XML messages
4. Add the country parameter to the `Main()` method's argument handling
5. Update the CW company code in the XML template (`<GlbCompany>`)

---

## 📜 License

You can use this project any way you want

---

<div align="center">

**Built with ❤️ for the logistics industry**

</div>
