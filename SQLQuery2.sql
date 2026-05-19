-- ============================================
-- STEP 1: Allow NULL values for PDF column
-- ============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Contracts') AND name = 'PdfFilePath' AND is_nullable = 0)
BEGIN
    ALTER TABLE Contracts ALTER COLUMN PdfFilePath NVARCHAR(MAX) NULL;
    PRINT 'Modified PdfFilePath column to allow NULL'
END
ELSE
BEGIN
    PRINT 'PdfFilePath column already allows NULL'
END

-- ============================================
-- STEP 2: Clear existing data
-- ============================================
DELETE FROM ServiceRequests;
DELETE FROM Contracts;
DELETE FROM Clients;
PRINT 'Cleared existing data'

-- ============================================
-- STEP 3: Reset identity columns
-- ============================================
DBCC CHECKIDENT ('Clients', RESEED, 0);
DBCC CHECKIDENT ('Contracts', RESEED, 0);
DBCC CHECKIDENT ('ServiceRequests', RESEED, 0);
PRINT 'Reset identity columns'

-- ============================================
-- STEP 4: Insert Clients
-- ============================================
INSERT INTO Clients (Name, Email, PhoneNumber, Address, Region) VALUES
('TechMove Logistics SA', 'contact@techmove.co.za', '+27 11 234 5678', '123 Main Street, Johannesburg', 'EMEA'),
('Global Freight Solutions', 'info@globalfreight.com', '+1 555 123 4567', '456 Harbor Drive, New York', 'NA'),
('Asia Cargo Express', 'dispatch@asiacargo.com', '+65 6789 1234', '789 Shipping Lane, Singapore', 'APAC');
PRINT 'Inserted 3 clients'

-- ============================================
-- STEP 5: Insert Contracts (with NULL PDF path)
-- ============================================
INSERT INTO Contracts (ClientId, ContractNumber, StartDate, EndDate, Status, ServiceLevel, TermsAndConditions, CreatedAt, PdfFilePath) VALUES
(1, 'CONTRACT-001', '2024-01-01', '2024-12-31', 'Active', 'Gold', 'Standard logistics terms', GETDATE(), NULL),
(1, 'CONTRACT-002', '2024-03-15', '2025-03-14', 'Active', 'Platinum', 'Express shipping terms', GETDATE(), NULL),
(2, 'CONTRACT-003', '2023-06-01', '2024-05-31', 'Expired', 'Silver', 'Standard freight', GETDATE(), NULL),
(3, 'CONTRACT-004', '2024-02-01', '2024-08-31', 'OnHold', 'Bronze', 'Under review', GETDATE(), NULL),
(2, 'CONTRACT-005', '2024-05-01', '2025-04-30', 'Draft', 'Gold', 'New partnership', GETDATE(), NULL);
PRINT 'Inserted 5 contracts'

-- ============================================
-- STEP 6: Verify data
-- ============================================
SELECT 'Clients Table:' as [Table], COUNT(*) as [Row Count] FROM Clients
UNION ALL
SELECT 'Contracts Table:', COUNT(*) FROM Contracts
UNION ALL
SELECT 'ServiceRequests Table:', COUNT(*) FROM ServiceRequests;

-- View the data
SELECT * FROM Clients;
SELECT * FROM Contracts;