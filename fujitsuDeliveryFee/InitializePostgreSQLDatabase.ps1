# PostgreSQL Database Initialization Script

# This script helps initialize the PostgreSQL database for the Fujitsu Delivery Fee application
# It assumes you have PostgreSQL installed and running locally

# Configuration
$PostgresUser = "postgres"
$PostgresPassword = "sql" # Replace with your actual password
$PostgresHost = "localhost"
$DatabaseName = "fujitsuDeliveryFee"

# Check if PostgreSQL is available
Write-Host "Checking PostgreSQL connection..." -ForegroundColor Cyan
try {
    # You may need to adjust the path to psql based on your PostgreSQL installation
    $psqlPath = "psql"
    $testConnection = & $psqlPath -U $PostgresUser -h $PostgresHost -c "SELECT 1;" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error connecting to PostgreSQL. Please ensure PostgreSQL is running and credentials are correct." -ForegroundColor Red
        Write-Host $testConnection -ForegroundColor Red
        exit 1
    }
    
    Write-Host "PostgreSQL connection successful!" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "Please ensure PostgreSQL is installed and running." -ForegroundColor Red
    exit 1
}

# Check if database exists
Write-Host "Checking if database '$DatabaseName' exists..." -ForegroundColor Cyan
$dbExists = & $psqlPath -U $PostgresUser -h $PostgresHost -t -c "SELECT 1 FROM pg_database WHERE datname = '$DatabaseName'" 2>&1

if ($dbExists -match "1") {
    Write-Host "Database '$DatabaseName' already exists." -ForegroundColor Yellow
    
    $confirmation = Read-Host "Do you want to drop and recreate the database? (y/n)"
    if ($confirmation -eq 'y') {
        Write-Host "Dropping database '$DatabaseName'..." -ForegroundColor Cyan
        & $psqlPath -U $PostgresUser -h $PostgresHost -c "DROP DATABASE \"$DatabaseName\"" 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error dropping database." -ForegroundColor Red
            exit 1
        }
        
        Write-Host "Database dropped successfully." -ForegroundColor Green
    } else {
        Write-Host "Skipping database recreation." -ForegroundColor Yellow
    }
}

# Create database if it doesn't exist or was dropped
$dbExists = & $psqlPath -U $PostgresUser -h $PostgresHost -t -c "SELECT 1 FROM pg_database WHERE datname = '$DatabaseName'" 2>&1

if ($dbExists -notmatch "1") {
    Write-Host "Creating database '$DatabaseName'..." -ForegroundColor Cyan
    & $psqlPath -U $PostgresUser -h $PostgresHost -c "CREATE DATABASE \"$DatabaseName\"" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error creating database." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Database created successfully." -ForegroundColor Green
}

Write-Host "\nDatabase setup complete!" -ForegroundColor Green
Write-Host "\nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update the PostgreSQL connection string in appsettings.json with your credentials" -ForegroundColor White
Write-Host "2. Run Entity Framework migrations to create the schema:" -ForegroundColor White
Write-Host "   dotnet ef migrations add InitialPostgreSQLMigration --context ApplicationDbContext --project ..\fujitsuDeliveryFee.Infrastructure\fujitsuDeliveryFee.Infrastructure.csproj --startup-project .\fujitsuDeliveryFee.API.csproj" -ForegroundColor White
Write-Host "3. Apply the migrations:" -ForegroundColor White
Write-Host "   dotnet ef database update --context ApplicationDbContext --project ..\fujitsuDeliveryFee.Infrastructure\fujitsuDeliveryFee.Infrastructure.csproj --startup-project .\fujitsuDeliveryFee.API.csproj" -ForegroundColor White