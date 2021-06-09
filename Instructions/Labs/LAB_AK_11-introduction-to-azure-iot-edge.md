---
lab:
    title: 'Lab 11: Introduction to Azure IoT Edge'
    module: 'Module 6: Azure IoT Edge Deployment Process'
---

# Introduction to Azure IoT Edge

## Lab Scenario

To encourage local consumers in a global marketplace, Contoso has partnered with local artisans to produce cheese in new regions around the globe.

Each location supports multiple production lines that are equipped with the mixing and processing machines that are used to create the local cheeses. Currently, the facilities have IoT devices connected to each machine. These devices stream sensor data to Azure and all data is processed in the cloud.

Due to the large amount of data being collected and the urgent time response needed on some of the machines, Contoso wants to use an IoT Edge gateway device to bring some of the intelligence to the edge for immediate processing. A portion of the data will still be sent to the cloud. Bringing data intelligence to the IoT Edge also ensures that they will be able to process data and react quickly even if the local network is poor.

You have been tasked with prototyping the Azure IoT Edge solution. To begin, you will be setting up an IoT Edge device that monitors temperature (simulating a device connected to one of the cheese processing machines). You will then deploy a Stream Analytics module to the device that will be used to calculate the average temperature and generate an alert notification if process control values are exceeded.

The following resources will be created:

![Lab 11 Architecture](media/LAB_AK_11-architecture.png)

## In This Lab

In this lab, you will complete the following activities:

* Verify that the lab prerequisites are met (that you have the required Azure resources)
* Deploy an Azure IoT Edge Enabled Linux VM
* Create an IoT Edge Device Identity in IoT Hub using Azure CLI
* Connect the IoT Edge Device to IoT Hub
* Add an Edge Module to the Edge Device
* Deploy Azure Stream Analytics as an IoT Edge Module

## Lab Instructions

### Exercise 1: Verify Lab Prerequisites

This lab assumes that the following Azure resources are available:

| Resource Type | Resource Name |
| :-- | :-- |
| Resource Group | rg-az220 |
| IoT Hub | iot-az220-training-{your-id} |

If these resources are not available, you will need to run the **lab11-setup.azcli** script as instructed below before moving on to Exercise 2. The script file is included in the GitHub repository that you cloned locally as part of the dev environment configuration (lab 3).

> **Note**:  The **lab11-setup.azcli** script is written to run in a **bash** shell environment - the easiest way to execute this is in the Azure Cloud Shell.

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
          * 11-Introduction to Azure IoT Edge
            * Setup

    The lab11-setup.azcli script file is located in the Setup folder for lab 11.

1. Select the **lab11-setup.azcli** file, and then click **Open**.

    A notification will appear when the file upload has completed.

1. To verify that the correct file has uploaded, enter the following command:

    ```bash
    ls
    ```

    The `ls` command lists the content of the current directory. You should see the lab11-setup.azcli file listed.

1. To create a directory for this lab that contains the setup script and then move into that directory, enter the following Bash commands:

    ```bash
    mkdir lab11
    mv lab11-setup.azcli lab11
    cd lab11
    ```

    These commands will create a directory for this lab, move the **lab11-setup.azcli** file into that directory, and then change directory to make the new directory the current working directory.

1. To ensure the **lab11-setup.azcli** has the execute permission, enter the following command:

    ```bash
    chmod +x lab11-setup.azcli
    ```

1. On the Cloud Shell toolbar, to enable access to the lab11-setup.azcli file, click **Open Editor** (second button from the right - **{ }**).

