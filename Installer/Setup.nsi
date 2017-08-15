
#=================================================================
# ������ ������������ .NET �������� �������� ���������
#=================================================================
# �������� ������, �������� � ������:
#------------------------------------
# Setup.nsi - ���� ������� (���� ����)
# FileAssociations.nsh - ������������ ���� (������� ��� �� ����)
# Header.bmp - ������� ����� ������� �����
# HeaderEN.bmp - ���������� ����� ������� �����
# NSARCReader.Default.NSConfig - ������������ NSARCReader
# Wizard.bmp - ������� ������� ������� �����
# WizardEN.bmp - ���������� ������� ������� �����
# Splash.bmp - �����
#=================================================================
# ��������! ������ �� ����� ��������������� �� ���������
# � ���������� "<-- ��������!!!" ������ ���������� ��������!
#=================================================================
# �������������� ������������ ���������� ��� ������ ���������
# ������� �������� � ��� (����������� � ��������� �������� �����).
# � ���� ���������� �� ���� �� ������ :)  
#=================================================================

# ������������ �����
!include MUI2.nsh
!include Sections.nsh
!include FileAssociations.nsh
!include WinVer.nsh
!include x64.nsh
!include UAC.nsh

# �������� ������������ (��� ����� ��������� �� ���������)
!define PRODUCT_NAME "DeviceAutoTest.NET" # <-- ��������!!! (������� �����������!)
# ������
!define PRODUCT_VERSION "1.6.0.0"
# ����� ������� ������ ���������
!define NEUROSOFT_CONTRIB_FOLDER "D:\NeuroSoft\Contrib\"
# ����� ������� ������ ��������� x64
!define NEUROSOFT_CONTRIB64_FOLDER "D:\NeuroSoft\Contrib\x64\"
# ������� ��������� ������
!define NEUROSOFT_ASSEMBLIES_FOLDER "D:\NeuroSoft\Assemblies\"
# ������� � ��������������� ������� ����������
!define INSTALL_FILES_FOLDER "D:\Neurosoft\Programs\DeviceAutoTest\bin\Debug\" # <-- ��������!!!
# ������� � ������� *.nsconfig
!define NSCONFIG_FILES_FOLDER "D:\NeuroSoft\Programs\DeviceAutoTest\NSConfig\" # <-- ��������!!!
# ������� � ����������
!define DRIVERS_FOLDER "D:\NeuroSoft\Hardware\Drivers\newezusb\drvdist\"
# ������� ������� �������� MSSQL-����
!define MSSQL_SCRIPT_FOLDER "D:\NeuroSoft\Prototype\ProviderMSSQL\"
# ������� ������� �������� MySQL-����
!define MYSQL_SCRIPT_FOLDER "D:\NeuroSoft\Prototype\ProviderMySQL\"
# ������� ������� OLEDB
!define OLEDB_SCRIPT_FOLDER "D:\NeuroSoft\Prototype\ProviderMdb\OleDbTemplate\"
# ������� ����������� PDB-������
!define COPY_PDB_FILES
# ����������� ���� ����������
!define PROGRAM_EXE_NAME "NeuroSoft.DeviceAutoTest.exe" # <-- ��������!!!
# ����������� ���� ���������� x86
!define PROGRAM_EXE_x86_NAME "NeuroSoft.DeviceAutoTest.exe" # <-- ��������!!!
# ��� �������
!define PROGRAM_FORMAT_NAME "NeuroSoft.DeviceAutoTest" # <-- ��������!!!
# ��� ����� � ������� ��������
# ���� ���� ���� �����������, �� ����� �������� �� �������������
!define LICENSE_FILE_NAME "License.id"
# ����������������� ��� ��������������� ������ �����
#!define FORCE_LANGUAGE_SELECTION

# ����������
SetCompressor /SOLID lzma

# �����������
!define REGKEY "SOFTWARE\Neurosoft\${PRODUCT_NAME}"

# MUI
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "Header.bmp"
!define MUI_WELCOMEPAGE_TITLE_3LINES
!define MUI_WELCOMEFINISHPAGE_BITMAP "Wizard.bmp"
!define MUI_ICON "DATIcon.ico"
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_TITLE_3LINES
!define MUI_STARTMENUPAGE_REGISTRY_ROOT HKLM
!define MUI_STARTMENUPAGE_NODISABLE
!define MUI_STARTMENUPAGE_REGISTRY_KEY ${REGKEY}
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME StartMenuGroup
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "$(CompanyName)\$(ProgramName)"
!define MUI_UNICON "DATIcon.ico"
!define MUI_UNFINISHPAGE_NOAUTOCLOSE
# �������� ���������
!define MUI_LANGDLL_ALLLANGUAGES
!define MUI_LANGDLL_REGISTRY_ROOT HKLM
!define MUI_LANGDLL_REGISTRY_KEY ${REGKEY}
!define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

!define MUI_CUSTOMFUNCTION_GUIINIT GUIInit

# �������������� ������
ReserveFile "${NSISDIR}\Plugins\nsDialogs.dll"

!insertmacro MUI_RESERVEFILE_LANGDLL

# ����������
Var UpgradeMode # "TRUE" ���� ��������� ��� ���� ����� �����������
Var OsIsOK # "TRUE" ���� ������� �� ��������������
Var StartMenuGroup # ������� � ���� "����"
Var SetupMode # ����� ���������: "x86" ��� "x64"

# �������� �����������
!insertmacro MUI_PAGE_WELCOME
!define MUI_PAGE_CUSTOMFUNCTION_PRE PRE_PageLicense
!insertmacro MUI_PAGE_LICENSE $(license)
Page custom CUSTOM_PAGE_LicenseNumber
Page custom CUSTOM_PAGE_SelectPlatform
!define MUI_PAGE_CUSTOMFUNCTION_PRE PRE_PageDirectory
!insertmacro MUI_PAGE_DIRECTORY
!define MUI_PAGE_CUSTOMFUNCTION_PRE PRE_PageStartMenu
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

