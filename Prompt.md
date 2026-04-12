# Identity Service

This is the identity service. The main purpose of this service is to manage user identities, including authentication and authorization. It will also handle user registration, password management, and other related functionalities.
This will use OpenIddict with Identity Framework

## Additional Information

I have other applications. Mainly a point of sale application and a golf booking system and later a digital wallet system.
They should all use the same identity service for authentication and authorization. This will allow for a single sign-on experience across all applications and centralize user management. The application itself will handle roles except for global roles like admin which will be handled by the identity service. The identity service will also handle user registration and password management, while the applications will handle role assignment and permissions.

## Technologies Used
- ASP.NET Core
- OpenIddict
- Identity Framework
- Entity Framework Core
- Postgres

## Help setup this project

This project can use sqlite for now until I later switch to postgres.
Help with entire flow to test this. I want to generate a token somehow. How can I create simple html page with openid-ts client to test auth. I want to also test creating a new user.