1. In the **Files** list, to expand the lab11 folder and open the script file, click **lab11**, and then click **lab11-setup.azcli**.

    The editor will now show the contents of the **lab11-setup.azcli** file.

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
    ./lab11-setup.azcli
    ```

    This will take a few minutes to run. You will see output as each step completes.

Once the script has completed, you will be ready to continue with the lab.

### Exercise 2: Deploy a Linux VM

In this exercise, you will deploy an Ubuntu Server VM.

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. In the **Search resources, services and docs** field, enter **Virtual machines**.

1. In the search results, under **Services**, click **Virtual machines**.

1. On the **Virtual machines** page, click **+ Add** and select **Virtual machine**.

1. On the **Create a virtual machine** blade, in the **Subscription** dropdown, select the Azure Subscription that you are using for this course.

1. To the right of **Resource group**, click **Create new**.

    > **Note**: You will be using a single resource group to help track/manage all of the Virtual Machine resources that you create during this course. You may encounter guidance that suggests creating a separate resource group for each of your VMs. In a production environment, having a separate resource group for each VM can help you to manage any addition resources that you add to the VM. For the simple manner in which you use VMs in this course, having separate resource groups for each VM is not necessary or practical.

1. In the popup for the new resource group, under **Name**, enter **rg-az220vm** and then click **OK**.

1. In the **Virtual machine name** textbox, enter **vm-az220-training-edge0001-{your-id}**

1. In the **Region** dropdown, select the region where your Azure IoT Hub is provisioned.

1. Leave **Availability options** set to **No infrastructure redundancy required**.

1. In the **Image** field, select **Ubuntu Server 18.04 LTS - Gen1** image.

1. Leave **Azure Spot instance** field unchecked.

1. To the right of **Size**, click **Change size**.

1. On the **Select a VM size** blade, under **VM Size**, click **Standard_B1ms**, and then click **Select**.

    If you don't see Standard_B1ms listed, you may need to use the **Clear all filters** link to make this size available in the list.

    > **Note**:  Not all VM sizes are available in all regions. If, in a later step, you are unable to select the VM size, try a different region. For example, if **West US** doesn't have the sizes available, try **West US 2**.

1. Under **Administrator account**, to the right of **Authentication type**, click **Password**.

1. For the VM Administrator account, enter values for the **Username**, **Password**, and **Confirm password** fields.

    > **Important:** Do not lose/forget these values - you cannot connect to your VM without them.

1. Notice that the **Inbound port rules** are configured to enable inbound **SSH** access to the VM.

    This will be used to remote into the VM to configure/manage it.

1. Click **Review + create**.

1. Wait for the **Validation passed** message to be displayed at the top of the blade, and then click **Create**.

    > **Note**:  Deployment can take as much as 5 minutes to complete. You can continue on to the next exercise while it is deploying.

### Exercise 3: Create an IoT Edge Device Identity in IoT Hub using Azure CLI

In this exercise, you will create a new IoT Edge Device Identity within Azure IoT Hub using the Azure CLI.

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. On the Azure portal toolbar, to open the Azure Cloud Shell, click **Cloud Shell**.

    On the Azure portal toolbar, not the left side navigation menu, the Cloud Shell button has an icon that looks similar ot a command prompt.

1. Ensure that you are using the **Bash** environment option.

    In the upper left corner of the Cloud Shell, Bash should be selected in the environment dropdown.

1. At the command prompt, to create an IoT Edge device identity in your IoT hub, enter the following command:

    ```bash
    az iot hub device-identity create --hub-name iot-az220-training-{your-id} --device-id sensor-th-0067 --edge-enabled
    ```

    Be sure to replace the `{your-id}` placeholder with the YOUR-ID value that you created at the start of this course.

    > **Note**: You could also create this IoT Edge device using your IoT Hub in the Azure portal: **IoT Hub** -> **IoT Edge** -> **Add an IoT Edge device**.

1. Review the output that the command created.

    Notice that the output contains information about the **Device Identity** that was created for the IoT Edge device. For example, you can see it defaults to `symmetricKey` authentication with auto-generated keys, and the `iotEdge` capability is set to `true` as indicated by the `--edge-enabled` parameter that was specified.

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
        "deviceId": "sensor-th-0067",
        "deviceScope": "ms-azure-iot-edge://sensor-th-0067-637093398936580016",
        "etag": "OTg0MjI1NzE1",
        "generationId": "637093398936580016",
        "lastActivityTime": "0001-01-01T00:00:00",
        "status": "enabled",
        "statusReason": null,
        "statusUpdatedTime": "0001-01-01T00:00:00"
    }
    ```

1. To display the **Connection String** for your IoT Edge device, enter the following command:

    ```bash
    az iot hub device-identity connection-string show --device-id sensor-th-0067 --hub-name iot-az220-training-{your-id}
    ```

    Be sure to replace the `{your-id}` placeholder with the YOUR-ID value that you created at the start of this course.

1. Copy the value of the `connectionString` from the JSON output of the command, and then save it for reference later.

    This connection string will be used to configure the IoT Edge device to connect to IoT Hub.

    ```json
        {
          "connectionString": "HostName={IoTHubName}.azure-devices.net;DeviceId=sensor-th-0067;SharedAccessKey=jftBfeefPsXgrd87UcotVKJ88kBl5Zjk1oWmMwwxlME="
        }
    ```

    > **Note**:  The IoT Edge Device Connection String can also be accessed within the Azure Portal, by navigating to **IoT Hub** -> **IoT Edge** -> **Your Edge Device** -> **Connection String (primary key)**

### Exercise 4: Install IotEdge and Connect IoT Edge Device to IoT Hub

