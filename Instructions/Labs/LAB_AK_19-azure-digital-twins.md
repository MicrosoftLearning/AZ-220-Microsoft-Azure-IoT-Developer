---
lab:
    title: 'Lab 19: Develop Azure Digital Twins (ADT) solutions'
    module: 'Module 11: Develop with Azure Digital Twins'
---

# Develop Azure Digital Twins (ADT) solutions

## Lab Scenario

Contoso Management has decided to take the next step in their digital evolution and develop a model of their cheese making facility using Azure Digital Twins (ADT). With Azure Digital Twins, it is possible to create and interact with live models of real-world environments. First, each individual element is modeled as a digital twin. Then, these models are connected into a knowledge graph that can respond to live events and be queried for information.

In order to better understand how to best leverage ADT, you have been asked to build a proof-of-concept prototype that demonstrates how the existing Cheese Cave Device sensor telemetry can be incorporated into a simple model hierarchy:

* Cheese Factory
* Cheese Cave
* Cheese Cave Device

In this first prototype, you have been asked to demonstrate the solutions to the following scenarios:

* How device telemetry can be mapped from an IoT Hub into the appropriate device in ADT
* How updates to a child digital twin property can be used to update a parent twin property (from a Cheese Cave Device to a Cheese Cave)
* How device telemetry can be routed via ADT to Time Series Insights

The following resources will be created:

![Lab 19 Architecture](media/LAB_AK_19-architecture.png)

## In This Lab

In this lab, you will complete the following activities:

* Verify that the lab prerequisites are met (that you have the required Azure resources)
* Design and develop digital twin models
  * Create and configure an Azure Digital Twin (ADT) instance
  * Map IoT device data to ADT models and relationships
  * Create digital twin models and validate models
* Create and configure digital twins
  * Create a digital twin by using the DTDL
  * Build ADT graph using digital twin instances
* Implement ADT graph interactions (ADT Explorer)
  * Install and run ADT Explorer
  * Query the ADT Graph
  * Update properties on ADT entities in the graph
* Integrate ADT with upstream and downstream systems
  * Ingest IoT device messages and translate messages to ADT
  * Configure ADT routes and endpoints to publish telemetry to Time Series Insights (TSI)

## Lab Instructions

### Exercise 1: Verify Lab Prerequisites

#### Task 1 - Create resources

This lab assumes that the following Azure resources are available:

| Resource Type  | Resource Name                |
| :------------- | :--------------------------- |
| Resource Group | rg-az220                     |
| IoT Hub        | iot-az220-training-{your-id} |
| TSI            | tsi-az220-training-{your-id} |
| TSI Access Policy | access1                   |

> **Note**:  The **lab19-setup.azcli** script is written to run in a **bash** shell environment - the easiest way to execute this is in the Azure Cloud Shell.

