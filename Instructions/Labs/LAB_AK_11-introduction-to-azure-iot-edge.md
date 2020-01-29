---
lab:
    title: 'Lab 11: Introduction to Azure IoT Edge'
    module: 'Module 6: Azure IoT Edge Deployment Process'
---

# Introduction to Azure IoT Edge

## Lab Scenario

Contoso has cheese producing factories worldwide. Factories are equipped with production lines have multiple machines to create their cheeses. At the moment they have IoT devices connected to each machine that streams sensor data to Azure and processes all data in the cloud. Due to the large amount of data being collected and urgent time response needed on some of the machines, Contoso's wants to add a gateway device to bring the intelligence to the edge for processing data to the only send important data to the cloud. Plus, be able to process data and react quickly even if the local network is poor.

You will be setting up a new IoT Edge device that can monitor temperature of one of the machines and deploying Stream Analytics module to calculate the average temperature and send an alert to the device to act quickly.

## In This Lab

* Verify Lab Prerequisites
* Deploy Azure IoT Edge Enabled Linux VM
* Create IoT Edge Device Identity in IoT Hub using Azure CLI
* Connect IoT Edge Device to IoT Hub
* Add Edge Module to Edge Device
* Deploy Azure Stream Analytics as IoT Edge Module

## Lab Instructions

### Exercise 1: Verify Lab Prerequisites

[tbd]

### Exercise 2: Deploy Azure IoT Edge enabled Linux VM

In this exercise, you will deploy an Ubuntu Server VM with Azure IoT Edge runtime support from the Azure Marketplace.

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. In the Azure Portal, click **Create a resource** open the Azure Marketplace.

1. On the **New** blade, in the **Search the Marketplace** box, type in and search for **Azure IoT Edge on Ubuntu**.

1. In the search results, select the **Azure IoT Edge on Ubuntu** item.

1. On the **Azure IoT Edge on Ubuntu** item, click **Create**.

1. On the **Create a virtual machine** blade, select your Azure Subscription and use the **Create new** Resource group option to create a new Resource Group for the VM named `AZ-220-VM-RG`.

    > **Note**:  Resource management best practices in recommend placing Virtual Machines in

1. In the **Virtual machine name** box, enter `AZ-220-VM-EDGE` for the name of the Virtual Machine.

1. In the **Region** dropdown, select the Azure Region closest to you, or the region where your Azure IoT Hub is provisioned.

1. Notice the **Image** dropdown has the **Ubuntu Server 16.04 LTS + Azure IoT Edge runtime** image selected.

1. Under **Size**, click **Change size**. In the displayed list of sizes, select **DS1_v2** and click **Select**.

    > **Note**:  Not all VM sizes are available in all regions. If, in a later step, you are unable to select the VM size, try a different region. For example, if **West US** doesn't have the sizes available, try **West US 2**.

1. Under **Administrator account**, select the **Password** option for **Authentication type**.

1. Enter an Administrator **Username** and **Password** for the VM.

    >**Important:** Do not lose/forget these values - you cannot connect to your VM without them.

1. Notice the **Inbound port rules** is configured to enable inbound **SSH** access to the VM. This will be used to remote into the VM to configure/manage it.

1. Click **Review + create**.

1. Once validation passes, click **Create** to begin deploying the virtual machine.

    > **Note**:  Deployment will take approximately 5 minutes to complete. You can continue on to the next unit while it is deploying.

### Exercise 3: Create IoT Edge Device Identity in IoT Hub using Azure CLI

In this exercise, you will create a new IoT Edge Device Identity within Azure IoT Hub using the Azure CLI.

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. Open the Azure Cloud Shell by clicking the **Terminal** icon within the top header bar of the Azure portal, and select the **Bash** shell option.

1. Run the following Azure CLI command to create an **IoT Edge Device Identity** in Azure IoT Hub with the **Device ID** set to `myEdgeDevice`.

    ```cmd/sh
    az iot hub device-identity create --hub-name {IoTHubName} --device-id myEdgeDevice --edge-enabled
    ```

    Be sure to replace the `{IoTHubName}` placeholder with the name of the Azure IoT Hub in your subscription.

    > **Note**:  The IoT Edge Device Identity can also be created using the Azure Portal by navigating to **IoT Hub** -> **IoT Edge** -> **Add an IoT Edge device**.