In this exercise, you will connect the IoT Edge Device to Azure IoT Hub.

#### Task 1: Connect to the VM

1. Verify that the IoT Edge virtual machine has been deployed successfully.

    You can check the Notification pane in the Azure portal.

1. On the Azure portal menu, click **Resource groups**.

1. On the **Resource groups** blade, locate the rg-az220vm resource group.

1. On the right-hand side of the blade, across from **rg-az220vm**, click **Click to open context menu** (the ellipsis icon - **...**)

1. On the context menu, click **Pin to dashboard**, and then navigate back to your dashboard.

    You can **Edit** your dashboard to rearrange the tiles if that makes it easier to access your resources.

1. On the **rg-az220vm** resource group tile, to open the IoT Edge virtual machine, click **vm-az220-training-edge0001-{your-id}**.

1. At the top of the **Overview** pane, click **Connect**, and then click **SSH**.

1. On the **Connect** pane, under **4. Run the example command below to connect to your VM**, copy the example command.

    This is a sample SSH command that can be used to connect to the virtual machine that contains the IP Address for the VM and the Administrator username. The command should be formatted similar to `ssh username@52.170.205.79`.

    > **Note**: If the sample command includes `-i <private key path>`, use a text editor to remove that portion of the command, and then copy the updated command into the clipboard.

1. If Cloud Shell is not still open, click **Cloud Shell**.

1. At the Cloud Shell command prompt, paste the `ssh` command that you updated in the text editor, and then press **Enter**.

1. When prompted with **Are you sure you want to continue connecting?**, type `yes` and then press **Enter**.

    This prompt is a security confirmation since the certificate used to secure the connection to the VM is self-signed. The answer to this prompt will be remembered for subsequent connections, and is only prompted on the first connection.

1. When prompted to enter the password, enter the administrator password that you created when the VM was provisioned.

1. Once connected, the terminal command prompt will change to show the name of the Linux VM, similar to the following.

    ```bash
    username@vm-az220-training-edge0001-{your-id}:~$
    ```

    This tells you which VM you are connected to.

    > **Important:** When you connect, you will likely be told there are outstanding OS updates for the Edge VM.  We are ignoring this for our lab purposes, but in production, you always want to be sure to keep your Edge devices up-to-date.

#### Task 2: Add the Microsoft installation packages to the package manager

1. To configure the VM to access the Microsoft installation packages, run the following command:

    ```bash
    curl https://packages.microsoft.com/config/ubuntu/18.04/multiarch/prod.list > ./microsoft-prod.list
    ```

1. To add the downloaded package list to the package manager, run the following command:

    ```bash
    sudo cp ./microsoft-prod.list /etc/apt/sources.list.d/
    ```

1. To install the packages, the Microsoft GPG public key must be installed. Run the following commands:

    ```bash
    curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
    sudo cp ./microsoft.gpg /etc/apt/trusted.gpg.d/
    ```

    > **IMPORTANT**: Azure IoT Edge software packages are subject to the license terms located in each package **(usr/share/doc/{package-name}** or the **LICENSE** directory). Read the license terms prior to using a package. Your installation and use of a package constitutes your acceptance of these terms. If you do not agree with the license terms, do not use that package.

#### Task 3: Install a container engine

Azure IoT Edge relies on an OCI-compatible container runtime. For production scenarios, the Moby engine is recommended. The Moby engine is the only container engine officially supported with Azure IoT Edge. Docker CE/EE container images are compatible with the Moby runtime.

1. To update the package lists on the device, run the following command:

    ```bash
    sudo apt-get update
    ```

    This command may take a few minutes to run.

1. To install the **Moby** engine, run the following command:

    ```bash
    sudo apt-get install moby-engine
    ```

    If prompted to continue, enter **Y**. The install may take a few minutes.

#### Task 4: Install IoT Edge

The IoT Edge security daemon provides and maintains security standards on the IoT Edge device. The daemon starts on every boot and bootstraps the device by starting the rest of the IoT Edge runtime.

1. Usually, updating the package list is a good practice before installing a new package, however the packages were updated in the previous task.To update the package lists on the device, you would run the following command:

    ```bash
    sudo apt-get update
    ```

1. To list the versions of **IoT Edge runtime** that are available, run the following command:

    ```bash
    apt list -a iotedge
    ```

    > **TIP**: This command is useful if you need to install an earlier version of the runtime.

1. To install the latest version of the **IoT Edge runtime**, run the following command:

    ```bash
    sudo apt-get install iotedge
    ```

    If prompted to continue, enter **Y**. The install may take a few minutes.

    > **TIP**: If you wanted to install an earlier version that appeared in the output of the `apt list -a iotedge` command, say **1.0.9-1**, you would use the following command:
    > ```bash
    > sudo apt-get install iotedge=1.0.9-1 libiothsm-std=1.0.9-1
    > ```

