# Download_all_Kursbuecher_DB

Die Anwendung ermöglicht die von der DB unter [kursbuch.bahn.de](https://kursbuch.bahn.de/hafas/kbview.exe) bereitgestellten Kursbücher automatisch herunterzuladen und für sich selbst zu archivieren.
Es verfügt über eine einfache Versionierung welche den doppelten Download von gleichen Dateien verhindert, bei Updates neuere Versionen herunterlädt und historische Versionen aufbewahrt.

This project is not affiliated with Deutsche Bahn or any of its subsidiaries.

## Nutzung
1. Projekt herunterladen. 
2. Ins Verzeichnis "Download_all_Kursbuecher_DB" wechseln.
3. `dotnet build` ausführen
4. Die erstellte ausführbare Datei starten, schon geht der Download los.

Anschließend sind die heruntergeladenen Kursbücher im Unterordner `Download` innerhalb des Ordners mit der ausführbaren Datei zu finden. 

## Versionsverwaltung der Downloads
Die Anwendung besitzt einfache Versionsverwaltung. 

Das heißt, dass bei weiteren Läufen der Anwendung geprüft wird, ob bestimmte Kursbücher bereits heruntergeladen wurden. 

Dies erfolgt mit der Datei `hashes.csv` die nach dem ersten Lauf im Downloadverzeichnis erstellt wird.

So werden heruntergeladene und gespeicherte Kursbücher in der Datei gespeichert. Dabei wird der Dateiname, der Hashwert, die ursprüngliche URL sowie das Downloaddatum gesichert.

Die Datei sieht wie folgt aus: 
```text
Datei;SHA256;URL;DownloadDatum
KB100_H_Taeglich_nur_15052026_12122026_G30042026.pdf;A85EA351BB4C7A43E4E41E2A215855A7BE8957CFA96E06113E5E8039B8AE3D18;https://kursbuch.bahn.de/hafas/kbview.exe/dn/KB100_H_Taeglich_nur_15052026_12122026_G30042026.pdf?filename=KB100_H_Taeglich_nur_15052026_12122026_G30042026.pdf&orig=ut;2026-08-11T09:14:29.7874892Z
```

Beim nächsten Lauf werden dann anhand des Hashwertes mehrere Dinge überprüft.
- Es wird überprüft, ob ein bestimmtes Kursbuch bereits heruntergeladen wurde. Ein gleiches Kursbuch wird selbst dann erkannt, wenn der Dateiname sich geändert hatte.
- Es wird überprüft, ob eine Datei gleich heißt, aber einen anderen Inhalt aufweist. In diesem Fall wird der Dateiname um die ersten 16 Zeichen des Hashwertes erweitert, um die Datei eindeutig zu benennen.

Damit kann sichergestellt werden, dass keine Dateien überschrieben werden und keine doppelten Downloads erfolgen.

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