1. Notice the output of the command contains information about the **Device Identity** that was created for the IoT Edge device. For example, you can see it defaults to `symmetricKey` authentication with auto-generated keys, and the `iotEdge` capability is set to `true` as indicated by the `--edge-enabled` parameter that was specified.

    ```json
        {
          "authentication": {
            "symmetricKey": {
              "primaryKey": "jftBfeefPsXgrd87UcotVKJ88kBl5Zjk1oWmMwwxlME=",
              "secondaryKey": "vbegAag/mTJReQjNvuEM9HEe1zpGPnGI2j6DJ7nECxo="
            },
            "type": "sas",
            "x509Thumbprint": {
              "primaryThumbprint": null,
              "secondaryThumbprint": null
            }
          },
          "capabilities": {
            "iotEdge": true
          },
          "cloudToDeviceMessageCount": 0,
          "connectionState": "Disconnected",
          "connectionStateUpdatedTime": "0001-01-01T00:00:00",
          "deviceId": "myEdgeDevice",
          "deviceScope": "ms-azure-iot-edge://myEdgeDevice-637093398936580016",
          "etag": "OTg0MjI1NzE1",
          "generationId": "637093398936580016",
          "lastActivityTime": "0001-01-01T00:00:00",
          "status": "enabled",
          "statusReason": null,
          "statusUpdatedTime": "0001-01-01T00:00:00"
        }
    ```

1. Once the IoT Edge Device Identity has been created, you can access the **Connection String** for the device by running the following Azure CLI command.

    ```cmd/sh
    az iot hub device-identity show-connection-string --device-id myEdgeDevice --hub-name {IoTHubName}
    ```

    Replace the `{IoTHubName}` placeholder with the name of the Azure IoT Hub in your subscription.

1. Copy the value of the `connectionString` from the JSON output of the command, and save it for reference later. This connection string will be used to configure the IoT Edge device to connect to IoT Hub.

    ```json
        {
          "connectionString": "HostName={IoTHubName}.azure-devices.net;DeviceId=myEdgeDevice;SharedAccessKey=jftBfeefPsXgrd87UcotVKJ88kBl5Zjk1oWmMwwxlME="
        }
    ```

    > **Note**:  The IoT Edge Device Connection String can also be accessed within the Azure Portal, by navigating to **IoT Hub** -> **IoT Edge** -> **Your Edge Device** -> **Connection String (primary key)**

### Exercise 4: Connect IoT Edge Device to IoT Hub

In this exercise, you will connect the IoT Edge Device to Azure IoT Hub.

1. Navigate to the `AZ-220-VM-EDGE` IoT Edge virtual machine within the Azure Portal.

1. On the **Overview** pane of the **Virtual machine** blade, click the **Connect** button at the top.

1. Within the **Connect to virtual machine** pane, select the **SSH** option, then copy the **Login using VM local account** value.

    This is a sample SSH command that will be used to connect to the virtual machine that contains the IP Address for the VM and the Administrator username. The command is formatted similar to `ssh demouser@52.170.205.79`.

1. At the top of the Azure Portal click on the **Cloud Shell** icon to open up the **Azure Cloud Shell** within the Azure Portal. When the pane opens, choose the option for the **Bash** terminal within the Cloud Shell.

1. Within the Cloud Shell, paste in the `ssh` command that was copied, and press **Enter**.

1. When prompted with **Are you sure you want to continue connecting?**, type `yes` and press Enter. This prompt is a security confirmation since the certificate used to secure the connection to the VM is self-signed. The answer to this prompt will be remembered for subsequent connections, and is only prompted on the first connection.

1. When prompted to enter the password, enter the Administrator password that was entered when the VM was provisioned.

1. Once connected, the terminal will change to show the name of the Linux VM, similar to the following. This tells you which VM you are connected to.

    ```cmd/sh
    demouser@AZ-220-VM-EDGE:~$
    ```

