
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'loandb')
BEGIN
    CREATE DATABASE loandb;
    PRINT 'Database loandb created successfully';
END
ELSE
BEGIN
    PRINT 'Database loandb already exists';
END
GO

USE loandb;
GO

PRINT 'Initialization complete';
GO
