# Simple Auth

## Description

A minimalist take on the [SimpleIdentityServer](https://github.com/thabart/SimpleIdentityServer) project.

Most features have been merged into the simpleauth project.

All EntityFramework dependencies have been stripped away. It is up to you to provide your own implementation of repositories.

## Runtime Environment

The project runs under .NET core >= 2.1.

This project has been tested to run in Docker, AWS Lambda and as a simple process, in both Windows and Linux.

## Supported Protocols

Supports OpenID Connect (OIDC), OAuth2 and UMA standards.

The support for SCIM has been removed.

## Building the Project

To build the project, run the build script (build.ps1 on Windows, build.sh on Linux/Mac). This will generate a set of nuget packages which can be used to integrate SimpleAuth into an ASP.NET Core server project.

See the example [Auth Server project](https://github.com/jjrdk/SimpleAuth/tree/master/src/simpleauth.authserver) for an example of how to use SimpleAuth as an auth server.

## Reporting Issues and Bugs

When reporting issues and bugs, please provide a clear set of steps to reproduce the issue. The best way is to provide a failing test case as a pull request.

If that is not possible, please provide a set of steps which allow the bug to be reliably reproduced. These steps must also reproduce the issue on a computer that is not your own.

## Contributions

All contributions are appreciated. Please provide them as an issue with an accompanying pull request.

This is an open source project. Work has gone into the [project](https://github.com/thabart/SimpleIdentityServer) it was forked from, as well as the later improvements.
Please respect the license terms and the fact that issues and contributions may not be handled as fast as you may wish. The best way to get your contribution adopted is to make it easy to pull into the code base.
