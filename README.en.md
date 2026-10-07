# aSql

[Deutsch](README.md) | **English**

******aSql is a .NET cross-platform lightweight SQL helper tool with an Avalonia UI as frontend.******

<img width="1071" height="836" alt="image" src="https://github.com/user-attachments/assets/c1a97708-790e-43c0-9abe-cb6ac3fdb1fd" />


It was written to quickly generate SQL SELECT queries based on the schema of a relational database, mainly by single or double click, so you do not have to write much SQL manually.

## Features

- Generation of simple SQL for quick analysis of RDBMS
  - also useful for quality assurance professionals in software projects
- Independence from vendor-specific SQL tools for RDBMS
  - SQL Server, ORACLE, MySQL and MariaDB
- Avalonia UI as frontend
  - providing OS independence for Windows, Linux and macOS
- Database schema as a tree structure including columns, indexes and foreign keys
- Fast SQL generation and execution via mouse click on the DB schema
- Editing database fields directly from the result grid
- OS independence as an option for WPF developers to explore
- The code can be used to test DB operations for your own purposes
- SQL execution via a timer
  - useful when analysing apps or debugging

## Motivation

More than 20 years ago, I implemented this tool on .NET 2.0 to learn C# and to be independent from vendor-specific SQL editors when I only wanted to quickly inspect a database. Another motivation was to have a code base with which I could test database access and SQL for different RDBMS. At that time I mainly worked with SQL Server and ORACLE. There were also some exotic systems such as SYBASE, DB2 and MS ACCESS for single-user versions and tests, which are less relevant today. For the new tool, in addition to SQL Server and ORACLE, I implemented support for MySQL and MariaDB.

The old tool has accompanied me throughout my career and often helped me reach a goal faster. I have now modernized it, again to learn: current .NET 10, OS independence and Avalonia. The new tool does not yet have all the features of the old tool, but I will add them step by step. There is more to come.

I think everyone has noticed how quickly things move in our profession. The same applies to me and my SQL tool. At some point it no longer really fit the times, and I wanted to learn something new. .NET had long since become cross-platform and I discovered Avalonia for the UI (many thanks to the friendly colleague who recommended it to me 😊). With WPF, cross-platform support was hardly conceivable—at least not really. A web application was not an option for me because it would have meant too much overhead. It should remain a lightweight tool that runs locally on my computer.

I believe many WPF applications are still in use and that many developers have considered modernizing them, but have so far been reluctant to pursue platform independence if that meant developing a completely new web application—or if they did not need or want an internet presence for good reasons such as security, overhead, maintenance, operating costs and so on. Why develop a web application if you do not want to sell T-shirts all over the world? Java was not my world either, and there were too many unstable derivatives and many other reasons that cannot all be covered here. Platform independence for WPF developers has therefore been rather tricky. This is where Avalonia came in for me, and I found it very appealing and exciting. Avalonia has long outgrown its early stages for me and is a genuine alternative. Avalonia is also licensed under the MIT License and its source code can be used, protecting your product because you can intervene yourself if necessary. Of course, Avalonia also has pitfalls, but every framework comes with those.

Perhaps this little tool can help some of you evaluate whether porting a WPF application to Avalonia is an option, in addition to serving its actual purpose. If your applications are clearly structured and use the MVVM pattern, modernization should not be too difficult. Or simply use aSql to quickly analyse data in a database. Have fun 😊

## Supported Databases

Like many others, I used the well-known Chinook database as a basis for development and added several tables and foreign keys so that tables with larger amounts of data are included. I also ported the Chinook database to ORACLE, MySQL and MariaDB. I include database backups for SQL Server, ORACLE, MySQL and MariaDB in this repository.

- SQL Server
- ORACLE
- MySQL
- MariaDB
- More RDBMS?
  - Pull requests are very welcome if you would like to contribute something

## Supported Platforms

