# aSql

[Deutsch](README.md) | **English**

******aSql is a .NET cross-platform lightweight SQL helper tool with an Avalonia UI as frontend.******

<img width="1071" height="836" alt="image" src="https://github.com/user-attachments/assets/c1a97708-790e-43c0-9abe-cb6ac3fdb1fd" />



It was written to quickly generate SQL SELECT queries based on the schema of a relational database — mainly by single or double click — so you don't have to write a lot of SQL manually.

## Features

- Generate simple SQL for quick analysis of RDBMS
	- also useful for QA professionals in software projects
- Independence from vendor-specific SQL tools for RDBMS
	- SQL Server, ORACLE, MySQL and MariaDB
- Avalonia UI as frontend
	- Enables cross-platform operation on Windows, Linux and macOS
- Database schema as a tree structure including columns, indexes and foreign keys
- Fast SQL generation and execution via mouse click on the DB schema
- Edit fields in the database directly from the result grid
- Low-effort cross-platform option for WPF developers
- The code can be used to test DB operations for your own purposes
- Timer-driven SQL execution
	- useful when analysing apps or debugging

## Motivation

I implemented a similar tool more than 20 years ago on .NET 2.0 to learn C# and to be independent from vendor-specific SQL editors when I just needed to quickly inspect a database. Another motivation was to have a code base to test database access and SQL for different RDBMS. At that time I mainly worked with SQL Server and ORACLE; there were also some exotic systems (SYBASE, DB2, MS ACCESS for single-user setups and tests) that are less relevant today. For the new tool I implemented support for SQL Server, ORACLE, MySQL and MariaDB.

The old tool helped me many times to reach goals faster. I modernized it to learn current .NET 10, achieve OS-independence and use Avalonia. The new tool does not yet include all features of the old one, but I will add them step by step.

I believe many still run WPF applications and have considered modernizing them. Full web migration often brings too much overhead or is not desired for security/maintenance reasons. Avalonia offers a pragmatic alternative for cross-platform desktop UI while keeping MVVM patterns. aSql can help evaluate whether porting from WPF to Avalonia is an option, or simply serve as a lightweight local tool to inspect databases quickly.

## Supported Databases

I used the well-known Chinook sample database as a basis and added several tables and foreign keys so larger table sizes are available. I also ported Chinook to ORACLE, MySQL and MariaDB. Backups for SQL Server, ORACLE, MySQL and MariaDB are included in this repository.

- SQL Server
- ORACLE
- MySQL
- MariaDB
- More RDBMS?
	- Pull requests are welcome if you want to add support for additional systems

## Supported Platforms

- Windows (64-bit)
- Linux (tested under WSL)
- macOS (should work — but beware Murphy's law)
	- I have not personally tested macOS due to lack of a Mac machine. Pull requests are welcome.

## Limitations

- aSql does not replace powerful vendor-specific SQL tools
- aSql currently focuses on simple SELECT statements
	- These are generated quickly and can be processed further in vendor-specific SQL editors if needed
- Sub-queries are not supported directly
	- You can generate a sub-query separately with aSql and test it
- SQL:2023 scalar standard functions are not supported
- CTEs (Common Table Expressions) or related concepts (WITH, etc.) are not supported
- These are interesting future enhancements; contributions welcome

## Features (detailed)

## Connect to different RDBMS

<img width="482" height="710" alt="179027121529228447063452147150" src="https://github.com/user-attachments/assets/acbd2237-dae3-470f-b1dd-3a33d5206c78" />

- Connections are saved automatically once they have been opened successfully.

### SQL generation via DB schema

<img width="1196" height="680" alt="schema" src="https://github.com/user-attachments/assets/0-placeholder-schema-image" />

- Browse the DB schema tree and generate SELECT statements quickly by clicking tables, columns and relations. Generated SQL can be edited and executed from the UI.

### Result grid and editing

- Edit fields directly in the result grid and persist changes back to the database (where supported)

### Timer and execution

- SQL execution can be scheduled periodically with a timer — useful for monitoring or debugging

### Status bar

<img width="541" height="27" alt="status" src="https://github.com/user-attachments/assets/4c6be565-3817-4a58-8352-ac6d5405a221" />

- The status bar (at the bottom) shows which RDBMS you are connected to

### Font size

- Font size can be changed for status messages, SQL and result area by holding CTRL and using the mouse wheel
	- Reset the font size with the reset button
	- <img width="95" height="61" alt="reset" src="https://github.com/user-attachments/assets/9a7aa9e5-1b14-4ae6-9b63-c43f640463c4" />

### Storage location for RDBMS connections

- Windows
	- C:\\Users\\{your username}\\AppData\\Roaming\\aSqlFiles\\connections.json
- Linux
	- {application directory}/aSqlFiles/connections.json
- macOS
	- unclear

## Publishing the application

### Releases

- Windows
	- Two release variants are provided in the current release: one single-file app and one single-file app with embedded .NET
- Linux
	- Must be built with a build tool (e.g. Visual Studio or similar)
	- Pull requests for build variations are welcome
- macOS
	- Must be built with a build tool (e.g. Visual Studio or similar)
	- Pull requests for macOS packaging are welcome

### Windows

```powershell
# Self-contained, single-file, native libraries, without debug symbols
dotnet publish aSql.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfContained=true /p:DebugType=None /p:DebugSymbols=false

# Framework-dependent, single-file, without debug symbols
dotnet publish aSql.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true
```

### Linux

```bash
# Linux (WSL, Ubuntu)
# Self-contained, single-file, without debug symbols
dotnet publish aSql.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
# To run under WSL install and copy the publish output to WSL, then:
# Install WSL (if needed): wsl --install
# Copy publish output to WSL (example): \\wsl.localhost\Ubuntu\home\{your_user}\{your_app}
# Open WSL terminal and install prerequisites:
sudo apt-get update && sudo apt-get install -y libfontconfig1
sudo apt-get install -y fontconfig fonts-dejavu
# Set execute permissions and start
cd ~/{myapp}
chmod +x ./aSql
./aSql
# If connecting to SQL Server on the local Windows machine you may need to open the firewall port:
# Determine WSL route IP: ip route show | grep default | awk '{print $3}'
# Test connectivity: nc -zv 172.X.X.X 1433
# Add firewall rule on Windows if required:
# netsh advfirewall firewall add rule name="SQL Server WSL" dir=in action=allow protocol=TCP localport=1433 remoteip=localsubnet
```

### macOS

This has not been personally tested by me.

```bash
# macOS (Apple Silicon – M1/M2...)
dotnet publish aSql.csproj -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
# macOS (Intel)
dotnet publish aSql.csproj -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
```

# License

****aSql is released under the MIT License****

## MIT LICENSE (original)

Copyright (c) 2025 - 2026 Hans Simon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