# �����
# ������ ���� ���� �� ��������� (���������, ���� ������� ���� Windows �� �������������� �������������)
!insertmacro MUI_LANGUAGE English
!insertmacro MUI_LANGUAGE Russian
#!insertmacro MUI_LANGUAGE French
#!insertmacro MUI_LANGUAGE German
#!insertmacro MUI_LANGUAGE Italian
#!insertmacro MUI_LANGUAGE Polish
#!insertmacro MUI_LANGUAGE Portuguese
#!insertmacro MUI_LANGUAGE PortugueseBR
#!insertmacro MUI_LANGUAGE Spanish

# ���
!define /date Year "%Y"

# �������� ������� 
LicenseLangString license ${LANG_RUSSIAN} "License.txt"
LicenseLangString license ${LANG_ENGLISH} "LicenseEN.txt"

LangString LicenseKeyPageHeader ${LANG_RUSSIAN} "����� ��������"
LangString LicenseKeyPageHeader ${LANG_ENGLISH} "Licence number"

LangString LicenseKeyInfo ${LANG_RUSSIAN} "�� ������ ����� ������������ ���� � ������ ������� ���� ��������� ������� ��������������� ����� ��� � ������������ ����������."
LangString LicenseKeyInfo ${LANG_ENGLISH} "The license key is indicated on the rear side of the distributive disk case and in the license agreement."

LangString LicenseKeyPageDescription ${LANG_RUSSIAN} "������� ����� ��������."
LangString LicenseKeyPageDescription ${LANG_ENGLISH} "Enter license number."

LangString PlatformPageHeader ${LANG_RUSSIAN} "����� ������ ���������"
LangString PlatformPageHeader ${LANG_ENGLISH} "Program mode"

LangString PlatformPageDescription ${LANG_RUSSIAN} "�������� ������� ��������� ���������."
LangString PlatformPageDescription ${LANG_ENGLISH} "Select program operating mode."

LangString x64Mode ${LANG_RUSSIAN} "64-������ ������ ���������"
LangString x64Mode ${LANG_ENGLISH} "64-bits program version"

LangString x86Mode ${LANG_RUSSIAN} "32-������ ������ ���������"
LangString x86Mode ${LANG_ENGLISH} "32-bits program version"

LangString CompanyName ${LANG_RUSSIAN} "���������"
LangString CompanyName ${LANG_ENGLISH} "Neurosoft"

LangString ProgramName ${LANG_RUSSIAN} "DeviceAutoTest.NET" # <-- ��������!!!
LangString ProgramName ${LANG_ENGLISH} "DeviceAutoTest.NET"

LangString JournalProgramName ${LANG_RUSSIAN} "�������� ������������"
LangString JournalProgramName ${LANG_ENGLISH} "Exams manager"

LangString ^UninstallLink ${LANG_RUSSIAN} "������� $(ProgramName)"
LangString ^UninstallLink ${LANG_ENGLISH} "Uninstall $(ProgramName)"

LangString Branding_Text ${LANG_RUSSIAN} "� ���������, 1992-${Year}"
LangString Branding_Text ${LANG_ENGLISH} "� Neurosoft, 1992-${Year}"

LangString NoX64SupportMessage ${LANG_RUSSIAN} "��������� �� ������������ 64-������ ������ Windows.$\r$\n��������� ����������."
LangString NoX64SupportMessage ${LANG_ENGLISH} "x64 Windows versions are not supported.$\r$\nSetup will be aborted."

LangString UnsupportedOS ${LANG_RUSSIAN} "��������� $(ProgramName) �� ������������ ������������� ������������ �������.$\r$\n$\r$\n�������������� ������������ �������:$\r$\n$\r$\n   Windows XP Service Pack 2$\r$\n   Windows Server 2003R2$\r$\n   Windows Vista Service Pack 1 (��� ����)$\r$\n   Windows Server 2008$\r$\n   Windows 7"
LangString UnsupportedOS ${LANG_ENGLISH} "$(ProgramName) do not supports this Windows version.$\r$\n$\r$\nSupported Windows list:$\r$\n$\r$\n   Windows XP Service Pack 2$\r$\n   Windows Server 2003R2$\r$\n   Windows Vista Service Pack 1 (or better)$\r$\n   Windows Server 2008$\r$\n   Windows 7"

LangString ReinstallProgram ${LANG_RUSSIAN} "��������� $(ProgramName) ��� �����������.$\r$\n��������� ��������� ���������?"
LangString ReinstallProgram ${LANG_ENGLISH} "Program $(ProgramName) already installed. Reinstall it?"

LangString UpdateProgram ${LANG_RUSSIAN} "��������� $(ProgramName) ������ $R0 ��� �����������.$\r$\n����� ��������� ���������� �� ������ ${PRODUCT_VERSION}.$\r$\n���������� ���������?"
LangString UpdateProgram ${LANG_ENGLISH} "Program $(ProgramName) of version $R0 has been detected.$\r$\nSetup will be update it to version ${PRODUCT_VERSION}.$\r$\nContinue?"

LangString DeleteUserSettings ${LANG_RUSSIAN} "������� ��������� ������������ ��������� $(ProgramName)?"
LangString DeleteUserSettings ${LANG_ENGLISH} "Are you wish to delete $(ProgramName) user settings?"

LangString InstallerIsRunning ${LANG_RUSSIAN} "��������� ��������� ��� ��������."
LangString InstallerIsRunning ${LANG_ENGLISH} "The installer is already running."

LangString ApplicationIsRunning ${LANG_RUSSIAN} "��������� $(ProgramName) ��������. ��������� �� ����� ����������."
LangString ApplicationIsRunning ${LANG_ENGLISH} "$(ProgramName) is running. Please close it first."

LangString SetupNeedsToRestartSystem ${LANG_RUSSIAN} "��������� ����� ���������� ����� ������������ ����������."
LangString SetupNeedsToRestartSystem ${LANG_ENGLISH} "Setup will be continued after system restart."

LangString FrameworkIsNotInstalled ${LANG_RUSSIAN} "��� ������ ${PRODUCT_NAME} ���������� ���������� Microsoft .NET Framework 4."
LangString FrameworkIsNotInstalled ${LANG_ENGLISH} "You need to install Microsoft Framework 4 before using the ${PRODUCT_NAME}."

