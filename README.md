# aSql

******aSql ist ein bescheidenes dotNet Multi-Plattform-SQL-Lightway-Tool, mit einer Avalonia UI als Frontend.******

<img width="1022" height="667" alt="17902711550915827384281017035089" src="https://github.com/user-attachments/assets/734e8622-47a9-4c1a-801f-8fa8c781bcac" />


Es wurde geschrieben, um, auf Basis des Daten-Schemas einer relationalen Datenbank, schnell SQL-Select-Abfragen, hauptsächlich per Klick oder/und Doppelklick zu generieren, ohne viel schreiben zu müssen...

## Funktionen

- Generierung von einfachen SQL zur schnellen Analyse von RDBMS
  - auch sehr nützlich für Fachleute der Qualitätssicherung in Software-Projekten
- Unabhängigkeit von Hersteller-spezifischen SQL-Tools für die RDBMS
  - SQL-Server, ORACLE, MySQL und MariaDB
- Avalonia UI als Frontend
  - Dadurch OS-Unabhängigkeit für Windows, Linux und mac OS
- DB-Schema als Baumstruktur inkl. Spalten, Indizes und Fremdschlüsseln
- Schnelle SQL-Generierung und Ausführung per Maus-Klick über das DB-Schema
- OS-Unabhängigkeit zum Schnuppern für WPF-Entwickler
- Der Code kann genutzt werden, um DB-Operationen für eigene Belange zu testen
- SQL-Ausführung über einen Timer
  - nützlich bei Analyse von Apps oder beim Debuggen

## Beweggründe

Ich habe schon einmal vor mehr als 20 Jahren dieses Tool unter .NET 2.0 implementiert. Damals um C# zu lernen und um frei zu sein von Hersteller-spezifischen SQL-Editoren, wenn es darum ging, nur mal schnell etwas in einer DB nachzusehen. Ein weiterer Antrieb war es, eine Code-Basis zu haben, mit der ich Datenbank-Zugriffe und SQL für verschiedene RDBMS testen konnte. Ich hatte damals dienstlich an einer Standard-Software gearbeitet, deren Kunden schnell die Anforderung hatten, das RDBMS frei wählen zu können. Damals habe ich hauptsächlich mit SQL-Server und ORACLE gearbeitet. Aber es gab auch einige Exoten (SYBASE, DB2, MS ACCESS für Einzelplatz-Versionen und Tests, etc.), die heute nicht mehr so relevant sind. Dafür habe ich im neuen Tool mal, neben SQL Server und ORACLE, MySQL und MariaDB dazu implementiert. 
Bis heute hat mich das gute alte Tool auf meinem Weg begleitet und oft dabei geholfen schnell(er) ein Ziel zu erreichen. Nun habe ich es modernisiert. Wieder um zu lernen: Aktuelles dotNET 10, OS-unabhängigkeit und Avalonia. Und natürlich nicht zuletzt, um nicht vom Wandel der Zeit überholt zu werden. 
Das neue Tool hat noch nicht alle Funktionen des alten Tools, aber das werde ich Stück für Stück nachholen. Es kommt also noch was...

Ich denke, jeder hat gemerkt, dass die Uhren in unserem Job sehr schnell laufen. So auch für mich und mein SQL-Tool. Es passte irgendwann einfach nicht mehr so richtig in die Zeit und zudem wollte ich mal wieder etwas Neues lernen. dotNET war schon lange OS-unabhängig geworden und ich habe Avalonia für das UI entdeckt (vielen Dank an den netten Kollegen, der mir das mal empfohlen hat 😊). Mit WPF war OS-Unabhängigkeit ja kaum denkbar – jedenfalls nicht wirklich. Und eine Web-Anwendung kam für mich nicht in Frage, weil das viel zu viel Overhead bedeutet hätte. Es sollte weiterhin ein „leichtes" Tool bleiben, welches lokal auf meinem Rechner läuft.