- Windows (64-bit)
- Linux (tested under WSL)
- macOS (should work—but beware Murphy's law)
  - I have not tested it because I currently do not have a Mac and alternatives were too time-consuming due to lack of time.
  - Pull requests are very welcome if you would like to contribute something

## Limitations

- aSql cannot replace the established, powerful vendor-specific SQL tools
- aSql currently focuses on simple SELECT statements
  - These are generated quickly
  - and can be further processed in vendor-specific SQL editors if necessary
- Sub-queries are not possible
  - but the sub-query itself can of course be generated and tested separately with aSql
- Scalar standard functions (SQL:2023 standard) are not supported
- CTEs (Common Table Expressions) and related concepts (WITH, etc.) are not supported
- However, all of these would be interesting projects for the future
  - Pull requests are very welcome

## Features (detailed)

## Connecting to different RDBMS

<img width="482" height="710" alt="179027121529228447063452147150" src="https://github.com/user-attachments/assets/acbd2237-dae3-470f-b1dd-3a33d5206c78" />

- Connections are saved automatically once they have been opened successfully.

### SQL generation via the DB schema

<img width="1196" height="680" alt="17902712455043578035427765380381" src="https://github.com/user-attachments/assets/f6fa19c2-bea6-4c4e-8ec3-f702fb3c3da4" />

- Double-clicking a table generates a SELECT statement for that table and executes it immediately
  - Any existing SQL is deleted first in this case
- Double-clicking a column generates a SELECT statement for that table containing only that column
  - Existing SQL is not deleted in this case
  - The SQL must then be executed manually again
- The generated command is displayed as a tree structure and as SQL text
- The output is displayed as a grid
- When the tree view has focus, press a letter on the keyboard to jump to the first table beginning with that letter

### Foreign keys

<img width="1416" height="428" alt="17902712767501597376862106218338" src="https://github.com/user-attachments/assets/0948c9e5-1c23-42cb-8443-4baa11f63e13" />

- Each table shows directly which tables it references (ForeignKey) and which other tables reference it (ReferencedBy)
- For ForeignKeys and ReferencedBy, the display shows “Table From ---> Table To”
  - The original key name from the database is shown as a tooltip when the mouse pointer rests over it
- Double-clicking the ForeignKeys folder adds all foreign keys as LEFT JOINs
  - The same applies to the ReferencedBy folder
  - This saves a lot of typing
- Double-clicking a specific foreign key adds only that key
- The SQL must be executed manually again afterwards

### Indexes

<img width="1031" height="521" alt="17902713068683520199580225185299" src="https://github.com/user-attachments/assets/0f6554ee-e4b4-43e9-ab03-1ffbb14b9a9c" />

- Double-clicking an index adds it as an ORDER BY clause
- The SQL must be executed manually again afterwards

### Editing tables

<img width="676" height="172" alt="17902713338342190692789303116164" src="https://github.com/user-attachments/assets/8158edde-7dea-4140-8e82-279a4619d19d" />

- Aliases for tables can be entered under the “Tables” tab
  - They are also generated automatically when necessary
- The SQL must be executed manually again afterwards

### Editing fields

<img width="737" height="396" alt="17902713591915491033142312759149" src="https://github.com/user-attachments/assets/a4c6627d-44f3-4b5f-a2b0-d5fc0139771f" />

- Fields can be selected under the “Fields” tab
- Fields can also be marked for grouping
- The SQL must be executed manually again afterwards

### Editing joins

<img width="527" height="193" alt="17902713856117180031998957994709" src="https://github.com/user-attachments/assets/e04cc067-0718-4e08-bb7f-0e71ffb2fe4c" />

- The type of joins can be changed under the “Joins” tab
  - Hold CTRL while clicking a Join Type arrow to change all joins
  - This is useful for checking whether a table has any references at all (LEFT JOIN), or for displaying only records that have a relation (INNER JOIN)
- The SQL must be executed manually again afterwards

### Conditions

<img width="737" height="294" alt="17902714131386616209953329703695" src="https://github.com/user-attachments/assets/06b7c448-5911-43bc-8855-72403acbc7a0" />

- Conditions can be entered under the “Where” and “Having” tabs
- The correct SQL formatting is generated according to the RDBMS and the data type of the respective field
- After changes, click the green check mark to update the SQL
- The SQL must be executed manually again afterwards

### Sorting

<img width="737" height="217" alt="17902714412065138157077757094652" src="https://github.com/user-attachments/assets/783cc547-b91e-4d66-ad38-885245738807" />

- Sort orders for fields can be selected under the “Sorts” tab
- By default, sorting is generated as ASC (ascending). It can be changed to DESC (descending) here
- The SQL must be executed manually again afterwards

### Logs and error messages

<img width="308" height="352" alt="17902714728595702691502949367917" src="https://github.com/user-attachments/assets/904779a8-ac18-4861-b2b0-09f065e1f62a" />

- Information about SQL execution and error messages is displayed in the lower-left corner of the main dialog
- Information about SQL execution time and the number of returned rows is also shown

### Output

<img width="1070" height="440" alt="image" src="https://github.com/user-attachments/assets/c9018ff9-a81d-4d89-b119-8f95a31914b7" />

- In the output area at the bottom right of the main dialog, you can:
  - automatically repeat SQL with a timer (one second)
    - useful when debugging an application and checking what is happening in the database
  - validate SQL
  - execute SQL manually again
  - cancel long-running SQL commands using the offered cancel command
  - adjust the maximum number of output rows at the bottom right when “SELECT TOP” is enabled in the settings
  - edit cells directly when the column header is displayed in green
    - columns become editable when a unique index for the table of the field is included in the result
    - if there is no unique index, the affected column header is displayed in red

### Settings

<img width="915" height="192" alt="17902715279545195547680479904015" src="https://github.com/user-attachments/assets/4d3d8242-c31c-4a9e-8891-24cdf27a309f" />

The main dialog's top toolbar contains the following buttons from left to right:

- New instance (opens a new main dialog with the current database connection)
- WITH (NOLOCK) is added to the SQL for SQL Server for every table (allows open transactions to be ignored)
- Field delimiters are added to the SQL, matching the respective RDBMS
- SQL is generated in upper case
- TOP (Count) is added to the SQL, matching the respective RDBMS
- A simple date format is used (without hours, minutes, etc.)
- The command timeout for executed SQL can be set at the far right

### Status bar

<img width="541" height="27" alt="17902715540146663662166728107414" src="https://github.com/user-attachments/assets/4c6be565-3817-4a58-8352-ac6d5405a221" />

- The status bar at the bottom shows which RDBMS is currently connected.

### Font size

- Font size in the status messages, SQL and output areas can be changed by holding CTRL and turning the mouse wheel
  - Font size can be reset with the button
  - <img width="95" height="61" alt="17902715829977305398138553842676" src="https://github.com/user-attachments/assets/9a7aa9e5-1b14-4ae6-9b63-c43f640463c4" />

### Storage location for RDBMS connections

- Windows
  - C:\\Users\\{your user name}\\AppData\\Roaming\\aSqlFiles\\connections.json
- Linux
  - {application directory}/aSqlFiles/connections.json
- macOS
  - unknown

## Publishing the application

### Releases

- Windows
  - Two releases are always offered on the right side of the current release page
    - one as a simple single application and one as a single application with .NET included
- Linux
  - must be created with a build tool such as Visual Studio or similar
  - Pull requests with corresponding build variants are welcome
- macOS
  - must be created with a build tool such as Visual Studio or similar
  - Pull requests with corresponding build variants are welcome

### Windows

```
# Self-contained, single-file, native libraries, without debug symbols
  dotnet publish aSql.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfContained=true /p:DebugType=None /p:DebugSymbols=false

# Framework-dependent, single-file, without debug symbols
  dotnet publish aSql.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true
  
```

### Linux

```
# Linux (WSL, Ubuntu)
# Self-contained, single-file, native libraries, without debug symbols
  dotnet publish aSql.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
# Install Linux on Windows (WSL)
  wsl --install
  # Copy the publish output to WSL (Ubuntu), into the home directory
    \\wsl.localhost\Ubuntu\home\{your_user_name}\{your_app}
  # Open the WSL terminal
    Start the “Ubuntu” app from the Windows Start menu
  # Install libfontconfig1
    sudo apt-get update && sudo apt-get install -y libfontconfig1
  # Install fonts-dejavu
    sudo apt-get install -y fontconfig fonts-dejavu
  # Set permissions and start
    cd ~/{myapp}
    chmod +x ./aSql
    ./aSql
  # Connect WSL to SQL Server on the local Windows computer
    # Determine the IP address used by WSL to access SQL Server
       ip route show | grep default | awk '{print $3}'
    # Check whether the IP address responds
       Example: nc -zv 172.X.X.X 1433
    # If the Windows firewall blocks the connection
       netsh advfirewall firewall add rule name="SQL Server WSL" dir=in action=allow protocol=TCP localport=1433 remoteip=localsubnet
```

### macOS

I have not tested this yet.

```
# macOS (Apple Silicon – M1/M2/M3/M4)
dotnet publish aSql.csproj -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
# macOS (older Intel processors)
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

## MIT LICENSE – German translation (unofficial)

Copyright (c) 2025 - 2026 Hans Simon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