1. To confirm that the Azure IoT Edge Runtime is installed on the VM, run the following command:

    ```bash
    iotedge version
    ```

    This command outputs the version of the Azure IoT Edge Runtime that is currently installed on the virtual machine.

#### Task 5: Configure connection string

This tasks walks through the steps to connect the VM with symmetric key authentication using the connection string from earlier.

1. To ensure that you are able to configure Azure IoT Edge, enter the following command:

    ```bash
    sudo chmod a+w /etc/iotedge/config.yaml
    ```

    To configure Azure IoT Edge, the **/etc/iotedge/config.yaml** configuration file needs to be modified to contain the full path to the certificate and key files on the IoT Edge Device. Before the file can be edited, you must be sure that the **config.yaml** file is not read-only. The command above sets the **config.yaml** file to be writable.

1. To open the **config.yaml** file within the vi/vim editor, enter the following command:

    ```bash
    sudo vi /etc/iotedge/config.yaml
    ```

    > **Note**: If you would rather use a different editor such as **code**, **nano**, or **emacs**, that's fine.

1. In the vi/vim editor, scroll down within the file until you locate the **Manual provisioning configuration using a connection string** section.

    > **Note**:  Here are some tips for using **vi** when editing the **config.yaml** file:
    > * Press the **i** key to put the editor into Insert mode, then you will be able to make changes.
    > * Press **Esc** to go stop Insert mode and return to Normal mode.
    > * To Save and Quit, type **:x** and then press **Enter**.
    > * Save the file, type **:w** and then press **Enter**.
    > * To quit vi, type **:quit** and then press **Enter**.
    >
    > You have to stop Insert mode before you can Save or Quit.

1. Find the provisioning configurations of the file and uncomment the Manual provisioning configuration using a connection string section, if it isn't already uncommented by removing the leading **'# '** (pound symbol and space) characters and replace `<ADD DEVICE CONNECTION STRING HERE>` with the Connection String you copied previously for your IoT Edge Device:

    ```yaml
    # Manual provisioning configuration using a connection string
    provisioning:
      source: "manual"
      device_connection_string: "<ADD DEVICE CONNECTION STRING HERE>"
      dynamic_reprovisioning: false
    ```

    > **Important**: YAML treats spaces as significant characters. In the lines entered above, this means that there should not be any leading spaces in front of **provisioning:** and that there should be two leading spaces in front of **source:**, **device_connection_string:**, and **dynamic_reprovisioning:**

1. To save your changes and exit the editor, press **Esc** and type **:x** and then press **Enter**

1. To apply the changes, the IoT Edge daemon must be restarted with the following command:

    ```bash
    sudo systemctl restart iotedge
    ```

1. To ensure the IoT Edge daemon is running, enter the following command:

    ```bash
    sudo systemctl status iotedge
    ```

    This command will display many lines of content, of which the first 3 lines indicate if the service is running. For a running service, the output will be similar to:

    ```bash
    ● iotedge.service - Azure IoT Edge daemon
       Loaded: loaded (/lib/systemd/system/iotedge.service; enabled; vendor preset: enabled)
       Active: active (running) since Fri 2021-03-19 18:06:16 UTC; 1min 0s ago
    ```

1. To verify the IoT Edge runtime has connected, run the following command:

    ```bash
    sudo iotedge check
    ```

    This runs a number of checks and displays the results. For this lab, ignore the **Configuration checks** warnings/errors. The **Connectivity checks** should succeed and be similar to:

    ```bash
    Connectivity checks
    -------------------
    √ host can connect to and perform TLS handshake with IoT Hub AMQP port - OK
    √ host can connect to and perform TLS handshake with IoT Hub HTTPS / WebSockets port - OK
    √ host can connect to and perform TLS handshake with IoT Hub MQTT port - OK
    √ container on the default network can connect to IoT Hub AMQP port - OK
    √ container on the default network can connect to IoT Hub HTTPS / WebSockets port - OK
    √ container on the default network can connect to IoT Hub MQTT port - OK
    √ container on the IoT Edge module network can connect to IoT Hub AMQP port - OK
    √ container on the IoT Edge module network can connect to IoT Hub HTTPS / WebSockets port - OK
    √ container on the IoT Edge module network can connect to IoT Hub MQTT port - OK
    ```

    If the connection fails, double-check the connection string value in **config.yaml**.

### Exercise 5: Add Edge Module to Edge Device

