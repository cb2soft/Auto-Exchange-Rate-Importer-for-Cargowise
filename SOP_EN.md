# Exchange Rate Importer — SOP (Standard Operating Procedure)

**Last Updated:** 09.03.2026

---

## 1. Purpose

This program automatically retrieves daily exchange rates (BUY/SELL) from various central banks and financial institutions, and imports them into **CargoWise One** via XML messages.

---

## 2. Supported Countries

| Parameter | Country | Source | Currencies | CW Company Code |
|---|---|---|---|---|
| `Turkey` | Turkey | TCMB (tcmb.gov.tr) | EUR, GBP, USD | TR1 |
| `India` | India | Axis Bank website | ~17 currencies | IND |
| `UAE` | UAE | Global Exchange Rates API | EUR, GBP, JPY, SAR, SGD, USD | AE4 |
| `Australia` | Australia | NAB Bank API | ~50 currencies | AU1 |
| `Vietnam` | Vietnam | Vietcombank API | USD, EUR, GBP | VN1 |

---

## 3. Prerequisites

### 3.1 Software
- **.NET 8.0 Runtime** must be installed.

### 3.2 Windows Environment Variables

The following environment variables must be configured under **System Environment Variables**:

| Variable Name | Description |
|---|---|
| `Cargowise_URI_TST` | CargoWise **Test** server API endpoint |
| `Cargowise_User_TST` | CargoWise **Test** username |
| `Cargowise_Password_TST` | CargoWise **Test** password |
| `Cargowise_URI_PRD` | CargoWise **Production** server API endpoint |
| `Cargowise_User_PRD` | CargoWise **Production** username |
| `Cargowise_Password_PRD` | CargoWise **Production** password |
| `Global_Exchange_Rates_API_Key` | Global Exchange Rates API key (required for UAE only) |

#### How to Add Environment Variables:
1. **Win + R** → `sysdm.cpl` → Enter
2. **Advanced** tab → **Environment Variables**
3. Under **System variables**, click **New**
4. Enter the variable name and value from the table above
5. Click **OK** to save
6. **Restart the server/computer** for changes to take effect

---

## 4. Usage

### 4.1 Command-Line Format

```
ExchangeRateImporter.exe <Country> <Server>
```

| Parameter | Description | Valid Values |
|---|---|---|
| **1st parameter** (Country) | Which country's rates to retrieve | `Turkey`, `India`, `UAE`, `Australia`, `Vietnam` |
| **2nd parameter** (Server) | Which CargoWise server to send to | `TST` (Test), `PRD` (Production) |

### 4.2 Examples

```bash
# Send Turkey rates to TEST server
ExchangeRateImporter.exe Turkey TST

# Send India rates to PRODUCTION server
ExchangeRateImporter.exe India PRD

# Send UAE rates to PRODUCTION server
ExchangeRateImporter.exe UAE PRD
```

---

## 5. Windows Task Scheduler Setup

A separate scheduled task should be created for each country.

### 5.1 Create a New Task

1. **Win + R** → `taskschd.msc` → Enter
2. In the right panel, select **Create Task**

### 5.2 General Tab
- **Name:** `ExchangeRate_Turkey_PRD` (example)
- **Run whether user is logged on or not:** ✅ Checked

### 5.3 Triggers Tab
- **New** → **Daily**
- **Start:** Set to a time after the country's central bank publishes rates

| Country | Recommended Time (Server local time) |
|---|---|
| Turkey | 15:30 (TCMB updates rates around 15:30) |
| India | 12:00 IST |
| UAE | 10:00 GST |
| Australia | 09:00 AEST |
| Vietnam | 10:00 ICT |

### 5.4 Actions Tab
- **Action:** Start a program
- **Program/script:** `C:\path\to\ExchangeRateImporter.exe`
- **Add arguments:** `Turkey PRD` (country and server parameter)
- **Start in:** `C:\path\to\` (directory where the program is located)

### 5.5 Settings Tab
- **Allow task to be run on demand:** ✅
- **Stop the task if it runs longer than:** `30 minutes`
- **If the task is already running:** `Do not start a new instance`

---

## 6. Log Files

The program automatically generates log files on each run:

- **Location:** `<program directory>/TextFiles/`
- **Format:** `ImportLog_<Country>_<DateTime>.txt`
- **Example:** `ImportLog_Turkey_20260309_1530.txt`

### Log Contents
```
15:30:05 - EUR BUY: 1 Insert
15:30:06 - EUR SEL: 1 Insert
15:30:07 - GBP BUY: 1 Update
15:30:08 - GBP SEL: 1 Update
15:30:09 - USD BUY: 1 Insert
15:30:10 - USD SEL: 1 Insert
```

| Status | Meaning |
|---|---|
| `1 Insert` | New exchange rate record created |
| `1 Update` | Existing exchange rate updated |
| `NO UPDATES` | No changes made — may require review |
| `Other/Error` | Error occurred — check the Details section |

---

## 7. Troubleshooting

| Issue | Possible Cause | Solution |
|---|---|---|
| Program does not run at all | .NET 8 Runtime not installed | Run `dotnet --version` to verify; install if missing |
| `Invalid server parameter` error | Invalid 2nd argument | Use `TST` or `PRD` |
| `Usage:` message appears | Missing parameters | Provide both parameters: `Turkey PRD` |
| Cannot retrieve rates (Turkey) | TCMB connection issue | Check internet connectivity and access to `tcmb.gov.tr` |
| Cannot retrieve rates (UAE) | API key missing or invalid | Verify `Global_Exchange_Rates_API_Key` environment variable |
| Cannot send to CargoWise | Incorrect/missing env variables | Verify `Cargowise_URI_TST/PRD` and credentials |
| Log file not created | Permission issue | Ensure write permissions in the program directory |
| No rates on weekends (Turkey) | TCMB does not publish weekend rates | Normal behavior — program assigns the next business day |

---

## 8. Country-Specific Notes

### Turkey
- TCMB only publishes rates on **business days**. The program automatically sets the date to the next business day for weekend runs.

### India
- Rates are scraped from the Axis Bank web page using HTML parsing. If the website structure changes, the program must be updated.
- `CNH` currency is automatically converted to `CNY` for CargoWise ISO compliance.

### UAE
- The `Global_Exchange_Rates_API_Key` environment variable is **required**.
- Rates are inverted (1/rate) because the source provides rates in "1 AED = X" format.

### Australia
- NAB Bank API access requires a 3-step OAuth token exchange. Tokens are obtained automatically.
- Raw API response is saved as `TextFiles/FX_NAB_raw_<datetime>.json` for reference.

### Vietnam
- Only **USD, EUR, GBP** rates are retrieved.
- Vietcombank API is used as the data source.