1. To confirm that the Azure IoT Edge Runtime is installed on the VM, run the following command:

    ```cmd/sh
    iotedge version
    ```

    This command will output the version of the Azure IoT Edge Runtime that is currently installed on the virtual machine.

1. You will need to run the command to configure the Edge device to connect to IoT Hub as Administrator. Run the following `sudo` command to elevate the terminal to run as Administrator:

    ```cmd/sh
    sudo su -
    ```

    > **Note**:  You will see the user id change in the shell prompt: `root@AZ-220-VM-EDGE:~$`

1. The `/etc/iotedge/configedge.sh` script is used to configure the Edge device with the Connection String necessary to connect it to Azure IoT Hub. This script is installed as part of the Azure IoT Edge Runtime.

1. To configure the Edge device with the **Device Connection String** for Azure IoT Hub that was copied when the IoT Edge Device ID was created, run the following command:

    ```cmd/sh
    /etc/iotedge/configedge.sh "{iot-edge-device-connection-string}"
    ```

    Be sure to replace the `{iot-edge-device-connection-string}` placeholder with the Connection String you copied previously for your IoT Edge Device.

1. Once this command completes, the IoT Edge Device will be configured to connect to Azure IoT Hub using the **Connection String** that was entered. The command will output a `Connection string set to ...` message that includes the Connection String that was set.

### Exercise 5: Add Edge Module to Edge Device

In this unit you will add a Simulated Temperature Sensor as a custom IoT Edge Module, and deploy it to run on the IoT Edge Device.

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. On your Resource group tile, click **AZ-220-HUB-_{YOUR-ID}_** to navigate to the Azure IoT Hub.

1. Under **Automatic Device Management**, click on **IoT Edge**.

1. Within the list of IoT Edge Devices, click on the `myEdgeDevice` Device ID for the Edge device that was created previously.

1. On the Device summary pane for the **myEdgeDevice** IoT Edge Device, notice the **Modules** tab displays a list of the modules currently configured for the device. Currently, the IoT Edge device is configured to only run the Edge Agent (`$edgeAgent`) and Edge Hub (`$edgeHub`) modules that are part of the IoT Edge Runtime.

1. On the IoT Edge Device blade, click on the **Set Modules** button at the top.

1. On the **Set modules** blade, locate the **IoT Edge Modules** sections, and click the **Add** button, then select **IoT Edge Module**.

1. On the **IoT Edge Custom Modules** pane, enter the following values to add a custom module named `tempsensor` using the Image URI for a simulated temperature sensor module.

    * IOT Edge Module Name: `tempsensor`
    * Image URI: `asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor`

1. Select the **Module Twin Settings** tab.

1. Enter the following JSON for the module twin's desired properties:

    ```json
    {
        "EnableProtobufSerializer": false,
        "EventGeneratingSettings": {
            "IntervalMilliSec": 500,
            "PercentageChange": 2,
            "SpikeFactor": 2,
            "StartValue": 20,
            "SpikeFrequency": 20
        }
    }
    ```

    This JSON configures the Edge Module by setting the **Desired** properties of its module twin.

1. Click **Add**.

1. On **Modules** step, of the **Set modules on device** pane, click **Next: Routes >**.

1. On the **Specify Routes** step, notice the default route is already configured that will send all messages from all modules on the IoT Edge Device to IoT Hub.

    * Name: **route**

    * Value: `FROM /messages/* INTO $upstream`

1. Click **Next: Review + create >**.

