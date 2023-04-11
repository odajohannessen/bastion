# Bastion
The Bastion secret sharing solution is a web application that makes sharing secrets fast, easy and safe for employees and customers of Bouvet. 
It offers sharing encrypted secrets through one-click links with a limited lifetime. Once the secret lifetime is reached, or the secret URL is accessed, the secret is deleted from storage permanently.
The secret URLs can be created anonymously, or a user can authenticate themselves in the web application, and be able to choose between one or more receivers of their choice prior to sharing the secret. 
If the URL somehow falls into the wrong hands, the secret is still protected by it's predefined receivers. A user accessing a URL where the sender has chosen any receivers will be prompted to log in to authenticate themselves. 
If they are one of the intended receivers, they will be able to view the secret, and if not, they are denied access.

## Core/Domain
### UserInputSecret
This folder represents the user input secret domain. It contains the UserInput object, UserInput DTO and a create user input pipeline. 
The purpose of this domain is to take the input data from the user, and create a result DTO which can be transferred to the encryption domain.

### Encryption
This folder represents the encryption domain. It has two objects, the UserSecret and the UserSecretJsonFormat. 
The pipeline EncryptAndSaveSecret takes as input the UserInput DTO, and utilizes the two services, IEncryptionService and IStorageService.

The IEncryption service takes the input plaintext message from the user and encrypts it. The encrypted message, key and IV are returned in a successful response from the service. 

The IStorageService takes as input the UserSecret object, and converts it to the JSON format object. The key is stored in the key vault, and the string in JSON format is stored in blob storage.

### Decryption
This folder represents the decryption domain. It does not contain any aggregate roots. 
The DecryptAndDeleteSecret pipeline takes as input a secret ID and potential OIDs of a user who is logged in. 
If the user is not logged in and remains anonymous, the default value of the OID is an empty string.

The pipelime utilizes two services, the IDecryptionService and IDeletionService. Before calling on any of the services, the pipeline checks if the secret has receivers.
If the secret has predefined receivers, and the user is not logged in, a response is returned prompting the user to login. 

The pipeline then gets the key from the key vault and the JSON format string from blob storage.

The IDecryptionService is called on the ciphertext if the logged in user is an inteded receiver, or if the secret has no receivers defined.

The IDeletionService is then called to delete the secret from the key vault and storage container, and the pipeline returns a response with the plaintext and the potential sender of the secret.

## Models
### UserInputModel
This model is utilized for taking user input in the case of a user creating a secret without logging in. The model inherits from the PageModel. 
It has two required parameters, the SecretPlainText and Lifetime. SecretPlainText has an upper limit of 5000 characters, and the Lifetime can be any integer value from 1 to 24 (hours). The default value is set to 1.
### AuthUserInputModel
This model is utilized for taking user input in the case of when an authenticated user creates a secret. The model inherits from the page model.
It has three required parameters, the SecretPlaintext and Lifetime, which are defined as above for UserInputModel. The OIDReiver is a string array which will contain the OID of any chosen receiver.

## Pages
### Index
The index page is structured to show different content to authenticated and unauthenticated users.
Both login and logout redirects to the index page.

### DisplaySecret
This page has the address /{id}, in case of a valid secret ID (GUID) format the DisplaySecret page is shown.
If the secret exists, and you are an inteded receiver, or the secret has no receivers, the secret will be displayed.
If the secret does not exist, or has already been viewed once, there will be an error message.

### Oops
In case a value is entered which does not correspond to a valid ID (GUID) format, the user is redirected to this page.

### About 
Short description of Bastion.

### Logout
A page setup to redirect to index after a user logs out. 

## Shared
Contains different razor components which are used in several pages.

## Managers
### LoggingManager
The logging manager sets up logging towards application insights. 

### CopyToClipBoardManager
The copy to clip board manager is utilized to copy text to the user's clip board.

## Helpers 
### GetSecretFromKeyVaultHelper
A helper method for retreiving a secret from the key vault. 

### GetUserAssignedDefaultCredentials
A helper method which returns Default Azure Credentials for the user assigned managed identity which is set up for the resources.

## Bastion.Tests
The test project contains four unit tests. 

# EndOfSecretLifetime
The EndOfSecretLifetime is a function app timer trigger which is run every 5 minutes. It checks if any secrets have expired, and deletes them if they have. 

## Managers
## StorageManager
The storage manager gets all blob names of the blobs currently in the blob container.
It checks the time stamp of the blob names. If there are any expired secrets, the key and the secret blob are deleted from key vault and storage account, respectively.

### LoggingManager
The logging manager sets up logging towards application insights. 

## Helpers
### GetSecretFromKeyVaultHelper
A helper method for retreiving a secret from the key vault. 

### GetUserAssignedDefaultCredentials
A helper method which returns Default Azure Credentials for the user assigned managed identity which is set up for the resources.