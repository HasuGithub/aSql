# Publish Commands

# Windows
    # Inklusive Framework, Single-File, Native Libraries, ohne Debug-Symbole
      dotnet publish aSql.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfContained=true /p:DebugType=None /p:DebugSymbols=false

    # Ohne Framework, Single-File, ohne Debug-Symbole
      dotnet publish aSql.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true

# Linux (WSL, Ubuntu)
# Inklusive Framework, Single-File, Native Libraries, ohne Debug-Symbole
dotnet publish aSql.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true

    # Linux unter Windows (WSL) installieren 
    wsl --install

    # Publish-Output kopieren nach WSL (Ubuntu) in das Home-Verzeichnis
    \\wsl.localhost\Ubuntu\home\dein_benutzername\deine_app

    # WSL-Terminal öffnen: Starte App "Ubuntu" über Ihr Windows-Startmenü

    # libfontconfig1 installieren
    sudo apt-get update && sudo apt-get install -y libfontconfig1
    
    # fonts-dejavu installieren
    sudo apt-get install -y fontconfig fonts-dejavu

    # Rechte setzen und Starten
    cd ~/myapp
    chmod +x ./aSql
    ./aSql

    # WSL und SQL Server auf dem lokalen Windows-Rechner verbinden
       # IP-Adresse festestellen, die WSL verwendet, um auf den SQL Server zuzugreifen
       ip route show | grep default | awk '{print $3}'

       # Versuchen, ob die IP-Adresse antwortet
         Beispiel: nc -zv 172.X.X.X 1433

        # Wenn es mit der FireWall auf dem lokalen Windows-Rechner nicht funktioniert
          netsh advfirewall firewall add rule name="SQL Server WSL" dir=in action=allow protocol=TCP localport=1433 remoteip=localsubnet

# macOS (Apple Silicon – M1/M2/M3/M4)
dotnet publish aSql.csproj -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true

#  macOS (Ältere Intel-Prozessoren)
dotnet publish aSql.csproj -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true