1. On the **Review Deployment** step, notice the JSON displayed in this pane. This is JSON is the **Deployment Manifest** for the IoT Edge Device.

    Under the `properties.desired` section is the `modules` section that declares the IoT Edge Modules that will be deployed to the IoT Edge Device. This includes the Image URIs of all the modules, including any container registry credentials.

    ```json
    {
        "modulesContent": {
            "$edgeAgent": {
                "properties.desired": {
                    "modules": {
                        "tempsensor": {
                            "settings": {
                                "image": "asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor",
                                "createOptions": ""
                            },
                            "type": "docker",
                            "version": "1.0",
                            "status": "running",
                            "restartPolicy": "always"
                        },
    ```

    Lower in the JSON is the **$edgeHub** section that contains the desired properties for the Edge Hub. This section also includes the routing configuration for routing events between modules, and to IoT Hub.

    ```json
        "$edgeHub": {
            "properties.desired": {
                "routes": {
                  "route": "FROM /messages/* INTO $upstream"
                },
                "schemaVersion": "1.0",
                "storeAndForwardConfiguration": {
                    "timeToLiveSecs": 7200
                }
            }
        },
    ```

    Lower in the JSON is a section for the **tempsensor** module, where the `properties.desired` section contains the desired properties for the configuration of the edge module.

    ```json
                },
                "tempsensor": {
                    "properties.desired": {
                        "EnableProtobufSerializer": false,
                        "EventGeneratingSettings": {
                            "IntervalMilliSec": 500,
                            "PercentageChange": 2,
                            "SpikeFactor": 2,
                            "StartValue": 20,
                            "SpikeFrequency": 20
                        }
                    }
                }
            }
        }
    ```

1. Click **Create** to finish setting the modules on the device.

1. Notice on the IoT Edge Device blade for the `myEdgeDevice` device, the list of **Modules** now includes the newly added `tempsensor` module.

    > **Note**:  You may have to click **Refresh** to see the module listed for the first time.

1. After a moment, click **Refresh** to update the current state of the Edge Device.

1. Notice the `tempsensor` module is now updated, displaying the **Runtime Status** as **running**.

1. Open a **Cloud Shell** session and connect to the `AZ-220-VM-EDGE` virtual machine using **SSH**.

1. Within Cloud Shell, run the following command to list out all the modules currently running on the IoT Edge Device.

    ```cmd/sh
    iotedge list
    ```

1. The output of the command look similar to the following. Notice `tempsensor` at the bottom of the list.

    ```cmd/sh
    demouser@AZ-220-VM-EDGE:~$ iotedge list
    NAME             STATUS           DESCRIPTION      CONFIG
    tempSensor       running          Up 34 seconds    asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor
    edgeAgent        running          Up 26 minutes    mcr.microsoft.com/azureiotedge-agent:1.0
    edgeHub          running          Up a minute      mcr.microsoft.com/azureiotedge-hub:1.0
    ```

1. The `iotedge logs` command can be used to view the module logs for the `tempsensor` module. Run the following command to view the module logs:

    ```cmd/sh
    iotedge logs tempSensor
    ```

    The output of the command looks similar to the following:

    ```cmd/sh
    demouser@AZ-220-VM-EDGE:~$ iotedge logs tempsensor
    11/14/2019 18:05:02 - Send Json Event : {"machine":{"temperature":41.199999999999925,"pressure":1.0182182583425192},"ambient":{"temperature":21.460937846433808,"humidity":25},"timeCreated":"2019-11-14T18:05:02.8765526Z"}
    11/14/2019 18:05:03 - Send Json Event : {"machine":{"temperature":41.599999999999923,"pressure":1.0185790159334602},"ambient":{"temperature":20.51992724976499,"humidity":26},"timeCreated":"2019-11-14T18:05:03.3789786Z"}
    11/14/2019 18:05:03 - Send Json Event : {"machine":{"temperature":41.999999999999922,"pressure":1.0189397735244012},"ambient":{"temperature":20.715225311096397,"humidity":26},"timeCreated":"2019-11-14T18:05:03.8811372Z"}
    ```

1. The Simulated Temperature Sensor Module will stop after it sends 500 messages. It can be restarted by running the following command:

    ```cmd/sh
    iotedge restart tempSensor
    ```

    You do not need to restart the module now, but if you find it stops sending telemetry later, then go back into the **Azure Cloud Shell** and run this command to reset it. Once reset, the module will start sending telemetry again.

### Exercise 6: Deploy Azure Stream Analytics as IoT Edge Module

Now that the tempSensor module is deployed and running on the IoT Edge device, we can add a Stream Analytics module that can process messages on the IoT Edge device before sending them on to the IoT Hub.

