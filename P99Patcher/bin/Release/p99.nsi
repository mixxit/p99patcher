; example1.nsi
;
; This script is perhaps one of the simplest NSIs you can make. All of the
; optional settings are left to their default settings. The installer simply 
; prompts the user asking them where to install, and drops a copy of example1.nsi
; there. 

;--------------------------------

; The name of the installer
Name "Project 99"

; The file to write
OutFile "Setup.exe"

; The default installation directory
InstallDir $PROGRAMFILES\Project99

;--------------------------------

; Pages

Page directory
Page instfiles

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR

  ; Start Menu

  CreateShortCut "$SMPROGRAMS\Project 99\Project 99.lnk" \
  	"$INSTDIR\P99Patcher.exe"  

  ; Desktop
  CreateShortCut "$DESKTOP\Project 99.lnk" "$INSTDIR\P99Patcher.exe"

  ; Vista fix
  WriteRegStr HKLM "Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" \
		 "$INSTDIR\P99Patcher.exe" "RUNASADMIN"

  ; Put file there
  File eqhost.txt
  File P99Patcher.exe
  File ICSharpCode.SharpZipLib.dll
  
SectionEnd ; end the section
