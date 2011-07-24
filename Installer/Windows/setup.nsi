; Script generated by the HM NIS Edit Script Wizard.

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "TorDNSd"
!define PRODUCT_VERSION "1.1"
!define PRODUCT_PUBLISHER "LETO Revolution"
!define PRODUCT_WEB_SITE "https://github.com/LETO-R/TorDNSd/"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\tordnsd-shell.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "..\ProxyPeople.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!define MUI_LICENSEPAGE_CHECKBOX
!insertmacro MUI_PAGE_LICENSE "..\..\LICENSE"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "tordnsd-v1.1-win-setup.exe"
InstallDir "$PROGRAMFILES\LETO-R\TorDNSd"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite try
  File "..\..\Build\1.1\ARSoft.Tools.Net.dll"
  File "..\..\Build\1.1\InteractiveShell.dll"
  File "..\..\Build\1.1\tordnsd-shell.exe"
  CreateDirectory "$SMPROGRAMS\TorDNSd"
  CreateShortCut "$SMPROGRAMS\TorDNSd\TorDNSd (Shell).lnk" "$INSTDIR\tordnsd-shell.exe"
  CreateShortCut "$DESKTOP\TorDNSd (Shell).lnk" "$INSTDIR\tordnsd-shell.exe"
  File "..\..\Build\1.1\tordnsd.conf"
  CreateShortCut "$SMPROGRAMS\TorDNSd\TorDNSd (Configuration).lnk" "$INSTDIR\tordnsd.conf"
  File "..\..\Build\1.1\tordnsd.exe"
  CreateShortCut "$SMPROGRAMS\TorDNSd\TorDNSd.lnk" "$INSTDIR\tordnsd.exe"
  SetOverwrite ifnewer
  File "..\..\Build\1.1\LICENSE.TXT"
  File "..\..\Build\1.1\ARSoft.Tools.NET-LICENSE"
SectionEnd

Section -AdditionalIcons
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\TorDNSd\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\TorDNSd\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\tordnsd-shell.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\tordnsd-shell.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd


Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\${PRODUCT_NAME}.url"
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\ARSoft.Tools.NET-LICENSE"
  Delete "$INSTDIR\LICENSE.TXT"
  Delete "$INSTDIR\tordnsd.exe"
  Delete "$INSTDIR\tordnsd.conf"
  Delete "$INSTDIR\tordnsd-shell.exe"
  Delete "$INSTDIR\InteractiveShell.dll"
  Delete "$INSTDIR\ARSoft.Tools.Net.dll"

  Delete "$SMPROGRAMS\TorDNSd\Uninstall.lnk"
  Delete "$SMPROGRAMS\TorDNSd\Website.lnk"
  Delete "$SMPROGRAMS\TorDNSd\TorDNSd.lnk"
  Delete "$SMPROGRAMS\TorDNSd\TorDNSd (Configuration).lnk"
  Delete "$DESKTOP\TorDNSd (Shell).lnk"
  Delete "$SMPROGRAMS\TorDNSd\TorDNSd (Shell).lnk"

  RMDir "$SMPROGRAMS\TorDNSd"
  RMDir "$INSTDIR"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd