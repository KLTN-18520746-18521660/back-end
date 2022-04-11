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

> rm -rf /etc/localtime && ln -s /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime

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

## Build and Run CoreApi
> cd CoreApi
- Base command
> dotnet run [ssl] [disable-cors] [show-sql-command] [--launch-profile (pro|dev)] [-c (release|debug)]
- debug mode + environment develop
> dotnet run ssl disable-cors --launch-profile dev -c debug
- release mode + environment production
> dotnet run ssl --launch-profile pro -c release
- release with disable cros
> dotnet run ssl disable-cors --launch-profile pro -c release

## Project Console is for testing

## Migration
- **NOTE**: Migreations auto run when run app (just handle manual when needed)
- **Using Manager Package console**
	- *Add migrations*
		```
		add-migration DBCreation --context DBContext
		```
	- *Update database*
		```
		update-database DBCreation --context DBContext
		```

- **Using Cmd**
	> dotnet tool install --global dotnet-ef
	- *Add migrations*
		```
		dotnet ef migrations add DBCreation --context DBContext
		```
	- *Update database*
		```
		dotnet ef database update DBCreation --context DBContext
		```
	- *Drop database*
		```
		dotnet ef database drop --context DBContext --no-build -f
		```
	- *Generate script database*
		```
		dotnet ef migrations script --context DBContext
		```
	- *Remove migrations*
		```
		dotnet ef migrations remove --context DBContext
		```
	- *Get models from database*
		```
		dotnet ef dbcontext scaffold -o Tests -d "Host=localhost;Username=postgres;Database=postgres;Password=a;Port=5432" "Npgsql.EntityFrameworkCore.PostgreSQL"
		```
## SQL
- **Fultext search**:
	- [Full Text Search | Npgsql Documentation](https://www.npgsql.org/efcore/mapping/full-text-search.html?tabs=pg12%2Cv5)
	- [PostgreSQL: Documentation: 14: 12.9. GIN and GiST Index Types](https://www.postgresql.org/docs/14/textsearch-indexes.html)
	- Summary:
		- GIN: using for static data because lookup faster
		- GiST: using for dynamic data because update index faster
> docker run --name postgresdb -e POSTGRES_PASSWORD=a -p 5432:5432 -d postgres:14.1-alpine3.15
```
CREATE DATABASE "config_db";
CREATE DATABASE "inventory_db";
CREATE DATABASE "social_db";
CREATE DATABASE "cachec_db";
```

## NOTIFICATION
- Need to notify: comment, like, follow
- "comment/ like": --> "%1 has been commented on your post" --> need post_id, last_user_id
- "follow"
- status: "unread" --> modify_time
- content: object
	- type: post --> post_id / user --> user_id / system / category
	- action: post -> "comment, like"; user -> "follow"; "no action"

