-- Extend the end date to next year
UPDATE Contracts 
SET EndDate = DATEADD(year, 1, GETDATE())
WHERE Id = 1;

-- Verify the update
SELECT Id, ContractNumber, Status, StartDate, EndDate 
FROM Contracts 
WHERE Id = 1;