Ich glaube, dass da draußen bei euch noch sehr viele WPF-Anwendungen laufen und viele von euch schon mal darüber nachgedacht haben diese zu modernisieren, aber bisher davor zurückgeschreckt sind eine Plattform-Unabhängigkeit zu erreichen, wenn das bedeutete eine völlig neue Web-Anwendung entwickeln zu müssen oder man gar keine Internetpräsenz brauchte bzw. aus guten Gründen gar nicht wollte (Sicherheit, Overhead, Pflegebardarf, Betriebskosten, etc.). Warum soll ich eine Web-Anwendung entwickeln, wenn ich gar keine T-Shirts in der ganzen Welt verkaufen will? Na ja, und Java war einfach nicht meine Welt und es gab zu viele unbeständige Derivate und sehr viele andere gute Gründe, für die hier zu wenig Zeit ist (außerdem kann man schlecht auf zu vielen Hochzeiten gleichzeitig tanzen). Die OS-Unabhängigkeit für WPF-Entwickler war bisher also echt etwas "tricky". Hier kam dann für mich Avalonia ins Spiel und ich fand es sehr ansprechend und spannend, wie das alles so läuft. Avalonia ist für mich längst raus aus den Kinderschuhen und eine echte Alternative. Und auch Avalonia läuft unter MIT-Lizenz und der Quell-Code ist verwendbar, was euer Produkt schützt, weil ihr zur Not auch selbst eingreifen könntet. Natürlich gibt es auch bei Avalonia Fallstricke, aber da muss man bei jedem Framework durch.

Na ja, vielleicht hilft dem einen oder anderen ja mein kleines Tool hier – neben seiner eigentlichen Funktion – auch mal zu evaluieren, ob eine Portierung von WPF zu Avalonia eine Alternative für ihn sein kann. Wer seine Anwendungen schon klar strukturiert hat und das MVVM-Pattern nutzt, dem sollte es nicht allzu schwer fallen zu modernisieren. Oder er nutzt aSql einfach, um schnell mal Daten in einer DB zu analysieren. Viel Spaß dabei 😊

## Unterstütze Datenbanken

Ich habe mir (wie viele andere auch) die bekannte Chinook-Db als Basis für die Entwicklung genommen und noch einige Tabellen und Fremdschlüssel hinzugefügt, damit Tabellen mit etwas größeren Datenmengen dabei sind. Und ich habe die Chinook DB zudem nach ORACLE, MySQL und MariaDB portiert. Ich füge Datensicherungen für SQL-Server, ORACLE, MySQL und MariaDB diesem Repository bei.

- SQL-Server
- ORACLE
- MySQL
- MariaDB
- Weitere RDBMS?
  - Hier würde ich mich über Pull-Requests sehr freuen, falls da jemand etwas zu beitragen möchte

## Unterstütze Plattformen

- Windows (64 Bit)
- Linux (getestet unter WSL)
- Mac OS (sollte funktionieren - aber denkt an den alten "Murphy")
  - Habe ich noch nicht getestet, weil ich aktuell keinen Mac-Rechner habe und, weil mir Alternativen aus Zeitmangel zu aufwändig waren.
  - Hier würde ich mich über Pull-Requests sehr freuen, falls da jemand etwas zu beitragen möchte

## Beschränkungen

- aSql kann natürlich nicht die einschlägigen, leistungsfähigen, Hersteller-spezifischen SQL-Tools ersetzen!
- aSql beschränkt sich aktuell auf einfache SELECT-Statements
  - Diese sind schnell generiert 
  - und können, wenn es nicht reicht, in Hersteller-spezifischen SQL-Editoren weiter verarbeitet werden
- Sub-Querys sind nicht möglich
  - aber das SubQuery selbst kann natürlich separat mit aSql erzeugt und getestet werden
- skalare Standardfunktionen (SQL:2023-Standard) sind nicht möglich
- CTE (Common Table Expressions) oder/und damit verbundene Konzepte ("WITH", etc.) sind nicht möglich
- aber das wären alles spannende Projekte für die Zukunft...
  - Hier würde ich mich über Pull-Requests sehr freuen, falls da jemand etwas zu beitragen möchte

## Funktionen

## Connect zu verschiedenen RDBMS

<img width="482" height="710" alt="179027121529228447063452147150" src="https://github.com/user-attachments/assets/acbd2237-dae3-470f-b1dd-3a33d5206c78" />


- Verbindungen werden automatisch gespeichert, wenn sie einmal erfolgreich geöffnet werden konnten.