In this exercise, you will add a Simulated Temperature Sensor as a custom IoT Edge Module, and deploy it to run on the IoT Edge Device.

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. On your Resource group tile, to open your IoT Hub, click **iot-az220-training-{your-id}**.

1. At the left of the **IoT Hub** blade, under **Automatic Device Management**, click **IoT Edge**.

1. On the list of IoT Edge Devices, click **sensor-th-0067**.

1. On the **sensor-th-0067** blade, notice that the **Modules** tab displays a list of the modules currently configured for the device.

    Currently, the IoT Edge device is configured with only the Edge Agent (`$edgeAgent`) and Edge Hub (`$edgeHub`) modules that are part of the IoT Edge Runtime.

1. At the top of the **sensor-th-0067** blade, click **Set Modules**.

1. On the **Set modules on device: sensor-th-0067** blade, locate the **IoT Edge Modules** section.

1. Under **IoT Edge Modules**, click **Add**, and then click **IoT Edge Module**.

1. On the **Add IoT Edge Module** pane, under **IOT Edge Module Name**, enter **tempsensor**

    We will be naming our custom module "tempsensor"

1. Under **Image URI**, enter **asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor**

    > **Note**: This image is a published image on Docker Hub that is provided by the product group to support this testing scenario.

1. To change the selected tab, click **Module Twin Settings**.

1. To specify the desired properties for the module twin, enter the following JSON:

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

    This JSON configures the Edge Module by setting the desired properties of its module twin.

1. At the bottom of the blade, click **Add**.

1. On the **Set modules on device: sensor-th-0067** blade, at the bottom of the blade, click **Next: Routes >**.

1. Notice that a default route is already configured.

    * Name: **route**
    * Value: `FROM /messages/* INTO $upstream`

    This route will send all messages from all modules on the IoT Edge Device to IoT Hub

1. Click **Review + create**.

