# ASPCoreGRPCSample

Sample solution to explore gRPC integration with .NET Core for both client and server.

- Shared proto files in the Protos folder define the contract for the example services.
- Different examples of client-server and server-server communication
- Client connection examples with the new managed Grpc.Net.Client package that uses `HttpClient` and `HttpClientFactory`, as well as the existing `Grpc.Core` package that users unmanaged C code underneath.
- HTTPS certificates for development locally and with docker
- Usage of `Grpc.Core` channels when client certificates are expected by the server in mutual validation scenarios

> Created using **preview7** of ASP.NET Core 3.0. The code is subject to change before the official 3.0 release, in particular `Grpc.Net.Client` (which should hopefully allow client SslCredentials for mutual authentication)

## Getting started

Start by cloning the repo. Specific instructions depend on whether you want to use your local machine or docker.

### Local machine
- Either build the solution or manually `dotnet restore && dotnet build` each of the .NET projects
- Run `npm install` from the /Products folder
- Run the `generate_certificates` script from the /Products/scripts folder
- Start the Orders .NET service by running `dotnet run` from the /Orders folder
- Start the Shippings .NET service by running `dotnet run` from the /Shippings folder
- Start the Products Node.js service by running `npm start` from the  /Products folder
- Start the client .NET console app by running `dotnet run` from the /Client folder

### Docker
- Run `docker-compose up orders` from the root folder
- Run `docker-compose up shippings` from the root folder
- Run `docker-compose up products` from the root folder
- Run `docker-compose build client && docker-compose run --rm client` from the root folder
    - alternatively run the client locally with `dotnet run` from the Client folder

If you dont care about seeing all the server logs in the same console window, you can also run `docker-compose up products, shippings, orders`

## Client
Sample .NET Core console application that sends requests to the Orders and Products services.

- Requests to the Orders .NET service are sent with the new `HttpClient` based Grpc.Net.Client package. HTTPS development certificates are automatically used (and shared with the docker containers as per `docker-compose.override.yml`) 
- Requests to the Products Node service can be send with:
    - `HttpClient` based client, as long as the server does not expect client certificates to be provided within the request for mutual authentication.
	  - `Grpc.Core` Channel with `SslCredentials`

## Orders
Sample ASP.NET Core application that implements the service defined by `orders.proto`.

- Will internally call Shipping as part of the sample implementation using a client managed by `HttpClientFactorty`.

## Shippings
Sample ASP.NET Core application that implements the service defined by `shippings.proto`.

## Products
Sample Node.js application that implements the service defined by `products.proto`.

- Provides a script to generate SSL certificates for both server and client inside the /scripts folder.
- When starting the server, client certificates will be enforced or not based on the `checkClientCertificate` parameter supplied to `grpc.ServerCredentials.createSsl`. Client needs to be adjusted accordingly to use the `HttpClient` based one or the `Grpc.Core` one.
