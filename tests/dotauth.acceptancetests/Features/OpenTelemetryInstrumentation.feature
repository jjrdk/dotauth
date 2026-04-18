@opentelemetry
Feature: OpenTelemetry instrumentation
	Describe the telemetry the token server must publish once OpenTelemetry is implemented.

	Background:
		Given a running OpenTelemetry collector container
		And a running auth server
		And the server's signing key

	Scenario: Client credentials token request publishes success traces and metrics
		Given a client credentials token client with clientCredentials, clientCredentials
		When requesting token
		Then the collector eventually contains the following traces
			| span_name                          |
			| dotauth.token.request             |
			| dotauth.token.client_credentials  |
			| dotauth.client.authenticate       |
			| dotauth.token_store.get          |
			| dotauth.token_store.add          |
			| dotauth.jwks.get_signing_key     |
		And the collector eventually contains the following metrics
			| metric_name                    |
			| dotauth.tokens.issued          |
			| dotauth.token.issuance.duration |
			| dotauth.client.auth.success    |
			| dotauth.token_store.operation.duration |

	Scenario: Password token request publishes resource owner telemetry
		Given a client credentials token client with client, client
		When getting a token for user, password for scope openid
		Then the collector eventually contains the following traces
			| span_name                          |
			| dotauth.token.request             |
			| dotauth.token.password            |
			| dotauth.resource_owner.authenticate |
		And the collector eventually contains the following metrics
			| metric_name                         |
			| dotauth.tokens.issued              |
			| dotauth.resource_owner.auth.success |

	Scenario: Invalid client token request publishes failure traces and metrics
		Given a token client with invalid client credentials
		When attempting to request token
		Then the collector eventually contains the following traces
			| span_name                    |
			| dotauth.token.request       |
			| dotauth.client.authenticate |
		And the collector eventually contains the following metrics
			| metric_name                  |
			| dotauth.tokens.issue.failures |
			| dotauth.client.auth.failure  |

	Scenario: Refresh token and revocation publish lifecycle telemetry
		Given a client credentials token client with clientCredentials, clientCredentials
		When requesting auth token
		And refreshing the token for telemetry verification
		And revoking the token for telemetry verification
		Then the collector eventually contains the following traces
			| span_name                |
			| dotauth.token.request   |
			| dotauth.token.refresh   |
			| dotauth.token.revoke    |
		And the collector eventually contains the following metrics
			| metric_name                |
			| dotauth.refresh_tokens.used |
			| dotauth.tokens.revoked     |

	Scenario: Device authorization publishes device flow telemetry
		Given a device token client
		And an access token
		When a device requests authorization
		And the device polls the token server too fast
		And the device polls the token server polls properly
		And user successfully posts user code
		Then token is returned from polling
		And the collector eventually contains the following traces
			| span_name                        |
			| dotauth.device_authorization.request |
			| dotauth.token.request           |
			| dotauth.token.device_code       |
		And the collector eventually contains the following metrics
			| metric_name                      |
			| dotauth.device_authorization.started |
			| dotauth.device_code.polls        |

	Scenario: Authorization request publishes authorization telemetry
		Given a properly configured auth client
		When requesting authorization for scope openid
		Then has authorization uri
		And the collector eventually contains the following traces
			| span_name                    |
			| dotauth.authorization.request |
		And the collector eventually contains the following metrics
			| metric_name |
			| dotauth.authorization_codes.issued |

	Scenario: User info and introspection endpoints publish telemetry
		Given a client credentials token client with clientCredentials, clientCredentials
		And a properly configured uma client
		And a PAT token
		And a registered resource
		And an updated authorization policy
		When getting a ticket
		And getting an RPT token
		And introspecting the RPT token for telemetry verification
		And getting user information for telemetry verification
		Then the collector eventually contains the following traces
			| span_name                     |
			| dotauth.token.uma_ticket      |
			| dotauth.introspection.request |
			| dotauth.userinfo.request      |
		And the collector eventually contains the following metrics
			| metric_name                   |
			| dotauth.uma.rpt.issued        |
			| dotauth.introspection.requests |
			| dotauth.userinfo.requests     |

