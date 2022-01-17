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
