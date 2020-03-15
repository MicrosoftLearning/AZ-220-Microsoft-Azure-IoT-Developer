YOUR_ID=""
LOCATION=""

while [[ -z "$YOUR_ID" ]] ; do
    echo ""
    read -p "Enter the Unique ID you created for this course: " YOUR_ID

    if [[ -z "$YOUR_ID" ]] 
    then
        echo -e "\nYour Unique ID cannot be empty!"
        continue
    fi

    if [[ ! $YOUR_ID =~ ^[A-Za-z0-9]+$ ]]
    then
        echo "Your Unique ID can only contain characters a-zA-Z0-9"
        YOUR_ID=""
        continue
    fi

done

while [[ -z "$LOCATION" ]] ; do
 
    echo -e "\nAzure Location List\n"
    az account list-locations -o table --query "[].{ShortName:name,DisplayName:displayName}"
    echo ""
    read -p "Enter the Short Name of your preferred location: " LOCATION

    if [[ -z "$LOCATION" ]] 
    then
        echo -e "\nYour location cannot be empty!"
        continue
    fi

    if [[ ! $LOCATION =~ ^[a-z0-9]+$ ]]
    then
        echo "Location must be lowercase/numeric  [a-z][0-9]"
        LOCATION=""
        continue
    fi

done


RG_NAME="AZ-220-RG"
IOT_HUB_NAME="AZ-220-HUB-$YOUR_ID"
CONTAINER_REGISTRY="AZ220ACR$YOUR_ID"

if [ $(az group exists --name $RG_NAME) = false ]; then
    echo -e "\nCreating Resouce Group named $RG_NAME in localtion $LOCATION"
    az group create --name $RG_NAME --location $LOCATION
fi

echo -e "\nCreating Azure IoT Hub named $IOT_HUB_NAME in Resource Group $RG_NAME"
az iot hub create --name $IOT_HUB_NAME -g $RG_NAME --sku S1 --location $LOCATION

echo -e "\nCreating Azure Container Registry named $CONTAINER_REGISTRY in Resource Group $RG_NAME"
az acr create --name $CONTAINER_REGISTRY --location $LOCATION --resource-group $RG_NAME --sku Standard