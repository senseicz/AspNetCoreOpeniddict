- create new blank database, setup db user, modify connection string

DB Schema:

`dotnet ef migrations add initSts -c ApplicationDbContext`

`dotnet ef database update Fido2StoredCredentialsPasskeys`

Initial data: 
- this is done by worker.cs class on fiorst start of OpenIdDict project