LangString ProgramRequiresAdminRights ${LANG_RUSSIAN} "��� ������ ������ ��������� ���������� ����� ��������������. ����������?"
LangString ProgramRequiresAdminRights ${LANG_ENGLISH} "This program requires admin privileges, try again?"

LangString ProgramHasNoAdminRights ${LANG_RUSSIAN} "��� ������ ������ ��������� ���������� ����� ��������������."
LangString ProgramHasNoAdminRights ${LANG_ENGLISH} "This program requires admin privileges."

LangString LogonServiceNotRunning ${LANG_RUSSIAN} "������ ����������� �� �������!"
LangString LogonServiceNotRunning ${LANG_ENGLISH} "Logon service is not running!"

LangString UnableToElevate ${LANG_RUSSIAN} "������ ��������� ����: "
LangString UnableToElevate ${LANG_ENGLISH} "Unable to elevate, error: "

LangString ADOEngineIsNotInstalled ${LANG_RUSSIAN} "��� ������ ${PRODUCT_NAME} ���������� ���������� Microsoft Access database engine 2010. �� ����� ��������� ������� �������� �������� ������. ����� �� ������ ������������� ��������� �������� ������� �� ����� ���������� 32-� ���������� MS Office. � ���� ������ ������� MS Office � ������ ����������, �������� ���������� ��������� ${PRODUCT_NAME}, � ����� ���������� 64-��������� ������ MS Office."
LangString ADOEngineIsNotInstalled ${LANG_ENGLISH} "You need to install Microsoft Access database engine 2010 before using the ${PRODUCT_NAME}. During installation some error occurs. The most probably reason of the error is that 32-bit Microsoft Office is installed on your computer. In this case you can uninstall Microsoft Office, then install ${PRODUCT_NAME} and after that install 64-bit Microsoft Office."

# ���������� � ������ ������������
VIProductVersion ${PRODUCT_VERSION}
VIAddVersionKey /LANG=${LANG_RUSSIAN} "ProductName" "������������� �������.NET" # <-- ��������!!!
VIAddVersionKey /LANG=${LANG_RUSSIAN} "CompanyName" "���������"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "LegalCopyright" "� ���������, 1992-${Year}"
VIAddVersionKey /LANG=${LANG_RUSSIAN} "FileDescription" "��������� ��� ������������� �������� ������� ��������" # <-- ��������!!!
VIAddVersionKey /LANG=${LANG_RUSSIAN} "FileVersion" "${PRODUCT_VERSION}"

VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "DeviceAutoTest Program" # <-- ��������!!!
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Neurosoft"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "� Neurosoft, 1992-${Year}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "DeviceAutoTest Program" # <-- ��������!!!
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${PRODUCT_VERSION}"

# ��������
Name ${PRODUCT_NAME}
BrandingText $(Branding_Text)
OutFile "${PRODUCT_NAME}-${PRODUCT_VERSION}-Setup.exe"
CRCCheck on
XPStyle on
ShowInstDetails show
InstallDirRegKey HKLM "${REGKEY}" Path
ShowUninstDetails show
RequestExecutionLevel user

!macro InitUAC

uac_tryagain:
    !insertmacro UAC_RunElevated
    ${Switch} $0
       ${Case} 0
          ${IfThen} $1 = 1 ${|} Quit ${|} ; ������� �������, �������
          ${IfThen} $3 <> 0 ${|} ${Break} ${|} ; ����� �������������� ��� ����
          ${If} $1 = 3 ; ���� �������������� ���
             MessageBox MB_YESNO|MB_ICONEXCLAMATION|MB_TOPMOST|MB_SETFOREGROUND "$(ProgramRequiresAdminRights)" /SD IDNO IDYES uac_tryagain IDNO 0
          ${EndIf}
       ${Case} 1223
          MessageBox MB_ICONSTOP|MB_TOPMOST|MB_SETFOREGROUND "$(ProgramHasNoAdminRights)"
          Quit
       ${Case} 1062
          MessageBox MB_ICONSTOP|MB_TOPMOST|MB_SETFOREGROUND "$(LogonServiceNotRunning)"
          Quit
       ${Default}
          MessageBox MB_ICONSTOP|MB_TOPMOST|MB_SETFOREGROUND "$(UnableToElevate)$0"
          Quit
    ${EndSwitch}
    SetShellVarContext all

!macroend

# ����������
Function .onInit

    !insertmacro InitUAC
    System::Call 'kernel32::CreateMutexA(i 0, i 0, t "$(ProgramName)") i .r1 ?e'
    Pop $R0
    StrCmp $R0 0 +3
    MessageBox MB_OK|MB_ICONEXCLAMATION $(InstallerIsRunning) /SD IDOK
    Abort

Retry:
    FindProcDLL::FindProc ${PROGRAM_EXE_NAME}
    IntCmp $R0 1 0 +5
    FindProcDLL::FindProc ${PROGRAM_EXE_x86_NAME}
    IntCmp $R0 1 0 +3
    MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $(ApplicationIsRunning) /SD IDOK IDRETRY Retry
    Abort

    # �������� ������ ������������ �������
    StrCpy $OSIsOK "FALSE"
    ${If} ${AtLeastWinXP}

       ${If} ${IsWinXP}
       ${AndIf} ${AtLeastServicePack} 2
          StrCpy $OSIsOK "TRUE"
       ${EndIf}

       ${If} ${IsWin2003}
       ${AndIf} ${AtLeastServicePack} 2
          StrCpy $OSIsOK "TRUE"
       ${EndIf}

       ${If} ${IsWin2003R2}
          StrCpy $OSIsOK "TRUE"
       ${EndIf}

       ${If} ${IsWinVista}
       ${AndIf} ${AtLeastServicePack} 1
          StrCpy $OSIsOK "TRUE"
       ${EndIf}

       ${If} ${IsWin2008}
          StrCpy $OSIsOK "TRUE"
       ${EndIf}

       ${If} ${AtLeastWin7}
          StrCpy $OSIsOK "TRUE"
       ${EndIf}
    ${EndIf}

    ${If} $OSIsOK == "FALSE"
       MessageBox MB_OK|MB_ICONSTOP $(UnsupportedOS) /SD IDOK
       Abort
    ${EndIf}

    InitPluginsDir