1. Under **Deployment**, take a minute to review the deployment manifest that is displayed.

    As you can see, the Deployment Manifest for the IoT Edge Device is formatted as JSON, which makes it pretty easy to read.

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
                            "status": "running",
                            "restartPolicy": "always",
                            "version": "1.0"
                       },
    ```

    Lower in the JSON is the **$edgeHub** section that contains the desired properties for the Edge Hub. This section also includes the routing configuration for routing events between modules, and to IoT Hub.

    ```json
        "$edgeHub": {
            "properties.desired": {
                "routes": {
                  "route": "FROM /messages/* INTO $upstream"
                },
                "schemaVersion": "1.1",
                "storeAndForwardConfiguration": {
                    "timeToLiveSecs": 7200
                }
            }
        },
    ```

    Lower still in the JSON is a section for the **tempsensor** module, where the `properties.desired` section contains the desired properties for the configuration of the edge module.

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

1. At the bottom of the blade, to finish setting the modules for the device, click **Create**.

1. On the **sensor-th-0067** blade, under **Modules**, notice that **tempsensor** is now listed.

    > **Note**: You may have to click **Refresh** to see the module listed for the first time.

    You may notice that the RUNTIME STATUS for **tempsensor** is not reported.

1. At the top of the blade, click **Refresh**.

1. Notice that the **RUNTIME STATUS** for the **tempsensor** module is now set to **running**.

    If the value is still not reported, wait a moment and refresh the blade again.

1. Open a Cloud Shell session (if it is not still open).

    If you are no longer connected to the `vm-az220-training-edge0001-{your-id}` virtual machine, connect using **SSH** as before.

1. At the Cloud Shell command prompt, to list the modules currently running on the IoT Edge Device, enter the following command:

    ```bash
    iotedge list
    ```

1. The output of the command look similar to the following.

    ```bash
    demouser@vm-az220-training-edge0001-{your-id}:~$ iotedge list
    NAME             STATUS           DESCRIPTION      CONFIG
    edgeHub          running          Up a minute      mcr.microsoft.com/azureiotedge-hub:1.1
    edgeAgent        running          Up 26 minutes    mcr.microsoft.com/azureiotedge-agent:1.1
    tempsensor       running          Up 34 seconds    asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor
    ```

    Notice that `tempsensor` is listed as one of the running modules.

1. To view the module logs, enter the following command:

    ```bash
    iotedge logs tempsensor
    ```

    The output of the command looks similar to the following:

    ```bash
    demouser@vm-az220-training-edge0001-{your-id}:~$ iotedge logs tempsensor
    11/14/2019 18:05:02 - Send Json Event : {"machine":{"temperature":41.199999999999925,"pressure":1.0182182583425192},"ambient":{"temperature":21.460937846433808,"humidity":25},"timeCreated":"2019-11-14T18:05:02.8765526Z"}
    11/14/2019 18:05:03 - Send Json Event : {"machine":{"temperature":41.599999999999923,"pressure":1.0185790159334602},"ambient":{"temperature":20.51992724976499,"humidity":26},"timeCreated":"2019-11-14T18:05:03.3789786Z"}
    11/14/2019 18:05:03 - Send Json Event : {"machine":{"temperature":41.999999999999922,"pressure":1.0189397735244012},"ambient":{"temperature":20.715225311096397,"humidity":26},"timeCreated":"2019-11-14T18:05:03.8811372Z"}
    ```

    The `iotedge logs` command can be used to view the module logs for any of the Edge modules.

1. The Simulated Temperature Sensor Module will stop after it sends 500 messages. It can be restarted by running the following command:

    ```bash
    iotedge restart tempsensor
    ```

    You do not need to restart the module now, but if you find it stops sending telemetry later, then go back into Cloud Shell, SSH to your Edge VM, and run this command to reset it. Once reset, the module will start sending telemetry again.

### Exercise 6: Deploy Azure Stream Analytics as IoT Edge Module

Now that the tempSensor module is deployed and running on the IoT Edge device, we can add a Stream Analytics module that can process messages on the IoT Edge device before sending them on to the IoT Hub.

#### Task 1: Create Azure Storage Account

1. If necessary, log in to your Azure portal using your Azure account credentials.

    If you have more than one Azure account, be sure that you are logged in with the account that is tied to the subscription that you will be using for this course.

1. On the Azure portal menu, click **+ Create a resource**.

1. On the **New** blade, in the search textbox type **storage** and then press **Enter**.

1. On the **Marketplace** blade, click **Storage account**.

1. On the **Storage account** blade, click **Create**.

1. On the **Create storage account** blade, ensure that the Subscription dropdown is displaying the subscription that you are using for this course.

1. In the **Resource group** dropdown, click **rg-az220**.

1. In the **Storage account name** textbox, enter **az220store{your-id}**.

    > **Note**: For this field, {your-id} must be entered in lower-case, and no dash or underline characters are allowed.

1. Set the **Location** field to the same Azure Region used for Azure IoT Hub.

1. Set the **Redundancy** field to **Locally-redundant storage (LRS)**.

1. Leave all other settings unchanged.

1. At the bottom of the blade, click **Review + create**.

1. Wait until you see the **Validation passed** message, and then click **Create**.

    It can take a few moments for the deployment to complete - you can continue creating the Stream Analytics resource while this is being created.

#### Task 2: Create an Azure Stream Analytics job

1. On the Azure portal menu, click **+ Create a resource**.

1. On the **New** blade, under **Azure Marketplace**, click **Internet of Things**, and then click **Stream Analytics job**.

    > **Note**:
    > You may need to click **See all** to see **Stream Analytics job** in the list.

1. On the **New Stream Analytics job** blade, in the **Job name** field, enter **asa-az220-training-{your-id}**

1. In the **Resource group** dropdown, click **rg-az220**.

1. In the **Location** dropdown, select the same Azure Region used for the Storage Account and Azure IoT Hub.

1. Set the **Hosting environment** field to **Edge**.

    This determines that the Stream Analytics job will deployed to an on-premises IoT Gateway Edge device.

1. At the bottom of the blade, click **Create**.

    It can take a few moments to for this resource to be deployed.

#### Task 3: Configure Azure Stream Analytics Job

1. When you see the **Your deployment is complete** message, click **Go to resource**.

    You should now be on the Overview pane of your new Stream Analytics job.

1. On the left side navigation menu, under **Job topology**, click **Inputs**.

1. On the **Inputs** pane, click **Add stream input**, and then click **Edge Hub**.

1. On the **Edge Hub** pane, in the **Input alias** field, enter **temperature**

1. In the **Event serialization format** dropdown, ensure that **JSON** is selected.

    Stream Analytics needs to understand the message format. JSON is the standard format.

1. In the **Encoding** dropdown, ensure that **UTF-8** is selected.

    > **Note**:  UTF-8 is the only JSON encoding supported at the time of writing.

1. In the **Event compression type** dropdown, ensure that **None** is selected.

    For this lab, compression will not be used. GZip and Deflate formats are also supported by the service.

1. At the bottom of the pane, click **Save**.

1. On the left side navigation menu, under **Job topology**, click **Outputs**.

1. On the **Outputs** pane, click **+ Add**, and then click **Edge Hub**.

1. On the **Edge Hub** pane, in the **Output alias** field, enter **alert**

1. In the **Event serialization format** dropdown, ensure that **JSON** is selected.

    Stream Analytics needs to understand the message format. JSON is the standard format, but CSV is also supported by the service.

1. In the **Format** dropdown, ensure that **Line separated** is selected.

1. In the **Encoding** dropdown, ensure that **UTF-8** is selected.

    > **Note**:  UTF-8 is the only JSON encoding supported at the time of writing.

1. At the bottom of the pane, click **Save**.

1. On the left side navigation menu, under **Job topology**, click **Query**.

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

    For more information about the `TumblingWindow` functions, reference this link: [https://docs.microsoft.com/en-us/stream-analytics-query/tumbling-window-azure-stream-analytics](https://docs.microsoft.com/en-us/stream-analytics-query/tumbling-window-azure-stream-analytics)

1. At the top of the query editor, click **Save query**.

#### Task 4: Configure Storage Account Settings

To prepare the Stream Analytics job to be deployed to an IoT Edge Device, it needs to be associated with an Azure Blob Storage container. When the job is deployed, the job definition is exported to the storage container.

1. On the **Stream Analytics job** blade, on the left side navigation menu under **Configure**, click **Storage account settings**.

1. On the **Storage account settings** pane, click **Add storage account**.

1. Under **Storage account settings**, ensure that **Select storage account from your subscriptions** is selected.

1. In the **Storage account** dropdown, ensure that the **az220store{your-id}** storage account is selected.

1. Under **Container**, click **Create new**, and then enter **jobdefinition** as the name of the container.

1. At the top of the pane, click **Save**.

    If prompted to confirm that you want to save the changes, click **Yes**

#### Task 5: Deploy the Stream Analytics Job

1. In the Azure portal, navigate to your **iot-az220-training-{your-id}** IoT Hub resource.

1. On the left side navigation menu, under **Automatic Device Management**, click **IoT Edge**.

1. Under **Device ID**, click **sensor-th-0067**.

1. At the top of the **sensor-th-0067** pane, click **Set Modules**.

1. On the **Set modules on device: sensor-th-0067** pane, locate the **IoT Edge Modules** section.

1. Under **IoT Edge Modules**, click **Add**, and then click **Azure Stream Analytics Module**.

1. On the **Edge deployment** pane, under **Subscription**, ensure that the subscription you are using for this course is selected.

1. In the **Edge job** dropdown, ensure that the **asa-az220-training-{your-id}** Steam Analytics job is selected.

    > **Note**:  The job may already be selected, yet the **Save** button is disabled - just open the **Edge job** dropdown again and select the **asa-az220-training-{your-id}** job again. The **Save** button should then become enabled.

1. At the bottom of the pane, click **Save**.

    Deployment may take a few moments.

1. Once the Edge package has been successfully published, notice that the new ASA module is listed under the **IoT Edge Modules** section

1. Under **IoT Edge Modules**, click **asa-az220-training-{your-id}**.

    This is the Steam Analytics module that was just added to your Edge device.

1. On the **Update IoT Edge Module** pane, notice that the **Image URI** points to a standard Azure Stream Analytics image.

    ```text
    mcr.microsoft.com/azure-stream-analytics/azureiotedge:1.0.8
    ```

    This is the same image used for every ASA job that gets deployed to an IoT Edge Device.

    > **Note**:  The version number at the end of the **Image URI** that is configured will reflect the current latest version when you created the Stream Analytics Module. At the time or writing this unit, the version was `1.0.8`.

1. Leave all values as their defaults, and close the **IoT Edge Custom Modules** pane.

1. On the **Set modules on device: sensor-th-0067** pane, click **Next: Routes >**.

    Notice that the existing routing is displayed.

1. Replace the default route defined with the following three routes:

    * Route 1
        * NAME: **telemetryToCloud**
        * VALUE: `FROM /messages/modules/tempsensor/* INTO $upstream`
    * Route 2
        * NAME: **alertsToReset**
        * VALUE: `FROM /messages/modules/asa-az220-training-{your-id}/* INTO BrokeredEndpoint("/modules/tempsensor/inputs/control")`
    * Route 3
        * NAME: **telemetryToAsa**
        * VALUE: `FROM /messages/modules/tempsensor/* INTO BrokeredEndpoint("/modules/asa-az220-training-{your-id}/inputs/temperature")`

    > **Note**: Be sure to replace the `asa-az220-training-{your-id}` placeholder with the name of your Azure Stream Analytics job module. You can click **Previous** to view the list of modules and their names, then click **Next** to come back to this step.

    The routes being defined are as follows:

    * The **telemetryToCloud** route sends the all messages from the `tempsensor` module output to Azure IoT Hub.
    * The **alertsToReset** route sends all alert messages from the Stream Analytics module output to the input of the **tempsensor** module.
    * The **telemetryToAsa** route sends all messages from the `tempsensor` module output to the Stream Analytics module input.

1. At the bottom of the **Set modules on device: sensor-th-0067** blade, click **Review + create**.

1. On the **Review + create** tab, notice that the **Deployment Manifest** JSON is now updated with the Stream Analytics module and the routing definition that was just configured.

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
                "alertsToReset": "FROM /messages/modules/asa-az220-training-CP122619/* INTO BrokeredEndpoint(\\\"/modules/tempsensor/inputs/control\\\")",
                "telemetryToAsa": "FROM /messages/modules/tempsensor/* INTO BrokeredEndpoint(\\\"/modules/asa-az220-training-CP122619/inputs/temperature\\\")"
            },
            "schemaVersion": "1.0",
            "storeAndForwardConfiguration": {
                "timeToLiveSecs": 7200
            }
        }
    },
    ```

1. At the bottom of the blade, click **Create**.

#### Task 6: View Data

1. Go back to the **Cloud Shell** session where you're connected to the **IoT Edge Device** over **SSH**.

    If it is closed or timed out, reconnect. Run the `SSH` command and login as before.

1. At the command prompt, to view a list of the modules deployed to the device, enter the following command:

    ```bash
    iotedge list
    ```

    It can take a minute for the new Stream Analytics module to be deployed to the IoT Edge Device. Once it's there, you will see it in the list output by this command.

    ```bash
    demouser@vm-az220-training-edge0001-{your-id}:~$ iotedge list
    NAME               STATUS           DESCRIPTION      CONFIG
    asa-az220-training-CP1119  running          Up a minute      mcr.microsoft.com/azure-stream-analytics/azureiotedge:1.0.5
    edgeAgent          running          Up 6 hours       mcr.microsoft.com/azureiotedge-agent:1.0
    edgeHub            running          Up 4 hours       mcr.microsoft.com/azureiotedge-hub:1.0
    tempsensor         running          Up 4 hours       asaedgedockerhubtest/asa-edge-test-module:simulated-temperature-sensor
    ```

    > **Note**:  If the Stream Analytics module does not show up in the list, wait a minute or two, then try again. It can take a minute for the module deployment to be updated on the IoT Edge Device.

1. At the command prompt, to watch the telemetry being sent from the Edge device by the `tempsensor` module, enter the following command:

    ```bash
    iotedge logs tempsensor
    ```

1. Take a minute to observe the output.

    While watching the temperature telemetry being sent by **tempsensor**, notice that a **reset** command is sent by the Stream Analytics job when the `machine.temperature` reaches an average above `25`. This is the action configured in the Stream Analytics job query.

    Output of this event will look similar to the following:

    ```bash
    11/14/2019 22:26:44 - Send Json Event : {"machine":{"temperature":231.599999999999959,"pressure":1.0095600761599359},"ambient":{"temperature":21.430643635304012,"humidity":24},"timeCreated":"2019-11-14T22:26:44.7904425Z"}
    11/14/2019 22:26:45 - Send Json Event : {"machine":{"temperature":531.999999999999957,"pressure":1.0099208337508767},"ambient":{"temperature":20.569532965342297,"humidity":25},"timeCreated":"2019-11-14T22:26:45.2901801Z"}
    Received message
    Received message Body: [{"command":"reset"}]
    Received message MetaData: {"MessageId":null,"To":null,"ExpiryTimeUtc":"0001-01-01T00:00:00","CorrelationId":null,"SequenceNumber":0,"LockToken":"e0e778b5-60ff-4e5d-93a4-ba5295b995941","EnqueuedTimeUtc":"0001-01-01T00:00:00","DeliveryCount":0,"UserId":null,"MessageSchema":null,"CreationTimeUtc":"0001-01-01T00:00:00","ContentType":"application/json","InputName":"control","ConnectionDeviceId":"sensor-th-0067","ConnectionModuleId":"asa-az220-training-CP1119","ContentEncoding":"utf-8","Properties":{},"BodyStream":{"CanRead":true,"CanSeek":false,"CanWrite":false,"CanTimeout":false}}
    Resetting temperature sensor..
    11/14/2019 22:26:45 - Send Json Event : {"machine":{"temperature":320.4,"pressure":0.99945886361358849},"ambient":{"temperature":20.940019742324957,"humidity":26},"timeCreated":"2019-11-14T22:26:45.7931201Z"}
    ```
