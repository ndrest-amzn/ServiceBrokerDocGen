# Generate Service Broker Doc Readme from JSON template.

This program automatically creates the Service Broker doc Readme.md from the Cloudformation Template provided.
It will include all parameters in the template into the Reademe.md

## Setup

1. Requires .net core 2.1 (https://dotnet.microsoft.com/download/dotnet-core/2.1) (for Windows/MacOS/Linux/Other)
2. No build process required
3. Run the application's DLL from command line (found in .\bin\Release\netcoreapp2.1\ServiceBrokerDocGen.dll) : 
```
dotnet \ServiceBrokerDocGen\bin\Release\netcoreapp2.1\ServiceBrokerDocGen.dll
```
4. Provide the requested template in JSON format. IMPORTANT: Ensure the Template contains the 'AWS::ServiceBroker::Specification' section before proceeding
5. Readme is output to a file called 'Readme.md' in the same directory as the template.
6. Edit the Readme.md to and remove parameters not needed within a service plan.

## Demo Output

```

Welcome.
This program automatically creates the Service Broker doc Readme.md from the Cloudformation Template provided.
==========================================================================================================
IMPORTANT: Ensure the Template contains the 'AWS::ServiceBroker::Specification' section before proceeding

Enter in path to '.json' cloudformation template:
C:\Test\template.json

============================================================================================
Generation of Readme.md completed, and located in the same directory as the template path.
Press a Key to Exit
```
