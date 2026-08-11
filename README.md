# Download_all_Kursbuecher_DB

Die Anwendung ermöglicht die von der DB unter https://kursbuch.bahn.de/hafas/kbview.exe bereitgestellten Kursbücher automatisch herunterzuladen und für sich selbst zu archivieren. Es verfügt über eine Versionierung welche den doppelten Download von gleichen Dateien verhindert.

## Nutzung
1. Projekt herunterladen. 
2. Ins Verzeichnis "Download_all_Kursbuecher_DB" wechseln.
3. `dotnet build` ausführen
4. Die erstellte ausführbare Datei starten, schon geht der Download los.

Anschließend sind die heruntergeladenen Kursbücher im Unterordner `Download` innerhalb des Ordners mit der ausführbaren Datei zu finden. 

## Versionsverwaltung der Downloads
Die Anwendung besitzt eine eingebaute Versionsverwaltung. 

Das heißt, dass bei weiteren Läufen der Anwendung geprüft wird, ob bestimmte Kursbücher heruntergeladen wurden. 

Dies erfolgt mit der Datei `hashes.csv` im Downloadverzeichnis.

Beim erstmaligen Download eines Kursbuchs wird dieser Download innerhalb der gegebenen Datei festgehalten. So wird hierbei der Dateiname, der Hashwert, die ursprüngliche URL sowie das Downloaddatum gesichert

Die Datei sieht wie folgt aus: 
```text
Datei;SHA256;URL;DownloadDatum
KB100_H_Taeglich_nur_15052026_12122026_G30042026.pdf;A85EA351BB4C7A43E4E41E2A215855A7BE8957CFA96E06113E5E8039B8AE3D18;https://kursbuch.bahn.de/hafas/kbview.exe/dn/KB100_H_Taeglich_nur_15052026_12122026_G30042026.pdf?filename=KB100_H_Taeglich_nur_15052026_12122026_G30042026.pdf&orig=ut;2026-08-11T09:14:29.7874892Z
```
Beim nächsten Lauf wird dann anhand des Hashwertes abgeglichen, ob ein bestimmtes Kursbuch bereits heruntergeladen wurde oder ob eine Datei gleich heißt, aber einen anderen Inhalt aufweist. 

## Systemvoraussetzungen

Diese Anwendung wurde unter Windows geschrieben und getestet. Unterstützung von anderen Betriebssystemen wie Linux oder MacOS kann gegeben sein, muss aber nicht zwingend funktionieren. 

## Verwendete Ressourcen
### Hardware
- Arbeitsplatz mit Notebook
### Software
- Windows 11 Pro 24H2 - Betriebssystem
- Visual Studio Community 2026 - Entwicklungsumgebung (IDE)
- Dotnet 10.0.302
- Google Chrome - Browser