#### Task 1: Create Azure Storage Account

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. In the Azure Portal, click **Create a resource** to open the Azure Marketplace.

1. On the **New** blade, select the **Storage** category under **Azure Marketplace**, then click on **Storage account**.

1. On the **Create storage account**, select the existing `AZ-220-RG` group in the **Resource group** field.

1. Set the **Storage account name** field to something unique. This needs to be a globally unique name for the Azure Storage Account.

    To provide a globally unique name, enter **az220store{your-id}** - i.e. followed by your initials and the current date.

    > **Note**:  Your initials must be in lower-case for this resource and no dashes.

1. Set the **Location** to the same Azure Region used for Azure IoT Hub.

1. Click **Review + create**.

1. Once validation has passed, click **Create** to deploy the Storage Account.

    This will take a few moments to complete - you can continue creating the Stream Analytics resource while this is being created.

#### Task 2: Create Azure Stream Analytics

1. In the Azure Portal, click **Create a resource** to open the Azure Marketplace.

1. On the **New** blade, select the **Internet of Things** category under **Azure Marketplace**, then click on **Stream Analytics job**.

1. On the **New Stream Analytics job** blade, enter **AZ-220-ASA-_{YOUR-ID}_** into the **Job name** field followed by your initials and the current date to make sure it's a unique name.

1. In the **Resource group** field, select the existing **AZ-220-RG** group in the **Resource group** field.

1. Set the **Location** to the same Azure Region used for the Storage Account and Azure IoT Hub.

1. Set the **Hosting environment** to **Edge**. This determines that the Stream Analytics job will deployed to an on-premises IoT Gateway Edge device.

1. Click **Create**.

    It will take a few moments to for this resource to be completed.

#### Task 3: Configure Azure Stream Analytics Job

1. Once the **Stream Analytics job** has provisioned, navigate to the resource.

1. In the left side navigation, click on **Inputs** under the **Job topology** section.

1. On the **Inputs** pane, click **Add stream input**, then select **Edge Hub**.

1. On the **New Input** pane, enter `temperature` in the **Input alias** field.

1. In the **Event serialization format** dropdown, select **JSON**. Stream Analytics needs to understand the message format. JSON is the standard format.

1. In the **Encoding** dropdown, select **UTF-8**.

    > **Note**:  UTF-8 is the only JSON encoding supported at the time of writing.

1. In the **Event compression type** dropdown, select **None**.

    For this lab, compression will not be used. GZip and Deflate formats are also supported by the service.

1. Click **Save**.

1. In the left side navigation, click on **Outputs** under the **Job topology** section.

1. On the **Outputs** pane, click **Add**, then select **Edge Hub**.

1. On the **New output** pane, enter `alert` in the **Output alias** field.

1. In the **Event serialization format** dropdown, select **JSON**. Stream Analytics needs to understand the message format. JSON is the standard format, but CSV is also supported by the service.

1. In the **Format** dropdown, select **Line separated**.

1. In the **Encoding** dropdown, select **UTF-8**.

    > **Note**:  UTF-8 is the only JSON encoding supported at the time of writing.

1. Click **Save**.

1. In the left side navigation, click on **Query** under the **Job topology** section.

1. In the **Query** pane, replace the Default query with the following:

    ```sql
    SELECT  
        'reset' AS command
    INTO
        alert
    FROM
        temperature TIMESTAMP BY timeCreated
    GROUP BY TumblingWindow(second,15)
    HAVING Avg(machine.temperature) > 25
    ```

    This query looks at the events coming into the `temperature` Input, and groups by a Tumbling Windows of 15 seconds, then it checks if the average temperature value within that grouping is greater than 25. If the average is greater than 25, then it sends an event with the `command` property set to the value of `reset` to the `alert` Output.

    For more information about the `TumblingWindow` functions, reference this link: <https://docs.microsoft.com/en-us/stream-analytics-query/tumbling-window-azure-stream-analytics>

1. Click **Save query**.

#### Task 4: Configure Storage Account Settings