!ifdef SPLASH_BITMAP
    File /oname=$PLUGINSDIR\Splash.bmp "${SPLASH_BITMAP}"
    #File /oname=$PLUGINSDIR\splash.wav
    splash::show 1000 $PLUGINSDIR\splash
    Pop $0
!endif

!ifdef FORCE_LANGUAGE_SELECTION
    # �������������� ����� �����
    !insertmacro MUI_LANGDLL_DISPLAY
!endif

    # �������� ������
    ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayVersion"
    StrCpy $UpgradeMode "FALSE"
    StrCmp $R0 "" Setup 0
    StrCmp $R0 "${PRODUCT_VERSION}" SameVersion DifferentVersion

SameVersion:
    StrCpy $UpgradeMode "TRUE"
    MessageBox MB_OKCANCEL|MB_ICONQUESTION $(ReinstallProgram) /SD IDOK IDOK Setup IDCANCEL 0
    Abort

DifferentVersion:
    StrCpy $UpgradeMode "TRUE"
    MessageBox MB_OKCANCEL|MB_ICONQUESTION $(UpdateProgram) /SD IDOK IDOK Setup IDCANCEL 0
    Abort

Setup:

FunctionEnd

Function GUIInit

    ${If} $LANGUAGE != ${LANG_RUSSIAN}
        File /oname=$PLUGINSDIR\modern-header.bmp HeaderEN.bmp
        File /oname=$PLUGINSDIR\modern-wizard.bmp WizardEN.bmp
        SetBrandingImage /IMGID=1046 "$PLUGINSDIR\modern-header.bmp"
    ${EndIf}
    
FunctionEnd

# ��������� ������� WindowsInstaller V3.1 ��� ���� � �������� �������� ��� ���� ������ ����
Function SetupWI

    GetDLLVersion "$SYSDIR\msi.dll" $R0 $R1
    IntOp $R2 $R0 / 0x00010000
    IntOp $R3 $R0 & 0x0000FFFF
    # R2.R3 - ������ msi.dll
    IntCmp $R2 3 Equal Setup End
Equal:
    IntCmp $R3 1 End Setup End
Setup:
    IfFileExists "$EXEDIR\..\WindowsInstaller\WindowsInstaller-KB893803-v2-x86.exe" CallSetup End
CallSetup:
    ExecWait "$EXEDIR\..\WindowsInstaller\WindowsInstaller-KB893803-v2-x86.exe /norestart /passive"
End:

FunctionEnd

# ��������� ������� .NET Framework � �������� ���������� ��� ���� �� �� ������
Function SetupFramework

    # ��������� ������� Framework 2, ���� �� ���������� -- ������
    ReadRegDWORD $R1 HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727" "Install"
    ${If} $R1 != 1
       ${If} ${RunningX64}
          ExecWait "$EXEDIR\..\DotNETFX\NetFx64.exe"
       ${Else}
          ExecWait "$EXEDIR\..\DotNETFX\DotNETFX20.exe"
          ExecWait "$EXEDIR\..\DotNETFX\NetFx20SP1_x86.exe"
       ${EndIf}
    ${EndIf}

    # ��������� ������� Framework 4, ���� �� ���������� -- ������
    ReadRegDWORD $R1 HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Install"
    ${If} $R1 != 1
       ExecWait "$EXEDIR\..\DotNETFX\dotNetFx40_Full_x86_x64.exe"
       # ��������� ���������� ���������
       ReadRegDWORD $R1 HKEY_LOCAL_MACHINE "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Install"
       ${If} $R1 == 1
          # �������� ���������, ��������� ����� ������������
          WriteRegStr HKEY_CURRENT_USER "SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "$EXEFILE" "$EXEDIR\$EXEFILE"
          MessageBox MB_OK|MB_ICONINFORMATION $(SetupNeedsToRestartSystem) /SD IDOK
          Quit
       ${Else}
          # Framework 4 �� ���������� -- �������� ������������, ��� �� ���������
          MessageBox MB_OK|MB_ICONEXCLAMATION $(FrameworkIsNotInstalled) /SD IDOK
       ${EndIf}
    ${EndIf}
    
    # ��������� ������� ���������� .NET Framework 4.0.3, ���� �� ���������� - ������
    ReadRegStr $R1 HKEY_CLASSES_ROOT "Installer\Patches\8D5FB485333C0E531A089FFA5FD3E8E7\SourceList" "PackageName"
    ${If} $R1 != "NDP40-KB2600211-x86-x64.msp"
        ExecWait "$EXEDIR\..\DotNETFX\NDP40-KB2600211-x86-x64.exe"
    ${EndIf}
FunctionEnd

# ��������� ������� IPP � �������� ���������� �� ���� ��� �� �������
Function SetupIPP
    ${DisableX64FSRedirection}
    IfFileExists "$SYSDIR\ipps-6.0.dll" End Setup
Setup:
    ExecWait "$EXEDIR\..\IPP\ipp60_runtime_setup.exe"
End:
    ${EnableX64FSRedirection}
FunctionEnd

Function SetupAccessDatabaseEngine

    ${If} ${RunningX64}
       SetRegView 64
       # ��������� ������� Access DatabaseEngine, ���� �� ���������� -- ������
       ReadRegStr $R1 HKEY_LOCAL_MACHINE "SOFTWARE\Classes\CLSID\{3BE786A0-0366-4F5C-9434-25CF162E475E}" ""
       ${If} $R1 != "Microsoft.ACE.OLEDB.12.0"
          ExecWait "$EXEDIR\..\ADO\AccessDatabaseEngine_x64.exe /quiet"
          # ����� ��������� ���  ��� ��������� ������� Access DatabaseEngine, ���� �� ����������, �� �������� �� ���� ������������
          ReadRegStr $R1 HKEY_LOCAL_MACHINE "SOFTWARE\Classes\CLSID\{3BE786A0-0366-4F5C-9434-25CF162E475E}" ""
          ${If} $R1 != "Microsoft.ACE.OLEDB.12.0"
              MessageBox MB_OK|MB_ICONEXCLAMATION $(ADOEngineIsNotInstalled) /SD IDOK
          ${EndIf}
       ${EndIf}
       SetRegView 32
    ${EndIf}
    