### SQL-Generierung über DB-Schema

<img width="1196" height="680" alt="17902712455043578035427765380381" src="https://github.com/user-attachments/assets/f6fa19c2-bea6-4c4e-8ec3-f702fb3c3da4" />


- Doppel-Klick auf eine Tabelle erzeugt ein Select für diese Tabelle und führt das Select direkt aus
  - Ein bereits vorhandenes SQL wird in diesem Fall vorher gelöscht
- Doppel-Klick auf eine Spalte erzeugt ein Select für diese Tabelle mit nur dieser Spalte
  - Dies löscht ein zuvor vorhandenes SQL **nicht**
  - Das SQL muss dann manuell neu ausgeführt werden
- Erzeugtes Command wird als Baumstruktur und als SQL-Text angezeigt
- Der Output wird als Grid angezeigt

### Foreign Keys

<img width="1416" height="428" alt="17902712767501597376862106218338" src="https://github.com/user-attachments/assets/0948c9e5-1c23-42cb-8443-4baa11f63e13" />


- Man sieht bei jeder Tabelle direkt, auf welche Tabellen verwiesen wird (ForeignKey) und welche anderen Tabellen auf diese Tabelle verweisen (ReferencedBy) 
- Bei den ForeignKeys und ReferencedBy werden jeweils "Tabelle Von ---> Tabelle Nach" angezeigt
  - Der originale Key-Name aus der DB wird als Tooltip angezeigt, wenn man mit der Maus darüber stehen bleibt
- Doppel-Klick auf den ForeignKeys-Ordner fügt alle ForeignKeys als Left-Join hinzu
  - Dasselbe passiert beim ReferencedBy Ordner
  - Da muss man weniger schreiben…
- Doppel-Klick auf einen bestimmten ForeignKey fügt nur diesen dazu
- Das SQL muss hierbei manuell neu ausgeführt werden

### Indizes

<img width="1031" height="521" alt="17902713068683520199580225185299" src="https://github.com/user-attachments/assets/0f6554ee-e4b4-43e9-ab03-1ffbb14b9a9c" />


- Doppel-Klick auf einen Index fügt diesen als „ORDER BY" hinzu
- Das SQL muss hierbei manuell neu ausgeführt werden

### Tabellen bearbeiten

<img width="676" height="172" alt="17902713338342190692789303116164" src="https://github.com/user-attachments/assets/8158edde-7dea-4140-8e82-279a4619d19d" />


- Unter dem Reiter „Tabellen" können Alias für die Tabellen eingegeben werden
  - Diese werden aber auch bereits automatisch generiert, wenn es nötig ist
- Das SQL muss hierbei manuell neu ausgeführt werden

### Felder bearbeiten

<img width="737" height="396" alt="17902713591915491033142312759149" src="https://github.com/user-attachments/assets/a4c6627d-44f3-4b5f-a2b0-d5fc0139771f" />


- Unter dem Reiter „Fields" können Felder ausgewählt werden
- Die Felder können zudem für eine Gruppierung markiert werden
- Das SQL muss hierbei manuell neu ausgeführt werden

### Joins bearbeiten

<img width="527" height="193" alt="17902713856117180031998957994709" src="https://github.com/user-attachments/assets/e04cc067-0718-4e08-bb7f-0e71ffb2fe4c" />


- Unter dem Reiter „Joins" kann die Art der Joins geändert werden
  - Mit der Taste „STRG" bei Klick auf einen Join Type Pfeil werden alle Joins geändert
  - Das ist praktisch, wenn man mal sehen möchte, ob eine Tabelle überhaupt Verweise hat (Left Join) oder eben nur die Datensätze anzeigen, die eine Verbindung haben (Inner Join)
- Das SQL muss hierbei manuell neu ausgeführt werden

### Bedingungen

<img width="737" height="294" alt="17902714131386616209953329703695" src="https://github.com/user-attachments/assets/06b7c448-5911-43bc-8855-72403acbc7a0" />


- Unter den Reitern „Where" und „Having" können Bedingungen eingegeben werden
- Hierbei werden je RDBMS und Datentyp des jeweiligen Feldes die korrekten Formatierungen im SQL gebildet
- Nach Änderungen muss der grüne Haken gedrückt werden, um das SQL zu akstualisierungen
- Das SQL muss hierbei manuell neu ausgeführt werden

