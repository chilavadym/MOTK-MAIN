# MOT
## Overview
MOT is a project that aims to connect physical devices, such as sensors, to measure oil condition loss factor and other related parameters. The project utilizes Avalonia UI and MVVM pattern to provide developers with a framework to develop applications that can interface with these devices and extract relevant data. By connecting these devices to the application, users can monitor the performance of their equipment and take necessary action when required to ensure optimal performance.
## Getting started
To get started with the project, developers should first fork the repository and create a new branch from the master branch. Once the branch has been created, developers can start working on the new feature or application.

Before making any changes to the code, developers should make sure to restore NuGet packages.

## Code Guidelines
All important functions for the sensor are located in the Common and Serial folders. Any changes to this code should only be made with the approval of Sejal.

Dependency injection registration of services can be done in Bootstraper.cs using the SPLAT DI container.

## Code merging
Code merging follows a specific process in MOT. Developers are required to create a pull request to the develop branch from their feature branch. Once the pull request is created, the code will be reviewed and tested by the QA team to ensure there are no bugs or errors. If everything is okay, the code will be merged into the develop branch.

If any bugs or errors are discovered during the QA process, the developer will be required to fix them before the code can be merged. Once all issues have been resolved, the developer can create a pull request to the master branch, which will be reviewed and merged after thorough testing.

## Issues 
If you encounter any issues or problems with the MOT project code, we encourage you to create an issue on our issue tracker. To do so, go to the project's GitHub page and click on the "Issues" tab. From there, you can create a new issue by clicking on the "New issue" button.

When creating an issue, please provide as much detail as possible about the problem you're experiencing. This includes steps to reproduce the issue, any error messages or logs, and any relevant code snippets or screenshots. Once you've created the issue, our team will investigate it and work to fix the problem as soon as possible.

We appreciate your feedback and help in improving the MOT project.

# License
This code is the property of the company and is protected under the company's licensing agreement. Sharing, selling, or copying the code without the express permission of the company is strictly prohibited and may result in legal consequences for violators.