To prepare the Stream Analytics job to be deployed to an IoT Edge Device, it needs to be associated with an Azure Blob Storage container. When the job is deployed, the job definition is exported to the storage container.

1. On the **Stream Analytics job** blade, in the left side navigation, click **Storage account settings** under the **Configure** section.

1. Click **Add storage account**.

1. Select the **Select storage account from your subscription** option.

1. In the **Storage account** dropdown, select the **az220store{your-id}** storage account that was created previously.

1. Under **Container**, select **Create new**, then enter `jobdefinition` as the name of the container.

1. Click **Save**.

#### Task 5: Deploy the Stream Analytics Job

1. In the Azure Portal, navigate to the **AZ-220-HUB-_{YOUR-ID}_** Azure IoT Hub resource.

1. In the left side navigation, click **IoT Edge** under the **Automatic Device Management** section.

1. Click on the **myEdgeDevice** IoT Edge device within the list of devices.

1. On the **Device Details** pane, click the **Set Modules** button at the top.

1. On the **Add Modules** step, locate the **IoT Edge Modules** section, then click **Add** and select **Azure Stream Analytics Module**.

1. In the **Edge job** dropdown, select the **Steam Analytics job** that was created previously.

    > **Note**:  The job may already be selected, yet the **Save** button is disabled - just open the **Edge job** dropdown again and select the **AZ-220-ASA-_{YOUR-ID}_** job again. The **Save** button should then become enabled.

1. Click **Save**. Deployment may take a few moments.

1. Under the **IoT Edge Modules** section, click on the **Steam Analytics Module** that was just added.

1. Notice the **Image URI** points to a standard Azure Stream Analytics image. This is the same image used for every job that gets deployed to an IoT Edge Device.

    ```text
    mcr.microsoft.com/azure-stream-analytics/azureiotedge:1.0.5
    ```

    > **Note**:  The version number at the end of the **Image URI** that is configured will reflect the current latest version when you created the Stream Analytics Module. At the time or writing this unit, the version was `1.0.5`.

1. Leave all values as their defaults, and close the **IoT Edge Custom Modules** pane.

1. Click **Next: Routes >**.

1. On the **Specify Routes** step, notice the existing routing is displayed.

1. Replace the default routes defined with the following three routes:

    * Route 1
        * Name: **telemetryToCloud**
        * Value: `FROM /messages/modules/tempsensor/* INTO $upstream`
    * Route 2
        * Name: **alertsToReset**
        * Value: `FROM /messages/modules/AZ-220-ASA-{YOUR-ID}/* INTO BrokeredEndpoint("/modules/tempsensor/inputs/control")`
    * Route 3
        * Name: **telemetryToAsa**
        * Value: `FROM /messages/modules/tempsensor/* INTO BrokeredEndpoint("/modules/AZ-220-ASA-{YOUR-ID}/inputs/temperature")`

    Be sure to replace the `AZ-220-ASA-{YOUR-ID}` placeholder with the name of your Azure Stream Analytics job module. You can click **Previous** to view the list of modules and their names, then click **Next** to come back to this step.

    The routes being defined are as follows:

    * The **telemetryToCloud** route sends the all messages from the `tempsensor` module output to Azure IoT Hub.
    * The **alertsToReset** route sends all alert messages from the Stream Analytics Module output to the input of the **tempsensor** module.
    * The **telemetryToAsa** route sends all messages from the `tempsensor` module output to the Stream Analytics Module input.

1. Click **Next: Review + create >**.

1. On the **Review + create** tab, notice the **Deployment Manifest** JSON is now updated with the Stream Analytics module and the routing definition that was just configured.

1. Notice the JSON configuration for the `tempsensor` Simulated Temperature Sensor module:

    ```json
    "tempsensor": {
        "settings": {
            "image": "asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor",
            "createOptions": ""
        },
        "type": "docker",
        "version": "1.0",
        "status": "running",
        "restartPolicy": "always"
    },
    ```

