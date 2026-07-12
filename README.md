# Home Service Automations

A .NET automation suite that eliminates repetitive manual work through two background processes: automated email-to-report processing and daily real estate market monitoring.

## What it does

### 📧 Email Report Automation
Monitors a mailbox for incoming emails with Excel attachments and turns them into finished reports — with zero human involvement:

1. Reads incoming emails from a configured inbox
2. Detects and downloads Excel attachments
3. Processes and transforms the data
4. Generates a new reporting spreadsheet
5. Sends the finished report to a designated email address

**Replaces:** the daily routine of checking email, downloading files, manually building a report, and sending it back.

### 🏠 Real Estate Market Monitor
A scheduled daily process that tracks property market movement over time:

1. Scrapes a real estate listings website on a daily schedule
2. Extracts property prices and details
3. Stores results in a SQL database
4. Builds a day-by-day history of market price movement

**Replaces:** manually checking listings and keeping spreadsheets of price changes.

## Architecture

- `HomeApi` — ASP.NET Core Web API
- `HomeService` — core services and background processing
- `Operations` — email processing, Excel handling, and scraping operations
- `DataLayer` — database access (Entity Framework Core)
- `Extensions` — shared helpers and configuration

## Tech Stack

C# / .NET · ASP.NET Core · Entity Framework Core · SQL · Gmail API · Excel processing · Web scraping · Scheduled background jobs

## Configuration

Credentials and tokens are not stored in this repository. Configure via `appsettings.Development.json` (gitignored) or environment variables.
