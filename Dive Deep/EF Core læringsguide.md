# EFcore migration og dataseeding 

Skift defaulit connectionstring i appsettings.json eller appsettings.Development.json til din egen database connectionstring

Efter du har tilsluttet dig til en database, skal du kære kommandoer som update-database siden migration allerede er der.

Derefter seed data ved at bruge dotnet run --project "/path/to/navn på projekt.csproj" -- --seed-catalog 

# Giv admin-rettigheder
dotnet run --project "C:\Users\chris\source\repos\cuipin\Dive-Deep\Dive Deep\Dive Deep.csproj" -- "--grant-admin=your-email@example.dk"