### Sortierungen

<img width="737" height="217" alt="17902714412065138157077757094652" src="https://github.com/user-attachments/assets/783cc547-b91e-4d66-ad38-885245738807" />


- Unter dem Reiter „Sorts" können Sortierungen für Felder gewählt werden
- Standardmäßig werden die Sortierungen als "ASC" (aufsteigend) generiert. Das kann hier auf "DESC" (absteigend) umgestellt werden
- Das SQL muss hierbei manuell neu ausgeführt werden

### Protokolle und Fehlermeldungen

<img width="308" height="352" alt="17902714728595702691502949367917" src="https://github.com/user-attachments/assets/904779a8-ac18-4861-b2b0-09f065e1f62a" />


- Unten links im Haupt-Dialog werden Infos zur Ausführung des SQL oder auch Fehlermeldungen angezeigt
- Zudem gibt es Infos zur Laufzeit des SQL und zur Anzahl der zurückgegebenen Zeilen

### Ausgabe

<img width="605" height="350" alt="17902714994067431319113914676913" src="https://github.com/user-attachments/assets/74cc263e-7d8a-4da2-8117-b99d89be99fa" />


- Im Ausgabe-Bereich (unten rechts im Haupt-Dialog) können

    - SQL über einen Timer (1 Sekunde) automatisch wiederholt werden 
      - Ist nützlich, wenn man gerade seine App debugt und sehen will, was auf der DB passiert
    - SQL geprüft werden
    - SQL einfach manuell nochmals ausgeführt werden
    - Bei länger laufenden SQL-Befehlen wird ein Cancel-Command angeboten, um die Ausführung abzubrechen
    - Unten rechts kann zudem noch die maximale Anzahl der Ausgabezeilen angepasst werden, wenn unter den Einstellungen "SELECT TOP" gewählt wurde

### Einstellungen

<img width="915" height="192" alt="17902715279545195547680479904015" src="https://github.com/user-attachments/assets/4d3d8242-c31c-4a9e-8891-24cdf27a309f" />


In der Oberen Symbolleiste des Haupt-Dialogs gibt es (von links ausgehend) Schaltflächen für
  - Neue Instanz (Ein neuer Haupt-Dialog wird mit der aktuellen DB-Verbindung geöffnet)
  - WITH (NOLOCK) wird beim SQL-Server im SQL für jede Tabelle dazugefügt (ermöglicht es offene Transaktionen zu ignorieren)
  - Field-Delimiter werden im SQL hinzugefügt (passend zum jeweiligen RDBMS)
  - SQL wird in Großbuchstaben erzeugt
  - TOP (Count) wird im SQL hinzugefügt (passend zum jeweiligen RDBMS)
  - Einfaches Datumsformat wird verwendet (ohne Stunden, Minuten, etc.)
  - Ganz rechts außen kann das Command-TimeOut für die ausgeführten SQL eingestellt werden

### Statusleiste

<img width="541" height="27" alt="17902715540146663662166728107414" src="https://github.com/user-attachments/assets/4c6be565-3817-4a58-8352-ac6d5405a221" />


- In der Statuszeile (ganz unten) wird angezeigt, mit welchem RDBMS man aktuell verbunden ist

### Schrift-Größe

- Die Schriftgröße kann in den Bereichen Statusmeldungen, SQL und Ausgabebereich verändert werden, indem die Taste „STRG" gedrückt – und am Mausrad gedreht wird
  - Zurücksetzen kann man die Schriftgröße dann mit der Schaltfläche
  - <img width="95" height="61" alt="17902715829977305398138553842676" src="https://github.com/user-attachments/assets/9a7aa9e5-1b14-4ae6-9b63-c43f640463c4" />


### Speicherort für die RDBMS-Verbindungen

- Windows
  - C:\\Users\\{dein Benutzername}\\AppData\\Roaming\\aSqlFiles\\connections.json
- Linux
  - {Anwendungsverzeichnis}/aSqlFiles/connections.json
- Mac OS
  - ist unklar

## Veröffentlichen der Anwendung

### Windows