1. Using a browser, open the [Azure Cloud Shell](https://shell.azure.com/) and login with the Azure subscription you are using for this course.

1. If you are prompted about setting up storage for Cloud Shell, accept the defaults.

1. Verify that the Cloud Shell is using **Bash**.

    The dropdown in the top-left corner of the Azure Cloud Shell page is used to select the environment. Verify that the selected dropdown value is **Bash**.

1. On the Cloud Shell toolbar, click **Upload/Download files** (fourth button from the right).

1. In the dropdown, click **Upload**.

1. In the file selection dialog, navigate to the folder location of the GitHub lab files that you downloaded when you configured your development environment.

    In Lab 3 of this course, "Setup the Development Environment", you cloned the GitHub repository containing lab resources by downloading a ZIP file and extracting the contents locally. The extracted folder structure includes the following folder path:

    * Allfiles
      * Labs
          * 19-Azure Digital Twins
            * Setup

    The lab19-setup.azcli script file is located in the Setup folder for lab 19.

1. Select the **lab19-setup.azcli** file, and then click **Open**.

    A notification will appear when the file upload has completed.

1. To verify that the correct file has uploaded, enter the following command:

    ```bash
    ls
    ```

    The `ls` command lists the content of the current directory. You should see the lab19-setup.azcli file listed.

1. To create a directory for this lab that contains the setup script and then move into that directory, enter the following Bash commands:

    ```bash
    mkdir lab19
    mv lab19-setup.azcli lab19
    cd lab19
    ```

    These commands will create a directory for this lab, move the **lab19-setup.azcli** file into that directory, and then change directory to make the new directory the current working directory.

1. To ensure the **lab19-setup.azcli** has the execute permission, enter the following command:

    ```bash
    chmod +x lab19-setup.azcli
    ```

1. On the Cloud Shell toolbar, to enable access to the lab19-setup.azcli file, click **Open Editor** (second button from the right - **{ }**).

1. In the **Files** list, to expand the lab19 folder and open the script file, click **lab19**, and then click **lab19-setup.azcli**.

    The editor will now show the contents of the **lab19-setup.azcli** file.

1. In the editor, update the values of the `{your-id}` and `{your-location}` variables.

    Referencing the sample below as an example, you need to set `{your-id}` to the Unique ID you created at the start of this course - i.e. **cah191211**, and set `{your-location}` to the location that matches your resource group.

    ```bash
    #!/bin/bash

    # Change these values!
    YourID="{your-id}"
    Location="{your-location}"
    ```

    > **Note**:  The `{your-location}` variable should be set to the short name for the region where you are deploying all of your resources. You can see a list of the available locations and their short-names (the **Name** column) by entering this command:

    ```bash
    az account list-locations -o Table

    DisplayName           Latitude    Longitude    Name
    --------------------  ----------  -----------  ------------------
    East Asia             22.267      114.188      eastasia
    Southeast Asia        1.283       103.833      southeastasia
    Central US            41.5908     -93.6208     centralus
    East US               37.3719     -79.8164     eastus
    East US 2             36.6681     -78.3889     eastus2
    ```

1. In the top-right of the editor window, to save the changes made to the file and close the editor, click **...**, and then click **Close Editor**.

    If prompted to save, click **Save** and the editor will close.

    > **Note**:  You can use **CTRL+S** to save at any time and **CTRL+Q** to close the editor.

1. To create the resources required for this lab, enter the following command:

    ```bash
    ./lab19-setup.azcli
    ```

    This will take a few minutes to run. You will see output as each step completes.

#### Task 2 - Verify tools

1. Open a command prompt and verify the Azure CLI is installed locally, by entering the following command:

    ```powershell
    az --version
    ```

1. Verify that azure-cli version 2.4.0 or later is listed

    If Azure CLI is not installed, you must install it before you continue.

### Exercise 2 : Create an instance of the Azure Digital Twins resource

In this exercise, the Azure portal will be used to create an Azure Digital Twins (ADT) instance. The connection data for Azure Digital Twins will then be stored in a text file for later use. Finally the current user will be assigned a role to allow the ADT resource to be accessed.

#### Task 1: Use the Azure portal to create a resource (Azure Digital Twins)

1. Open the [Azure portal](https://portal.azure.com) in new browser window.

1. On the Azure portal menu, click **+ Create a resource**.

    The **New** blade that opens is a front-end to the Azure Marketplace, which is a collection of all the resources you can create in Azure. The marketplace contains resources from both Microsoft and the community.

1. In the **Search the marketplace** text box, type **Azure Digital Twins**

1. When the option appears, select **Azure Digital Twins**, and then click **Create**.

1. On the **Create Resource** pane, under **Subscription**, ensure that the subscription you are using for this course is selected.

    > **Note**: Your account must have the administrator role for the subscription

1. For the **Resource group**, select **rg-az220**.

1. In the **Location** dropdown, select the region where your Azure IoT Hub is provisioned.

1. For the **Resource name**, enter **adt-az220-training-{your-id}**.

1. To review the values entered, click **Review + create**.

1. To start the deployment process, click **Create**.

    Wait a few moments while **Deployment in progress** is displayed.

1. Select **Go to resource**.

    You should see the Overview pane for your ADT resource, which includes a body section titled **Get started with Azure Digital Twins**.

#### Task 2: Save the connection data to a reference file

1. Using **Notepad** or a similar text editor, create a file named **adt-connection.txt**.

1. Add the name of the Azure Digital Twins instance to the file - **adt-az220-training-{your-id}**

1. Add the resource group to the file - **rg-az220**

1. In your browser, return to the Digital Twins instance **Overview** pane.

1. In the **Essentials** section of the pane, locate the **Host name** field.

1. Hover the mouse pointer over the **Host name** field, use the icon than appears to the right of the value to copy the host name to the clipboard, and then paste it into the text file.

1. In the text file, convert the host name to a connection url to your digital twins instance by adding **https://** to the start of the host name.

    The modified url will be similar to:

    ```http
    https://adt-az220-training-dm030821.api.eus.digitaltwins.azure.net
    ```

1. Save the **adt-connection.txt** file.

#### Task 3: Configure the ADT Role assignment

1. To update user roles, on the left-side menu, click **Access control (IAM)**.

1. To view the current user access, click **View my access**.

    A pane will be displayed that lists the current user assignments. Currently, the user will have the **Service Administrator** role. This role was assigned during the ADT creation.

1. Close the **assignments** pane.

1. To view existing role assignments for all users, select the **Role assignments** tab.

    Your current account user should be listed beneath the **Owner** role.

    > **Note**: If the following error is displayed, you can ignore it. This lab does not require access to view other role assignments.
    > ![Permission Error](media/LAB_AK_19-permission-error.png)

1. To add a new assignment, click **+ Add** and then click **Add role assignment**.

1. On the **Add role assignment** pane, in the **Role** drop-down, select **Azure Digital Twins Data Owner**

1. Ensure the **Assign access to** field value is **User, group or service principal**.

    **Note**: There are many types of security principal that can be chosen including **Function Apps**, **Logic Apps**, **Virtual Machines**, etc.

    A list of available users should be provided.

1. To filter the list of users, in the **Select** field, enter enough of your name or email so that your full name and email address appears in the search results list.

1. Select the entry that corresponds to your user account.

    If you are unsure which account you are using, open an **Azure Shell** and run the following command:

    ```bash
    az ad signed-in-user show --query "userPrincipalName" -o tsv
    ```

    The output will show which account should be selected.

    > **Note**: User accounts that are managed in Azure Active Directory have a different format than user accounts that are managed in an external authority such as a Microsoft Account created for Skype, Xbox, etc. Ensure you chose the correct entry for the account you are logged in as. For example:
    > * **joe@contoso.com** - user account managed by AAD with a custom domain
    > * **joe@joesazure.onmicrosoft.com** - user account managed by AAD with the default domain
    > * **joe.smith_gmail.com#EXT#@joesazure.onmicrosoft.com** - user account managed by the external Microsoft Account authority that has been added as a guest to AAD with the default domain

1. To assign the role, click **Save**.

    After a few moments, the role will be assigned and the current user should be listed under the **Azure Digital Twins Data Owner** role.

    > **Note**: It may take longer to display an external user. You can try clicking **Refresh** in the toolbar, or navigating to the **Overview** pane and back to the **Access control (IAM)** pane.

The Azure Digital Twin resource is now created and the user account has been updated so that the resource can be accessed via APIs.

### Exercise 3 - Map IoT device data to ADT models and relationships

In this exercise, the Digital Twins Definition Language (DTDL) will be used to define a subset of the models that represent the Contoso Cheese Factory. A model defines the characteristics of a real-world object, such as a cheese production line. The level of detail, and the size of the object, depend upon the needs of the business. The object can be as small as a temperature sensor, as grand as a building or factory, and as diverse as a sensor, a person, a vehicle, or a process. It can be about anything relevant to your operations. Models have names, and later in this lab you'll create digital twin instances of the models.

> **NOTE**: For more information about DTDL, a full list of required/optional entries, and the complete list of acceptable units, see [the DTDL spec](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md).

As mentioned earlier, when it comes to representing a business via models, much of the taxonomy and detail will be driven by the needs of the business: these include the problems to be solved and the simulations to be created. However, certain aspects of the models will be driven by the IoT devices that form part of the solution - more specifically, the properties, telemetry and operations that they perform.

In this exercise, the relationship between a Cheese Cave Device and a Cheese Cave will be considered and a model hierarchy will be created. Then, the characteristics of the Cheese Cave Device will be considered and mapped to the capabilities of a digital twin model.

> **NOTE**: The Cheese Cave Device simulator (code project) that you developed earlier in the course will be used to define the device characteristics for the purpose of this exercise. The source code is included in the **Starter** folder for this lab.

#### Task 1 - Consider the device capabilities

As with any type of design work, designing a ADT model involves a certain amount of planning. When the ADT model will represent an IoT device, the planning includes an investigation of how to map the characteristics of the IoT device to the fields of the corresponding ADT model. The following steps should be completed for each IoT device type.

1. Evaluate the IoT hub device twin.

    The Cheese Cave Device uses a device twin (not to be confused with a digital twin) to specify the following settings:

    * Desired temperature in Fahrenheit
    * Desired humidity in a range of 0 - 100

1. Evaluate the contents of the IoT device messages.

    The Cheese Cave Device sends messages to an IoT hub that contain:

    * Properties
        * sensorID - current sensor ID (set to **S1**)
        * fanAlert - **true** if the fan is in a failed state, otherwise **false**.
        * temperatureAlert - only present if true - set to **true** if temperature is +/- 5 degrees from desired temperature
        * humidityAlert - only present if true - set to **true** if humidity is +/- 10 of desired humidity percentage
    * Telemetry
        * temperature - current temperature in Fahrenheit
        * humidity - current humidity in range 0 - 100

1. Evaluate any additional Cloud-to-Device or Device-to-Cloud interactions.

    The Cheese Cave Device supports the following direct method:

    * SetFanState - attempts to turn the cheese cave fan on or off (fan may fail)

    For your devices, you may also want to consider message enhancements if any are being applied.

    The next step will be to consider how to represent the IoT device characteristics within in the ADT model.

1. Recall that for an ADT model there are four main fields within the DTDL: *Properties*, *Telemetry*, *Components*, and *Relationships*.

1. Consider mapping the properties specified in both the IoT device twin and the device-to-cloud messages to Properties of the ADT model.

    You may want your ADT model to include a direct mapping of all IoT device properties, or you could choose a subset that fits your business requirements. The current version of Azure Digital Twins does not provide automatic integration between IoT hub (device properties) and ADT model Properties.

1. Consider mapping the IoT device telemetry measurements to corresponding ADT model Telemetry fields.

1. Consider any additional device characteristics or interactions that may need to be represented in the ADT model.

    Some characteristics of an IoT device may not directly map to an ADT model. In the case of our Cheese Cave Device, there is a gap when it comes to the direct method call **SetFanState**. There is no direct mapping for a direct method - the DTDL specification does include a definition for commands, however ADT does not currently support them. Therefore the direct method cannot be mapped and code must be written as part of some business logic  - usually implemented within an Azure Function.

#### Task 2 - Construct the DTDL code for Cheese Cave Device properties

> **Note**: In this task, you will be constructing DTDL code that could be used in an ADT model. We leave it up to you to decide which tool you use, and suggest that either Visual Studio Code or Notepad will work well for purpose of this exercise.

1. Construct the DTDL code corresponding to the IoT device message property **sensorID**.

    This property is a string value and could be expressed in a DTDL fragment as:

    ```json
    {
        "@type": "Property",
        "name": "sensorID",
        "schema": "string",
        "description": "Manufacturer Sensor ID",
        "writable": true
    }
    ```

    In accordance with the DTDL specification for a Property field, the **@type** is required and must at least have a value of **Property** and may optionally be an array that defines a semantic type as well:

    > **TIP**: You can refer back to the following resource for the available semantic types:
    > * [Digital Twins Definition Language (DTDL)](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#semantic-types)

    The **name** property is required and must uniquely identify the property within the current model definition. In this example, the **name** matches the mapped property from the device message - this is not required, however it does simplify the mapping process.

    The **schema** property is required and defines the data type of the property - in this case, a **string**. The schema may be defined as a [Primitive schema](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#primitive-schemas) or a [Complex schema](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#complex-schemas) - however it may not be an **Array** or a complex schema that contains an **Array**.

    The **description** property is optional and is a localizable description for display.

    > **TIP**: You can review the following resource for more information on localization:
    > * [Display string localization](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#display-string-localization).

    The **writable** property is optional an indicates that the property is writable by an external source. As an example, if the value will be updated from a device message, the **writable** value should be **true**. The default value is false (read-only).

    > **TIP**: These are just some of the available properties for defining a Digital Twin Property. Review the following resource to see the full list:
    > * [DTDL Property](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#property)

1. Construct the DTDL code corresponding to the IoT device message property **desiredTemperature**.

    This property is a double value and could be expressed in a DTDL fragment as:

    ```json
    {
        "@type": ["Property", "Temperature"],
        "name": "desiredTemperature",
        "schema": "double",
        "unit": "degreeFahrenheit",
        "description": "Cave desired temperature in Fahrenheit",
        "writable": true
    }
    ```

    Notice that the **@type** value is declared as an array, and contains the required **Property** value as well as the **Temperature** semantic type. By adding a semantic type, the **unit** value can be added, specifying that the property values will be in Fahrenheit.

    As expected, the **schema** value is **double**.

1. Update your DTDL with the remaining device properties:

    ```json
    {
        "@type": "Property",
        "name": "sensorID",
        "schema": "string",
        "description": "Manufacturer Sensor ID",
        "writable": true
    },
    {
        "@type": ["Property", "Temperature"],
        "name": "desiredTemperature",
        "schema": "double",
        "unit": "degreeFahrenheit",
        "description": "Cave desired temperature in Fahrenheit",
        "writable": true
    },
    {
        "@type": "Property",
        "name": "desiredHumidity",
        "schema": "double",
        "description": "Cave desired humidity in percent",
        "writable": true
    },
    {
        "@type": "Property",
        "name": "fanAlert",
        "schema": "boolean",
        "description": "Fan failure alert",
        "writable": true
    },
    {
        "@type": "Property",
        "name": "temperatureAlert",
        "schema": "boolean",
        "description": "Over/Under desired temperature alert",
        "writable": true
    },
    {
        "@type": "Property",
        "name": "humidityAlert",
        "schema": "boolean",
        "description": "Over/Under desired humidity alert",
        "writable": true
    },
    ```

    As you can see, the additional properties follow the same pattern.

#### Task 3 - Construct the DTDL code for Cheese Cave Device telemetry

1. Construct the DTDL code corresponding to the IoT device telemetry **temperature** values.

    Consider the device message telemetry value **temperature**. This is a double value containing a temperature reading in Fahrenheit and could be expressed in a DTDL fragment as:

    ```json
    {
        "@type": ["Telemetry", "Temperature"],
        "name": "temperature",
        "schema": "double",
        "unit": "degreeFahrenheit",
        "description": "Current measured temperature"
    }
    ```

    A **Telemetry** field is defined in a similar way to a **Property**. The **@type** is required and must at least have a value of **Telemetry** and, similar to a **Property**, may optionally be an array that defines a semantic type as well.

    The **name** property is required and must uniquely identify the field within the current model definition. In this example, the **name** matches the mapped value from the device message - this is not required, however it does simplify the mapping process.

    The **schema** property is required and defines the data type of the telemetry value - in this case, a **double**. The schema may be defined as a [Primitive schema](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#primitive-schemas) or a [Complex schema](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#complex-schemas) - however it may not be an **Array** or a complex schema that contains an **Array**.

    The **description** property is optional and is a localizable description for display.

    Notice that there is no **writable** value in the above snippet or in the specification - **Telemetry** values are expected to be written from an external source.

    > **TIP**: These are just some of the available properties for defining a Telemetry field within an ADT model. Review the following resource to see the full list:
    > * [DTDL Telemetry](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#telemetry)

1. Construct the DTDL code for the device message telemetry **humidity** values.

    > **TIP**: Recall that the humidity values are specified without any units (a relative humidity measurement between 0 and 100).

1. Compare your completed DTDL code with to the following:

    ```json
    {
        "@type": ["Telemetry", "Temperature"],
        "name": "temperature",
        "unit": "degreeFahrenheit",
        "description": "Current measured temperature",
        "schema": "double"
    },
    {
        "@type": "Telemetry",
        "name": "humidity",
        "description": "Current measured humidity",
        "schema": "double"
    }
    ```

### Exercise 4 - Create digital twin models and validate models

In the previous exercise, the Cheese Cave Device message content was mapped to DTDL **Property** and **Telemetry** field definitions. In order to use these DTDL code fragments, they must be incorporated into an **Interface** (the top-level code item for a model). The **Interface** for a Cheese Cave Device model would form just a small part of the Azure Digital Twins environment for a Contoso Cheese Factory. However, modeling an environment that represents an entire factory is beyond the scope of this course. With this in mind, a greatly simplified environment that focuses on a Cheese Cave Device model, an associated Cheese Cave model, and a Factory model will be considered instead. The model hierarchy is as follows:

* Cheese Factory Interface
* Cheese Cave Interface
* Cheese Cave Device Interface

Considering the hierarchy of Interface definitions above, and the relationships between them, it can be said that **a Cheese Factory has Cheese Caves**, and **a Cheese Cave has Cheese Cave Devices**.

When designing the Digital Twin models for an ADT environment, it is best to use a consistent approach for creating the IDs used for the Interfaces, Schemas, and Relationships. Each entity within the environment has an **@id** property (it is required for Interfaces) and should uniquely identify that entity. The format of the ID value is that of a **digital twin model identifier (DTMI)**. A DTMI has three components: scheme, path, and version. The scheme and path are separated by a colon `:`, while path and version are separated by a semicolon `;`. The format looks like this: `<scheme> : <path> ; <version>`. The value for scheme within the DTMI formatted ID is always **dtmi**.

One example for the ID value of a Contoso cheese factory would be: `dtmi:com:contoso:digital_factory:cheese_factory;1`.

In this example, the scheme value is **dtmi** as expected and the version is set to **1**. The `<path>` component within this ID value utilizes the following taxonomy:

* The source of the model - **com:contoso**
* The model category - **digital_factory**
* The type within the category - **cheese_factory**

> **TIP**: To learn more about the DTMI format, review the following resource:
> * [Digital Twin Model Identifier](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#digital-twin-model-identifier)

Recalling the model hierarchy and relationships identified above, an example of the IDs used could be:

| Interface                      | ID                                                                     |
| :----------------------------- | :--------------------------------------------------------------------- |
| Cheese Factory Interface       | dtmi:com:contoso:digital_factory:cheese_factory;1                      |
| Cheese Cave Interface        | dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1        |
| Cheese Cave Device Interface | dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1 |

And the relationships between IDs could be:

| Relationship | ID                                                                | From ID                                                         | To ID                                                                  |
| :----------- | :---------------------------------------------------------------- | :-------------------------------------------------------------- | :--------------------------------------------------------------------- |
| Has Caves  | dtmi:com:contoso:digital_factory:cheese_factory:rel_has_caves;1 | dtmi:com:contoso:digital_factory:cheese_factory;1               | dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1        |
| Has Devices  | dtmi:com:contoso:digital_factory:cheese_cave:rel_has_devices;1  | dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1 | dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1 |

> **NOTE**: In _Lab 3: Setup the Development Environment_, you cloned the GitHub repository containing lab resources by downloading a ZIP file and extracting the contents locally. The extracted folder structure includes the following folder path:
>
> * Allfiles
>   * Labs
>       * 19-Azure Digital Twins
>           * Final
>               * Models
>
>  The complete models referenced in this exercise are available in this folder location.

#### Task 1 - Creating the Factory Interface

The Contoso Cheese company's business analysts have determined that the Cheese Factory model should be simple and have the following properties:

| Name        | Schema | Description                                                                                              |
| :---------- | :----- | :------------------------------------------------------------------------------------------------------- |
| FactoryName | String | the name of the factory                                                                                  |
| GeoLocation | Object | the location of the factory - a complex property with Latitude and Longitude values expressed as doubles |

In addition, the Factory will have a relationship with Cheese Caves.

1. To begin the task of creating the Cheese Factory model, open Visual Studio Code.

    > **NOTE**: Microsoft provides an extension for Visual Studio Code, the **DTDL Editor for Visual Studio Code**, that makes using DTDL more efficient by taking full advantage of the following key features:
    >
    > * Create interfaces from the command palette with predefined or customized templates.
    > * Intellisense to help you with the language syntax (including auto-completion).
    > * Use predefined code snippets to develop DTDL efficiently.
    > * Syntax validation.

1. To use the DTDL extension to create a new interface file, open the VS Code Command Palette, and then select **DTDL: Create Interface**.

    The Command Palette is available on the View menu.

1. When prompted to **Select folder**, browse to the location you wish to store the interface files.

1. When prompted for the **Interface name**, enter **CheeseFactoryInterface**.

    Visual Studio Code will open the folder location chosen and will create a file, **CheeseFactoryInterface.json**

1. Select the **CheeseFactoryInterface.json** to open it for editing - the contents will be similar to:

    ```json
    {
        "@context": "dtmi:dtdl:context;2",
        "@id": "dtmi:com:example:CheeseFactoryInterface;1",
        "@type": "Interface",
        "displayName": "CheeseFactoryInterface",
        "contents": [
            {
                "@type": "Telemetry",
                "name": "temperature",
                "schema": "double"
            },
            {
                "@type": "Property",
                "name": "deviceStatus",
                "schema": "string"
            },
            {
                "@type": "Command",
                "name": "reboot",
                "request": {
                    "name": "delay",
                    "schema": "integer"
                }
            }
        ]
    }
    ```

    This starting template illustrates the required content and structure for an Interface file. Of course, it must be customized to suit the requirements for the Contoso Cheese Factory.

    The **@context** property is required and, for this version of the DTDL, it must be set to **dtmi:dtdl:context;2**.

    The **@type** property is required and must be set to **Interface**.

    The remaining properties will be discussed in the following steps.

1. Locate the **@id** property and update the value to **"dtmi:com:contoso:digital_factory:cheese_factory;1"**.

    The **@id** property is required and should uniquely identify the Interface. The value used above utilizes the following taxonomy for the `<path>`:

    * The source of the model - **com:contoso**
    * The model category - **digital_factory**
    * The type within the category - **cheese_factory**

    The version of the model is **1**

1. To provide an improved display name, locate the **displayName** property and update the value to **"Cheese Factory - Interface Model"**.

    > **NOTE**: The **displayName** value can be localized.

1. To remove the sample properties, locate the **contents** array and delete the contents.

    After editing, your CheeseFactoryInterface.json file should look similar to the following:

    ```json
    {
        "@context": "dtmi:dtdl:context;2",
        "@id": "dtmi:com:contoso:digital_factory:cheese_factory;1",
        "@type": "Interface",
        "displayName": "Cheese Factory - Interface Model",
        "contents": [
        ]
    }
    ```

1. To add a DTDL Property for the **FactoryName**, update the **contents** property as follows:

    ```json
    "contents": [
        {
            "@type": "Property",
            "name": "FactoryName",
            "schema": "string",
            "writable": true
        }
    ]
    ```

    Because the **contents** property is defined as a JSON array, DTDL Properties and Telemetry are added as objects within the array.

1. To prepare to add another Property, position the cursor after the closing curly brace `}` above, add a comma **,** and press **Enter**.

1. To use a code snippet to create **Property**, enter **dtp** and select the **Add DTDL Property** snippet (or press **TAB**):

    ![DDTL Property Code Snippet](media/LAB_AK_19-dtp-snippet.png)

    Once the snippet has been expanded. the code will be similar to:

    ![Expanded DDTL Property Snippet](media/LAB_AK_19-dtp-snippet-expanded.png)

1. Set the **name** property value to **GeoLocation**.

    According to the business requirements, the **GeoLocation** DTDL property is a complex property consisting of Latitude and Longitude. One way to specify this is to include an inline schema. A schema is used to describe the on-the-wire or serialized format of the data in a digital twin interface.

    > **TIP**: To learn more about schemas, review the following resource:
    > * [DTDL Schemas](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#schemas)

1. To add a complex schema definition for **GeoLocation**, update the **schema** property value as follows:

    ```json
    {
        "@type": "Property",
        "name": "GeoLocation",
        "schema": {
            "@id": "dtmi:com:contoso:digital_factory:custom_schema:GeoCord;1",
            "@type": "Object",
            "fields": [
                {
                    "@id": "dtmi:com:contoso:digital_factory:custom_schema:GeoCord:lat;1",
                    "name": "Latitude",
                    "schema": "double"
                },
                {
                    "@id": "dtmi:com:contoso:digital_factory:custom_schema:GeoCord:lon;1",
                    "name": "Longitude",
                    "schema": "double"
                }
            ]
        }
    },
    ```

    Notice that the schema has an **@id** value (if no value is added, one is auto generated) that follows the DTMI specification and extends the taxonomy used to define the Factory.

    The **@type** property specifies the type of complex schema - currently the following complex schemas are provided: **Array**, **Enum**, **Map**, and **Object**. In this example the **Object** type is used. An **Object** describes a data type made up of named fields (like a struct in C). The fields in an Object map can be primitive or complex schemas.

    The **fields** property is set to an array of field descriptions, one for each field in the **Object**. Each field has an optional **@id**, a required **name** and a required **type**.

    > **TIP**: To learn more about the available complex types, review the following resources:
    >
    > * [Array](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#array)
    > * [Enum](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#enum)
    > * [Map](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#map)
    > * [Object](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#object)

1. To add the relationship between the Factory and the Cave (the Cave interface will be defined in the next task), add the following JSON object to the **content** array:

    ```json
    {
        "@type": "Relationship",
        "@id": "dtmi:com:contoso:digital_factory:cheese_factory:rel_has_caves;1",
        "name": "rel_has_caves",
        "displayName": "Has caves",
        "target": "dtmi:com:contoso:digital_factory:cheese_cave;1"
    }
    ```

    The **@type** property is required and must be set to **Relationship**.

    The **@id** is optional - if no value is added, one is auto-generated. The value used here uses a structure that indicates the relationship belongs to the **cheese_factory**.

    The **name**  property is required and is the "programming" name of the relationship - relationships are referred to by this value during queries, etc.

    The **displayName** property is optional and is localizable.

    Finally, the **target** property - although optional, this specifies the interface **@id** value of the target. A missing **target** means the relationship can target any interface. The value used here, **"dtmi:com:contoso:digital_factory:cheese_cave;1"** targets a Cave model that will be created in the next task.

    > **TIP**: There are more optional properties available, including two that can constrain the minimum and maximum number of instances of this relationship, etc. To learn more, review the documentation below:
    > * [DTDL Relationships](https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#relationship)

1. Once complete, the Interface definitions should look like:

    ```json
    {
        "@context": "dtmi:dtdl:context;2",
        "@id": "dtmi:com:contoso:digital_factory:cheese_factory;1",
        "@type": "Interface",
        "displayName": "Cheese Factory - Interface Model",
        "contents": [
            {
                "@type": "Property",
                "name": "FactoryName",
                "schema": "string",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "GeoLocation",
                "schema": {
                    "@id": "dtmi:com:contoso:digital_factory:custom_schema:GeoCord;1",
                    "@type": "Object",
                    "fields": [
                        {
                            "@id": "dtmi:com:contoso:digital_factory:custom_schema:GeoCord:lat;1",
                            "name": "Latitude",
                            "schema": "double"
                        },
                        {
                            "@id": "dtmi:com:contoso:digital_factory:custom_schema:GeoCord:lon;1",
                            "name": "Longitude",
                            "schema": "double"
                        }
                    ]
                }
            },
            {
                "@type": "Relationship",
                "@id": "dtmi:com:contoso:digital_factory:cheese_factory:rel_has_caves;1",
                "name": "rel_has_caves",
                "displayName": "Has caves",
                "target": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1"
            }
        ]
    }
    ```

#### Task 2 - Review the Cheese Cave Interface

The Contoso Cheese company's business analysts have determined that the Cheese Cave model will have properties that track the current state of the cave, as well as the desired temperature and humidity for that cave. Often, these values will be duplicating those reported by the connected Cheese Cave Device - however the devices may be replaced or relocated to another cave. Rolling the values up from the device to the cave ensures that the latest state of the cave is available.

The full list of Properties identified for the Cheese Cave model is as follows:

| Name               | Schema  | Description                                   |
| :----------------- | :------ | :-------------------------------------------- |
| inUse              | boolean | Indicates whether the Cheese Cave is in use |
| temperatureAlert   | boolean | Over/under desired temperature alert          |
| humidityAlert      | boolean | Over/under desired humidity alert             |
| fanAlert           | boolean | Fan failure alert                             |
| temperature        | double  | Last measured temperature in Fahrenheit       |
| humidity           | double  | Last measured humidity                        |
| desiredTemperature | double  | Cave desired temperature in Fahrenheit      |
| desiredHumidity    | double  | Cave desired humidity in percent            |

1. Review the Interface definition for the **Cheese Cave Interface**.

    ```json
    {
        "@id": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1",
        "@type": "Interface",
        "displayName": "Cheese Cave - Interface Model",
        "@context": "dtmi:dtdl:context;2",
        "contents": [
            {
                "@type": "Property",
                "name": "inUse",
                "schema": "boolean",
                "description": "Indicates whether the Cheese Cave is in use",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "temperatureAlert",
                "schema": "boolean",
                "description": "Over/under desired temperature alert",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "humidityAlert",
                "schema": "boolean",
                "description": "Over/under desired humidity alert",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "fanAlert",
                "schema": "boolean",
                "description": "Fan failure alert",
                "writable": true
            },
            {
                "@type": ["Property", "Temperature"],
                "name": "temperature",
                "schema": "double",
                "unit": "degreeFahrenheit",
                "description": "Last measured temperature",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "humidity",
                "schema": "double",
                "description": "Last measured humidity",
                "writable": true
            },
            {
                "@type": ["Property", "Temperature"],
                "name": "desiredTemperature",
                "schema": "double",
                "unit": "degreeFahrenheit",
                "description": "Cave desired temperature in Fahrenheit",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "desiredHumidity",
                "schema": "double",
                "description": "Cave desired humidity in percent",
                "writable": true
            },
            {
                "@type": "Relationship",
                "@id": "dtmi:com:contoso:digital_factory:cheese_cave:rel_has_devices;1",
                "name": "rel_has_devices",
                "displayName": "Has devices",
                "target": "dtmi:com:contoso:digital_factory:cheese_cave:cheese_cave_device;1"
            }
        ]
    }
    ```

    Notice how the property definitions align with the requirements. A Relationship to Cheese Cave Device is also provided.

    > **NOTE**: The completed model file **CheeseCaveInterface.json** is available in the **Final\Models** folder associated with this lab.

#### Task 3 - Review the Cheese Cave Device Interface

In this task, the mapped property and telemetry values identified in **Exercise 2 - Map IoT device data to ADT models and relationships** are incorporated into an Interface definition.

| Property Name      | Schema  | Description                              |
| :----------------- | :------ | :--------------------------------------- |
| sensorID           | string  | Manufacturer Sensor ID                   |
| desiredTemperature | double  | Cave desired temperature in Fahrenheit |
| desiredHumidity    | double  | Cave desired humidity in percent       |
| fanAlert           | boolean | Fan failure alert                        |
| temperatureAlert   | boolean | Over/under desired temperature alert     |
| humidityAlert      | boolean | Over/under desired humidity alert        |

| Telemetry Name | Schema | Description                  |
| :------------- | :----- | :--------------------------- |
| temperature    | double | Current measured temperature |
| humidity       | double | Current measured humidity    |

1. Review the Interface definition for the **Cheese Cave Device Interface**.

    ```json
    {
        "@context": "dtmi:dtdl:context;2",
        "@id": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1",
        "@type": "Interface",
        "displayName": "Cheese Cave Device - Interface Model",
        "contents": [
            {
                "@type": "Property",
                "name": "sensorID",
                "schema": "string",
                "description": "Manufacturer Sensor ID",
                "writable": true
            },
            {
                "@type": ["Property", "Temperature"],
                "name": "desiredTemperature",
                "schema": "double",
                "unit": "degreeFahrenheit",
                "description": "Cave desired temperature in Fahrenheit",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "desiredHumidity",
                "schema": "double",
                "description": "Cave desired humidity in percent",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "fanAlert",
                "schema": "boolean",
                "description": "Fan failure alert",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "temperatureAlert",
                "schema": "boolean",
                "description": "Over/under desired temperature alert",
                "writable": true
            },
            {
                "@type": "Property",
                "name": "humidityAlert",
                "schema": "boolean",
                "description": "Over/under desired humidity alert",
                "writable": true
            },
            {
                "@type": ["Telemetry", "Temperature"],
                "name": "temperature",
                "schema": "double",
                "unit": "degreeFahrenheit",
                "description": "Current measured temperature",
            },
            {
                "@type": "Telemetry",
                "name": "humidity",
                "schema": "double",
                "description": "Current measured humidity"
            }
        ]
    }
    ```

    Notice how the property definitions align with the requirements.

    > **NOTE**: The completed model file **CheeseCaveDeviceInterface.json** is available in the **Final\Models** folder associated with this lab.

#### Task 4 - Install DTDL Validator

The **DTDL Editor for Visual Studio Code** extension does a good job of validating the syntax for an individual model, however it cannot validate a hierarchy of models - i.e. ensure that the **target** identified in a Relationship exists. To assist with this challenge, Microsoft has developed a command-line tool - the **DTDL Validator** that can validate a directory tree of DTDL files. This utility makes use of the **Microsoft.Azure.DigitalTwins.Parser** NuGet package to parse and validate the files.

1. To install the **DTDL Validator**, open a browser and navigate to the [DTDL Validator](https://docs.microsoft.com/samples/azure-samples/dtdl-validator/dtdl-validator/) page.

1. To download the source zip, click **Download ZIP**.

1. Unzip the **DTDL_Validator.zip** to a location of your choice.

1. Open a command prompt and navigate to the **{UnZip-Location}\DTDLValidator-Sample\DTDLValidator** folder.

1. To view the command-line options for the **DTDL Validator**, enter the following command:

    ```powershell
    dotnet run --help
    ```

    The output will be similar to:

    ```cmd
    DTDLValidator 1.0.0
    Copyright (C) 2021 DTDLValidator

    -e, --extension      (Default: json) File extension of files to be processed.

    -d, --directory      (Default: .) Directory to search files in.

    -r, --recursive      (Default: true) Search given directory (option -d) only (false) or subdirectories too (true)

    -i, --interactive    (Default: false) Run in interactive mode

    --help               Display this help screen.

    --version            Display version information.
    ```

#### Task 5 - Validate models with the DTDL Validator

1. To validate the model files in folder and sub-folders, enter the following command:

    ```powershell
    dotnet run --directory {model-location}
    ```

    Replace the **{model-location}** token with the folder where the models are located - for example, the **Allfiles\Labs\19-Azure Digital Twins\Final\Models** folder.

    Here is a sample output running against models included with this lab:

    ```cmd
    dotnet run --directory "D:\D-Repos\AZ220-DeveloperLabs\Allfiles\Labs\19-Azure Digital Twins\Final\Models"
    Simple DTDL Validator (dtdl parser library version 3.12.5.0)
    Validating *.json files in folder 'D:\D-Repos\AZ220-DeveloperLabs\Allfiles\Labs\19-Azure Digital Twins\Final\Models'.
    Recursive is set to True

    Read 3 files from specified directory
    Validated JSON for all files - now validating DTDL

    **********************************************
    ** Validated all files - Your DTDL is valid **
    **********************************************
    ```

1. To see an example of the output for a file that has errors, the **Allfiles\Labs\19-Azure Digital Twins\Final\Models** folder contains a file with errors - **CheeseCaveDeviceInterface.json.bad**. To run the **DTDL Validator** tool against this file, the **--extension** argument can be used like so:

    ```powershell
    dotnet run --extension bad --directory "D:\Az220\Allfiles\Labs\19-Azure Digital Twins\Final\Models"
    ```

    The output for this will identify an error in the JSON:

    ```cmd
    Simple DTDL Validator (dtdl parser library version 3.12.5.0)
    Validating *.bad files in folder 'D:\Az220\Allfiles\Labs\19-Azure Digital Twins\Final\Models'.
    Recursive is set to True

    Read 1 files from specified directory
    Invalid json found in file D:\Az220\Allfiles\Labs\19-Azure Digital Twins\Final\Models\CheeseCaveDeviceInterface.json.bad.
    Json parser error
    The JSON object contains a trailing comma at the end which is not supported in this mode. Change the reader options. LineNumber: 55 | BytePositionInLine: 8.

    Found  1 Json parsing errors
    ```

1. To correct the error, open the file in Visual Studio Code and remove the additional comma `,` at the end of line 55.

1. Save the file and re-run the command:

    ```powershell
    dotnet run --extension bad --directory "D:\Az220\Allfiles\Labs\19-Azure Digital Twins\Final\Models"
    ```

    This time, the validator reports an error indicating the **@context** is set to DTDL Version 1.

1. Return to Visual Studio Code and locate the **@context** property and set it to **dtmi:dtdl:context;2**.

1. Save the file and re-run the command:

    ```powershell
    dotnet run --extension bad --directory "D:\Az220\Allfiles\Labs\19-Azure Digital Twins\Final\Models"
    ```

    This time the validator reports that *Top-level element dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1 does not have @type of Array, Command, CommandPayload, Component, Enum, EnumValue, Field, Interface, Map, MapKey, MapValue, Object, Property, Relationship, or Telemetry. Provide a @type in the set of allowable types.*

1. Return to Visual Studio Code and locate line 4. Note that instead of **@type**, the property is name **@typo** - correct this.

1. Save the file and re-run the command:

    ```powershell
    dotnet run --extension bad --directory "D:\Az220\Allfiles\Labs\19-Azure Digital Twins\Final\Models"
    ```

    This time the validator reports 2 errors related to the **desiredTemperature** property - the **schema** is set to **byte** instead of **double**.

As you can see, the **DTDL Validator** can be very useful in identifying problems, however it must be run a number of times to identify all of the issues.

### Exercise 5 - Create a graph of the models

Now the interfaces have been defined for each of the digital twins that will be used in the proof-of-concept, it is time to construct the actual graph of digital twins. The flow for building a graph is straightforward:

* Import the model definitions
* Create twin instances from the appropriate models
* Create relationships between the twin instances using the defined model relationships

There are a number of ways that this flow can be achieved:

* Using Azure CLI commands from the command-line or in scripts
* Programmatically using one of the SDKs or directly via the REST APIs
* Using tools such as the ADT Explorer sample

As the ADT Explorer includes rich visualization of an ADT graph, it is well suited for building out the simple model for the proof-of-concept. However, larger, more complex, models are also supported and a comprehensive bulk import/export capability helps with iterative design. During this exercise the following tasks will be completed:

* Install ADT Explorer locally
* Connect it to the ADT instance
* Import the Contoso Cheese models
* Use the models to create digital twins
* Add relationships to the graph
* Learn how to use delete twins, relationships and models from ADT
* Bulk import a graph into ADT

#### Task 1 - Install ADT Explorer

The **ADT Explorer** is a sample application for the Azure Digital Twins service. The app connects to an Azure Digital Twins instance and provides the following features/capabilities:

* Upload and explore models
* Upload and edit graphs of twins
* Visualize the twins graph with a number of layout techniques
* Edit properties of twins
* Run queries against the twins graph

ADT explorer is written as a single-page JavaScript application. You can run it locally as a node.js application - see instructions below.

1. To verify **node.js** is installed, open a command shell and enter the following command:

    ```powershell
    node --version
    ```

    If node is installed, ensure that the version displayed is 10+. If not, [download and install node.js](https://nodejs.org/en/#home-downloadhead).

1. Open a browser and navigate to [Azure Digital Twins (ADT) explorer](https://docs.microsoft.com/samples/azure-samples/digital-twins-explorer/digital-twins-explorer/).

1. To download the application, click **Download ZIP**.

    The browser will download the **Azure_Digital_Twins__ADT__explorer.zip** file.

1. Extract the files from the **Azure_Digital_Twins__ADT__explorer.zip** to a location of your choice.

1. In a command shell, navigate to the location where the **Azure_Digital_Twins__ADT__explorer.zip** was extracted.

    This folder contains documentation as well as folders that contain the application.

1. Navigate to the **client\src** folder.

1. To restore the application dependencies, enter the following command:

    ```powershell
    npm install
    ```

    This will take a few moments.

1. In order to access the Azure Digital Twin instance, the user must be logged into Azure using the Azure CLI. To ensure the current user is logged in, enter the following command:

    ```powershell
    az login
    ```

    Login via the browser as usual.

1. Open the **adt-connection.txt** file (you saved it after creating the ADT instance earlier in this lab) and get a copy of the ADT URL.

    To connect the **ADT Explorer** to the ADT instance running in Azure, the ADT URL is required. It should be included in the **adt-connection.txt** file. However, if you don't have the file available you use the command line to determine the hostname by entering the following command:

    ```powershell
    az dt list --query "[].hostName" -o tsv
    ```

    > **NOTE**: Remember to prefix the **hostname** value with **https://** - e.g.:
    > ```http
    > https://adt-az220-training-dm030821.api.eus.digitaltwins.azure.net
    > ```

1. To launch the application, enter the following command:

    ```powershell
    npm run start
    ```

    Once started, a browser page should be opened addressing [http://localhost:3000](http://localhost:3000)

1. In the upper-right corner of the browser page, use the ADT URL icon to open the **Azure Digital Twins URL** dialog.

1. In the **Azure Digital Twins URL** dialog, under **ADT URL**, enter your ADT URL value.

1. Click **Save** to add the information to the browser app local storage and connect to the ADT instance.

    > **NOTE**: You might have to grant consent for the app if a **Permissions requested** pop-up appears.

The **ADT Explorer** sample application is now ready for use. Loading models is your next task, so don't be alarmed if you see an error message telling you that there are no models available.

#### Task 2 - Import Models

In order to create Digital Twins in ADT, it is necessary to first upload models. There are a number of ways that models can be uploaded:

* [Data Plane SDKs](https://docs.microsoft.com/azure/digital-twins/how-to-use-apis-sdks)
* [Data Plane REST APIs](https://docs.microsoft.com/rest/api/azure-digitaltwins/)
* [Azure CLI](https://docs.microsoft.com/cli/azure/ext/azure-iot/dt?view=azure-cli-latest)
* The import feature of the [ADT Explorer](https://docs.microsoft.com/samples/azure-samples/digital-twins-explorer/digital-twins-explorer/)

The first two options are more appropriate for programmatic scenarios, whereas the Azure CLI can be useful in **configuration as code** scenarios or "one-off" requirements. The **ADT Explorer** app provides an intuitive way to interact with ADT.

> **TIP**: What is **configuration as code**? As configuration is written as source code (for example, scripts containing Azure CLI commands), you can use all best development practices to optimise it, such as: creating reusable definitions of model uploads, parameterization, using loops to create lots of different instances of the models and so on. These scripts can then be stored in source code control to ensure they are retained, version controlled, etc.

In this task, you will use Azure CLI commands and the ADT Explorer sample app to upload the models included in the **Allfiles\Labs\19-Azure Digital Twins\Final\Models** folder.

1. Open a new command prompt window.

    You will need to leave the ADT-explorer running from the open command line tool.

1. To ensure that you using correct Azure account credentials, login to Azure using the following command:

    ```powershell
    az login
    ```

1. To upload the **Cheese Factory Interface**, enter the following command:

    ```powershell
    az dt model create --models "{file-root}\Allfiles\Labs\19-Azure Digital Twins\Final\Models\CheeseFactoryInterface.json" -n adt-az220-training-{your-id}
    ```

    Ensure you replace the **{file-root}** with the folder the companion files for this lab are located, and replace **{your-id}** with your unique identifier.

    If successful, output similar to the following will be displayed.

    ```json
    [
        {
            "decommissioned": false,
            "description": {},
            "displayName": {
            "en": "Cheese Factory - Interface Model"
            },
            "id": "dtmi:com:contoso:digital_factory:cheese_factory;1",
            "uploadTime": "2021-03-24T19:56:53.8723857+00:00"
        }
    ]
    ```

1. In a browser, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

    > **TIP**: If the application is already running, refresh the browser to refresh the **MODEL VIEW**.

    The uploaded **Cheese Factory - Interface Model** should be listed:

    ![ADT Explorer MODEL VIEW with Factory Model](media/LAB_AK_19-modelview-factory.png)

1. To import the remaining two models using the **ADT Explorer**, in the **MODEL VIEW**, click the **Upload a Model** icon

    ![ADT Explorer MODEL VIEW Upload a Model button](media/LAB_AK_19-modelview-addmodel.png)

1. In the **Open** dialog, navigate to the **Models** folder, select the **CheeseCaveInterface.json** and the **CheeseCaveDeviceInterface.json** files, and then click **Open**.

    The two files will then be uploaded to ADT and the models added. Once complete, the **MODEL VIEW** will update and list all three models.

Now that the models are uploaded, Digital Twins can be created.

#### Task 3 - Creating Twins

In an Azure Digital Twins solution, the entities in your environment are represented by digital twins. A digital twin is an instance of one of your custom-defined models. It can be connected to other digital twins via relationships to form a twin graph: this twin graph is the representation of your entire environment.

Similar to models, digital twins and relationships can be created in multiple ways:

* [Data Plane SDKs](https://docs.microsoft.com/azure/digital-twins/how-to-use-apis-sdks)
* [Data Plane REST APIs](https://docs.microsoft.com/rest/api/azure-digitaltwins/)
* [Azure CLI](https://docs.microsoft.com/cli/azure/ext/azure-iot/dt?view=azure-cli-latest)
* The import feature of the [ADT Explorer](https://docs.microsoft.com/samples/azure-samples/digital-twins-explorer/digital-twins-explorer/)

As before, the first two options are more appropriate for programmatic scenarios, whereas the Azure CLI remains useful in **configuration as code** scenarios or "one-off" requirements. The most intuitive way to create digital twins and relationships is via the **ADT Explorer**, however there are some limitations when it comes to initializing properties.

1. Open the command line window that you used to upload the CheeseFactoryInterface model.

1. To create a digital twin from the Cheese Factory model using the Azure CLI, enter the following command:

    ```powershell
    az dt twin create --dt-name adt-az220-training-{your-id} --dtmi "dtmi:com:contoso:digital_factory:cheese_factory;1" --twin-id factory_1 --properties "{file-root}\Allfiles\Labs\19-Azure Digital Twins\Final\Properties\FactoryProperties.json"
    ```

    Ensure you replace the **{file-root}** with the folder the companion files for this lab are located, and replace **{your-id}** with your unique identifier.

    Notice the following:

    * The **--dt-name** value specifies the ADT twin instance.
    * The **--dtmi** value specifies the Cheese Factory model uploaded earlier
    * The **--twin-id** specifies the ID given to the digital twin
    * The **--properties** value provides a file path for a JSON document that will be used to initialize the twin. Alternatively, simple JSON can also be specified in-line.

    If successful, the output of the command is similar to:

    ```json
    {
        "$dtId": "factory_1",
        "$etag": "W/\"09e781e5-c31f-4bf1-aed4-52a4472b0c5b\"",
        "$metadata": {
            "$model": "dtmi:com:contoso:digital_factory:cheese_factory;1",
            "FactoryName": {
                "lastUpdateTime": "2021-03-24T21:51:04.1371421Z"
            },
            "GeoLocation": {
                "lastUpdateTime": "2021-03-24T21:51:04.1371421Z"
            }
        },
        "FactoryName": "Contoso Cheese 1",
        "GeoLocation": {
            "Latitude": 47.64319985218156,
            "Longitude": -122.12449651580214
        }
    }
    ```

    Notice that the **$metadata** property contains an object that tracks the last time properties were updated.

1. The **FactoryProperties.json** file contains the following JSON:

    ```json
    {
        "FactoryName": "Contoso Cheese 1",
        "GeoLocation": {
            "Latitude": 47.64319985218156,
            "Longitude": -122.12449651580214
        }
    }
    ```

    The property names match the DTDL Property values declared in the Cheese Factory Interface.

    > **NOTE**: The complex property **GeoLocation** is assigned via a JSON object with **Latitude** and **Longitude** properties. Currently, the **ADT Explorer** cannot initialize these complex properties using the UI.

1. In a browser, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

1. To display the digital twins created so far, click **Run Query**.

    > **NOTE**: Queries and the Query Language will be discussed shortly.

    After a few moments, the **factory_1** digital twin should be displayed in the **GRAPH VIEW**.

    ![ADT Explorer GRAPH VIEW Factory 1](media/LAB_AK_19-graphview-factory_1.png)

1. To view the digital twin properties, in the **GRAPH VIEW**, click **factory_1**.

    The properties for **factory_1** are displayed in the **Property View** as nodes in a tree view.

1. To view the longitude and latitude property values, click **GeoLocation**.

    Notice that the values are consistent with those in the **FactoryProperties.json** file.

1. To create another digital twin from the Cheese Factory model, in the **MODEL VIEW**, locate the **Cheese Factory** model, and then click **Create a Twin**

    ![ADT Explorer MODEL VIEW Create a Twin button](media/LAB_AK_19-modelview-createtwin.png)

1. When prompted for the **New Twin Name** enter **factory_2** and then click **Save**.

1. To view the digital twin properties for **factory_2**, in the **GRAPH VIEW**, click **factory_2**.

    Notice that the **FactoryName** and **GeoLocation** properties are uninitialized.

1. To set the **factoryName**, position the mouse cursor to the right of the property - a textbox control should appear. Enter **Cheese Factory 2**.

    ![ADT Explorer Property View enter factory name](media/LAB_AK_19-propertyexplorer-factoryname.png)

1. In the Property Explorer pane, to save the update to the property, select the **Patch Twin** icon.

    > **Note**: The Patch Twin icon appears identical to the Save Query icon located to the right of the Run Query button. You don't want the Save Query icon.

    Selecting Patch Twin will result in a JSON Patch being created and sent to update the digital twin. The **Patch Information** will be displayed in a dialog. Notice that as this is the first time the value has been set, the **op** (operation) property is **add**. Subsequent changes to the value would be **replace** operations - to see this, click **Run Query** to refresh the **GRAPH VIEW** before making another update.

   > **TIP**: To learn more about a JSON Patch document, review the following resources:
   > * [Javascript Object Notation (JSON) Patch](https://tools.ietf.org/html/rfc6902)
   > * [What is JSON Patch?](http://jsonpatch.com/)

1. In the **PROPERTY EXPLORER**, expand the **GeoLocation** node - notice the value is **{empty object}**

    Currently, the **ADT Explorer** is unable to add a complex object to an uninitialized property.

1. Add the following digital twins by selecting the appropriate model in **MODEL VIEW** and clicking **Add a Twin**:

    | Model Name                             | Digital Twin Name |
    | :------------------------------------- | :---------------- |
    | Cheese Cave - Interface Model        | cave_1          |
    | Cheese Cave - Interface Model        | cave_2          |
    | Cheese Cave Device - Interface Model | device_1          |
    | Cheese Cave Device - Interface Model | device_2          |

    ![ADT Explorer graph view displaying created twins](media/LAB_AK_19-graphview-createdtwins.png)

Now that some twins have been created, it is time to add some relationships.

#### Task 4 - Adding relationships

Twins are connected into a twin graph by their relationships. The relationships that a twin can have are defined as part of its model.

For example, the **Cheese Factory** model defines a "contains" relationship that targets twins of type **Cheese Cave**. With this definition, Azure Digital Twins will allow you to create **rel_has_caves** relationships from any **Cheese Factory** twin to any **Cheese Cave** twin (including any twins that may be subtypes of a **Cheese Cave** - for example a specialized **Cheese Cave** for a specific cheese).

The result of this process is a set of nodes (the digital twins) connected via edges (their relationships) in a graph.

Similar to Models and Twins, relationships can be created in multiple ways.

1. To create a relationship via the Azure CLI, return to the command prompt and execute the following command:

    ```powershell
    az dt twin relationship create -n adt-az220-training-{your-id} --relationship-id factory_1_has_cave_1 --relationship rel_has_caves --twin-id factory_1 --target cave_1
    ```

    Ensure you replace the **{file-root}** with the folder the companion files for this lab are located, and replace **{your-id}** with your unique identifier.

    If successful, the output of the command is similar to:

    ```json
    {
        "$etag": "W/\"cdb10516-36e7-4ec3-a154-c050afed3800\"",
        "$relationshipId": "factory_1_has_cave_1",
        "$relationshipName": "rel_has_caves",
        "$sourceId": "factory_1",
        "$targetId": "cave_1"
    }
    ```

1. To visualize the relationship, in a browser, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

1. To display the updated digital twins, click **Run Query**.

    The diagram will refresh and the new relationship will be displayed.

    ![ADT Explorer graph view with a relationship](media/LAB_AK_19-graphview-relationship.png)

    If you don't see the relationship, refresh the browser window and then run the query.

1. To add a relationship using the **ADT Explorer**, click **cave_1** and then hold the **Ctrl** key and click **device_1**.

    Both twins should now be selected and the **Add Relationship** button will be enabled.

1. To add a relationship, click the **Add Relationship** button:

    ![ADT Explorer graph view add relationship button](media/LAB_AK_19-graphview-addrelationship.png)

1. In the **Create Relationship** dialog, under **Source ID**, confirm that **cave_1** is displayed.

1. Under **Target ID**, confirm that **device_1** is displayed.

1. Under **Relationship**, confirm that **rel_has_devices** is displayed.

    > **NOTE**: Unlike relationships created with the Azure CLI, there is no UI to supply a **$relationshipId** value. Instead, a GUID will be assigned.

1. To Create the relationship, click **Save**.

    The relationship will be created and the diagram will update to display the relationship. The diagram now shows that **factory_1** has **cave_1**, which has **device_1**.

1. Add two more relationships:

    | Source    | Target   | Relationship    |
    | :-------- | :------- | :-------------- |
    | factory_1 | cave_2 | rel_has_caves |
    | cave_2  | device_2 | rel_has_devices |

    The graph should now appear similar to:

    ![ADT Explorer graph view with updated graph](media/LAB_AK_19-graphview-updatedgraph.png)

1. To view the layout options for the **GRAPH VIEW**, click the dropdown to the right of the **Run Layout** button.

    The **GRAPH VIEW** can use different algorithms to layout the graph. The **Klay** layout is selected by default. You can try selecting different layouts to see how the graph is impacted.

#### Task 5 - Deleting models, relationships and Twins

During the design process for modeling with ADT, it is likely that a number of proof-of-concepts will be created, many of which will be deleted. Similar to the other operations on digital twins, there are programmatic approaches (API, SDK, and CLI) to deleting models and twins, and you can also use the **ADT Explorer**.

> **NOTE**: One thing to note is that the delete operations are asynchronous and although, for example, a REST API call or a delete in **ADT Explorer** may appear to complete instantly, it may take few minutes for the operation to complete within the ADT service. Attempting to upload revised models with the same name as recently deleted models may fail unexpectedly until the back-end operations have completed.

1. To delete the **factory_2** digital twin via the CLI, return to your command prompt window, and then enter the following command:

    ```powershell
    az dt twin delete -n adt-az220-training-{your-id} --twin-id factory_2
    ```

    Unlike other commands, no output is displayed (unless the command produces an error).

1. To delete the relationship between **factory_1** and **cave_1**, enter the following command:

    ```powershell
    az dt twin relationship delete -n adt-az220-training-{your-id} --twin-id factory_1 --relationship-id factory_1_has_cave_1
    ```

    Notice that this command requires the relationship ID. You can view the relationship IDs for a given twin. For example, to view the relationship IDs for **factory_1**, you can enter the following command:

    ```powershell
    az dt twin relationship list -n adt-az220-training-{your-id} --twin-id factory_1
    ```

    If you ran this command prior to deleting the relationship to cave 1, you would see output similar to the following:

    ```json
    [
        {
            "$etag": "W/\"a6a9f506-3cfa-4b62-bcf8-c51b5ecc6f6d\"",
            "$relationshipId": "47b0754a-25d1-4b71-ac47-c2409bb08535",
            "$relationshipName": "rel_has_caves",
            "$sourceId": "factory_1",
            "$targetId": "cave_2"
        },
        {
            "$etag": "W/\"b5207e88-7c86-498f-a272-7f81dde88dee\"",
            "$relationshipId": "factory_1_has_cave_1",
            "$relationshipName": "rel_has_caves",
            "$sourceId": "factory_1",
            "$targetId": "cave_1"
        }
    ]
    ```

1. To delete a model, enter the following command:

    ```powershell
    az dt model delete -n adt-az220-training-{your-id} --dtmi "dtmi:com:contoso:digital_factory:cheese_factory;1"
    ```

    Again no output is displayed.

    > **IMPORTANT**: This command deleted the factory model and succeeded, even though a digital twin **factory_1** still exists. Digital twins that were created using the deleted model can still be found by querying the graph, however, properties of the twin can no longer be updated without the model. Be very careful when completing model management tasks (versioning, deleting, etc.) to avoid creating inconsistent graphs.

1. To display the recent changes to the digital twins, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

1. To update the display, refresh the browser page and then click **Run Query**.

    The **Cheese Factory** model should be missing from the **MODEL VIEW** and there should be no relationship between **factory_1** and **cave_1** in the **GRAPH VIEW**.

1. To select the relationship between **cave_1** and **device_1**, click the line between the two twins.

    The line should thicken indicating it is selected and the **Delete Relationship** button will be enabled.

    ![ADT Explorer graph view delete relationship](media/LAB_AK_19-graphview-deleterel.png)

1. To delete the relationship, click **Delete Relationship**, and confirm by clicking **Delete**.

    The relationship will be deleted and the graph will update.

1. To select the **device_1** digital twin for deletion, click **device_1**.

    The **Delete Selected Twins** button will be enabled.

    ![ADT Explorer graph view delete twin](media/LAB_AK_19-graphview-deletetwin.png)

    > **NOTE**: By using **CTRL**, multiple twins can be selected and deleted.

1. To delete **device_1**, click **Delete Selected Twins**, and confirm by clicking **Delete**.

    The twin will be deleted and the graph will update.

1. In the upper-right corner of the ADT Explorer page, to delete all of the digital twins in a graph, click **Delete All Twins**, and confirm by clicking **Delete**.

    ![ADT Explorer delete all twins](media/LAB_AK_19-deletealltwins.png)

    > **IMPORTANT**: Use with care - there is no undo this capability!

    > **NOTE**: After deleting all of the twins, the **MODEL VIEW** may also appear empty - the models **have not** been deleted. Refresh the browser and the models will re-appear.

1. To delete the **Cheese Cave Device** model from the **MODEL VIEW**, click the associated **Delete Model** button, and confirm by clicking **Delete**.

1. To delete all models, click **Delete All Models** at the top of the **MODEL VIEW**.

    > **IMPORTANT**: Use with care - there is no undo capability!

At this point, the ADT instance should be clear of all models, twins and relationships. Don't worry - in the next task the **Import Graph** feature will be used to create a new graph.

#### Task 6 - Bulk import with ADT Explorer

The **ADT Explorer** supports the import and export of a digital twin graph. The **Export** capability serializes the most recent query results to a JSON-based format, including models, twins, and relationships. The **Import** capability deserializes from either a custom Excel-based format or the JSON-based format generated on export. Before import is executed, a preview of the graph is presented for validation.

The excel import format is based on the following columns:

* **ModelId**: The complete dtmi for the model that should be instantiated.
* **ID**: The unique ID for the twin to be created
* **Relationship**: A twin id with an outgoing relationship to the new twin
* **Relationship name**: The name for the outgoing relationship from the twin in the previous column
* **Init data**: A JSON string that contains **Property** settings for the twins to be created

> **NOTE**: The Excel import capability **does not** import model definitions, only twins and relationships. The JSON format supports models as well.

The following table shows the twins and relationships that will be created in this task (the **Init Data** values are removed for readability):

| ModelID                                                                | ID             | Relationship (From) | Relationship Name | Init Data |
| :--------------------------------------------------------------------- | :------------- | :------------------ | :---------------- | :-------- |
| dtmi:com:contoso:digital_factory:cheese_factory;1                      | factory_1      |                     |                   |           |
| dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1        | cave_1       | factory_1           | rel_has_caves   |           |
| dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1        | cave_2       | factory_1           | rel_has_caves   |           |
| dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1        | cave_3       | factory_1           | rel_has_caves   |           |
| dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1 | sensor-th-0055 | cave_1            | rel_has_devices   |           |
| dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1 | sensor-th-0056 | cave_2            | rel_has_devices   |           |
| dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1 | sensor-th-0057 | cave_3            | rel_has_devices   |           |

The spreadsheet **cheese-factory-scenario.xlsx** can be found in the **{file-root}\Allfiles\Labs\19-Azure Digital Twins\Final\Models** folder.

1. In a browser, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

1. To import your models using the **ADT Explorer**, in the **MODEL VIEW**, click the **Upload a Model** icon

1. In the **Open** dialog, navigate to the **Models** folder, select the **CheeseFactoryInterface.json**, **CheeseCaveInterface.json**, and **CheeseCaveDeviceInterface.json** files, and then click **Open**.

    This will reload all of the models.

1. To import the **cheese-factory-scenario.xlsx** spreadsheet, click **Import Graph**.

    ![ADT Explorer graph view import graph](media/LAB_AK_19-graphview-importgraph.png)

1. In the **Open** dialog, navigate to the **Models** folder and select the **cheese-factory-scenario.xlsx** file, then click **Open**.

    A preview of the graph to be imported is displayed in an **Import** view:

    ![ADT Explorer graph view import preview](media/LAB_AK_19-graphview-importpreview.png)

1. To complete the import, click **Start Import**.

    An **Import Successful** dialog will be displayed, detailing that 7 twins and 6 relationships were imported. Click **Close** to proceed.

    The **Import** pane closes and the **GRAPH VIEW** is displayed,

1. To refresh the **GRAPH VIEW**, click **Run Query**.

    The imported graph should now be displayed. You can click on each twin to view the properties (each twin has been initialized with values).

1. To export the current graph as JSON, click **Export Graph** (next to the **Import Graph** button used earlier).

    The **Export** view is displayed with a **Download** link in the top-left corner.

1. To download the model in JSON, click the **Download** link.

    The browser will download the model.

1. To view the JSON, open the downloaded file in Visual Studio Code.

    If the JSON is shown as a single line, reformat the JSON using the **Format Document** command via the command palette or by pressing **SHIFT+ALT+F**.

    The JSON has three primary sections:

    * **digitalTwinsFileInfo** - contains the version of the exported file format
    * **digitalTwinsGraph** - contains the instance data for every twin and relationship that was displayed on the exported graph (i.e. only those displayed based upon the query)
    * **digitalTwinsModels** - the model definitions

    > **NOTE**: Unlike the Excel format, the JSON file includes the model definitions, meaning that everything can be imported with just the one file.

1. To import the JSON file, use **ADT Explorer** to delete the models and twins following the instructions in earlier tasks, and then import the JSON export file that was just created. Note that the models, twins and their properties, and the relationships are recreated.

This twin graph will be used as the basis for the exercise on querying.

### Exercise 6 - Query the graph using ADT Explorer

>**NOTE**: This exercise requires the graph imported in Exercise 5.

Now let's review the digital twin graph query language.

You can query the digital twin graph that you just built, to get information about the digital twins and relationships it contains. You write these queries in a custom, SQL-like query language, referred to as the Azure Digital Twins query language. This language is also similar to the query language for Azure IoT Hub.

Queries can be made through the Digital Twins REST API and with the SDKs. In this exercise, you'll be using the Azure Digital Twins explorer sample app to handle the API calls for you. Additional tools will be explored later in this lab.

> **NOTE**: The **ADT Explorer** is  designed to visualize a graph and can only display entire twins, rather than just single values selected from a twin, such as the name.

#### Task 1 - Query using the ADT Explorer

In this task, the ADT Explorer will be used to execute graph queries and render the results as a graph. Twins can be queried by properties, model type and by relationships. Queries can be combined into compound queries using combination operators that can query for more than one type of twin descriptor at a a time.

1. In a browser, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

1. Ensure the **QUERY EXPLORER** query is set to the following:

    ```sql
    SELECT * FROM digitaltwins
    ```

    If you are at all familiar with SQL, you will expect that this will return everything from digital twins.

1. To run this query, click **Run Query**.

    As expected, the entire graph is displayed.

1. To save this as a named query, click on the **Save** icon (just to the right of the **Run Query** button).

1. In the **Save Query** dialog, enter the name **All Twins** and click **Save**.

    This query will then be saved locally and be available in the **Saved Queries** drop down to the left of the query text box. To delete a saved query, click on the **X** icon next to the name of the query when the **Saved Queries** drop down is open.

    > **TIP**: Run the **All Twins** query to return to the full view at any time.

1. To filter the graph so that only **Cheese Cave** twins are displayed, enter and run the following query:

    ```sql
    SELECT * FROM digitaltwins
    WHERE IS_OF_MODEL('dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1')
    ```

    The graph will now display just the 3 **Cheese Cave** twins.

    Save this query as **Just Caves**.

1. To display only the **Cheese Cave** twins that are **inUse**, enter and run the following query:

    ```sql
    SELECT * FROM digitaltwins
    WHERE IS_OF_MODEL('dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1')
    AND inUse = true
    ```

    The graph should now display just **cave_3** and **cave_1**.

1. To display only the **Cheese Cave** twins that are **inUse** and have a **temperatureAlert**, enter and run the following query:

    ```sql
    SELECT * FROM digitaltwins
    WHERE IS_OF_MODEL('dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1')
    AND inUse = true
    AND temperatureAlert = true
    ```

    The graph should now display just **cave_3**.

1. To use relationships to find the parent of the device **sensor-th-0055** via a join, enter the following query:

    ```sql
    SELECT Parent FROM digitaltwins Parent
    JOIN Child RELATED Parent.rel_has_devices
    WHERE Child.$dtId = 'sensor-th-0055'
    ```

    The **cave_1** twin should be displayed.

    For those familiar with the SQL JOIN, the syntax used here will look different from what you might be used to. Notice that the name of the relationship **rel_has_devices** is specified, rather than correlating this JOIN with a key value in a WHERE clause or specifying a key value inline with the JOIN definition. This correlation is computed automatically, as the relationship properties themselves identify the target entity. Here is the relationship definition:

    ```json
    {
        "@type": "Relationship",
        "@id": "dtmi:com:contoso:digital_factory:cheese_cave:rel_has_devices;1",
        "name": "rel_has_devices",
        "displayName": "Has devices",
        "target": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1"
    }
    ```

#### Task 2 - Query for properties using the ADT Explorer

A key limitation of the **ADT Explorer** is is that it is designed to render a graph and the primary display cannot the results for queries that return just properties. In this task, you will learn how it is possible to see the results of such queries without resorting to coding solutions.

1. To run a valid query that returns just a property, enter the following query:

    ```sql
    SELECT Parent.desiredTemperature FROM digitaltwins Parent
    JOIN Child RELATED Parent.rel_has_devices
    WHERE Child.$dtId = 'sensor-th-0055'
    AND IS_PRIMITIVE(Parent.desiredTemperature)
    ```

    Despite the fact that the query will run without error, no graph is displayed. However, there is a way to view the results in **ADT Explorer**, and you will open the **Output** pane to view the query results in the next task.

    > **IMPORTANT**: Notice the use of the **IS_PRIMITIVE** function in the query above. Complex properties are not supported in ADT queries (an example of a complex property would be the **GeoLocation** property on the **Cheese Factory**). To ensure that projection properties are valid, ADT queries require the inclusion of an IS_PRIMITIVE check. In this case, the `IS_PRIMITIVE(Parent.desiredTemperature)` function returns true and confirms that the **Parent.desiredTemperature** property is a primitive. Omitting this check will result in a an error and a failed query.

1. To open the **Output** pane, click the **Settings** icon at the top-right of the page.

1. On the dialog that appears, under **View**, enable **Output**, and then close the dialog.

    The **Output** pane should appear at the bottom of the page.

1. Rerun the query above and review the contents of the **Output** pane.

    The OUTPUT pane should display the **Requested query**, and then show the returned JSON. The JSON should be similar to the following:

    ```json
    {
        "queryCharge": 20.259999999999998,
        "connection": "close",
        "content-encoding": "gzip",
        "content-type": "application/json; charset=utf-8",
        "date": "Thu, 25 Mar 2021 21:34:40 GMT",
        "strict-transport-security": "max-age=2592000",
        "traceresponse": "00-182f5e54efb95c4b8b3e2a6aac15499f-9c5ffe6b8299584e-01",
        "transfer-encoding": "chunked",
        "vary": "Accept-Encoding",
        "x-powered-by": "Express",
        "value": [
            {
            "desiredTemperature": 50
            }
        ],
        "continuationToken": null
    }
    ```

    Along with additional result metadata, notice that the **value** property contains the selected **desiredTemperature** property and value.

### Exercise 7 - Configure and launch device simulator

In the preceding exercises, the digital twin model and graph for the proof-of-concept were created. In order to demonstrate how to route device message traffic from IoT Hub to ADT, it is useful to use a device simulator. In this exercise, you will be configuring the simulated device app that was developed during LAB 15 to send telemetry to your IoT Hub.

#### Task 1: Open the device simulator project

In this task, the Cheese Cave Device simulator app will be opened in Visual Studio Code in preparation for configuration.

1. Open **Visual Studio Code**.

1. On the **File** menu, click **Open Folder**

1. In the Open Folder dialog, navigate to the lab 19 Starter folder.

    In _Lab 3: Setup the Development Environment_, you cloned the GitHub repository containing lab resources by downloading a ZIP file and extracting the contents locally. The extracted folder structure includes the following folder path:

    * Allfiles
        * Labs
            * 19-Azure Digital Twins
                * Starter
                    * cheesecavedevice

1. Click **cheesecavedevice**, and then click **Select Folder**.

    You should see the following files listed in the EXPLORER pane of Visual Studio Code:

    * cheesecavedevice.csproj
    * Program.cs

1. To open the code file, click **Program.cs**.

    A cursory glance will reveal that this application is very similar to the simulated device applications that you have worked on in the preceding labs. This version uses symmetric Key authentication, sends both telemetry and logging messages to the IoT Hub, and has a more complex sensor implementation.

1. On the **Terminal** menu, click **New Terminal**.

    Notice the directory path indicated as part of the command prompt. You do not want to start building this project within the folder structure of a previous lab project.

1. At the terminal command prompt, to verify the application builds, enter the following command:

    ```bash
    dotnet build
    ```

    The output will be similar to:

    ```text
    > dotnet build
    Microsoft (R) Build Engine version 16.5.0+d4cbfca49 for .NET Core
    Copyright (C) Microsoft Corporation. All rights reserved.

    Restore completed in 39.27 ms for D:\Az220\AllFiles\Labs\19-Azure Digital Twins\Starter\CheeseCaveDevice\CheeseCaveDevice.csproj.
    CheeseCaveDevice -> D:\Az220\AllFiles\Labs\19-Azure Digital Twins\Starter\CheeseCaveDevice\bin\Debug\netcoreapp3.1\CheeseCaveDevice.dll

    Build succeeded.
        0 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:01.16
    ```

In the next task, you will configure the connection string and review the application.

#### Task 2: Configure connection and review code

The simulated device app that you will build in this task simulates an IoT device that monitors temperature and humidity. The app will simulate sensor readings and communicate sensor data every two seconds and is the same app that was built in Lab 15.

1. In **Visual Studio Code**, ensure that the Program.cs file is open.

1. In the code editor, locate the following line of code:

    ```csharp
    private readonly static string deviceConnectionString = "<your device connection string>";
    ```

1. Replace the **\<your device connection string\>** with the device connection string that you saved near the end of the lab setup exercise.

    This is the only change that you need to implement before sending telemetry to the IoT Hub.

    > **NOTE**: You saved both a Device and Service connection string. Be sure to provide the Device connection string.

1. On the **File** menu, click **Save**.

#### Task 3: Test your Code to Send Telemetry

In this task, the configured simulator app is launched and the the successful transmission of telemetry is verified.

1. In Visual Studio Code, ensure that you still have the Terminal open.

1. At the Terminal command prompt, to run the simulated device app, enter the following command:

    ```bash
    dotnet run
    ```

   This command will run the **Program.cs** file in the current folder.

1. Notice the output being sent to the Terminal.

    You should quickly see console output, similar to the following:

    ![Console Output](media/LAB_AK_19-cheesecave-telemetry.png)

    > **Note**:  Green text is used to indicate when things are working as they should be. Red text is used to indicate when there is a problem. If you don't get a screen similar to the image above, start by checking your device connection string.

1. Leave this app running.

    You need to be sending telemetry to IoT Hub later in this lab.

### Exercise 8 - Set up Azure Function to ingest data

A key part of the proof-of-concept is to demonstrate how data from a device can be delivered to Azure Digital Twins. Data can be ingested into Azure Digital Twins through external compute resources such as Virtual Machines, Azure Functions, and Logic Apps. In this exercise, a function app will be invoked by an IoT Hub's built-in Event Grid. The function app receives the data and uses the Azure Digital Twins APIs to set properties on the appropriate digital twin instance.

#### Task 1 - Create and configure a function app

In order to configure an IoT Hub event grid endpoint to route telemetry to an Azure Function, it is necessary to first create the Azure Function. In this task, an Azure Function App is created that provides the execution context in which individual Azure Functions run.

In order to access Azure Digital Twins and it's APIs, it is necessary to utilize a service principal with the appropriate permissions. During this task,.a service principal is created for the function app and then assigned the appropriate permission. Once the function app has the appropriate permission, any Azure Functions that execute within the function app context will use that service principal and will therefore have permission to access ADT.

The function app context also provides an environment for managing app settings for one or more functions. This capability will be used to define a setting that contains the ADT connection string which can then be read by the Azure Functions. Encapsulating connection strings and other configurations values in app settings is considered a much better practice than hard-coding the values in the function code.

1. Open the browser window containing your Azure portal, and then open the Azure Cloud Shell.

1. At the Cloud Shell command prompt, to create an Azure Function App, enter the following command:

    ```bash
    az functionapp create --resource-group rg-az220 --consumption-plan-location {your-location} --name func-az220-hub2adt-training-{your-id} --storage-account staz220training{your-id} --functions-version 3
    ```

    > **Note**: Remember to replace the **{your-location}** and **{your-id}** tokens above.

    The Azure function requires that a bearer token to be passed to it in order to authenticate with Azure Digital Twins. To make sure that this token is passed, you'll need to create a managed identity for the function app.

1. To create (assign) the system-managed identity for the function app and display the associated principal Id, enter the following command:

    ```bash
    az functionapp identity assign -g rg-az220 -n func-az220-hub2adt-training-{your-id} --query principalId -o tsv
    ```

    > **Note**: Remember to replace the **{your-id}** token above.

    The output will be similar to following:

    ```bash
    1179da2d-cc37-48bb-84b3-544cbb02d194
    ```

    This is the principal ID that was assigned to the function app - you will need the principal ID in the next step.

1. To assign the **Azure Digital Twins Data Owner** role to the Function App principal, enter the following command:

    ```bash
    az dt role-assignment create --dt-name adt-az220-training-{your-id} --assignee {principal-id} --role "Azure Digital Twins Data Owner"
    ```

    > **Note**: Remember to replace the **{your-id}** and **{principal-id}** tokens above. The **{principal-id}** value was displayed as the output of the previous step.

    Now that the principal has been assigned to the Azure Function App, that principal must be assigned the **Azure Digital Twins Data Owner** role so that is can access the Azure Digital Twins instance.

1. In order to supply the Azure Digital Twin instance URL to the Azure Function App as an environment variable, enter the following command:

    ```bash
    az functionapp config appsettings set -g rg-az220 -n func-az220-hub2adt-training-{your-id} --settings "ADT_SERVICE_URL={adt-url}"
    ```

    > **Note**: Remember to replace the **{your-id}** and **{adt-url}** tokens above. The **{adt-url}** value was saved to the **adt-connection.txt** file in an earlier task and will be similar to `https://adt-az220-training-dm030821.api.eus.digitaltwins.azure.net`.

    Once complete, the command lists all of the available settings. The Azure Function will now be able to obtain the ADT service URL by reading the **ADT_SERVICE_URL** value.

#### Task 2 - Create an Azure function project in Visual Studio Code

In this task, you will use Visual Studio Code to create a local Azure Functions project that will use the Function App created above. The project will be configured to use C# and the initial function will be configured to be triggered by an Event Grid event. Later in this exercise, you'll publish your function code to the Azure Function App that you created above.

1. Open **Visual Studio Code**.

1. Select the **Azure** icon in the Activity bar, and then, in the **Azure: Functions** area, select the **Create New Project** icon.

    ![Create a new Azure Function Project](media/LAB_AK_19-create-new-project.png)

1. Create a new folder, name it **Contoso.AdtFunctions**, select the folder, and then click **Select**.

    > **Note**: This directory should be new, empty, and unique for this Azure Functions project.

1. Provide the following information at the prompts:

   * **Select a language for your function project**: Select **C#**.
   * If prompted to **Select a .NET runtime**, select **.NET Core 3**.
   * **Select a template for your project's first function**: Select **Change template filter**.
   * **Select a template filter**: Select **All**.
   * **Select a template for your project's first function**: Select **EventGridTrigger**.
   * **Provide a function name**: Type **HubToAdtFunction**.
   * **Provide a namespace**: Type **Contoso.AdtFunctions**.
   * **When prompted for a storage account choose**: click **Skip for now**.
   * **Select how you would like to open your project**: Select **Add to workspace**.

    Visual Studio Code will then create the project and open the selected folder.

#### Task 3 - Add NuGet packages

The Azure Function will make use of various NuGet packages to interact with ADT and Azure Identity. In this task you will add them to the project.

1. In Visual Studio Code, in the **View** menu, select **Terminal**.

    The **Terminal** pane will open.

1. In the **Terminal** pane, to add the required NuGet packages, enter the following commands:

    ```powershell
    dotnet add package Azure.DigitalTwins.Core
    dotnet add package Azure.identity
    dotnet add package System.Net.Http
    ```

    * The **Azure.DigitalTwins.Core** package contains the SDK for the Azure Digital Twins service. This library provides access to the Azure Digital Twins service for managing twins, models, relationships, etc.
    * The **Azure.identity** package contains the implementation of the Azure SDK Client Library for Azure Identity. The Azure Identity library provides Azure Active Directory token authentication support across the Azure SDK. It provides a set of TokenCredential implementations which can be used to construct Azure SDK clients which support AAD token authentication
    * The **System.Net.Http** package provides a programming interface for modern HTTP applications, including HTTP client components that allow applications to consume web services over HTTP and HTTP components that can be used by both clients and servers for parsing HTTP headers.

#### Task 4 - Write an Azure function with an Event Grid trigger

In this task you will develop the Azure Function that will be executed whenever an event occurs on the associated Event Grid. The event will be processed and the message and telemetry will be routed to ADT.

1. In Visual Studio Code, open the **HubToAdtFunction.cs** file.

1. Replace the contents of the file with the following code:

    ```csharp
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.EventGrid.Models;
    using Microsoft.Azure.WebJobs.Extensions.EventGrid;
    using Microsoft.Extensions.Logging;
    using Azure.DigitalTwins.Core;
    using Azure.Identity;
    using System.Net.Http;
    using Azure.Core.Pipeline;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Text;

    namespace Contoso.AdtFunctions
    {
        public static class HubToAdtFunction
        {
            // INSERT member variables below here

            // INSERT Run method below here
        }
    }
    ```

1. To add the member variables for the function, locate the `// INSERT member variables below here` comment and insert the follow code below it:

    ```csharp
    //Your Digital Twins URL is stored in an application setting in Azure Functions.
    private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
    private static readonly HttpClient httpClient = new HttpClient();
    ```

    Notice that the **adtInstanceUrl** variable is assigned the value of the **ADT_SERVICE_URL** environment variable defined earlier in the exercise. The code also follows a best practice of using a single, static, instance of the **HttpClient**.

1. To add the **Run** method declaration, locate the `// INSERT Run method below here` comment and insert the following code below it:

    ```csharp
    [FunctionName("HubToAdtFunction")]
    public async static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
    {
        // INSERT log statement below here

        // INSERT environment variable check below here

        // INSERT try/catch block below here
    }
    ```

    Notice the use of the **FunctionName** attribute to mark the **Run** method as the entry point **Run** for **HubToAdtFunction**. The method is also declared `async` as the code to update the Azure Digital Twin runs asynchronously.

    The **eventGridEvent** parameter is assigned the Event Grid event that triggered the function call and the **log** parameter provides access to a logger that can be used for debugging.

    > **TIP**: To learn more about the Azure Event Grid trigger for Azure Functions, review the resource below:
    > * [Azure Event Grid trigger for Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-bindings-event-grid-trigger?tabs=csharp%2Cbash)

1. To log informational data concerning the Event Grid event data, locate the `// INSERT log statement below here` comment and insert the following code below it:

    ```csharp
    log.LogInformation(eventGridEvent.Data.ToString());
    ```

    The **ILogger** interface is defined in the **Microsoft.Extensions.Logging** namespace and aggregates most logging patterns to a single method call. In this case, a log entry is created at the **Information** level - other methods exists for various levels including critical, error. etc. As the Azure Function is running in the cloud, logging is essential during development and production.

    > **TIP:** To learn more about the **Microsoft.Extensions.Logging** capability, review the following resources:
    > * [Logging in .NET](https://docs.microsoft.com/dotnet/core/extensions/logging?tabs=command-line)
    > * [Microsoft.Extensions.Logging namespace](https://docs.microsoft.com/dotnet/api/microsoft.extensions.logging?view=dotnet-plat-ext-5.0&viewFallbackFrom=netcore-3.1)
    > * [ILogger Interface](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger?view=dotnet-plat-ext-5.0&viewFallbackFrom=netcore-3.1)

1. To check that the **ADT_SERVICE_URL** environment variable has been defined, locate the `// INSERT environment variable check below here` comment and insert the following code below it:

    ```csharp
    if (adtInstanceUrl == null)
    {
        log.LogError("Application setting \"ADT_SERVICE_URL\" not set");
        return;
    }
    ```

    This code checks if the **adtInstanceUrl** environment variable has been set - if not, the error is logged and the function exits. This demonstrates the value of logging to capture the fact that the function has been incorrectly configured.

1. To ensure any exceptions are logged, locate the `// INSERT try/catch block below here` and insert the following code below it:

    ```csharp
    try
    {
        // INSERT authentication code below here

        // INSERT event processing code below here
    }
    catch (Exception e)
    {
        log.LogError(e.Message);
    }
    ```

    Notice that the exception message is logged.

1. To use the function app principal to authenticate to ADT and create a client instance, locate the `// INSERT authentication code below here` comment and insert the following code below it:

    ```csharp
    ManagedIdentityCredential cred = new ManagedIdentityCredential("https://digitaltwins.azure.net");
    DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
    log.LogInformation($"Azure digital twins service client connection created.");
    ```

    Notice the use of the **ManagedIdentityCredential** class. This class attempts authentication using the managed identity that has been assigned to the deployment environment earlier. Once the credential is returned, it is used to construct an instance of the **DigitalTwinsClient**. The client contains methods to retrieve and update digital twin information, like models, components, properties and relationships.

1. To start processing the Event Grid event, locate the `// INSERT event processing code below here` comment and insert the following code below it:

    ```csharp
    if (eventGridEvent != null && eventGridEvent.Data != null)
    {
        // Read deviceId and temperature for IoT Hub JSON.
        JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
        string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
        var fanAlert = (bool)deviceMessage["properties"]["fanAlert"]; // cast directly to a bool
        var temperatureAlert = deviceMessage["properties"].SelectToken("temperatureAlert") ?? false; // JToken object
        var humidityAlert = deviceMessage["properties"].SelectToken("humidityAlert") ?? false; // JToken object
        log.LogInformation($"Device:{deviceId} fanAlert is:{fanAlert}");
        log.LogInformation($"Device:{deviceId} temperatureAlert is:{temperatureAlert}");
        log.LogInformation($"Device:{deviceId} humidityAlert is:{humidityAlert}");

        var bodyJson = Encoding.ASCII.GetString((byte[])deviceMessage["body"]);
        JObject body = (JObject)JsonConvert.DeserializeObject(bodyJson);
        log.LogInformation($"Device:{deviceId} Temperature is:{body["temperature"]}");
        log.LogInformation($"Device:{deviceId} Humidity is:{body["humidity"]}");
        // INSERT ADT update code below here
    }
    ```

    Notice the use of JSON deserialization to access the event data. The event data JSON will be similar to:

    ```JSON
    {
        "properties": {
            "sensorID": "S1",
            "fanAlert": "false",
            "temperatureAlert": "true",
            "humidityAlert": "true"
        },
        "systemProperties": {
            "iothub-connection-device-id": "sensor-th-0055",
            "iothub-connection-auth-method": "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}",
            "iothub-connection-auth-generation-id": "637508617957275763",
            "iothub-enqueuedtime": "2021-03-11T03:27:21.866Z",
            "iothub-message-source": "Telemetry"
        },
        "body": "eyJ0ZW1wZXJhdHVyZSI6OTMuOTEsImh1bWlkaXR5Ijo5OC4wMn0="
    }
    ```

    The message **properties** and **systemProperties** are easily accessible using an indexer approach, however where properties are optional, such as **temperatureAlert** and **humidityAlert**, the use of `SelectToken` and a null-coalescing operation is required to prevent an exception being thrown.

    > **TIP**: To learn more about the null-coalescing operator `??`, review the following content:
    > * [?? and ??= operators (C# reference)](https://docs.microsoft.com/dotnet/csharp/language-reference/operators/null-coalescing-operator)

    The message **body** contains the telemetry payload and is ASCII encoded JSON. Therefore, it must first be decoded and then deserialized before the telemetry properties can be accessed.

    > **TIP**: To learn more about the event schema, review the following resource:
    > * [Event schema](https://docs.microsoft.com/azure/azure-functions/functions-bindings-event-grid-trigger?tabs=csharp%2Cbash#event-schema)

1. To add the code that updates the ADT twin, locate the `// INSERT ADT update code below here` comment and insert the following code below it:

    ```csharp
    //Update twin
    var patch = new Azure.JsonPatchDocument();
    patch.AppendReplace<bool>("/fanAlert", fanAlert); // already a bool
    patch.AppendReplace<bool>("/temperatureAlert", temperatureAlert.Value<bool>()); // convert the JToken value to bool
    patch.AppendReplace<bool>("/humidityAlert", humidityAlert.Value<bool>()); // convert the JToken value to bool

    await client.UpdateDigitalTwinAsync(deviceId, patch);

    // publish telemetry
    await client.PublishTelemetryAsync(deviceId, null, bodyJson);
    ```

    There are two approaches being used to apply data to the digital twin - the first via property updates using a JSON patch, the second via the publishing of telemetry data.

    The ADT client utilizes a JSON Patch document to add or update digital twin properties. The JSON Patch defines a JSON document structure for expressing a sequence of operations to apply to a JSON document. The various values are added to the patch as append or replace operations, and the ADT is then updated asynchronously.

   > **TIP**: To learn more about a JSON Patch document, review the following resources:
   > * [Javascript Object Notation (JSON) Patch](https://tools.ietf.org/html/rfc6902)
   > * [What is JSON Patch?](http://jsonpatch.com/)
   > * [JsonPatchDocument Class](https://docs.microsoft.com/dotnet/api/azure.jsonpatchdocument?view=azure-dotnet)

   > **IMPORTANT**: The digital twin instance must have existing values before the `AppendReplace` operation is used.

   Notice that the telemetry data is handled differently than the properties - rather than being used to set digital twin properties, it is instead being published as telemetry events. This mechanism ensures that the telemetry is available to be consumed by any downstream subscribers to the digital twins event route.

   > **NOTE**: The digital twins event route must be defined before publishing a telemetry message, otherwise the message will not be routed for consumption.

1. On the Visual Studio Code **File** menu, click **Save**.

#### Task 5 - Publish the function app to Azure

Now that the Azure Function has been written, it must be published to Azure.

1. In the Azure Functions extension for Visual Studio Code, select **Deploy to Function App**:

    ![Visual Studio Code deploy to function app](media/LAB_AK_19-deploy-to-function-app.png)

1. When prompted, make these selections:

    * **Select subscription**: Select the subscription you are using for this course.
    * **Select Function App in Azure**: Select **func-az220-hub2adt-training-{your-id}**.

    When asked to confirm the deploy, click **Deploy**.

    The function will then be compiled and, if successful, deployed. This may take a few moments.

1. Once the deployment has completed, the following prompt will be displayed:

    ![Visual Studio Code deployment complete - select stream logs](media/LAB_AK_19-function-stream-logs.png)

    Click **Stream logs** and in the confirmation dialog to enable application logging, click **Yes**.

    The **OUTPUT** pane will now display the log stream for the deployed function - this will timeout after 2 hours. There will be some status information displayed, however there will not be any diagnostic information from the function itself until it is launched. That will be covered in the next exercise.

    The streaming can be stopped or started at any time by rght-clicking the Azure function in Visual Studio Code and select **Start Streaming Logs** or **Stop Streaming Logs**:

    ![Visual Studio Code Azure Function start streaming logs](media/LAB_AK_19-start-function-streaming.png)

### Exercise 9 - Connect IoT Hub to the Azure Function

In this exercise, the IoT Hub created by the setup script will be configured to publish events as they occur to the Azure Function created in the previous exercise. The telemetry from the device simulator created earlier will then be routed to the ADT instance.

1. Open a browser and navigate to the [Azure portal](https://portal.azure.com/).

1. Navigate to the **iot-az220-training-{your-id}** IoT Hub.

1. In the left-hand navigation area, select **Events**.

1. To add an event subscription, click **+ Event Subscription**.

1. In the **EVENT SUBSCRIPTION DETAILS** section, in the **Name** field, enter **device-telemetry**.

1. In the **Event Grid Schema** dropdown, ensure **Event Grid Schema** is selected.

1. In the **TOPIC DETAILS** section, verify that the **Topic Type** is set to **IoT Hub** and the **Source Resource** is set to **iot-az220-training-{your-id}**.

1. In the **System Topic Name** field, enter **Twin-Topic**

1. In the **EVENT TYPES** section, in the **Filter to Event Types** dropdown, select _only_ **Device Telemetry**.

1. In the **ENDPOINT DETAILS** section, in the **Endpoint Type** dropdown, select **Azure Function**.

    Notice that the UI updates to provide for the selection of an endpoint.

1. In the **Endpoint** field, click **Select an endpoint**.

1. In the **Select Azure Function** pane, under **Subscription**, ensure the correct subscription is selected.

1. Under **Resource group**, ensure **rg-az220** is selected.

1. Under **Function app**, select **func-az220-hub2adt-training-{your-id}**.

1. Under **Slot**, confirm **Production** is selected.

1. Under **Function**, confirm **HubToAdtFunction** is selected.

1. To choose this endpoint, click **Confirm Selection**.

1. Verify that **HubToAdtFunction** is now the specified Endpoint.

    On the **Create Event Subscription** pane, in the **ENDPOINT DETAILS** section, the **Endpoint** field should now display **HubToAdtFunction**.

1. To create this event subscription, click **Create**.

    After the subscription has been created, you should see messages in the Azure Functions log stream that you configured in the preceding exercise. The Azure Functions log stream shows the telemetry being received from Event Grid. It also shows any errors that occur when connecting to Azure Digital Twins or updating the twin.

    The log output for a successful function call will be similar to:

    ```log
    2021-03-12T19:14:17.180 [Information] Executing 'HubToAdtFunction' (Reason='EventGrid trigger fired at 2021-03-12T19:14:17.1797847+00:00', Id=88d9f9e8-5cfa-4a20-a4cb-36e07a78acd6)
    2021-03-12T19:14:17.180 [Information] {
    "properties": {
        "sensorID": "S1",
        "fanAlert": "false",
        "temperatureAlert": "true",
        "humidityAlert": "true"
    },
    "systemProperties": {
        "iothub-connection-device-id": "sensor-th-0055",
        "iothub-connection-auth-method": "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}",
        "iothub-connection-auth-generation-id": "637508617957275763",
        "iothub-enqueuedtime": "2021-03-12T19:14:16.824Z",
        "iothub-message-source": "Telemetry"
    },
    "body": "eyJ0ZW1wZXJhdHVyZSI6NjkuNDcsImh1bWlkaXR5Ijo5Ny44OX0="
    }
    2021-03-12T19:14:17.181 [Information] Azure digital twins service client connection created.
    2021-03-12T19:14:17.181 [Information] Device:sensor-th-0055 fanAlert is:False
    2021-03-12T19:14:17.181 [Information] Device:sensor-th-0055 temperatureAlert is:true
    2021-03-12T19:14:17.181 [Information] Device:sensor-th-0055 humidityAlert is:true
    2021-03-12T19:14:17.181 [Information] Device:sensor-th-0055 Temperature is:69.47
    2021-03-12T19:14:17.181 [Information] Device:sensor-th-0055 Humidity is:97.89
    2021-03-12T19:14:17.182 [Information] Executed 'HubToAdtFunction' (Succeeded, Id=88d9f9e8-5cfa-4a20-a4cb-36e07a78acd6, Duration=2ms)
    ```

    Here is an example log if the Digital Twin instance is not found:

    ```log
    2021-03-11T16:35:43.646 [Information] Executing 'HubToAdtFunction' (Reason='EventGrid trigger fired at 2021-03-11T16:35:43.6457834+00:00', Id=9f7a3611-0795-4da7-ac8c-0b380310f4db)
    2021-03-11T16:35:43.646 [Information] {
    "properties": {
        "sensorID": "S1",
        "fanAlert": "false",
        "temperatureAlert": "true",
        "humidityAlert": "true"
    },
    "systemProperties": {
        "iothub-connection-device-id": "sensor-th-0055",
        "iothub-connection-auth-method": "{\"scope\":\"device\",\"type\":\"sas\",\"issuer\":\"iothub\",\"acceptingIpFilterRule\":null}",
        "iothub-connection-auth-generation-id": "637508617957275763",
        "iothub-enqueuedtime": "2021-03-11T16:35:43.279Z",
        "iothub-message-source": "Telemetry"
    },
    "body": "eyJ0ZW1wZXJhdHVyZSI6NjkuNzMsImh1bWlkaXR5Ijo5OC4wOH0="
    }
    2021-03-11T16:35:43.646 [Information] Azure digital twins service client connection created.
    2021-03-11T16:35:43.647 [Information] Device:sensor-th-0055 fanAlert is:False
    2021-03-11T16:35:43.647 [Information] Device:sensor-th-0055 temperatureAlert is:true
    2021-03-11T16:35:43.647 [Information] Device:sensor-th-0055 humidityAlert is:true
    2021-03-11T16:35:43.647 [Information] Device:sensor-th-0055 Temperature is:69.73
    2021-03-11T16:35:43.647 [Information] Device:sensor-th-0055 Humidity is:98.08
    2021-03-11T16:35:43.648 [Information] Executed 'HubToAdtFunction' (Succeeded, Id=9f7a3611-0795-4da7-ac8c-0b380310f4db, Duration=2ms)
    2021-03-11T16:35:43.728 [Error] Service request failed.
    Status: 404 (Not Found)

    Content:
    {"error":{"code":"DigitalTwinNotFound","message":"There is no digital twin instance that exists with the ID sensor-th-0055. Please verify that the twin id is valid and ensure that the twin is not deleted. See section on querying the twins http://aka.ms/adtv2query."}}

    Headers:
    Strict-Transport-Security: REDACTED
    traceresponse: REDACTED
    Date: Thu, 11 Mar 2021 16:35:43 GMT
    Content-Length: 267
    Content-Type: application/json; charset=utf-8
    ```

1. Return to the **ADT Explorer** instance in the browser and query the graph.

    You should be able to see that the **fanAlert**, **temperatureAlert** and **humidityAlert** properties have been updated.

At this point, the data ingestion from device (in this case a device simulator) into ADT has been demonstrated. In the next few exercises, the ADT REST APIs will be explored. However, exercises 12 and 13 will return to the proof-of-concept implementation and demonstrate how parent twins can be updated and telemetry can be streamed to Time Series Insights (TSI).

### Exercise 10 - Create an app service principal to access ADT APIs

Stepping away from the prototype for a moment, you have been asked to investigate the ADT REST APIs as an alternative means to interact with ADT.

In order to access the ADT REST APIs, it is necessary to authenticate to Azure Active Directory using a service principal with appropriate permissions and roles. The following tasks will walk through the process of creating an app registration and service principal that can be used with REST calls. The service principal will then be granted READ / WRITE access to the ADT APIs and then assigned the **Azure Digital Twins Owner** role.

#### Task 1 - Create an app registration to use with Azure Digital Twins

In circumstances where the authentication strategy requires a **client ID** and **tenant ID** to authenticate, such as using OAuth style authentication via a REST API, then an app registration must be created in Azure Active Directory (AAD). Creating an app registration also creates a service principal that can be used to access ADT.

1. In a browser, navigate to the [Azure Portal](https://portal.azure.com).

1. In the **Search resources, services and docs** field, enter **Azure Active Directory** and click the **Azure Active Directory** search result.

    The **Default Directory Overview** will be displayed.

1. In the left navigation area, under **Manage**, click **App registrations**.

1. To add a new app registration for use with the REST API, click **+ New registration**.

1. Under **Name**, enter **app-az220-rest**.

1. Under **Supported account types**, ensure **Accounts in this organizational directory only (Default Directory only - Single tenant)** is selected.

    > **TIP**: The different account type are:
    > * **Accounts in this organizational directory only (Default Directory only - Single tenant)** - All user and guest accounts in your directory can use your application or API. Use this option if your target audience is internal to your organization.
    > * **Accounts in any organizational directory (Any Azure AD directory - Multitenant)** - All users with a work or school account from Microsoft can use your application or API. This includes schools and businesses that use Office 365. Use this option if your target audience is business or educational customers and to enable multitenancy.
    > * **Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)** - All users with a work or school, or personal Microsoft account can use your application or API. It includes schools and businesses that use Office 365 as well as personal accounts that are used to sign in to services like Xbox and Skype. Use this option to target the widest set of Microsoft identities and to enable multitenancy.
    > * **Personal Microsoft accounts only** - Personal accounts that are used to sign in to services like Xbox and Skype. Use this option to target the widest set of Microsoft identities.

1. Under **Redirect URI**, change the dropdown value to **Public client/native (mobile & desktop)** and enter **http://localhost**.

1. To create the registration, click **Register**.

    Once the registration is complete, the **app-az220-rest** registration detail page is shown.

1. In the text editor of your choice, create a file **appid-details.txt** and record the **Application (client) ID** and the **Directory (tenant) ID**.

    Save the file so you can access the details later.

#### Task 2 - Add the ADT API permission

In order to access the Azure Digital Twin APIs, the app registration must be granted the **Azure Digital Twins** permission.

1. On the **app-az220-rest** registration detail page, in the left navigation area, click **API  permissions**.

1. To start the **Request API permissions** activity, click **+Add a permission**.

1. On the **Request API permissions** page, switch to the **APIs my organization uses** tab and search for **azure digital twins**.

    > **NOTE**: Azure Digital Twins has a specific **Application (client) ID** - `0b07f429-9f4b-4714-9392-cc5e8e80c8b0`. You may see this value referred to in other labs.

1. Select **Azure Digital Twins** from the search results to proceed with assigning permissions for the Azure Digital Twins APIs.

    > **NOTE**:  If your subscription still has an existing Azure Digital Twins instance from the previous public preview of the service (before July 2020), you'll need to search for and select **Azure Smart Spaces Service** instead. This is an older name for the same set of APIs (the Application (client) ID is the same), and your experience won't be changed beyond this step.

1. To select which permissions to grant for these APIs, expand the **Read** permission section (if collapse) and check **Read.Write**.

1. To grant the app registration the reader and writer permission, click **Add permissions**.

    Once completed, the **API permissions** page will be shown and the new permissions will be listed.

1. Review Admin consent settings

    The **Grant admin consent for [company]** action is often required for app registration. Your organization might have **Admin Consent Required** turned on globally in Azure Active Directory (Azure AD) for all app registrations within your subscription. If so, for the app registration to be valid, the owner or administrator must select the button for your company on the API permissions page for that app registration:

    ![Grant admin consent](media/LAB_AK_19-grant-admin-consent.png)

#### Task 3- Add secret-based authentication

In order to use the REST APIs, it is necessary to pass a secret (or password) in the HTTP header. The following steps will create the application secret.

1. On the **app-az220-rest** registration detail page, in the left navigation area, click **Certificates and secrets**.

1. In the **Client secrets** section, click **+New client secret**.

1. In the **Add a client secret** popup, under **Description**, enter **Secret for AZ220 ADT lab**

1. Under **Expires**, select **in 1 year**.

    > **TIP**: Although setting a secret expiration to **Never** may be convenient, organizations often require token expiration as part of their security policies.

1. To create the secret, click **Add**.

    > **IMPORTANT**: It is vital that you complete the next step to make a note of the generated **Value** now (add it to the **appid-details.txt** file you created earlier) - you cannot view the value once you leave this page. The only recourse would be to delete the secret and add a new one.

1. Copy you app secret information (values assigned to Value and ID) into the **appid-details.txt** file that you created earlier.

#### Task 4 - Add Azure Digital Twin Owner role to the App service principal

Finally, the app registration service principal must be granted an ADT role.

1. Open your **adt-az220-training-{your-id}** ADT instance.

    You can use the **Search resources, services and docs** field to navigate to the ADT instance by entering **adt-az220** and then clicking the **adt-az220-training-{your-id}** search result.

1. To update user roles, on the left-side menu, click **Access control (IAM)**.

1. To add a new assignment, click **+ Add** and then click **Add role assignment**.

1. In the **Role** drop-down, select **Azure Digital Twins Data Owner**

1. Ensure the **Assign access to** field value is **User, group or service principal**.

    **Note**: There are many types of security principal that can be chosen including **Function Apps**, **Logic Apps**, **Virtual Machines**, etc.

    A list of available users should be listed - however, the app service principal is not shown.

1. To show the app service principal, in the **Select** field, enter **app-az220-rest**.

    The app registration service principal is now shown.

1. Select **app-az220-rest**, note that it is added to **Selected members**, and click **Save**.

1. To confirm, the role assignment, click **Role assignments** and **app-az220-rest** should be listed beneath the **Azure Digital Twins Owner** role.

### Exercise 11 - Call REST APIs with Postman

In the previous exercise, a service principal was created so that the Azure Digital Twins REST APIs could be explored. As some application architectures don't support the Azure Digital Twins SDK, the Azure Digital Twins REST APIs can be used to perform both control-plane operations (for example, to create Azure Digital Twins instances) and data-plane operations (for example, ingesting telemetry or modifying Azure Digital Twins instance properties).

#### Task 1 - Retrieve authentication token using Postman

In this task, the Postman application is launched and configured for use with the ADT REST APIs. A GET request is created that accesses the global Azure Active Directory authentication service using the details from the service principal created earlier. The service call returns an access token from the Azure AAD authentication endpoint that can the be used in subsequent calls to ADT REST APIs.

1. Launch the **Postman** application

1. If you have an existing **Postman** account, you can sign in with your current account

    **or**

    Sign up for a free account.

1. On the **File** menu, select **New...**.

1. On the **Create New** dialog, click **Request**.

1. Under **Request name**, enter **Retrieve ADT Token**.

1. Under **Select a collection or folder to save to:**, click **+ Create Collection**.

1. In the **Name your collection** field, enter **ADT APIs** and then click the check mark to accept the entry.

1. To save the request to the new collection, click **Save to ADT APIs**.

    The new request will be displayed.

1. Locate the **GET** value in a dropdown, and change it to **POST**.

1. In the **Enter request URL** field, enter **https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token**

    This URL is the authentication endpoint for the Azure Active Directory tenant.

    > **NOTE**: Replace the **{tenant-id}** token with the **Directory (tenant) ID** value saved in the **appid-details.txt** file.

1. Under the URL, select the **Headers** tab  and enter the following:

    | Key          | Value                             |
    | :----------- | :-------------------------------- |
    | Content-Type | application/x-www-form-urlencoded |

1. To enter the data to be sent, select the **Body** tab.

1. To configure the data entry to key / value pairs, select **x-www-form-urlencoded** tab and enter the following:

    | Key           | Value                                   |
    | :------------ | :-------------------------------------- |
    | client_id     | {application-id}                        |
    | scope         | https://digitaltwins.azure.net/.default |
    | client_secret | {secret}                                |
    | grant_type    | client_credentials                      |

    > **NOTE**: Replace the **{application-id}** token with the **Application (client) ID** value, and the **{secret}** token with the **Secret** value saved in the **appid-details.txt** file.

1. To retrieve a token, click **Send**.

    The successful response will include a JSON body similar to:

    ```json
    {
        "token_type": "Bearer",
        "expires_in": 3599,
        "ext_expires_in": 3599,
        "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyIsImtpZCI6Im5PbzNaRHJPRFhFSzFqS1doWHNsSFJfS1hFZyJ9.eyJhdWQiOiJodHRwczovL2RpZ2l0YWx0d2lucy5henVyZS5uZXQiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83ODFkZTliYS05NWI1LTRlZWMtODg4NC02YWMwNWI5MGNhYmIvIiwiaWF0IjoxNjE1NjY2OTg5LCJuYmYiOjE2MTU2NjY5ODksImV4cCI6MTYxNTY3MDg4OSwiYWlvIjoiRTJZQWd1VzZuN1k4RUpPWFRuK3UrWmJQL3dNQSIsImFwcGlkIjoiMzY2NGY4Y2UtZjFhMC00MWY4LTg1NjItMmIyZjEwMTg2N2UxIiwiYXBwaWRhY3IiOiIxIiwiaWRwIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvNzgxZGU5YmEtOTViNS00ZWVjLTg4ODQtNmFjMDViOTBjYWJiLyIsIm9pZCI6IjViNjExY2I4LTkyZjAtNDg3YS05ZGUyLWQ3YjA0NTBhMmY1OSIsInJoIjoiMC5BUWNBdXVrZGVMV1Y3RTZJaEdyQVc1REt1ODc0WkRhZzhmaEJoV0lyTHhBWVotRUhBQUEuIiwic3ViIjoiNWI2MTFjYjgtOTJmMC00ODdhLTlkZTItZDdiMDQ1MGEyZjU5IiwidGlkIjoiNzgxZGU5YmEtOTViNS00ZWVjLTg4ODQtNmFjMDViOTBjYWJiIiwidXRpIjoiYjNRZFVxVm9hVTJfbGtGd2h5SWhBQSIsInZlciI6IjEuMCJ9.b8epFAPh9SBC-8Scm5elgb2JsjS0HP0Q6kTNOd_dbXA5VYBdWw7xbuXF-yb4CRxyzqK1nyedZfDO43vIxzyLsKKSsq4ebfx3ixdnUMKWkhZMOWl8vBcJ7kZ_Xoyj_GMONgVRjL52iplr2FD9qCMK1S4icoPfW_8ceTgwLtWwHYfsfDh65-6nDw-kJ3AqBGCwjdEZTQrej05j1KdfDrR6PY2gFAwVNDQvkQs7WlZ37NOroCDBRixf1xiEZzjYccjGF4-z3R6hJNDXmMroa6bRwzHCT78Mb5OGcVABXCTS4Hh_UF1iXX0aZE-ESqJRvBhSvuzINT6L1-FHgxlaw2Nxfg"
    }
    ```

    Notice the **expires_in** value - this value is in seconds and the value supplied indicates the above token will expire in approximately 1 hour.

1. Copy the **access_token** value (the multi-line string between the quotes) returned by **your** request and add to the **appid-details.txt** file.

#### Task 2 - Use REST API to update an ADT property

In this task, the ADT REST API will be used to update a Cheese Cave twin and update the **desiredTemperature** and **desiredHumidity** properties.

1. In the **Postman** application, in the **File** menu, select **New..**.

1. On the **Create New** dialog, click **Request**.

1. Under **Request name**, enter **Update Cave desired properties**.

1. Under **Select a collection or folder to save to:**, ensure **ADT APIs** is selected.

1. To save the new request to your collection, click **Save to ADT APIs**.

    The new request will be displayed.

1. Locate the **GET** value in a dropdown, and change it to **PATCH**.

1. In the **Enter request URL** field, enter **https://{adt-hostname}/digitaltwins/{twin_id}**

    > **NOTE**: Replace the **{twin_id}** token with **cave_2** and the **{adt-hostname}** token with the hostname of your Azure Digital Twin instance. The following Azure CLI command will list the host names of all ADT instances in the current subscription: `az dt list --query "[].hostName"`.

1. Under the URL, select the **Query params** tab and enter the following:

    | Key         | Value      |
    | :---------- | :--------- |
    | api-version | 2020-10-31 |

1. Under the URL, select the **Headers** tab and enter the following:

    | Key           | Value              |
    | :------------ | :----------------- |
    | Content-Type  | application/json   |
    | Authorization | Bearer {aad_token} |

    > **NOTE**: Replace the **{aad_token}** token with  **access_token** value saved to the **appid-details.txt** file.

    The vales will be similar to:

    ![Postman authorization token](media/LAB_AK_19-postman-auth-token.png)

1. To supply the JSON Patch data, select the **Body** tab.

1. To specify the data format, select **raw** and notice that the format dropdown appears and **JSON** is selected by default.

1. In the body field, enter the following:

    ```json
    [
        {
            "op": "replace",
            "path": "/desiredTemperature",
            "value": 75.0
        },
        {
            "op": "replace",
            "path": "/desiredHumidity",
            "value": 85.0
        }
    ]
    ```

1. To send the request, click **Send**.

    If the call is successful, an HTTP Status of **204 No Content** will be returned. If, however, an error occurs, a JSON document will be displayed along with an appropriate HTTP Status. For example, if an attempt to update a property that does not exist is made, the following is returned with an HTTP status of **400 Bad Request**:

    ```json
    {
        "error": {
            "code": "JsonPatchInvalid",
            "message": "DesiredMissingHumidity does not exist on component. Please provide a valid patch document. See section on update apis in the documentation http://aka.ms/adtv2twins."
        }
    }
    ```

#### Task 3 - Use REST API to query ADT

In this task, the ADT REST API will be used to run a simple query.

1. In the **Postman** application, in the **File** menu, select **New...**.

1. On the **Create New** dialog, click **Request**.

1. Under **Request name**, enter **Query ADT**.

1. Under **Select a collection or folder to save to:**, ensure **ADT APIs** is selected.

1. To save the request to the new collection, click **Save to ADT APIs**.

    The new request will be displayed.

1. Locate the **GET** value in a dropdown, and change it to **POST**.

1. In the **Enter request URL** field, enter **https://{adt-hostname}/query**

    > **NOTE**: Replace the **{adt-hostname}** token with the hostname of your Azure Digital Twin instance. The following Azure CLI command will list the host names of all ADT instances in the current subscription: `az dt list --query "[].hostName"`.

1. Under the URL, select the **Query params** tab and enter the following:

    | Key         | Value      |
    | :---------- | :--------- |
    | api-version | 2020-10-31 |

1. Under the URL, select the **Headers** tab and enter the following:

    | Key           | Value              |
    | :------------ | :----------------- |
    | Content-Type  | application/json   |
    | Authorization | Bearer {aad_token} |

    > **NOTE**: Replace the **{aad_token}** token with  **access_token** value saved to the **appid-details.txt** file.

    The vales will be similar to:

    ![Postman authorization token](media/LAB_AK_19-postman-auth-token.png)

1. To supply the JSON Patch data, select the **Body** tab.

1. To specify the data format, select **raw** and notice that the format dropdown appears and **JSON** is selected by default.

1. In the body field, enter the following:

    ```json
    {
        "query": "SELECT desiredTemperature, desiredHumidity FROM DIGITALTWINS where $dtId = 'cave_2' AND IS_PRIMITIVE(desiredTemperature) AND IS_PRIMITIVE(desiredHumidity)"
    }
    ```

    > **NOTE**: At this time, complex properties are not supported. To make sure that projection properties are valid, combine the projections with an **IS_PRIMITIVE** check as shown above.

1. To send the request, click **Send**.

    If the call is successful, an HTTP Status of **200 OK** will be returned with JSON similar to the following:

    ```json
    {
        "value": [
            {
                "DesiredTemperature": 75,
                "DesiredHumidity": 85
            }
        ],
        "continuationToken": null
    }
    ```

    > **NOTE**: The **continuationToken** is used to retrieve the next set of results from a previous query, if the number of results exceed the limit for a single call. The **continuationToken** key and value would be added to the key / value list in the body of the subsequent query call:
    > ```json
    > {
    >     "query": "SELECT DesiredTemperature, DesiredHumidity FROM DIGITALTWINS where $dtId = 'cave_5' AND IS_PRIMITIVE(DesiredTemperature) AND IS_PRIMITIVE(DesiredHumidity)",
    >     "continuationToken": "{continuation-token}"
    > }
    > ```

### Exercise 12 - Create a route and filter for twin update notification

Returning to the proof-of-concept scenario, it is now time to consider how to update the properties of a Cheese Cave based upon the changes to the properties of a child Cheese Cave Device. In the Cheese Factory scenario, each Cheese Cave Device has a parent Cheese Cave. However, the specific relationship between a Cheese Cave Device twin and the parent Cheese Cave twin is somewhat temporary - while the Cheese Cave twin represents a physical location and is unlikely to change, the Cheese Cave Device twin represents a device that may fail and be replaced. In the case of a failure, the relationship between the failed device and the cave would be deleted and a relationship with the replacement device created.

When considering this relationship, the Contoso business analysts decided that the Cheese Cave twin would be considered the record of the current state of the Cheese Cave and queries should only consider the Cheese Cave twins. Therefore, to ensure that the Cheese Cave twins are up to date, any change to the  property values on a Cheese Cave Device twin should be propagated to the parent Cheese Cave.

As the developer of the proof-of-concept you decided that Digital Twin Change Notifications will be monitored - if a change is made to a Cheese Cave Device, the parent Cheese Cave digital twin will be found and an update made.

There are a number of different events in Azure Digital Twins that produce notifications, which in turn alert the solution backend when the events occur. The event notifications are then routed to different locations inside and outside of Azure Digital Twins where the information can be used to take a desired action.

There are several types of notifications that can be generated, and notification messages may look different depending on which type of event generated the notification.

The chart below shows the different notification types:

| Notification type                             | Routing source name                           | Generated from...                           |
| :-------------------------------------------- | :-------------------------------------------- | :------------------------------------------ |
| Digital Twin Change Notification              | Digital Twin Change Notification              | any digital twin property change            |
| Digital Twin Lifecycle Notification           | Digital Twin Lifecycle Notification           | any digital twin create or delete operation |
| Digital Twin Relationship Change Notification | Digital Twin Relationship Change Notification | any digital twin relationship change        |
| Digital Twin Telemetry Messages               | Telemetry Messages                            | any telemetry message                       |

In general, notifications are made up of two parts: the header and the body.

> **NOTE**: More details on notifications can be viewed below:
> * [Understand event data](https://docs.microsoft.com//azure/digital-twins/how-to-interpret-event-data)

Earlier in this course, an Azure Function was implemented that processed IoT hub device messages for input into Azure Digital Twins. The Azure function performed the following actions:

* Extracted properties from the device message and updated the mapped properties on the device Digital Twin - the fan, temperature and humidity alerts.
* Published the device temperature and humidity telemetry to ADT

> **NOTE**: Since an update to any digital twin property will raise a Digital Twin Change Notification, it is important to ensure that the function responds to only the intended events.

Here are the actions that will be completed to set up this data flow:

* Create an Event Grid endpoint in Azure Digital Twins that connects the instance to Event Grid
* Set up a route within Azure Digital Twins to send twin property change events to the endpoint
* Deploy an Azure Functions app that listens (through Event Grid) to the endpoint, and if the message originates from a Cheese Cave Device, apply updates to the parent Cheese Cave
* Run the simulated device and query Azure Digital Twins to see the live results

> **TIP**: **Event Grid** vs **Event Hub** - although they sound similar, and both handle events, the two services are used for different purposes. The **Event Grid** is used when your application deals with discrete events, such as a Twin Update. This means the **Event Grids** are used when there is a need for your application to work in a publisher/subscriber model. An **Event Hub**, however, is used when an application deals with a series of events that might occur at massive scale, such as the streaming telemetry from many sensors. In this exercise, an **Event Grid** is appropriate for handling the twin update events. In a later exercise, an **Event Hub** will be used to stream the device telemetry from Azure Digital Twins to Time Series Insights.

#### Task 1 - Create Event Grid Topic

Event Grid is an Azure service that helps you route and deliver events coming from Azure Services to other places within Azure. You can create an event grid topic to collect certain events from a source, and then subscribers can listen on the topic to receive the events as they come through.

1. In a browser, navigate to the [Azure Portal](https://portal.azure.com).

1. On the Azure portal menu, click **+ Create a resource**.

1. In the Search textbox, type **Event Grid Topic**

    The searhc results will update to display the available services matching your search criteria.

1. On the **New** blade, click the **Event Grid Topic** search result.

1. To begin the process of creating the Event Grid Topic, click **Create**.

1. On the **Create Topic** blade, in the **Subscription** dropdown, ensure that the Azure subscription that you intend to use for this course is selected.

1. To the right of **Resource group**, open the dropdown, and then click **rg-az220**

1. In the **Topic Details** section, in the **Name** field, enter **evgt-az220-adtoutput**.

1. In the **Location** dropdown, select a region that is near you.

1. Click **Review + Create**.

1. Verify that the Validation succeeded message is displayed, and then click **Create**.

   Once the resource has been deployed successfully, proceed to the next task.

#### Task 2 - Set up endpoint

In order for Azure Digital Twins to send events, an endpoint must be defined.

1. In the Azure portal, navigate to the Azure Digital Twins instance - **adt-az220-training-{your-id}**.

1. In the left navigation area, under **Connect outputs**, click **Endpoints**.

    A list of existing endpoints is displayed.

1. To add a new endpoint, click **+ Create an endpoint**.

1. In the **Create an endpoint** pane, under **Name**, enter **eventgrid-endpoint**.

1. Under **Endpoint type**, ensure that **Event Grid** is selected.

    > **NOTE**: The value of this selection will change the UI beneath to fields relevant to the selection.

1. Under **Subscription**, ensure that the Azure subscription that you intend to use for this course is selected.

1. Under **Event Grid topic**, open the dropdown and select **evgt-az220-adtoutput** (the Event Grid Topic created earlier).

1. To create the endpoint, click **Save**.

    The **Create an endpoint** pane will close and the list of endpoints will update to include the new endpoint. Notice that the endpoint status is **Provisioning**. Wait a few moments and click **Refresh** to update the list. Repeat until the endpoint status is **Active**.

#### Task 3 - Setup route

For Azure Digital Twins to send events via an endpoint, a route must be created.

1. In the left navigation area, under **Connect outputs**, click **Event routes**.

    A list of existing event routes is displayed.

1. To add a new event route, click **+ Create an event route**.

1. In the **Create an event route** pane, under **Name**, enter **eventgrid-updateeventroute**

1. Under **Endpoint**, select **eventgrid-endpoint**.

1. In the **Add an event route filter** section, leave the **Advanced editor** setting disabled.

    > **NOTE**: Without filtering, endpoints receive a variety of events from Azure Digital Twins:
    > * Telemetry fired by digital twins using the Azure Digital Twins service API
    > * Twin property change notifications, fired on property changes for any twin in the Azure Digital Twins instance
    > * Life-cycle events, fired when twins or relationships are created or deleted
    >
    > You can restrict the events being sent by adding a filter for an endpoint to your event route. The **Advanced editor** allows the filter expression to be written as text. To learn more about filters, review the resource below:
    > * [Filter events](https://docs.microsoft.com/azure/digital-twins/how-to-manage-routes-apis-cli#filter-events)

1. To configure the route such that only Twin update events are sent to the **eventgrid-endpoint**, expand the **Event types** dropdown and select ONLY the **Twin update** option.

    Notice that when the options are changed, the text displayed under **Filter** updates. Enabling the **Advanced editor** would allow this text to be updated directly.

1. To save this route and filter, click **Save**.

    The **Create an event route** pane will close and the list of event routes will update to include the new route. Click **Refresh** to update the list if necessary.

#### Task 4 - Create twin update function

In this task, a new function will be added to the existing **func-az220-hub2adt-training-{your-id}** function app and associated code project. This function will respond to the routed Twin update events from the Cheese Cave Device and update properties on the parent Cheese Cave twin.

> **NOTE**: As the function will be deployed within the existing Function App, it will inherit the service principal that has the rights to access ADT.

1. In Visual Studio Code, open the **Contoso.AdtFunctions** project.

1. To add a new function to the existing project, on the **View** menu, click **Command Palette**, and then enter **Azure Functions: Create Function**

1. Provide the following information at the prompts:

   * **Select a template for your function**: Select **Change template filter**.
   * **Select a template filter**: Select **All**.
   * **Select a template for your function**: Select **EventGridTrigger**.
   * **Provide a function name**: Type **UpdateTwinFunction**.
   * **Provide a namespace**: Type **Contoso.AdtFunctions**.
   * **When prompted for a storage account choose**: click **Skip for now**.

    You should see the **UpdateTwinFunction.cs** file added to the project and opened for editing.

1. Replace the `using` statements at the top of the file with the following:

    ```csharp
    using Azure;
    using Azure.Core.Pipeline;
    using Azure.DigitalTwins.Core;
    using Azure.Identity;
    using Microsoft.Azure.EventGrid.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.EventGrid;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    ```

1. Replace the **UpdateTwinFunction** class definition with the following code:

    ```csharp
    public static class UpdateTwinFunction
    {
        //Your Digital Twins URL is stored in an application setting in Azure Functions.
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        private static string[] mappedProperties = new string[] {
                                "/fanAlert",
                                "/humidityAlert",
                                "/temperatureAlert"
                            };

        [FunctionName("UpdateTwinFunction")]
        public async static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

            if (adtInstanceUrl == null)
            {
                log.LogError("Application setting \"ADT_SERVICE_URL\" not set");
                return;
            }

            try
            {
                // INSERT authentication code below here
                ManagedIdentityCredential cred = new ManagedIdentityCredential("https://digitaltwins.azure.net");
                DigitalTwinsClient client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"Azure digital twins service client connection created.");

                // INSERT event processing code below here
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }
    }
    ```

    This code is virtually identical to that used in the **HubToAdtFunction** created earlier. The **mappedProperties** array is a list of the Cheese Cave Device properties that will be read and applied to matching properties on the parent Cheese Cave.

1. To start processing the Event Grid event, locate the `// INSERT event processing code below here` comment and insert the following code below it:

    ```csharp
    if (eventGridEvent != null && eventGridEvent.Data != null)
    {
        string twinId = eventGridEvent.Subject.ToString();
        JObject message = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());

        log.LogInformation($"Reading event from {twinId}: {eventGridEvent.EventType}: {message["data"]}");

        if (message["data"]["modelId"] != null && (string)message["data"]["modelId"] == "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1")
        {
            // INSERT Find the device parent model (the Cheese Cave)

        }
        else
        {
            log.LogInformation($"Source model is not a Cheese Cave Device: {(message["data"]["modelId"] != null ? (string)message["data"]["modelId"] : "null")}");
        }
    }
    ```

    As in the earlier function, after ensuring the **eventGridEvent** has data, the event is processed. Notice that the originating twin ID is present as the subject.

    > **NOTE**: To learn more about the format of a digital twin change notification, review the following resource:
    >
    > * [Digital twin change notification](https://docs.microsoft.com/azure/digital-twins/how-to-interpret-event-data#digital-twin-change-notifications)

    The message is then deserialized from the **eventGridEvent** data. Here is an example of the message data JSON:

    ```json
    {
        "modelId": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1",
        "patch": [
            {
                "value": false,
                "path": "/fanAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/temperatureAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/humidityAlert",
                "op": "replace"
            }
        ]
    }
    ```

    As can be seen from the code above, the **modelId** property in the message data identifes the source model. The function checks that the originating model is a Cheese Cave Device before processing the event, otherwise it logs a message indicating that the source was not a Cheese Cave Device.

    > **NOTE**: This is logged as informational, not as an error, as events from other models are expected. Scenarios exist where the function would be enhanced to handle updates from many different models, each resulting in different actions.

1. To find the parent Cheese Cave for the source Cheese Cave Device, locate the `// INSERT Find the device parent model (the Cheese Cave)` comment and insert the following code below it:

    ```csharp
    AsyncPageable<IncomingRelationship> rels = client.GetIncomingRelationshipsAsync(twinId);

    string parentId = null;
    await foreach (IncomingRelationship ie in rels)
    {
        if (ie.RelationshipName == "rel_has_devices")
        {
            parentId = ie.SourceId;
            break;
        }
    }

    if (parentId == null)
    {
        log.LogError($"Unable to find parent for {twinId}");
    }
    else
    {
        // INSERT Update the parent
    }
    ```

    To find the parent Cheese Cave, the code retrieves all of the relationships that target the digital twin. The results are actually encapsulated in an **AsyncPageable** collection. This is collection of values that may take multiple service requests to iterate over and dramatically simplifies the consumption of paged asynchronous data calls. In simplified terms, the implementation of **GetIncomingRelationshipsAsync** returns an object that:

    * Asynchronously retrieves the first page of results
    * Provides an enumerator that can iterate through the page of results and, if the end of a page is reached before the last result, authomatically retrieve the next page. The iteration then continues through the new page, and so on, until the last result is reached.

    > **TIP**: The **AsyncPageable** collection is part of the Azure SDK for .NET and is documented at the resource below:
    >
    > * [AsyncPageable<T> Class](https://docs.microsoft.com/dotnet/api/azure.asyncpageable-1?view=azure-dotnet)

    Notice the use of `await foreach (IncomingRelationship ie in rels)` to iterate through the relationships return by **GetIncomingRelationshipsAsync**.

    The code then looks for a relationship that has the desired name - **rel_has_devices**. This must be the name of the relationship defined in the digital twin model. If found, the relationship source ID is the parent ID.

    If no relationship is found, then an error is logged, otherwise the the message is processed to update the parent.

1. To update the parent Cheese Cave, locate the `// INSERT Update the parent` comment and insert the following code below it:

    ```csharp
    // Read properties which values have been changed in each operation
    var patch = new Azure.JsonPatchDocument();
    foreach (var operation in message["data"]["patch"])
    {

        string opValue = (string)operation["op"];
        if (opValue.Equals("replace"))
        {
            string propertyPath = ((string)operation["path"]);

            if (mappedProperties.Contains(propertyPath))
            {
                var value = operation["value"].Value<bool>();
                patch.AppendReplace<bool>(propertyPath, value);
                log.LogInformation($"Updating parent {parentId}: {propertyPath} = {value}");
            }
        }
    }

    await client.UpdateDigitalTwinAsync(parentId, patch);
    ```

    This code creates a new **JsonPatchDocument** and then iterates through each object in the **patch** property of the event data:

    ```json
    {
        "modelId": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1",
        "patch": [
            {
                "value": false,
                "path": "/fanAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/temperatureAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/humidityAlert",
                "op": "replace"
            }
        ]
    }
    ```

    If the object operation is a **replace**, and the **path** (the property) is contained in the **mappedProperties** list declared at the top of the function, then the change to the property is added to the new **JsonPatchDocument**:

    ```csharp
    private static string[] mappedProperties = new string[] {
                            "/fanAlert",
                            "/humidityAlert",
                            "/temperatureAlert"
                        };
    ```

    After all the operations are checked, the digital twin is updated.

#### Task 5 - Deploy the function app to Azure

Now that the function has been written, the whole Function App can be redeployed to Azure.

1. In the Azure Functions extension for Visual Studio Code, select **Deploy to Function App**:

    ![Visual Studio Code deploy to function app](media/LAB_AK_19-deploy-to-function-app.png)

1. When prompted, make these selections:

    * **Select subscription**: Select the subscription you are using for this course.
    * **Select Function App in Azure**: Select **func-az220-hub2adt-training-{your-id}**.

    When asked to confirm the deploy, click **Deploy**.

    The function will then be compiled and, if successful, deployed. This may take a few moments.

1. Once the deployment has completed, the following prompt will be displayed:

    ![Visual Studio Code deployment complete - select stream logs](media/LAB_AK_19-function-stream-logs.png)

    Click **Stream logs** and in the confirmation dialog to enable application logging, click **Yes**.

    The **OUTPUT** pane will now display the log stream for the deployed function - this will timeout after 2 hours. There will be some status information displayed, however there will not be any diagnostic information from the function itself until it is launched. That will be covered in the next exercise.

    The streaming can be stopped or started at any time by rght-clicking the Azure function in Visual Studio Code and select **Start Streaming Logs** or **Stop Streaming Logs**:

    ![Visual Studio Code Azure Function start streaming logs](media/LAB_AK_19-start-function-streaming.png)

#### Task 6 - Connect the function to Event Grid

The function is deployed, but it still needs to subscribe to the event.

1. In a browser, return to the Azure portal and navigate to the **evgt-az220-adtoutput** Event Grid Topic.

1. On the **Overview** pane, click **+ Event Subscription**.

    The steps to create this event subscription are similar to when you subscribed the first Azure function to IoT Hub earlier in this module. This time, you don't need to specify Device Telemetry as the event type to listen for, and you'll connect to a different Azure function.

1. On the **Create Event Subscription** page, in the **Name** field, enter **twin-updates**.

1. In the **Event schema** field, ensure that **Event Grid Schema** is selected.

1. In the **Topic Details** section, notice that the **Topic Type** and **Source Resource** are pre-populated and read-only.

1. In the **Event Types** section, notice that a filter can be added to restrict the types of events that will be published via this subscription. For this exercise, leave it empty.

    > **NOTE**: By default, all event types for the event source are sent to the endpoint. You can decide to send only certain event types to your endpoint. For example, you can get notified of updates to your resources, but not notified for other operations like deletions. In that case, filter by the **Microsoft.Resources.ResourceWriteSuccess** event type. To learn more about event type filtering, review the following resource:
    >
    > * [Understand event filtering for Event Grid subscriptions](https://docs.microsoft.com/azure/event-grid/event-filtering)

1. In the **Endpoint Details** section, for the **Endpoint type**, select **Azure Function**.

1. For the **Endpoint**, click **Select an endpoint**.

1. In the **Select Azure Function** pane, under **Subscription**, ensure that the correct subscription is selected.

1. Under **Resource group**, ensure that  **rg-az220** is selected.

1. Under **Function app**, select **func-az220-hub2adt-training-{your-id}**.

1. Under **Slot**, confirm **Production** is selected.

1. Under **Function**, select **UpdateTwinFunction**.

1. To choose this endpoint, click **Confirm Selection**.

1. On the **Create Event Subscription** pane, in the **ENDPOINT DETAILS** section, verify that the **Endpoint** field now displays **UpdateTwinFunction**.

1. To create the subscription, click **Create**.

Once the subscription has been created, any updates to a digital twin will flow through to the **UpdateTwinFunction**.

#### Task 7 - Run the simulator

In _Exercise 7_ of this lab, the **CheeseCaveDevice** simulator was configured and launched. If this app is still running, jump to the next task - otherwise, return to _Exercise 7_, and complete the tasks and steps to launch the the simulator.

#### Task 8 - Confirm updates applied to cave_1

Ensure that the **CheeseCaveDevice** simulator is running and that the **TwinUpdateFunction** is streaming logs.

1. In the Visual Studio Code instance that is streaming the function logs, review the output of the functions. An example of the output from the **UpdateTwinFunction** is shown below.

    ```log
    2021-03-26T16:42:47.983 [Information] Executing 'UpdateTwinFunction' (Reason='EventGrid trigger fired at 2021-03-26T16:42:47.9834088+00:00', Id=b3a742e7-c2bd-4e42-af8d-759c416af238)
    2021-03-26T16:42:47.984 [Information] Azure digital twins service client connection created.
    2021-03-26T16:42:47.984 [Information] Reading event from sensor-th-0055: Microsoft.DigitalTwins.Twin.Update: {
        "modelId": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1",
        "patch": [
            {
                "value": false,
                "path": "/fanAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/temperatureAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/humidityAlert",
                "op": "replace"
            }
        ]
    }
    2021-03-26T16:42:47.984 [Information] Executed 'UpdateTwinFunction' (Succeeded, Id=b3a742e7-c2bd-4e42-af8d-759c416af238, Duration=1ms)
    2021-03-26T16:42:48.002 [Information] Updating parent cave_1: /fanAlert = False
    2021-03-26T16:42:48.003 [Information] Updating parent cave_1: /temperatureAlert = True
    2021-03-26T16:42:48.003 [Information] Updating parent cave_1: /humidityAlert = True
    ```

    As expected, the twin update notification from **sensor-th-0055** is processed and an update is applied to **cave_1**.

    An example of an ignored update to **cave_1** is shown below:

    ```log
    2021-03-26T16:42:48.175 [Information] Executing 'UpdateTwinFunction' (Reason='EventGrid trigger fired at 2021-03-26T16:42:48.1754078+00:00', Id=03e26a1d-e2df-477c-ac49-0039d87450ad)
    2021-03-26T16:42:48.176 [Information] Azure digital twins service client connection created.
    2021-03-26T16:42:48.176 [Information] Reading event from cave_1: Microsoft.DigitalTwins.Twin.Update: {
        "modelId": "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1",
        "patch": [
            {
                "value": false,
                "path": "/fanAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/temperatureAlert",
                "op": "replace"
            },
            {
                "value": true,
                "path": "/humidityAlert",
                "op": "replace"
            }
        ]
    }
    2021-03-26T16:42:48.176 [Information] Source model is not a Cheese Cave Device: dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave;1
    2021-03-26T16:42:48.176 [Information] Executed 'UpdateTwinFunction' (Succeeded, Id=03e26a1d-e2df-477c-ac49-0039d87450ad, Duration=1ms)
    ```

1. In a browser, return to the **ADT Explorer** at [http://localhost:3000](http://localhost:3000).

1. Ensure the **QUERY EXPLORER** query is set to the following:

    ```sql
    SELECT * FROM digitaltwins
    ```

1. To run this query, click **Run Query**.

1. Select the **cave_1** device and review the properties in the **PROPERTY EXPLORER**.

    When the **cave_1** twin was initialized, the **temperatureAlert** and **humidityAlert** were set to false. After a number of updates have been applied, the **temperatureAlert** and **humidityAlert** properties will now be set to true.

1. To determine the last update time for the properties on the **cave_1** twin, expand the **$metadata** property.

    The **lastUpdateTime** values (displayed in UTC) should indicate the properties have been recently updated.

1. Re-run the query by clicking **Run Query** and verify that the **temperatureAlert** and **humidityAlert** properties **lastUpdateTime** values are changing.

    If the **lastUpdateTime** values are not updating, refresh the browser windows and then re-run the query.

### Exercise 13 - Connect ADT to TSI

In this exercise, you will complete the final part of the proof-of-concept - streaming the device telemetry sent from the Cheese Cave Device sensor-th-0055 via the simulator, through Azure Digital Twins to Time Series Insights.

> **NOTE**: The setup script for this lab created the Azure Storage Account, Time Series Insights Environment and Access Policy that will be used during this exercise.

Routing event data from ADT to TSI is achieved by the following basic flow:

* ADT publishes notification events to an Event Hub (adt2func)
* An Azure Function is triggered by events from the adt2func Event Hub, creates a new event for TSI, adds a partition key and published to another Event Hub (func2tsi)
* TSI subscribes to events published to the func2tsi Event Hub.

In order to implement this flow, the following resources must be created (in addition to ADT and TSI):

* An Azure Function
* An Event Hub Namespace
* Two Event Hubs - one for events from ADT to the Azure Function, another for events from the Azurre Function for TSI.

The Azure function can serve a number of purposes:

* Map device specific telemetry message formats (property names, data types, etc.) to a single format for TSI. This can provide a consolidated view across different devices as well as insulate the TSI solution from changes to other parts of the solution.
* Enrich the messages from other sources
* Add a field to each event for use as the Time Series ID Property within TSI.

#### Task 1 - Create event hub namespace

An Event Hubs namespace provides DNS integrated network endpoints and a range of access control and network integration management features such as IP filtering, virtual network service endpoint, and Private Link and is the management container for one of multiple Event Hub instances (or topics, in Kafka parlance). The two Event Hubs required for this solution will be created within this namespace.

1. Login to [portal.azure.com](https://portal.azure.com) using your Azure account credentials.

1. On the Azure portal menu, click **+ Create a resource**.

1. In the Search textbox, type **Event Hubs** and then click the **Event Hubs** search result.

1. To create an **Event Hub**, click **Create**.

    The **Create Namespace** page will open.

1. On the **Create Namespace** blade, in the **Subscription** dropdown, ensure that the Azure subscription that you are using for this course is selected.

1. To the right of **Resource group**, open the dropdown, and then click **rg-az220**

1. In the **Namespace name** field, enter **evhns-az220-training-{your-id}**.

    This resource is publically accessible and must have a unique name.

1. To the right of **Location**, open the drop-down list and select the same location that you selected for your resource group.

1. To the right of **Pricing tier**, open the drop-down list and select **Standard**.

    > **TIP**: In this exercise, the **Basic** tier would also work, however in most production scenarios, **Standard** would be the better choice:
    >
    > * **Basic**
    >   * 1 Consumer Group
    >   * 100 Brokered Connections
    >   * Ingress events - $0.028 per million (at time of writing)
    >   * Message retention - 1 day
    > * **Standard**
    >   * 20 Consumer Group
    >   * 1000 Brokered Connections
    >   * Ingress events - $0.028 per million (at time of writing)
    >   * Message retention - 1 day
    >   * Additional storage - up to 7 days
    >   * Publisher policies

1. To the right of **Throughput units**, leave the selection at **1**.

    > **TIP**: The throughput capacity of Event Hubs is controlled by throughput units. Throughput units are pre-purchased units of capacity. A single throughput lets you:
    >
    > * Ingress: Up to 1 MB per second or 1000 events per second (whichever comes first).
    > * Egress: Up to 2 MB per second or 4096 events per second.

1. To start the validation of the data entered, click **Review + create**.

1. Once validation has succeeded, click **Create**.

    After a few moments the resource will be deployed. Click **Go to resource**.

This namespace will contain the event hub used to integrate the digital twin telemetry with an Azure function, and an event hub that will take the output of the Azure function and integrate it with Time Series Insights.

#### Task 2 - Add an event hub for ADT

This task will create an Event Hub that will subscribe to the twin telemetry events and pass them to an Azure Function.

1. On the **Overview** page of the **evhns-az220-training-{your-id}** namespace, click **+ Event Hub**.

1. On the **Create Event Hub** page, under **Name**, enter **evh-az220-adt2func**

1. Under **Partition Count**, leave the value as **1**.

    > **TIP**: Partitions are a data organization mechanism that relates to the downstream parallelism required in consuming applications. The number of partitions in an event hub directly relates to the number of concurrent readers you expect to have.

1. Under **Message Retention**, leave the value as **1**.

    > **TIP**: This is the retention period for events. You can set the retention period between 1 and 7 days.

1. Under **Capture**, leave the value set to **Off**.

    > **TIP**: Azure Event Hubs Capture enables you to automatically deliver the streaming data in Event Hubs to an Azure Blob storage or Azure Data Lake Store account of your choice, with the added flexibility of specifying a time or size interval. Setting up Capture is fast, there are no administrative costs to run it, and it scales automatically with Event Hubs throughput units. Event Hubs Capture is the easiest way to load streaming data into Azure, and enables you to focus on data processing rather than on data capture.

1. To create the Event Hub, click **Create**.

    After a moment, the Event Hub is created and Event Hubs namespace **Overview** is displayed. If necessary, scroll to the bottom of the page and the **evh-az220-adt2func** Event Hub is listed.

#### Task 3 - Add an authorization rule to the event hub

Each Event Hubs namespace and each Event Hubs entity (an event hub instance or a Kafka topic) has a shared access authorization policy made up of rules. The policy at the namespace level applies to all entities inside the namespace, irrespective of their individual policy configuration. For each authorization policy rule, you decide on three pieces of information: name, scope, and rights. The name is a unique name in that scope. The scope is the URI of the resource in question. For an Event Hubs namespace, the scope is the fully qualified domain name (FQDN), such as `https://evhns-az220-training-{your-id}.servicebus.windows.net/`.

The rights provided by the policy rule can be a combination of:

* Send – Gives the right to send messages to the entity
* Listen – Gives the right to listen or receive to the entity
* Manage – Gives the right to manage the topology of the namespace, including creation and deletion of entities

In this task, an authorization rule with **Listen** and **Send** permissions will be created.

1. On the **Overview** page of the **evhns-az220-training-{your-id}** namespace, click the **evh-az220-adt2func** Event Hub.

1. In the left navigation area, under **Settings**, click **Shared access policies**.

    An empty list of policies that are specific to his event hub is shown.

1. To create a new authorization rule, click **+ Add**.

1. In the **Add SAS Policy** pane, under policy name, enter **ADTHubPolicy**.

1. In the list of permissions, check only **Send** and **Listen**.

1. Click **Create** to save the authorization rule.

    After a moment, the pane closes and list of policies refresh.

1. To retrieve the primary connection string for the authorization rule, click **ADTHubPolicy** in the list.

    The **SAS Policy: ADTHubPolicy** pane will open.

1. Copy the **Connection string-primary key** value and add it to a new text file named **telemetry-function.txt**.

1. Close the **SAS Policy: ADTHubPolicy** pane.

#### Task 4 - Add an event hub endpoint to the ADT instance

Now that the Event Hub is created, it must be added as an endpoint that the ADT instance can use as an output to send events.

1. Navigate to the **adt-az229-training-{your-id}** instance.

1. In the left navigation area, under **Connect outputs**, click **Endpoints**.

    A list of endpoints, including the **eventgrid-endpoint** that was created earlier, will be displayed.

1. To add a new endpoint, click **+ Create an endpoint**.

1. In the **Create an endpoint** pane, under **Name**, enter **eventhub-endpoint**.

1. Under **Endpoint type**, select **Event Hub**.

    The UI updates to include fields to specify the Event Hub details.

1. In the **Subscription** dropdown, ensure that the Azure subscription that you intend to use for this course is selected.

1. In the **Event hub namespace** dropdown, select **evhns-az220-training-{your-id}**.

1. In the **Event Hub** dropdown, select **evh-az220-adt2func**.

1. In the  **Authorization rule** dropdown, select **ADTHubPolicy**.

   This is the authorization rule that was created earlier.

1. To create the endpoint, click **Save**.

    The pane closes and, after a moment, the endpoint list updates to include the new endpoint.

#### Task 5 - Add a route to send telemetry to the event hub

With the addition of the Event Hub endpoint to the ADT instance, it is now necessary to create a route that sends twin telemetry events to this endpoint.

1. In the left navigation area, under **Connect outputs**, click **Event routes**.

    A list of existing routes are displayed, including the **eventgrid-updateeventroute** that was created earlier.

1. To add a new event route, click **+ Create an event route**.

1. In the **Create an event route** pane, under **Name**, enter **eventhub-telemetryeventroute**.

1. In the **Endpoint** dropdown, select **eventhub-endpoint**.

1. In the **Event types** dropdown, select only **Telemetry**.

1. To create the event route, click **Save**.

    The pane closes and, after a moment, the event route list updates to include the new route.

#### Task 6 - Create TSI Event Hub and Policy

This time, the Event Hub and authorization rule will be created using the Azure CLI.

1. Return to the command prompt and verify the session is still logged in by entering:

    ```powershell
    az account list
    ```

    If prompted to run `az login`, do so and login.

1. To create the Event Hub between the Azure Function and TSI, enter the following command:

    ```powershell
    az eventhubs eventhub create --name evh-az220-func2tsi --resource-group rg-az220 --namespace-name evhns-az220-training-{your-id}
    ```

    Remember to replace **{your-id}**.

1. To create an authorization rule with listen and send permissions on the new Event Hub, enter the folowing command:

    ```powershell
    az eventhubs eventhub authorization-rule create --rights Listen Send --resource-group rg-az220  --eventhub-name evh-az220-func2tsi --name TSIHubPolicy --namespace-name evhns-az220-training-{your-id}
    ```

    Remember to replace **{your-id}**.

1. To retrieve the primary connection string for the authorization rule, enter the following command:

    ```powershell
    az eventhubs eventhub authorization-rule keys list --resource-group rg-az220 --eventhub-name evh-az220-func2tsi --name TSIHubPolicy --namespace-name evhns-az220-training-{your-id} --query primaryConnectionString -o tsv
    ```

    Remember to replace **{your-id}**.

    The output will be similar to:

    ```text
    Endpoint=sb://evhns-az220-training-dm030821.servicebus.windows.net/;SharedAccessKeyName=TSIHubPolicy;SharedAccessKey=x4xItgUG6clhGR9pZe/U6JqrNV+drIfu1rlvYHEdk9I=;EntityPath=evh-az220-func2tsi
    ```

1. Copy the **Connection string-primary key** value and add it to the text file named **telemetry-function.txt**.

    The two connection strings will be required in the next task.

#### Task 7 - Add the Endpoint addresses as app settings for Azure Function

In order for an Azure Function to connect to an Event Hub, it must have access to the connection string for a policy with the appropriate rights. In this scenario, two Event Hubs are involved  - the hub that publishes the event from ADT and the hub that publishes the data transformed by the Azure Function to TSI.

1. To supply the Event Hub authorization rule connections strings as environments variable, in the Azure portal, navigate to the **func-az220-hub2adt-training-{your-id}** instance.

1. In the left navigation area, under **Settings**, click **Configuration**.

1. On the **Configuration** page, on the **Application settings** tab, review the current **Application settings** that are listed.

    The **ADT_SERVICE_URL** that was added earlier via the CLI should be listed.

1. To add an environment variable for the adt2func rule connection string, click **+ New application setting**.

1. On the **Add/Edit application setting** pane, in the **Name** field, enter **ADT_HUB_CONNECTIONSTRING**

1. In the **Value** field, enter the autrhorization rule connection string value that was saved to the **telemetry-function.txt** file in an earlier task and ends with `EntityPath=evh-az220-adt2func`.

    The value should be similar to `Endpoint=sb://evhns-az220-training-dm030821.servicebus.windows.net/;SharedAccessKeyName=ADTHubPolicy;SharedAccessKey=fHnhXtgjRGpC+rR0LFfntlsMg3Z/vjI2z9yBb9MRDGc=;EntityPath=evh-az220-adt2func`.

1. To close the pane, click **OK**.

    > **NOTE**: The setting is not yet saved.

1. To add an environment variable for the func2tsi rule connection string, click **+ New application setting**.

1. On the **Add/Edit application setting** pane, in the **Name** field, enter **TSI_HUB_CONNECTIONSTRING**

1. In the **Value** field, enter the autrhorization rule connection string value that was saved to the **telemetry-function.txt** file in an earlier task and ends with `EntityPath=evh-az220-func2tsi`.

    The value should be similar to `Endpoint=sb://evhns-az220-training-dm030821.servicebus.windows.net/;SharedAccessKeyName=TSIHubPolicy;SharedAccessKey=x4xItgUG6clhGR9pZe/U6JqrNV+drIfu1rlvYHEdk9I=;EntityPath=evh-az220-func2tsi`

1. To close the pane, click **OK**.

    > **NOTE**: The setting is not yet saved.

1. To save the both of the new settings, click **Save** and click **Continue**.

    > **NOTE**: Any change to the application settings will restart the functions.

#### Task 8 - Add a telemetry Azure Function

In this task, another Azure function will be added to the **func-az220-hub2adt-training-{your-id}** function app. This function will be responsible for mapping the device telemetry messages to an alternate format for TSI. This approach has the advantage of being able to handle changes to the device telemetry format without changing the TSI solution.

1. In Visual Studio Code, open the **Contoso.AdtFunctions** project.

1. To add a new function to the existing project, on the View menu, click **Command Palette** and then enter **Azure Functions: Create Function**

1. Provide the following information at the prompts:

   * **Select a template for your function**: Select **Change template filter**.
   * **Select a template filter**: Select **All**.
   * **Select a template for your function**: Select **EventHubTrigger**.
   * **Provide a function name**: Type **TelemetryFunction**.
   * **Provide a namespace**: Type **Contoso.AdtFunctions**.
   * **Select setting from local.settings.json**: Press ENTER.
   * **Select subscription**: Select the subscription you're using.
   * **Select an event hub namespace**: Select **evhns-az220-training-{your-id}**.
   * **Select an event hub**: Select **evh-az220-adt2func**.
   * **Select an event hub policy**: Select **ADTHubPolicy**.
   * **When prompted for a storage account**: Select Skip for now.

    You should see the **TelemetryFunction.cs** file added to the project and opened for editing.

    > **TIP**: Double-check that **EventHubTrigger**, not **EventGridTrigger** was chosen.

1. Replace the `using` statements at the top of the file with the following:

    ```csharp
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.EventHubs;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    ```

1. Replace the **TelemetryFunction** class definition with the following code:

    ```csharp
    public static class TelemetryFunction
    {
        [FunctionName("TelemetryFunction")]
        public static async Task Run(
            [EventHubTrigger("evh-az220-adt2func", Connection = "ADT_HUB_CONNECTIONSTRING")] EventData[] events,
            [EventHub("evh-az220-func2tsi", Connection = "TSI_HUB_CONNECTIONSTRING")] IAsyncCollector<string> outputEvents,
            ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    // INSERT check telemetry below here
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
    ```

    Take a momment to look at the **Run** method definition. The **events** parameter makes use of the **EventHubTrigger** attribute - the attribute's constructor takes the name of the event hub, the **optional** name of the consumer group (**$Default** is used if omitted), and the name of an app setting that contains the connection string. This configures the function trigger to respond to an event sent to an event hub event stream. As **events** is defined as an array of EventData, it can be populated with a batch of events.

    > **TIP** To learn more about the **EventHubTrigger**, review the following resource:
    > [Azure Event Hubs trigger for Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-bindings-event-hubs-trigger?tabs=csharp)

    The next parameter, **outputEvents** has the **EventHub** attribute - the attribute's constructor takes the name of the event hub and the name of an app setting that contains the connection string. Adding data to the **outputEvents** variable will publish it to the associated Event Hub.

    As this function is processing a batch of events, a way to handle errors is to create a collection to hold exceptions. The function will then iterate through each event in the batch, catching exceptions and adding them to the collection. At the end of the function, if there are multiple exceptions, an **AggregaeException** is created with the collection, if a single exception is generated, then the single exception is thrown.

1. To add the code that checks to see if the event contains Cheese Cave Device telemetry, locate the `// INSERT check telemetry below here` comment and insert the following code below it:

    ```csharp
    if ((string)eventData.Properties["cloudEvents:type"] == "microsoft.iot.telemetry" &&
        (string)eventData.Properties["cloudEvents:dataschema"] == "dtmi:com:contoso:digital_factory:cheese_factory:cheese_cave_device;1")
    {
        // INSERT TSI Event creation below here
    }
    else
    {
        log.LogInformation($"Not Cheese Cave Device telemetry");
        await Task.Yield();
    }
    ```

    This code checks that the current event is telemetry from a Cheese Cave Device ADT twin - if not, logs that it isn't and then forces the method to complete asynchronously - this can make better use of resources.

    > **TIP**: To learn more about the use of `await Task.Yield();` review the following resource:
    > * [Task.Yield Method](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task.yield?view=net-5.0)

1. To add the code that processes the event and creates a message for TSI, locate the `// INSERT TSI Event creation below here` comment and insert the following code below it:

    ```csharp
    // The event is Cheese Cave Device Telemetry
    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
    JObject message = (JObject)JsonConvert.DeserializeObject(messageBody);

    var tsiUpdate = new Dictionary<string, object>();
    tsiUpdate.Add("$dtId", eventData.Properties["cloudEvents:source"]);
    tsiUpdate.Add("temperature", message["temperature"]);
    tsiUpdate.Add("humidity", message["humidity"]);

    var tsiUpdateMessage = JsonConvert.SerializeObject(tsiUpdate);
    log.LogInformation($"TSI event: {tsiUpdateMessage}");

    await outputEvents.AddAsync(tsiUpdateMessage);
    ```

    As the **eventData.Body** is defined as an **ArraySegment**, rather than just an array, the portion of the underlying array that contains the **messageBody** must be extracted, and then deserialized.

    > **TIP**: To learn more about an **ArraySegment** review the following content:
    > * [ArraySegment&lt;T&gt; Struct](https://docs.microsoft.com/dotnet/api/system.arraysegment-1?view=net-5.0)

    A **Dictionary** is then instantiated to hold the key/value pairs that will make up the properties sent within the TSI event. Notice that the **cloudEvents:source** property (which contains the fully qualified twin ID - similar to `adt-az220-training-dm030821.api.eus.digitaltwins.azure.net/digitaltwins/sensor-th-0055`) is assigned to the **\$dtId** key. This key has special meaning as the Time Series Insights environment created during setup is using **\$dtId** as the **Time Series ID Property**.

    The **temperature** and **humidity** values are extracted from the message and added to the TSI update.

    The update is then serialized to JSON and added to the **outputEvents** which publishes the update to the Event Hub.

    The function is now ready to be published.

#### Task 9 - Publish the function app to Azure

1. In the Azure Functions extension for Visual Studio Code, select **Deploy to Function App**:

    ![Visual Studio Code deploy to function app](media/LAB_AK_19-deploy-to-function-app.png)

1. When prompted, make these selections:

    * **Select subscription**: Select the subscription you are using for this course.
    * **Select Function App in Azure**: Select **func-az220-hub2adt-training-{your-id}**.

    When asked to confirm the deploy, click **Deploy**.

    The function will then be compiled and, if successful, deployed. This may take a few moments.

1. Once the deployment has completed, the following prompt will be displayed:

    ![Visual Studio Code deployment complete - select stream logs](media/LAB_AK_19-function-stream-logs.png)

    Click **Stream logs** and in the confirmation dialog to enable application logging, click **Yes**.

    The **OUTPUT** pane will now display the log stream for the deployed function - this will timeout after 2 hours. There will be some status information displayed, however there will not be any diagnostic information from the function itself until it is launched. That will be covered in the next exercise.

    The streaming can be stopped or started at any time by rght-clicking the Azure function in Visual Studio Code and select **Start Streaming Logs** or **Stop Streaming Logs**:

    ![Visual Studio Code Azure Function start streaming logs](media/LAB_AK_19-start-function-streaming.png)

#### Task 10 - Configure TSI

1. In a browser, connect to the Azure Portal and locate the **tsi-az220-training-{your-id}** resource.

    > **NOTE**: This resource was created by the setup script. If you have not run it, do so now - the script will not impact existing resources.

1. On the **Overview** pane, in the **Essentials** section, locate the **Time Series ID Property** field and the value - **$dtid**. This value was specified during the creation of the TSI environment and should match with a field in the event data being streamed to TSI.

    > **IMPORTANT**: The **Time Series ID Property** value is specified during the creation of the TSI environment and cannot be changed later.

1. In the left navigation area, under **Settings**, click **Event Sources**.

    The list of **Event Sources** will be displayed - at this point, it should be empty.

1. To add a new Event Source, click **+ Add**.

1. On the **New event source** pane, under **Event source name**, enter **adt-telemetry**

1. Under **Source**, select **Event Hub**.

1. Under **Import option**, select **Use Event Hub from available subscriptions**.

1. Under **Subscription ID**, select the subscription being used for this course.

1. Under **Event Hub namespace**, select **echns-az220-training-{your-id}**

1. Under **Event Hub name**, select **evh-az220-func2tsi**.

1. Under **Event Hub policy value**, select **TSIHubPolicy**.

    Notice that the read-only field **Event Hub policy key** is automatically populated.

1. Under **Event Hub consumer group**, select **$Default**.

    > **TIP**: As there is only one event reader for the **evh-az220-func2tsi** Event Hub, using the **$Default** consumer group is fine. If more readers were to be added, then it is recommended that one consumer group per reader is used. Consumer groups are created on the Event Hub.

1. Under **Event serialization format**, notice that the read-only value is **JSON**.

1. Under **Timestamp property name**, leave the value empty.

    > **TIP**: The name of the event property that should be used as the event timestamp. When not specified, event enqueue time within the event source will be used as the event timestamp.

1. To create the Event Source, click **Save**.

#### Task 11 - Visualize the telemetry data in Time Series Insights

Now, data should be flowing into your Time Series Insights instance, ready to be analyzed. Follow the steps below to explore the data coming in.

1. In the browser, return to the **Overview** pane of the **tsi-az220-training-{your-id}** resource.

1. To navigate to the **TSI Explorer**, click **Go to TSI Explorer**.

    The **TSI Explorer** will open in a new tab in the browser.

1. In the explorer, you will see Azure Digital Twins shown on the left.

    ![TSI Explorer data](media/LAB_AK_19-tsi-explorer.png)

1. To add telemetry to the graph, click the twin and select **EventCount**, **humidity** and **temperature**.

    Select an appropriate time range, and the displayed data will be similar to:

    ![TSI Explorer showing time series data](media/LAB_AK_19-tsi-explorer-data.png)

By default, digital twins are stored as a flat hierarchy in Time Series Insights, however they can be enriched with model information and a multi-level hierarchy for organization. You can write custom logic to automatically provide this information using the model and graph data already stored in Azure Digital Twins.
