# AZ-220: Microsoft Azure IoT Developer

- **[Download Latest Student Handbook and AllFiles Content](../../releases/latest)**
- **Are you a MCT?** - Have a look at our [GitHub User Guide for MCTs](https://microsoftlearning.github.io/MCT-User-Guide/)
- **Need to build the lab instructions manually?** - Instructions are available in the [MicrosoftLearning/Docker-Build](https://github.com/MicrosoftLearning/Docker-Build) repository
- **[Want to know what is installed on the lab VM?](lab.md)**

## What are we doing?

- To support this course, we will need to make frequent updates to the course content to keep it current with the Azure services used in the course.  We are publishing the lab instructions and lab files on GitHub to allow for open contributions between the course authors and MCTs to keep the content current with changes in the Azure platform.

- We hope that this brings a sense of collaboration to the labs like we've never had before - when Azure changes and you find it first during a live delivery, go ahead and make an enhancement right in the lab source.  Help your fellow MCTs.

## How should I use these files relative to the released MOC files?

- The instructor handbook and PowerPoints are still your primary source for teaching the course content.

- These files on GitHub are designed to be used in conjunction with the student handbook but are in GitHub as a central repository so MCTs and course authors can have a shared source for the latest lab files.

- It will be recommended that for every delivery, trainers check GitHub for any changes that may have been made to support the latest Azure services and get the latest files for their delivery.

## What about changes to the student handbook?

- We will review the student handbook every quarter and update it through the normal MOC release channels as needed.

## How do I contribute?

- Updates to course labs are in development. Some minor changes are being implemented, and some more global updates are being developed. It may be advisable for contributors to propose changes by raising issues before creating pull requests.

- Any MCT can submit a pull request (against the Master branch) to the code or content in the GitHub repo. Microsoft and the course author will triage and include content and lab code changes as needed. If you encounter any blocking issues in the labs, please raise issues immediately.

- You can submit bugs, changes, improvements, and ideas. Find a new Azure feature before we have? Submit a new demo!

## How do I use these labs for self-study?

These labs are written as companions to the instructor-led AZ-220 course.  Although great effort has been made to ensure that the labs include explanations of each step to allow them to be instructional tools, not all requirements for successful completion are documented.  For example, in a classroom environment, students have access to a relatively unrestricted Azure subscription; thus, a complete list of resource providers used in the labs does not exist.  This means that lab steps in a locked-down subscription (one with [Azure Policy](https://docs.microsoft.com/azure/governance/policy/overview) restrictions, for example) may fail.  This also means that one-on-one lab support is not available outside of an instructor-led environment; please do not open issues requesting one-on-one lab support.

Microsoft Learn offers several [learning paths](https://docs.microsoft.com/en-us/learn/browse/?resource_type=learning%20path&products=azure-iot&roles=developer) for IoT that include sandboxed lab environments.  These are going to be more suitable for many students looking for IoT self-study opportunities.

## Notes

### Classroom Materials

It is strongly recommended that MCTs and Partners access these materials and, in turn, provide them separately to students.  Pointing students directly to GitHub to access Lab steps as part of an ongoing class will require them to access yet another UI as part of the course, contributing to a confusing experience. An explanation to the student regarding why they are receiving separate Lab instructions can highlight the nature of an always-changing cloud-based interface and platform. Microsoft Learning support for accessing GitHub files and support for navigation of the GitHub site is limited to MCTs teaching this course only.

You can find a processed, formatted version of the Markdown files at <https://aka.ms/az220labs>.

### Known Issues in the Current release

**Please** refer to the currently open issues to be sure that a new issue doesn't duplicate an existing issue.  The following are known issues that are not in the open issues as we are working on them already:

- Most labs require running the setup script to create specifically named IoT devices. Work is being done to "clean up" the device resources within and across labs.

- Many lab Exercises have not been broken up into defined Tasks, which can make it easy to get lost or miss a step. Work is being done to address this formatting.

- There are places within the lab steps where images could improve the clarity of the instructions. Images are currently being added to the labs where we feel they are required. However, we intend to keep the image count low due to the fluid nature of the Azure UI.
