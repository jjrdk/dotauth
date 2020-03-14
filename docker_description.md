The [SimpleAuth](https://github.com/jjrdk/simpleauth) images provide a ready to use OAuth/OpenID Connect server.

The auth server has port 80 exposed. It does not have any HTTPS bindings, in order to avoid installing a certificate.

It should only be used behind an SSL terminating proxy Do NOT pass user credentials over an insecure channel in production.

To run the server you will need to pass certain configuration values as environment variables:

- ApplicationName (optional): Sets the application name. Default is 'SimpleAuth'.
- Google:ClientId (optional): If set, configures Google as an external authentication provider. If null, Google authentication is disabled.
- Google:ClientSecret (optional): If set, configures the client secret of the Google external authentication.
- Google:Scopes (optional): If set, configures the identity scopes to request from Google. Default is openid, profile, email
- ConnectionString: This is the connection string to the PostgreSql database. Only used for postgres container.

Sometimes the server may not accept environment variable names with colon (:) in. In this case, it can often be replaced with double underscore (__).

The inmemory server comes with an existing user: administrator / password.

The postgres server requires the underlying database to be populated. Details later.