```
# Inklusive Framework, Single-File, Native Libraries, ohne Debug-Symbole
  dotnet publish aSql.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfContained=true /p:DebugType=None /p:DebugSymbols=false

# Ohne Framework, Single-File, ohne Debug-Symbole
  dotnet publish aSql.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true
  
```

### Linux

```
# Linux (WSL, Ubuntu)
# Inklusive Framework, Single-File, Native Libraries, ohne Debug-Symbole
  dotnet publish aSql.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
# Linux unter Windows (WSL) installieren
  wsl --install
  # Publish-Output kopieren nach WSL (Ubuntu) in das Home-Verzeichnis
    \\wsl.localhost\Ubuntu\home\{dein_benutzername}\{deine_app}
  # WSL-Terminal öffnen: 
    Starte App "Ubuntu" über Ihr Windows-Startmenü
  # libfontconfig1 installieren
    sudo apt-get update && sudo apt-get install -y libfontconfig1
  # fonts-dejavu installieren
    sudo apt-get install -y fontconfig fonts-dejavu
  # Rechte setzen und Starten
    cd ~/{myapp}
    chmod +x ./aSql
    ./aSql
  # WSL und SQL Server auf dem lokalen Windows-Rechner verbinden
    # IP-Adresse feststellen, die WSL verwendet, um auf den SQL Server zuzugreifen
       ip route show | grep default | awk '{print $3}'
    # Versuchen, ob die IP-Adresse antwortet
       Beispiel: nc -zv 172.X.X.X 1433
    # Wenn es mit der FireWall auf dem lokalen Windows-Rechner nicht funktioniert
       netsh advfirewall firewall add rule name="SQL Server WSL" dir=in action=allow protocol=TCP localport=1433 remoteip=localsubnet
```

### Mac OS

Das habe ich noch nicht getestet!

```
# macOS (Apple Silicon – M1/M2/M3/M4)
dotnet publish aSql.csproj -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
#  macOS (Ältere Intel-Prozessoren)
dotnet publish aSql.csproj -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
```

# License

****aSql wird unter MIT Lizenz veröffentlicht****

## MIT LICENSE Englisch (original)

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

## MIT LIZENZ Deutsch (inoffizielle Übersetzung)

Urheberrecht (c) 2025 - 2026 Hans Simon

Hiermit wird jeder Person, die eine Kopie dieser Software und der
zugehörigen Dokumentationsdateien (die „Software“) erhält, die Erlaubnis
kostenlos erteilt, ohne Einschränkung mit der Software zu handeln,
einschließlich, aber nicht beschränkt auf die Rechte, Kopien der Software zu
verwenden, zu kopieren, zu modifizieren, zusammenzufügen, zu veröffentlichen,
zu verbreiten, unterzulizenzieren und/oder zu verkaufen, und Personen, denen
die Software zur Verfügung gestellt wird, dies unter den folgenden
Bedingungen zu gestatten:

Der obige Urheberrechtshinweis und dieser Genehmigungshinweis müssen in allen
Kopien oder wesentlichen Teilen der Software enthalten sein.

DIE SOFTWARE WIRD OHNE MÄNGELGEWÄHR UND OHNE JEGLICHE AUSDRÜCKLICHE ODER
KONKRETISIERTE GEWÄHRLEISTUNG ZUR VERFÜGUNG GESTELLT, EINSCHLIESSLICH, ABER
NICHT BESCHRÄNKT AUF DIE GEWÄHRLEISTUNG DER MARKTGÄNGIGKEIT, DER EIGNUNG FÜR
EINEN BESTIMMTEN ZWECK UND DER NICHTVERLETZUNG VON RECHTEN DRITTER. IN KEINEM
FALL SIND DIE AUTOREN ODER URHEBERRECHTSINHABER FÜR ANSPRÜCHE, SCHÄDEN ODER
ANDERWEITIGE HAFTUNG EVTL. VERANTWORTLICH, OB IN EINEM VERTRAGSVERFAHREN,
EINER UNERLAUBTEN HANDLUNG ODER ANDERWEITIG, DIE SICH AUS, IN VERBINDUNG MIT
DER SOFTWARE ODER DER NUTZUNG ODER ANDEREN GESCHÄFTEN MIT DER SOFTWARE ERGEBEN.


  






























