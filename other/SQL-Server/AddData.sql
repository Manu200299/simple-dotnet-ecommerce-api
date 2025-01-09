-- Query to add data to ISI_API database
-- Data generated with ChatGPT

SELECT * FROM [Address]
SELECT * FROM Carts
SELECT * FROM Orders
SELECT * FROM OrderDetails
SELECT * FROM OrderStatus
SELECT * FROM PaymentMethods
SELECT * FROM Payments
SELECT * FROM PaymentStatus
SELECT * FROM Products
SELECT * FROM Users
SELECT * FROM UsersAddress

-- SELECT ORDERS
SELECT * FROM Users
SELECT * FROM Orders
SELECT * FROM OrderDetails
SELECT * FROM OrderStatus

-- UPDATE Table
UPDATE Carts
SET IsShared = 1
WHERE CartID = 9;

-- DROP DATABASE/COLUMN
DROP DATABASE ISI_API

-- ALTER TABLE
ALTER TABLE Payments
ADD StripePaymentIntentID NVARCHAR(MAX)
DROP COLUMN salt

-- SELECT Order BY UserID
SELECT 
    o.OrderID,
    o.OrderDate,
    os.StatusName AS OrderStatus,
    o.Total,
    od.OrderDetailID,
    p.Name AS ProductName,
    od.Quantity,
    od.ProductPrice,
    (od.Quantity * od.ProductPrice) AS TotalPrice
FROM 
    Orders o
JOIN 
    OrderStatus os ON o.StatusID = os.StatusID
JOIN 
    OrderDetails od ON o.OrderID = od.OrderID
JOIN 
    Products p ON od.ProductID = p.ProductID
WHERE 
    o.UserID = 1  -- Replace with the specified user ID
ORDER BY 
    o.OrderDate DESC;



DELETE FROM Users WHERE UserID = 1





-- Data Insertion
-- Insert Users
INSERT INTO Users (Username, Email, Token, CreatedAt, UpdatedAt, Password)
VALUES
('john_doe', 'john@example.com',  'test', GETDATE(), GETDATE(), 'password123'),
('jane_smith', 'jane@example.com', '', GETDATE(), GETDATE(), 'password123'),
('TEST', 'TEST@example.com', '', GETDATE(), GETDATE(), 'password123');


-- Insert Address
INSERT INTO "Address" (FirstName, LastName, PhoneNumber, StreetName, StreetAdditional, PostalCode, District, City, Country, AdditionalNote)
VALUES
('John', 'Doe', '123456789', '123 Elm Street', 'sum shit', '12345', 'Downtown', 'New York', 'USA', 'leave at recception'),
('Jane', 'Smith', '987654321', '456 Oak Avenue', 'Apt 5B', '67890', 'Uptown', 'Los Angeles', 'USA', 'Leave at the front desk.');

-- Insert UsersAddress
INSERT INTO UsersAddress (UserID, AddressID)
VALUES
(1, 1),
(2, 2);

-- Insert Categories
INSERT INTO Category (CategoryName, "Description")
VALUES
('Electronics', 'Electronic devices and accessories'),
('Books', 'Wide range of books and reading materials');


-- Insert Products
INSERT INTO Products (CategoryID, Name, Description, Color, Price, Stock, CreatedAt, UpdatedAt)
VALUES
(1, 'iPhone 15 PRO MAX', 'Last gen smartphone', 'Red', 1099.99, 100, GETDATE(), GETDATE()),
(1, 'iPhone X Plus', 'Old gen smartphone', 'Black', 599.99, 20, GETDATE(), GETDATE()),
(2, 'Learn C# and .NET', 'Book about c# and .net', '', 29, 200, GETDATE(), GETDATE())



-- Insert Orders
INSERT INTO Orders (UserID, StatusID, Total, OrderDate)
VALUES
(1, 1, 719.98, GETDATE()),
(2, 3, 19.99, GETDATE());

-- Insert OrderStatus
INSERT INTO OrderStatus (StatusName)
VALUES 
('Pending'),
('Paid'),
('Cancelled')

-- Insert OrderDetails
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, ProductPrice)
VALUES
(2, 1, 1, 699.99),
(2, 2, 1, 19.99),
(3, 2, 1, 19.99);

-- Insert Carts
INSERT INTO Carts (UserID, ProductID, Quantity, IsShared, SharedToken, AddedAt)
VALUES
(20, 2, 2, 1, 'test123', GETDATE())
(19, 6, 1, 1, 'test123', GETDATE()),
(3, 3, 1, 1, 'token', GETDATE()),
(2, 2, 2, 1, 'shared_token_123', GETDATE()),

UPDATE Carts
SET IsShared = 1
WHERE CartID = 9;

-- Insert Payments
INSERT INTO Payments (OrderID, PaymentMethodID, PaymentStatusID, PaymentDate, Amount)
VALUES
(2, 1, 1, GETDATE(), 719.98);

INSERT INTO PaymentMethods (MethodName)
VALUES
('Card'),
('Credit Card');

INSERT INTO PaymentStatus (StatusName)
VALUES
('Pending'),
('Completed'),
('Refused')

