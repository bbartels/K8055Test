# K8055Test

### Synopsis

This testing tool allows you to interface with a K8055 to check if the device is working correctly. 
I wrote this initially to test my [K8055 Simulator](https://github.com/bbartels/K8055Simulator), though it works with both the simulator and the actual K8055, depending which .dll you use.

The user interface is designed to be very close to the by Velleman created [K8055Demo](https://www.velleman.eu/support/downloads/?code=K8055), which provides essentially the same functionality, but doesn't work with the simulator.

### Prerequisites

  - You have to have the [.NET Framework V4.5+](https://www.microsoft.com/en-us/download/details.aspx?id=30653) installed.
  - (Optional) If you want to build this Project from source yourself, you have to have [Visual Studio](https://www.visualstudio.com/downloads/) installed.

### Installation

I provided an already compiled project [here](https://github.com/bbartels/K8055Test/releases/tag/1.0). 
But if you prefer to build the library yourself I provided instructions below:

Type the following in the Visual Studio Commandline:
```cmd
> git clone https://github.com/bbartels/K8055Test.git
> msbuild.exe K8055Test\K8055Test.sln /p:Configuration=Release
```
Alternatively you can open the solutions in Visual Studio and compile there.

## License

This project is licensed under the MIT License, for further information please refer to the LICENSE file in the projects root directory.