FunctionEnd

# ������������� ��������
var infOldTime
var infNewTime
var drvOldVersion
var drvNewVersion

Function SetupDrivers

    GetFileTime "$COMMONFILES64\Neurosoft\Drivers\newezusb.inf" $R0 $R1
    StrCpy $infOldTime "$R0.$R1"

    GetDLLVersion "$COMMONFILES64\Neurosoft\Drivers\newezusb_x86.sys" $R0 $R1
    IntOp $R2 $R0 / 0x00010000
    IntOp $R3 $R0 & 0x0000FFFF
    IntOp $R4 $R1 / 0x00010000
    IntOp $R5 $R1 & 0x0000FFFF
    StrCpy $drvOldVersion "$R2.$R3.$R4.$R5"

    SetOverwrite ifnewer
    SetOutPath "$COMMONFILES64\Neurosoft\Drivers"
    File /nonfatal /r "${DRIVERS_FOLDER}*.inf"
    File /nonfatal /r "${DRIVERS_FOLDER}*.sys"
    File /nonfatal /r "${DRIVERS_FOLDER}*.cat"

    GetFileTime "$COMMONFILES64\Neurosoft\Drivers\newezusb.inf" $R0 $R1
    StrCpy $infNewTime "$R0.$R1"

    ${If} $infNewTime = $infOldTime
       GetDLLVersion "$COMMONFILES64\Neurosoft\Drivers\newezusb_x86.sys" $R0 $R1
       IntOp $R2 $R0 / 0x00010000
       IntOp $R3 $R0 & 0x0000FFFF
       IntOp $R4 $R1 / 0x00010000
       IntOp $R5 $R1 & 0x0000FFFF
       StrCpy $drvNewVersion "$R2.$R3.$R4.$R5"
    ${Else}
       StrCpy $drvNewVersion ""
    ${EndIf}

    ${If} $drvNewVersion != $drvOldVersion
       ${If} ${RunningX64}
          File /oname=$PLUGINSDIR\devcon.exe devcon_x64.exe
       ${Else}
          File /oname=$PLUGINSDIR\devcon.exe devcon.exe
       ${EndIf}
       File /oname=$PLUGINSDIR\PIDParser.exe PIDParser.exe

       # ���� ���������� � ������������ ��������
       FileOpen $0 "$PLUGINSDIR\GetPIDS.cmd" w
       ${If} $0 != 0
          FileWrite $0 '@echo off$\r$\n'
          FileWrite $0 'color 80$\r$\n'
          FileWrite $0 'echo Installing Neurosoft USB driver package...$\r$\n'
          FileWrite $0 'path "$PLUGINSDIR"$\r$\n'
          FileWrite $0 'cd "$PLUGINSDIR"$\r$\n'
          FileWrite $0 'devcon dp_add "$COMMONFILES64\Neurosoft\Drivers\newezusb.inf"$\r$\n'
          FileWrite $0 'devcon findall USB\VID_ACCA* > PIDS.TXT$\r$\n'
          FileWrite $0 'PIDParser PIDS.TXT$\r$\n'
          FileClose $0
       ${EndIf}
       ExecWait "$PLUGINSDIR\GetPIDS.cmd"

       # ������ �������� ��� ������� ���������� �� ��� hwid
       FileOpen $0 "$PLUGINSDIR\PIDS.TXT" r
       FileOpen $2 "$PLUGINSDIR\Update.cmd" w
       ${If} $2 != 0
          FileWrite $2 '@echo off$\r$\n'
          FileWrite $2 'color 80$\r$\n'
          FileWrite $2 'echo Updating Neurosoft USB drivers...$\r$\n'
          FileWrite $2 'path "$PLUGINSDIR"$\r$\n'
          FileWrite $2 'cd "$PLUGINSDIR"$\r$\n'
ReadHWID:
          FileRead $0 $1
          StrLen $3 $1
          IntOp $3 $3 - 2
          StrCpy $1 $1 $3
          StrCmp $1 "" +3
          FileWrite $2 'devcon update "$COMMONFILES64\Neurosoft\Drivers\newezusb.inf" "$1"$\r$\n'
          Goto ReadHWID
          FileWrite $2 'devcon remove USB\VID_ACCA*$\r$\n'
          FileWrite $2 'devcon rescan$\r$\n'
          FileClose $2
          FileClose $0
          ExecWait "$PLUGINSDIR\Update.cmd"
       ${EndIf}
    ${EndIf}

FunctionEnd

# �������� *.nskey ����� �� �������� � �������������
Function CopyNSKeyFiles

    SetOverwrite on
    IfFileExists "$EXEDIR\*.nskey" DoCopy End
DoCopy:
    CopyFiles /SILENT "$EXEDIR\*.nskey" $INSTDIR
End:

FunctionEnd

# ������� LANG.CFG
Function CreateLangCFG

    ${If} $LANGUAGE = ${LANG_RUSSIAN}
        StrCpy $1 "RU"
    ${ElseIf} $LANGUAGE = ${LANG_ENGLISH}
        StrCpy $1 "EN"
/*  
    ${ElseIf} $LANGUAGE = ${LANG_FRENCH}
        StrCpy $1 "FR"
    ${ElseIf} $LANGUAGE = ${LANG_GERMAN}
        StrCpy $1 "DE"
    ${ElseIf} $LANGUAGE = ${LANG_ITALIAN}
        StrCpy $1 "IT"
    ${ElseIf} $LANGUAGE = ${LANG_POLISH}
        StrCpy $1 "PL"
    ${ElseIf} $LANGUAGE = ${LANG_PORTUGUESE}
        StrCpy $1 ""
    ${ElseIf} $LANGUAGE = ${LANG_PORTUGUESEBR}
        StrCpy $1 ""
    ${ElseIf} $LANGUAGE = ${LANG_SPANISH}
        StrCpy $1 ""
*/
    ${EndIf}
    FileOpen $0 "$PLUGINSDIR\LANG.CFG" w
    ${If} $0 != 0
        FileWrite $0 $1
        FileClose $0
        CopyFiles /silent "$PLUGINSDIR\LANG.CFG" "$APPDATA\Neurosoft\${PRODUCT_NAME}"
        IfFileExists "$APPDATA\Neurosoft\NsArcReader\LANG.CFG" SkipCopy
        CopyFiles /silent "$PLUGINSDIR\LANG.CFG" "$APPDATA\Neurosoft\NsArcReader\LANG.CFG"
