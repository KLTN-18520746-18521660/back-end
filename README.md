# Web server use in back-end
* **Kestrel** [Kestrel on asp.net core 5.0](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-5.0)
    - Configuration *still update*
    - Documention *still update*
* **Wiz toolset**
    - Build installer *still update*
* **REST API**
    - **GET**
    - **POST**
    - **PUT**
    - **DEL**
    *still update*

> **NOTE**
> * Back-end only use **https** connection. (config on hard coding - see on config Kestrel server)
> * Back-end api base on **REST API**
> * **Api port default**
>   - core-api: 7005
>   - search-api: 7006
>   - recommend-api: 7007
> * Config port in file ``appsettings.json`` with key ``Port``
> * **Certitficate use on Kestrel**
>   - .pfx format. Convert cert x509 to .pfx by command:
>   ```
>   openssl pkcs12 -export -out <out_cert_pfx> -in <in_cert_x509> -name <name_certificate>
>   ```
>   - Password is hard coding is ``Ndh90768``
>   - Need config path of certificate in file ``appsettings.json`` with key ``Certificate.Path``

## Project Reference
- edit `*.csproj`
- Way 1 (Need generate ProjectGuid):
    ```
    <ItemGroup>
        <ProjectReference Include="..\demo-lib\demo-lib.csproj">
            <Project>{7B05D051-5909-4DBD-818C-61B6361BB246}</Project>
            <Name>DemooLib</Name>
        </ProjectReference>
    </ItemGroup>
    ```
- Way 2:
    ```
    <ItemGroup>
        <ProjectReference Include="..\demo-lib\demo-lib.csproj" />
    </ItemGroup>
    ```
## Unit test
- add project reference
- command:
> cd test
> dotnet test

## Build and Run coreApi
> cd coreApi
- debug mode + environment develop
> dotnet run ssl
- release mode + environment production
> dotnet run https --launch-profile pro -c release
- run without ssl + environment develop
> dotnet run

## Project Console is for testing