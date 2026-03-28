# SQL Server Security

## Authentication

### SQL Authentication vs Windows/Entra ID

```csharp
// Prefer Entra ID (formerly Azure AD) for Azure SQL
"Server=myserver.database.windows.net;Database=MyApp;Authentication=Active Directory Default"

// Managed Identity (Azure App Service / Container Apps)
"Server=myserver.database.windows.net;Database=MyApp;Authentication=Active Directory Managed Identity"

// Windows Authentication (on-premises)
"Server=localhost;Database=MyApp;Trusted_Connection=True"
```

## Parameterized Queries (Mandatory)

```csharp
// ALWAYS parameterized — prevents SQL injection
const string sql = "SELECT * FROM Products WHERE Name = @Name AND Status = @Status";
var products = await connection.QueryAsync<Product>(
    new CommandDefinition(sql, new { Name = name, Status = status }));

// NEVER concatenate user input
// BAD: $"SELECT * FROM Products WHERE Name = '{userInput}'"
```

## Row-Level Security

```sql
-- Create a security predicate function
CREATE FUNCTION dbo.fn_SecurityPredicate(@TenantId NVARCHAR(36))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS result
    WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS NVARCHAR(36));

-- Apply to table
CREATE SECURITY POLICY ProductsPolicy
    ADD FILTER PREDICATE dbo.fn_SecurityPredicate(TenantId) ON dbo.Products,
    ADD BLOCK PREDICATE dbo.fn_SecurityPredicate(TenantId) ON dbo.Products
WITH (STATE = ON);
```

```csharp
// Set tenant context in middleware
using var command = connection.CreateCommand();
command.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";
command.Parameters.AddWithValue("@TenantId", currentTenantId);
await command.ExecuteNonQueryAsync(cancellationToken);
```

## Always Encrypted

```csharp
// Connection string with column encryption
"Server=myserver;Database=MyApp;Column Encryption Setting=Enabled"
```

```sql
-- Encrypt sensitive columns
ALTER TABLE Users
ALTER COLUMN SocialSecurityNumber NVARCHAR(11)
ENCRYPTED WITH (
    ENCRYPTION_TYPE = DETERMINISTIC,
    ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256',
    COLUMN_ENCRYPTION_KEY = MyCEK
);
```

## Transparent Data Encryption (TDE)

```sql
-- Enable TDE (Azure SQL: enabled by default)
ALTER DATABASE MyApp SET ENCRYPTION ON;
```

## Least Privilege Roles

```sql
-- Application user with minimal permissions
CREATE USER AppUser FOR LOGIN AppLogin;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO AppUser;
-- Don't grant: ALTER, CREATE, DROP, EXECUTE on DDL

-- Read-only reporting user
CREATE USER ReportUser FOR LOGIN ReportLogin;
GRANT SELECT ON SCHEMA::dbo TO ReportUser;
```

## Auditing

```sql
-- Enable SQL Server Audit
CREATE SERVER AUDIT MyAudit
TO FILE (FILEPATH = 'C:\Audits\', MAXSIZE = 100 MB);

CREATE DATABASE AUDIT SPECIFICATION ProductAudit
FOR SERVER AUDIT MyAudit
ADD (SELECT, INSERT, UPDATE, DELETE ON dbo.Products BY public);

ALTER SERVER AUDIT MyAudit WITH (STATE = ON);
```

## Official References

- [SQL Server Security](https://learn.microsoft.com/en-us/sql/relational-databases/security/)
- [Row-Level Security](https://learn.microsoft.com/en-us/sql/relational-databases/security/row-level-security)
- [Always Encrypted](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-database-engine)
- [Azure SQL Security Best Practices](https://learn.microsoft.com/en-us/azure/azure-sql/database/security-best-practice)