1. Notice the JSON configuration for the routes that were previously configured, and how they are configured in the JSON Deployment definition:

    ```json
    "$edgeHub": {
        "properties.desired": {
            "routes": {
                "telemetryToCloud": "FROM /messages/modules/tempsensor/* INTO $upstream",
                "alertsToReset": "FROM /messages/modules/AZ-220-ASA-CP122619/* INTO BrokeredEndpoint(\\\"/modules/tempsensor/inputs/control\\\")",
                "telemetryToAsa": "FROM /messages/modules/tempsensor/* INTO BrokeredEndpoint(\\\"/modules/AZ-220-ASA-CP122619/inputs/temperature\\\")"
            },
            "schemaVersion": "1.0",
            "storeAndForwardConfiguration": {
                "timeToLiveSecs": 7200
            }
        }
    },
    ```

1. Click **Create**.

#### Task 6: View Data

1. Go back to the **Cloud Shell** session where you're connected to the **IoT Edge Device** over **SSH**.

1. Run the following command to view a list of the modules deployed to the device:

    ```cmd/sh
    iotedge list
    ```

    It can take a minute for the new Stream Analytics module to be deployed to the IoT Edge Device. Once it's there, you will see it in the list output by this command.

    ```cmd/sh
    demouser@AZ-220-VM-EDGE:~$ iotedge list
    NAME               STATUS           DESCRIPTION      CONFIG
    AZ-220-ASA-CP1119  running          Up a minute      mcr.microsoft.com/azure-stream-analytics/azureiotedge:1.0.5
    edgeAgent          running          Up 6 hours       mcr.microsoft.com/azureiotedge-agent:1.0
    edgeHub            running          Up 4 hours       mcr.microsoft.com/azureiotedge-hub:1.0
    tempsensor         running          Up 4 hours       asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor
    ```

    > **Note**:  If the Stream Analytics module does not show up in the list, wait a minute or two, then try again. It can take a minute for the module deployment to be updated on the IoT Edge Device.

1. Run the `iotedge log` command within the Azure Cloud Shell SSH session on the **IoT Edge Device** to watch the telemetry being sent by the `tempsensor` Simulated Temperature Sensor module:

    ```cmd/sh
    iotedge logs tempsensor
    ```

1. Notice that while watching the temperature telemetry being sent by **tempsensor**, you will see the **reset** command sent by the Stream Analytics job when the `machine.temperature` reaches an average above `500` as configured in the Stream Analytics job query.

    Output of this event will look similar to the following:

    ```cmd/sh
    11/14/2019 22:26:44 - Send Json Event : {"machine":{"temperature":231.599999999999959,"pressure":1.0095600761599359},"ambient":{"temperature":21.430643635304012,"humidity":24},"timeCreated":"2019-11-14T22:26:44.7904425Z"}
    11/14/2019 22:26:45 - Send Json Event : {"machine":{"temperature":531.999999999999957,"pressure":1.0099208337508767},"ambient":{"temperature":20.569532965342297,"humidity":25},"timeCreated":"2019-11-14T22:26:45.2901801Z"}
    Received message
    Received message Body: [{"command":"reset"}]
    Received message MetaData: {"MessageId":null,"To":null,"ExpiryTimeUtc":"0001-01-01T00:00:00","CorrelationId":null,"SequenceNumber":0,"LockToken":"e0e778b5-60ff-4e5d-93a4-ba5295b995941","EnqueuedTimeUtc":"0001-01-01T00:00:00","DeliveryCount":0,"UserId":null,"MessageSchema":null,"CreationTimeUtc":"0001-01-01T00:00:00","ContentType":"application/json","InputName":"control","ConnectionDeviceId":"myEdgeDevice","ConnectionModuleId":"AZ-220-ASA-CP1119","ContentEncoding":"utf-8","Properties":{},"BodyStream":{"CanRead":true,"CanSeek":false,"CanWrite":false,"CanTimeout":false}}
    Resetting temperature sensor..
    11/14/2019 22:26:45 - Send Json Event : {"machine":{"temperature":320.4,"pressure":0.99945886361358849},"ambient":{"temperature":20.940019742324957,"humidity":26},"timeCreated":"2019-11-14T22:26:45.7931201Z"}
    ```

Once you have finished this lab, keep the resources around - you will need them for the next lab.