SkipCopy:
    ${Endif}
    
FunctionEnd

# �������� ��� ����� ������ ��������.
# ������������ ������ ���� � �������� � ������������� ���� ���� LICENSE_FILE_NAME
# � ��� ���������� ������� �� �������� � �������.
Var PAGE_LicenseNumber
Var LicenseNumber
var RegistryLicenseNumber
Var EnteredLicenseNumber
Var LicenseNumberLabel
Var LicenseNumberBox
Var InfoLabel

Function CUSTOM_PAGE_LicenseNumber

    IfFileExists "$EXEDIR\${LICENSE_FILE_NAME}" 0 SkipDialog
    FileOpen $0 "$EXEDIR\${LICENSE_FILE_NAME}" r
    ${If} $0 != 0
        # ������ ����� �������� �� �����
        FileRead $0 $LicenseNumber
        FileClose $0
        ${If} $LicenseNumber != ""
            # ������ ����� �������� �� �������
            ReadRegStr $RegistryLicenseNumber HKLM "${REGKEY}" "LicenseNumber"
            ${If} $LicenseNumber != $RegistryLicenseNumber
                nsDialogs::Create 1018
                Pop $PAGE_LicenseNumber
                ${If} $PAGE_LicenseNumber == error
                    Abort
                ${EndIf}
                !insertmacro MUI_HEADER_TEXT $(LicenseKeyPageHeader) $(LicenseKeyPageDescription)
                ${NSD_CreateLabel} 30 72 100 12u $(LicenseKeyPageHeader):
                Pop $LicenseNumberLabel
                ${NSD_CreateText} 150 70 270 12u ""
                Pop $LicenseNumberBox
                ${NSD_CreateLabel} 30 140 390 36u $(LicenseKeyInfo)
                Pop $InfoLabel
                ${NSD_SetFocus} $LicenseNumberBox
                ${NSD_SetText} $LicenseNumberBox $EnteredLicenseNumber
                ${NSD_OnChange} $LicenseNumberBox LicenseNumberBoxChange
                EnableWindow $mui.Button.Next 0
                nsDialogs::Show
                ${If} $LicenseNumber == $EnteredLicenseNumber
                    # ����� ����� �������� � ������
                    WriteRegStr HKLM "${REGKEY}" "LicenseNumber" $LicenseNumber
                ${EndIf}
            ${EndIf}
        ${EndIf}
    ${EndIf}
SkipDialog:

FunctionEnd

Function LicenseNumberBoxChange

    ${NSD_GetText} $LicenseNumberBox $EnteredLicenseNumber
    ${If} $LicenseNumber == $EnteredLicenseNumber
        EnableWindow $mui.Button.Next 1
        ${NSD_SetFocus} $mui.Button.Next
    ${EndIf}
    
FunctionEnd

# ���������� �������� ������������� ���������� ��� �������������/��������
Function PRE_PageLicense

    ${If} $UpgradeMode == "TRUE"
        Abort
    ${EndIf}
    
FunctionEnd

# �������� ��� ������ ��������� (x86/x64).
Var PAGE_SelectPlatform
Var X64RadioButton
Var X86RadioButton

Function CUSTOM_PAGE_SelectPlatform
    StrCpy $SetupMode "x86"
    StrCpy $INSTDIR "$PROGRAMFILES\Neurosoft\${PRODUCT_NAME}"
    Return
    
#    ${If} ${RunningX64}
#        ${If} $UpgradeMode == "TRUE"
#           ReadRegStr $1 HKLM "${REGKEY}" "Platform"
#           ${If} $1 == "x64"
#               StrCpy $SetupMode "x64"
#               StrCpy $INSTDIR "$PROGRAMFILES64\Neurosoft\${PRODUCT_NAME}"
#           ${Else}
#               StrCpy $SetupMode "x86"
#               StrCpy $INSTDIR "$PROGRAMFILES\Neurosoft\${PRODUCT_NAME}"
#           ${EndIf}
#           Return
#        ${EndIf}

#        nsDialogs::Create 1018
#        Pop $PAGE_SelectPlatform
#        ${If} $PAGE_SelectPlatform == error
#            Abort
#        ${EndIf}
#        !insertmacro MUI_HEADER_TEXT $(PlatformPageHeader) $(PlatformPageDescription)
#        ${NSD_CreateRadioButton} 130 60 300 12u $(x64Mode)
#        Pop $X64RadioButton
#        ${NSD_CreateRadioButton} 130 110 300 12u $(x86Mode)
#        Pop $X86RadioButton
#        ${NSD_OnClick} $X64RadioButton X64RadioButtonClick
#        ${NSD_OnClick} $X86RadioButton X86RadioButtonClick
#        ReadRegStr $1 HKLM "${REGKEY}" "Platform"
#        ${If} $1 == "x86"
#            StrCpy $SetupMode "x86"
#            ${NSD_Check} $X86RadioButton
#            ${NSD_SetFocus} $X86RadioButton
#        ${Else}
#            StrCpy $SetupMode "x64"
#            ${NSD_Check} $X64RadioButton
#            ${NSD_SetFocus} $X64RadioButton
#        ${EndIf}
#        nsDialogs::Show
#        ${If} $SetupMode == "x64"
#            StrCpy $INSTDIR "$PROGRAMFILES64\Neurosoft\${PRODUCT_NAME}"
#        ${Else}
#            StrCpy $INSTDIR "$PROGRAMFILES\Neurosoft\${PRODUCT_NAME}"
#        ${EndIf}
#    ${Else}
#        StrCpy $INSTDIR "$PROGRAMFILES\Neurosoft\${PRODUCT_NAME}"
#        StrCpy $SetupMode "x86"
#    ${EndIf}

