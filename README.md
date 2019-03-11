# Simple Auth

A minimalist take on the [SimpleIdentityServer](https://github.com/thabart/SimpleIdentityServer) project.

The project runs under .NET core 2.1.

This project has been tested to run in Docker, AWS Lambda and as a simple process.

Supports OpenID Connect, OAuth2 and UMA standards.

Most features have been merged into the simpleauth project.

All EntityFramework dependencies have been stripped away. It is up to you to provide your own implementation of repositories.
