# Simple Auth

![GitHub CI](https://github.com/jjrdk/simpleauth/actions/workflows/github.yml/badge.svg)

## Description

SimpleAuth is an authorization server SDK. The simplest way consume it is as a ready build container at [Docker Hub](https://hub.docker.com/r/jjrdk/simpleauth).

You can also use the SDK to create an authorization server and add more custom configurations. See the different [server examples](https://github.com/jjrdk/SimpleAuth/tree/master/src) on how to get started.

## Origin

SimpleAuth is based on the [SimpleIdentityServer](https://github.com/thabart/SimpleIdentityServer) project, but has been reduced and adjusted to make it more cloud friendly.

Most features have been merged into the simpleauth project.

All EntityFramework dependencies have been stripped away. It is up to you to provide your own implementation of repositories.

## Runtime Environment

The project runs under .NET 5.

This project has been tested to run in Docker, AWS Lambda and as a simple process, in both Windows and Linux.

## Supported Protocols

Supports OpenID Connect (OIDC), OAuth2 and UMA standards.

The support for SCIM has been removed.

## Building the Project

To build the project, run the build script (build.ps1 on Windows, build.sh on Linux/Mac). This will generate a set of nuget packages which can be used to integrate SimpleAuth into an ASP.NET Core server project.

See the example [Auth Server project](https://github.com/jjrdk/SimpleAuth/tree/master/src/simpleauth.authserver) for an example of how to use SimpleAuth as an auth server.

## Configuration Values for Demo Servers

The demo servers can be customized by setting the environment variables defined below. In addition to the application specific variables below, the standard ASP.NET environments can also be passed.

Note that some environment variables use double underscore ```__```. This is to ensure compatibility with the .NET conversion from environment variable to hierarchical configuration value.

|Environment Variable|Type|Description|
|---|---|---|
|SALT|string|Defines a hashing salt to be used. Default value is ```string.Empty```.|
|SERVER__NAME|string|Defines a custom name to display as the application name in UI headers. Default value is ```SimpleAuth```|
|SERVER__REDIRECT|bool|When set to ```true``` then requests for ```/``` or ```/home``` are redirected to ```/authenticate```. This effectively hides the default home page.|
|SERVER__ALLOWSELFSIGNEDCERT|bool|When set to ```true``` then allows self signed certificates and certificates whose root certificate is not trusted. The certificate must still be issued to a valid host.|
|SERVER__ALLOWHTTP|bool|When set to ```true``` then allows downloading OAuth metadata over HTTP. This option should only be set in development environments. Default value is ```false```|
|OAUTH__AUTHORITY|url string|Used to set the OAuth server where authorization for access to management UI.|
|OAUTH__VALIDISSUERS|comma separated url strings|The comma-separated set of valid issuers for access tokens.|
|DB__CONNECTIONSTRING|string|Sets the connection string when using a backing database.|
|DB__REDISCONFIG|string|Sets the connection string for the redis server.|
|AMAZON__ACCESSKEY|string|When set then the server will configure sms authentication.|
|AMAZON__SECRETKEY|string|When set then the server will configure sms authentication.|
|KNOWN_PROXIES|comma separated string|Sets the list of known proxy IP addresses.|

## Reporting Issues and Bugs

When reporting issues and bugs, please provide a clear set of steps to reproduce the issue. The best way is to provide a failing test case as a pull request.

If that is not possible, please provide a set of steps which allow the bug to be reliably reproduced. These steps must also reproduce the issue on a computer that is not your own.

## Contributions

All contributions are appreciated. Please provide them as an issue with an accompanying pull request.

This is an open source project. Work has gone into the [project](https://github.com/thabart/SimpleIdentityServer) it was forked from, as well as the later improvements.
Please respect the license terms and the fact that issues and contributions may not be handled as fast as you may wish. The best way to get your contribution adopted is to make it easy to pull into the code base.