FunctionEnd

#Function X64RadioButtonClick

#    StrCpy $SetupMode "x64"

#FunctionEnd

#Function X86RadioButtonClick

#    StrCpy $SetupMode "x86"

#FunctionEnd

# ���������� �������� ������ �������� ��� �������������/��������
Function PRE_PageDirectory

    ${If} $UpgradeMode == "TRUE"
        Abort
    ${EndIf}
    
FunctionEnd

# ���������� �������� ������ ����� ���������� ����
Function PRE_PageStartMenu

    ${If} $UpgradeMode == "TRUE"
        !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
        Abort
    ${EndIf}
    
FunctionEnd

# ������� ������ �����������
Section -Main SEC0000

    # ��������� ���������� � �������������� ��������
    call SetupWI
    call SetupFramework
    call SetupIPP
    call SetupAccessDatabaseEngine
    
    ${If} $UpgradeMode == "TRUE"
        ReadRegStr $INSTDIR HKLM "${REGKEY}" Path
    ${EndIf}
    
    SetOverwrite on

    # ����������� �����
    SetOutPath "$INSTDIR"
    File /nonfatal /r "${INSTALL_FILES_FOLDER}*.dll"
    File /nonfatal /r "${INSTALL_FILES_FOLDER}*.bmp"
    File /nonfatal /r "${INSTALL_FILES_FOLDER}*.txt"
    File /nonfatal "${MSSQL_SCRIPT_FOLDER}db_script_mssql.txt"
    File /nonfatal "${MYSQL_SCRIPT_FOLDER}db_script_mysql.txt"
    File "${NEUROSOFT_CONTRIB_FOLDER}Microsoft.VC90.CRT.manifest"
    File "${NEUROSOFT_CONTRIB_FOLDER}MspFetCon.exe"
    File "${NEUROSOFT_CONTRIB_FOLDER}msp430.dll"
    File "${NEUROSOFT_CONTRIB_FOLDER}hil.dll"
    File "${NEUROSOFT_CONTRIB_FOLDER}USBHID.dll"
    File "${NEUROSOFT_CONTRIB_FOLDER}SiUtil.dll"
    
    # ��������� �� ����������� �� �����
    ${If} $SetupMode == "x64"
    # x64
       #File "${NEUROSOFT_CONTRIB64_FOLDER}<��� ����� 1>"
       #File "${NEUROSOFT_CONTRIB64_FOLDER}<��� ����� 2>"
    ${Else}
    # x32
        #File "${NEUROSOFT_CONTRIB_FOLDER}neuro_emg_dll.dll"
        #File "${NEUROSOFT_CONTRIB_FOLDER}ns4m.dll"
        #File "${NEUROSOFT_CONTRIB_FOLDER}NsEzUSB.dll"
        File "${NEUROSOFT_CONTRIB_FOLDER}RichEd20.dll"
        File "${NEUROSOFT_CONTRIB_FOLDER}SlimDX.dll"
        File "${NEUROSOFT_CONTRIB_FOLDER}msvcp100.dll"
        File "${NEUROSOFT_CONTRIB_FOLDER}msvcr100.dll"
        File "${NEUROSOFT_CONTRIB_FOLDER}msvcr90.dll"
    ${EndIf}
    
    # AutoUpgrade
    File "${NEUROSOFT_ASSEMBLIES_FOLDER}\NeuroSoft.AutoUpgrade.exe"
    
 !ifdef COPY_PDB_FILES
    File /nonfatal /r "${INSTALL_FILES_FOLDER}*.pdb"
 !endif
 
    File "${INSTALL_FILES_FOLDER}${PROGRAM_EXE_NAME}"
    File "${INSTALL_FILES_FOLDER}${PROGRAM_EXE_NAME}.config"
    ${If} ${RunningX64}
        #File "${INSTALL_FILES_FOLDER}${PROGRAM_EXE_x86_NAME}"
    ${EndIf}

    SetOutPath "$INSTDIR\OleDbTemplate"
    File /nonfatal "${OLEDB_SCRIPT_FOLDER}*.mdb"

    # ������������ ��� �������������� ������
    SetOutPath "$APPDATA\Neurosoft\${PRODUCT_NAME}"
    SetOverwrite off
    File /nonfatal /r "${NSCONFIG_FILES_FOLDER}Common.NSConfig"
    SetOverwrite on
    File /nonfatal /r "${NSCONFIG_FILES_FOLDER}DefaultOptions.NSConfig"
    SetOutPath "$APPDATA\Neurosoft\${PRODUCT_NAME}\Images"
    File /nonfatal /r "${NSCONFIG_FILES_FOLDER}Images\*.*"
    SetOutPath "$APPDATA\Neurosoft\${PRODUCT_NAME}\Templates"
    File /nonfatal /r "${NSCONFIG_FILES_FOLDER}Templates\*.*"
    SetOutPath "$APPDATA\Neurosoft\${PRODUCT_NAME}\ProtocolPatterns"
    File /nonfatal /r "${NSCONFIG_FILES_FOLDER}ProtocolPatterns\*.*"
    # ����� (*.nskey)
    call CopyNSKeyFiles

    SetOverwrite ifnewer
    
    # NsArcReader
    SetOutPath "$INSTDIR"
    File "${NEUROSOFT_ASSEMBLIES_FOLDER}\NeuroSoft.NSARCReader.exe"
    ${CreateFileAssociation} ".nsarc" "NSArc.nsarc_file" "Neurosoft Archieve File" "$INSTDIR\NeuroSoft.NSARCReader.exe" "$INSTDIR\NeuroSoft.NSARCReader.exe,0"
    
    SetOverwrite off
    
    # ������������ NsArcReader 
    SetOutPath "$APPDATA\Neurosoft\NsArcReader"
    File /nonfatal "NSARCReader.Default.NSConfig"
    
    SetOverwrite ifnewer
    
    # ��������
    call SetupDrivers
    
    # LANG.CFG
    call CreateLangCFG
    
    SetOutPath "$INSTDIR"
    
    # ������ ����� ������� � ���� � ���������
    WriteRegStr HKCU "Software\Neurosoft\FormatsNET\${PROGRAM_FORMAT_NAME}" Path "$INSTDIR\${PROGRAM_EXE_NAME}"

    # ������������� ������ ������ � �������� � �����������
    AccessControl::GrantOnFile "$APPDATA\Neurosoft\${PRODUCT_NAME}" "(BU)" "FullAccess"
    
    # ������������� ������ ������ � �������� � ����������� NSARCReader
    AccessControl::GrantOnFile "$APPDATA\Neurosoft\NSARCReader" "(BU)" "FullAccess"

    WriteRegStr HKLM "${REGKEY}\Components" Main 1
    
