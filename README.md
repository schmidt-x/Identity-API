# Introduction

This API provides functionality for user registration, authentication and account management.

The API may be accessed on <link_to_deployed_app> (it's not deployed yet)

- [How to run](#how-to-run)
- [API Documentation](#api-documentation)
  - [Registration](#registration)
  - [Log in](#log-in)
  - [Refresh tokens](#refresh-tokens)
  - [Forgot password](#forgot-password)
  - [Get Me](#get-me)
  - [Username update](#username-update)
  - [Email update](#email-update)
  - [Password update](#password-update)
  - [Log out](#log-out)


# How to run

To run this app on your local machine:

1. make a copy of the .env.example file in the root folder:

	Unix:
	```bash
	cp .env.example .env
	```

	Windows:
	```bash
	copy .env.exapmle .env
	```

2. replace all the placeholders with your values.

3. run docker compose:

	```bash
	docker compose -p <top_level_container_name> up -d
	```

# API Documentation

## Registration

### Step 1:
To initiate the registration process by starting a session, make a POST request to the path:
	
`/api/auth/registration/start-session`
	
with the following request body:

```json
{
  "email": "user@example.com"
}
```

<br>

Response, if the email address is valid and not taken yet:
```
StatusCode: 200 (OK)
Headers:
- Set-Cookie: session_id=your_session_id; expires=<expiry_date>; secure; httponly
```
with the following response body:
```json
{
  "message": "response message"
}
```
Also, you will receive a verification code on your email address.

<br>

On failure, you will receive an [Error response](#error-response) body:
###### Error response
```json
{
  "errors": {
    "error_1": [
      "error message",
      "error message 2"
    ],
    "error_2": [
     "error message"
    ]
  }
}
```
with a status code:
- 400 (Bad Request) if:
  - email address is not valid
  - email address is already taken 

### Step 2:

To verify the session, make a PATCH request to the path:

`/api/auth/session/verify`

including the verification code you've received, into the request body:

```json
{
  "code": "verification-code"
}
```

<br>

Response, if the verification code is correct:
```
StatusCode: 204 (No content)
Headers:
- Set-Cookie: session_id=your_session_id; expires=expiry_date; secure; httponly
```
with no body.

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - session ID is not present
  - session is not found (expired)
  - verification code is wrong

### Step 3:

To finally finish the registration, make a POST request to the path:

`/api/auth/registration/register`

with the following request body:

```json
{
  "username": "vasya_pupkin",
  "password": "Pwd12345@"
}
``` 

###### Username restrictions
- Username length must be in the range of 3 to 32 characters, and can only contain:
  - letters ( a-z )
  - numbers ( 0-9 )
  - underscores ( _ )
  - and periods ( . )
###### Password restrictions
- Password minimum length must be at least 8 characters, and:
  - must contain at least one:
	- lower case letter
	- upper case letter
	- symbol
	- number
  - must not contain any white spaces

<br>

Response, if the restriction are considered and the username is not taken yet:

`StatusCode: 200 (OK)`

with a [Token Response](#token-response) body:

###### Token Response
```json
{
  "access_token": "your short-lived Jwt access token",
  "refresh_token": "your long-lived refresh token"
}
```
This is the body structure referred to as 'Token Response'.
Throughout this documentation, whenever it's mentioned, it pertains to this structure. 

Btw, you do know what these tokens are for, right?

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - username is already taken
  - username is invalid
  - password is invalid
  - session ID is not present
  - session is not found (expired)
  - session is not verified

## Log in

To log in, make a POST request to the path:

`/api/auth/login`

including your credentials into the request body:
```json
{
  "login": "user@example.com",
  "password": "Pwd12345@"
}
```

Your login is the email address, you used on [Registration](#registration).

<br>

Response, if the credentials are correct:

`StatusCode: 200 (OK)`

with the [Token Response](#token-response) body.

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - username/password validation failed
- 401 (Unauthorized) if:
  - username/password are wrong 

## Refresh tokens

To refresh your tokens, make a POST request to the path:

`/api/auth/refresh`

including the tokens into the request body:

```json
{
  "access_token": "your short-lived Jwt access token",
  "refresh_token": "your long-lived refresh token"
}
```

Note that the Access and Refresh tokens are connected to each other and should be the same pair as you received them. 

Also, after the refreshing, both tokens are invalidated and can no longer be used.

<br>

Response, if the tokens are valid:

`StatusCode: 200 (OK)`

with the [Token Response](#token-response) body.

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - tokens are missing
- 401 (Unauthorized) if:
  - access token is invalid
  - refresh token is invalid (already expired, used or invalidated)
  - wrong pair

## Forgot password

### Step 1:

To start the process of restoring your password, make a POST request to the path:

`/api/auth/forgot-password/start-session`

along with your email address in the request body:

```json
{
  "email": "user@example.com"
}
```

<br>

Response, if succeeded:
```
StatusCode: 200 (OK)
Headers:
- Set-Cookie: session_id=your_session_id; expires=expiry_date; secure; httponly
```

with a body:
```json
{
  "message": "response message"
}
```

Also, you will receive a verification code on your email.

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - email address is not valid
  - email address doesn't exist (not registered)

### Step 2:

This step is completely the same as [Step 2](#step-2) described on [Registration](#registration).

Just make a PATCH request to the path:

`/api/auth/session/verify`

including the verification code you've received, into the request body:

```json
{
  "code": "verification-code"
}
```

### Step 3:

To finish the restoring process, make a PATCH request to the path:

`/api/auth/forgot-password/restore`

with the new password, included into the request body:
```json
{
  "password": "Pwd12345@"
}
```

<br>

Response, if succeeded:

`StatusCode: 200 (OK)`

with the [Token Response](#token-response) body.

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - new password is invalid (see [Password restrictions](#password-restrictions))
  - new password is the same as the previous one
  - session ID is not present
  - session is not found (expired)
  - session is not verified


## Get Me

To get your profile, make a GET request to the path:

`/api/me`

with a [Bearer Authorization](#bearer-authorization) header:

###### Bearer Authorization
`Authorization: Bearer <your Jwt access token>`

Note that this header should always be included in all subsequent requests of the path:

`/api/me`


<br>

Response, if the token is valid:

`StatusCode: 200 (OK)`

with a [Me response](#me-response) body:
###### Me response
```json
{
  "username": "vasya_pupkin",
  "email": "user@example.com",
  "created_at": "1970-01-01T00:00:00Z",
  "updated_at": "1970-01-01T00:00:00Z",
  "role": "user",
  "token": "jwt_access_token"
}
```

Note that it's always recommended to use the returned token from the response body.<br>
At some endpoints, such as [Email update](#step-4) or [Password update](#password-update), it might be modified.

<br>

On failure, the [Error response](#error-response) with a status code:
- 401 (Unauthorized) if:
  - access token is invalid or expired


## Username update

To update your username, make a PATCH request to the path:

`/api/me/username-update`

with a request body:

```json
{
  "username": "your_new_username",
  "password": "your_password"
}
```

including the [Bearer Authorization](#bearer-authorization) header. 

<br>

Response, if succeeded:

`StatusCode: 200 (OK)`

with the [Me response](#me-response) body.

<br>

On failure, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - new username is not valid (see [Username restrictions](#username-restrictions))
  - new username is already taken
- 401 (Unauthorized) if:
  - access token is invalid or expired

## Email update

### Step 1:

To start the email updating process, make a POST request to the path:

`/api/me/email-update`

including the [Bearer Authorization](#bearer-authorization) header, with no request body.

<br>

Response, if succeeded:

`StatusCode: 200 (OK)`

with the following response body:

```json
{
  "message": "response message"
}
```

Also, you will receive a verification code on your email.

<br>

On failure, the [Error response](#error-response) with a status code:
- 401 (Unauthorized) if:
  - access token is invalid or expired


### Step 2:

To verify your old email address, make a PATCH request to the path:

`/api/me/email-update/verify-old-email`

including the [Bearer Authorization](#bearer-authorization) header, and the verification code you've received, into the request body:

```json
{
  "code": "verification code"
}
```

<br>

Response, if succeeded:

`StatusCode: 200 (OK)`

with the following response body:

```json
{
  "message": "response message"
}
```

<br>

If failed, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - verification code is wrong
- 401 (Unauthorized) if:
  - access token is invalid or expired

### Step 3:

To register a new email address, make a PATCH request to the path:

`/api/me/email-update/register-new-email`

including the [Bearer Authorization](#bearer-authorization) header, and a new email address into the the request body:

```json
{
  "email": "newEmail@example.com"
}
```

<br>

Response, if the new email address is valid and not taken yet:

`StatusCode: 200 (OK)`

with a response body:

```json
{
  "message": "response message"
}
```

<br>

If failed, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - email address is not valid
  - email address is already taken
- 401 (Unauthorized) if:
  - access token is invalid or expired

### Step 4:

Finally, to finish the process of updating email, verify the new email by making a PATCH request to the path:

`/api/me/email-update/verify-new-email`

including the [Bearer Authorization](#bearer-authorization) header, and the verification code you've received, into the request body:

```json
{
  "code": "verification code"
}
```

<br>

Response, if the verification code is correct:

`StatusCode: 200 (OK)`

with the [Me response](#me-response) body.

Note that the returned Token in the response body is the new valid access token (until it's expired). The previous access token is invalidated and can no longer be used.

The amount of time left until it's expired - is not changed.<br>
It's Refresh token pair is still valid and can be used on [Token refreshing](#refresh-tokens) process (and can only be used with the updated access token).

It's done because the Jwt token contains the email address in it's payload, and since the email has changed, the token should be updated too.
 
<br>

If failed, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - verification code is wrong
- 401 (Unauthorized) if:
  - access token is invalid or expired

## Password update

To update your password, make a PATCH request to the path:

`/api/me/password-update`

including the [Bearer Authorization](#bearer-authorization) header, with the following request body:

```json
{
  "password": "current_password",
  "new_password": "new_password"
}
```

<br>

Response, if succeeded:

`StatusCode: 200 (OK)`

with the [Me response](#me-response) body.

Note that the returned token in the response body is the new valid access token. The previous one is invalidated and can no longer be used. 

The amount of time left until it's expired - is not changed.<br>
The Refresh token pair is still valid and can be used on [Token refreshing](#refresh-tokens) process (and can only be used with the updated access token).

Also, all the other access and refresh tokens are now invalidated, and only you have an access to your account. 

<br>

If failed, the [Error response](#error-response) with a status code:
- 400 (Bad request) if:
  - current password is wrong
  - new password is invalid (see [Password restrictions](#password-restrictions))
  - new and current passwords are the same
- 401 (Unauthorized) if:
  - access token is invalid or expired


## Log out

To log out from all devices, make a POST request to the path:

`/api/me/log-out`

including the [Bearer Authorization](#bearer-authorization) header, with no body.

<br>

Response, if succeeded:

`StatusCode: 204 (No content)`

with no body.

All the access and refresh tokens are now invalidated.

<br>

If failed, the [Error response](#error-response) with a status code:
- 401 (Unauthorized) if:
  - access token is invalid or expired