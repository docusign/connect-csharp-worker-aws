# C#: Connect Worker for AWS

Repository: [connect-csharp-worker-aws](https://github.docusignhq.com/chen-ostrovski/connect-csharp-worker-aws)

## Introduction

This is an example worker application for
Connect webhook notification messages sent
via the [AWS SQS (Simple Queueing System)](https://aws.amazon.com/sqs/).

This application receives DocuSign Connect
messages from the queue and then processes them:

* If the envelope is complete, the application
  uses a DocuSign JWT Grant token to retrieve
  the envelope's combined set of documents,
  and stores them in the `output` directory.
  
   For this example, the envelope **must** 
   include an Envelope Custom Field
   named `Sales order.` The Sales order field is used
   to name the output file.

## Architecture

![Connect listener architecture](connect-csharp-worker-aws/common/Resources/connect_listener_architecture.png)

AWS has [SQS](https://aws.amazon.com/tools/)
SDK libraries for C#, Java, Node.js, Python, Ruby, C++, and Go. 

## Installation

### Introduction
First, install the **Lambda listener** on AWS and set up the SQS queue.

Then set up this code example to receive and process the messages
received via the SQS queue.

### Installing the Lambda Listener

Install the example 
   [Connect listener for AWS](https://github.com/docusign/connect-node-listener-aws)
   on AWS.
   At the end of this step, you will have the
   `Queue URL`, `Queue Region` and `Enqueue url` that you need for the next step.

### Installing the worker (this repository)

#### Requirements
Requirements: C# and .NET Core 2.1 or later.
This repository has been tested as a Visual Studio 2017
Community Edition solution.

1. Download or clone this repository.

1. Using AWS IAM, create an IAM `User` with access to your SQS queue.

1. Configure the **App.config** file: [App.config](connect-csharp-worker-aws/App.config)
    The application uses the OAuth JWT Grant flow.

    If consent has not been granted to the application by
    the user, then the application provides a url
    that can be used to grant individual consent.

    **To enable individual consent:** either
    add the URL [https://www.docusign.com](https://www.docusign.com) as a redirect URI
    for the Integration Key, or add a different URL and
    update the `oAuthConsentRedirectURI` setting
    in the `App.config` file.

1.  Creating the Integration Key
    Your DocuSign Integration Key must be configured for a JWT OAuth authentication flow:
    * Create a public/private key pair for the key. Store the private key
    in a secure location. You can use a file or a key vault.
    * The example requires the private key. Store the private key in the
    [App.config](connect-csharp-worker-aws/App.config) file.
  
    **Note:** the private key's second and subsequent
    lines need to have a space added at the beginning due
    to requirements from the Python configuration file
    parser. Example:

````
# private key string
# NOTE: the csharp config file parser requires that you 
# add &#xA at the ending of every line of the multiline key value:  
DS_PRIVATE_KEY=-----BEGIN RSA PRIVATE KEY-----&#xA;
MIIEowIBAAKCAQEXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX&#xA;
N7b6a66DYU8/0BwXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX&#xA;
7lBHBbJcc76v+18XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX&#xA;
jCt15ZT4aux//2ZXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX&#xA;
....
n80GP7CRK+Ge6IePzEPpg9XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX&#xA;
-----END RSA PRIVATE KEY-----&#xA;
````  

### The impersonated user's guid
The JWT will impersonate a user within your account. The user can be
an individual or a user representing a group such as "HR".

The example needs the guid assigned to the user.

The guid value for each user in your account is available from
the DocuSign Admin tool in the **Users** section.
To see a user's guid, **Edit** the user's information.
On the **Edit User** screen, the guid for the user is shown as
the `API Username`.

## Run the examples

Build, then run the solution to see its output.

## Testing
Configure a DocuSign Connect subscription to send notifications to
the Cloud Function. Create / complete a DocuSign envelope.
The envelope **must include an Envelope Custom Field named "Sales order".**

* Check the Connect logs for feedback.
* Check the console output of this app for log output.
* Check the `output` directory to see if the envelope's
  combined documents and CoC were downloaded.

  For this code example, the 
  envelope's documents will only be downloaded if
  the envelope is `complete` and includes a 
  `Sales order` custom field, or if send from [SavingEnvelopeTest.cs](UnitTests/SavingEnvelopeTest.cs) unit test.

## Unit Tests
Includes three types of testing:
* [SavingEnvelopeTest.cs](UnitTests/SavingEnvelopeTest.cs) allow you to send an envelope to your amazon sqs from the program. The envelope is saved at `output` directory although its status is `sent`.

* [RunTest.cs](UnitTests/RunTest.cs) divides into two types of tests, both submits tests for 8 hours and updates every hour about the amount of successes or failures that occurred in that hour, the differences between the two are:
    * `few` - Submits 5 tests every hour.
    * `many` - Submits many tests every hour.

Note: Make sure that `UnitTests` uses a initialized config file.

In order to run the tests you need to open two windows of the program in Visual Studio, In the first one run the connect-csharp-worker-aws project. In the second window go to the `Test Explorer` choose the wanted test, right click on it and select `Run Selected Tests`.



## Support, Contributions, License

Submit support questions to [StackOverflow](https://stackoverflow.com). Use tag `docusignapi`.

Contributions via Pull Requests are appreciated. Pull requests for the common
files must be contributed to the 
[eg-01-csharp-jwt-common](https://github.com/docusign/eg-01-csharp-jwt-common)
repository.
See the [Contributing.md](https://github.com/docusign/eg-01-csharp-jwt-common/blob/master/docs/Contributing.md/)
file for information on contributing to this project.

All contributions must use the MIT License.

This repository uses the MIT license, see the
LICENSE file.
