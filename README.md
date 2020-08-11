[![Build status](https://ci.appveyor.com/api/projects/status/ebq0byc0f61surk4?svg=true)](https://ci.appveyor.com/project/jakenjarvis/preventreboot) [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

# PreventReboot
This program prevents reboot during login by updating the ActiveTime of Windows10 on a regular basis.

## Overview
This program rewrites the Windows 10 registry and periodically corrects the ActiveTime. (Prevent automatic restart after Windows Update)
This will cause the OS to reboot while you are away from computer so that you will not lose any data you are working on.

## Policy
 - Do not stop Windows Update. (WindowsUpdate is necessary for security improvement. This program does not disturb Windows Update)
 - This program makes it a simple console application. (This works with one EXE file)
 - This program does not communicate with the Internet.

## Environment
 - .NET Framework 4.6.1

## Install
 1. [Download(latest)](https://github.com/jakenjarvis/PreventReboot/releases/latest) the program and extract the ZIP file. Deploy to your arbitrary folder. However, do not move the EXE installation folder after installation.
 2. At the command prompt, execute the following command. This will register this program call once an hour to the Task Scheduler.

```command line
 > PreventReboot.exe -i
 ```

## Uninstall
 1. At the command prompt, execute the following command. This will delete the task of this program registered in Task Scheduler.

 ```command line
 > PreventReboot.exe -u
 ```

 2. Delete the installed EXE file.

## Internal operation
This program performs the following operations when executed from the task scheduler and when started with the following parameters.

 ```command line
 > PreventReboot.exe -s
 ```

 1. Calculate the time to set the ActiveTime from the current time. Approximately two hours before the current time to approximately 10 hours after the current time is the ActiveTime.
 2. Set the following values in the Windows registry. (Registry details are not discussed here)
 
 ```
 HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings
    SetActiveHours DWord, 1
    ActiveHoursStart DWord, <now - 2h>
    ActiveHoursEnd DWord, <now + 10h>

HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate
    SetActiveHours DWord, 1
    ActiveHoursStart DWord, <now - 2h>
    ActiveHoursEnd DWord, <now + 10h>

HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU
    AuOptions DWord, 4
    AlwaysAutoRebootAtScheduledTime DWord, 0
    NoAutoRebootWithLoggedOnUsers DWord, 1

HKLM\SOFTWARE\Policies\Microsoft\Windows\Installer
    DisableAutomaticApplicationShutdown DWord, 1
```

## Dependency(Special thanks)
 - [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils)
 - [Task Scheduler Managed Wrapper](https://github.com/dahall/taskscheduler)
 - [ILRepack](https://github.com/gluck/il-repack)
 - [ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)
 
## License
### Apache License, Version 2.0
```
This document is part of the PreventReboot.

Copyright (c) 2018, PreventReboot.
                    Jaken Jarvis (jaken.jarvis@gmail.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

The author may be contacted via 
https://github.com/jakenjarvis/PreventReboot
```