SectionEnd

# ���������� ����������
Section -Post SEC0001
    
    # MS Word
    ${If} $SetupMode == "x64"
        SetRegView 64
    ${EndIf}
    WriteRegDWord HKLM "SOFTWARE\Classes\Word.Document.8" "BrowserFlags" 0x80000024
    WriteRegDWord HKLM "SOFTWARE\Classes\Word.Document.12" "BrowserFlags" 0x80000024
    WriteRegBIn HKCU "Software\Microsoft\Windows\Shell\AttachmentExecute\{0002DF01-0000-0000-C000-000000000046}" "Word.Document.8" 0
    WriteRegBIn HKCU "Software\Microsoft\Windows\Shell\AttachmentExecute\{0002DF01-0000-0000-C000-000000000046}" "Word.Document.12" 0
    ${If} $SetupMode == "x64"
        SetRegView 32
    ${EndIf}

    # ������
    StrCpy $1 "$INSTDIR\${PROGRAM_EXE_NAME}"
    ${If} ${RunningX64}
        ${If} $SetupMode == "x86"
            StrCpy $1 "$INSTDIR\${PROGRAM_EXE_x86_NAME}"
        ${EndIf}
    ${EndIf}
    SetOutPath $INSTDIR
    CreateShortCut "$DESKTOP\$(ProgramName).lnk" "$1"
    CreateShortCut "$DESKTOP\$(JournalProgramName).lnk" "$INSTDIR\NeuroSoft.NSARCReader.exe"
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortCut "$SMPROGRAMS\$StartMenuGroup\$(ProgramName).lnk" "$1"
    CreateShortCut "$SMPROGRAMS\$(CompanyName)\$(JournalProgramName).lnk" "$INSTDIR\NeuroSoft.NSARCReader.exe"
    
    WriteRegStr HKLM "${REGKEY}" Path $INSTDIR
    WriteRegStr HKLM "${REGKEY}" "Platform" $SetupMode
    WriteUninstaller $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetOutPath $SMPROGRAMS\$StartMenuGroup
    CreateShortcut "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk" $INSTDIR\uninstall.exe
    !insertmacro MUI_STARTMENU_WRITE_END
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" DisplayName "${PRODUCT_NAME}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" DisplayVersion "${PRODUCT_VERSION}"
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" DisplayIcon $INSTDIR\uninstall.exe
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" UninstallString $INSTDIR\uninstall.exe
    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" Publisher $(CompanyName)
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" NoModify 1
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" NoRepair 1
SectionEnd

# ������������
!macro SELECT_UNSECTION SECTION_NAME UNSECTION_ID

    Push $R0
    ReadRegStr $R0 HKLM "${REGKEY}\Components" "${SECTION_NAME}"
    StrCmp $R0 1 0 next${UNSECTION_ID}
    !insertmacro SelectSection "${UNSECTION_ID}"
    GoTo done${UNSECTION_ID}
next${UNSECTION_ID}:
    !insertmacro UnselectSection "${UNSECTION_ID}"
done${UNSECTION_ID}:
    Pop $R0
    
!macroend

# ������� ������ �������������
Section /o -un.Main UNSEC0000

    DeleteRegValue HKLM "${REGKEY}\Components" Main
    
SectionEnd

Section -un.post UNSEC0001

    Delete "$DESKTOP\$(ProgramName).lnk"
    Delete "$DESKTOP\$(JournalProgramName).lnk"
    Delete "$SMPROGRAMS\$StartMenuGroup\$(ProgramName).lnk"
    Delete "$SMPROGRAMS\$(CompanyName)\$(JournalProgramName).lnk"
    Delete "$SMPROGRAMS\$StartMenuGroup\$(^UninstallLink).lnk"
    Delete "$INSTDIR\uninstall.exe"
    RmDir "$SMPROGRAMS\$StartMenuGroup"
    RmDir /r "$INSTDIR"
    
    DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
    DeleteRegValue HKLM "${REGKEY}" "StartMenuGroup"
    DeleteRegValue HKLM "${REGKEY}" "Path"
    DeleteRegValue HKLM "${REGKEY}" "Platform"
    DeleteRegValue HKLM "${REGKEY}" "${MUI_LANGDLL_REGISTRY_VALUENAME}"    
    
    DeleteRegKey /IfEmpty HKLM "${REGKEY}\Components"
    DeleteRegKey /IfEmpty HKLM "${REGKEY}"
    
    # �������� ���������������� ��������
    MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 $(DeleteUserSettings) /SD IDNO IDYES 0 IDNO End
    RmDir /r "$APPDATA\Neurosoft\${PRODUCT_NAME}\en"
    # �������� ��� ������ ������
    #RmDir /r "$APPDATA\Neurosoft\${PRODUCT_NAME}\pt"
    #RmDir /r "$APPDATA\Neurosoft\${PRODUCT_NAME}\sk"
    Delete "$APPDATA\Neurosoft\${PRODUCT_NAME}\*.NSConfig"
    Delete "$APPDATA\Neurosoft\${PRODUCT_NAME}\*.NSBackup"
    Delete "$APPDATA\Neurosoft\${PRODUCT_NAME}\*.cfg"
    Delete "$APPDATA\Neurosoft\${PRODUCT_NAME}\*.xml"
End:

SectionEnd

Function un.onInit

    !insertmacro InitUAC
    ReadRegStr $INSTDIR HKLM "${REGKEY}" Path
    !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup
    !insertmacro MUI_UNGETLANGUAGE
    !insertmacro SELECT_UNSECTION Main ${UNSEC0000}
    
FunctionEnd
