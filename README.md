# About EmployeesApi
The Web API was created for testing purposes. It consist of .Net WebApi.

## How to build
* [Install](https://dotnet.microsoft.com/en-us/download#/current) the latest .NET 8.0 SDK to run the WebApi

## Testing
Crypto has 2 types of tests:
* Unit test 
* Mutation test

### Set up the testing
* For setting up the unit test you have to load runsettings file (available at `{Solution directory}\Tests\CodeCoverage.runsettings`), then run the tests. Would recommend to install [Fine Code Coverage](https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage2022) extention, to be sure that the coverage format of the runsettings file fits to the Visualization of unit test code coverage.

* For running the mutation testing:
  * Open `Comand prompt` and navigate to project(solution) directory.
  * Run command `dotnet tool restore`. Tool `dotnet-stryker` should be restored.
  * Run command `dotnet stryker`.
  * When it's completed, the reports could be found in `{Solution directory}\StrykerOutput`.
