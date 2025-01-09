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

UPDATE Users
SET Token = 'temporarytoken'
WHERE Token IS NULL;

DROP DATABASE ISI_API

ALTER TABLE Users
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
('john_doe', 'john@example.com',  NULL, GETDATE(), GETDATE()),
('jane_smith', 'jane@example.com',  NULL, GETDATE(), GETDATE()),
('TEST', 'TEST@example.com', NULL, GETDATE(), GETDATE(), 'password123');


-- Insert Address
INSERT INTO "Address" (FirstName, LastName, PhoneNumber, StreetName, StreetAdditional, PostalCode, District, City, Country, AdditionalNote)
VALUES
('John', 'Doe', '123456789', '123 Elm Street', NULL, '12345', 'Downtown', 'New York', 'USA', NULL),
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

-- Insert ProductDetails
INSERT INTO ProductDetails (CategoryID, "Name", "Description", Color)
VALUES
(1, 'Smartphone', 'Latest model with 5G connectivity', 'Black'),
(2, 'Hardcover Book', 'Fiction novel by a famous author', 'Blue');

-- Insert Products
INSERT INTO Products (ProductDetailID, Price, CreatedAt, UpdatedAt)
VALUES
(1, 699.99, GETDATE(), GETDATE()),
(2, 19.99, GETDATE(), GETDATE());


-- Insert Orders
INSERT INTO Orders (UserID, Total, OrderDate)
VALUES
(1, 719.98, GETDATE(), 'Completed'),
(2, 19.99, GETDATE(), 'Pending');

-- Insert OrderStatus
INSERT INTO OrderStatus (StatusName)
VALUES 
('Pending'),
('Accepted'),
('Shipped')

-- Insert OrderDetails
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, Price)
VALUES
(1, 1, 1, 699.99),
(1, 2, 1, 19.99),
(2, 2, 1, 19.99);

-- Insert Carts
INSERT INTO Carts (UserID, ProductID, Quantity, IsShared, SharedToken, AddedAt)
VALUES
(1, 1, 1, 0, NULL, GETDATE()),
(2, 2, 2, 1, 'shared_token_123', GETDATE());

-- Insert Payments
INSERT INTO Payments (OrderID, PaymentMethodID, PaymentDate, Amount, PaymentStatus)
VALUES
(1, 1, GETDATE(), 719.98, 'Paid');

INSERT INTO PaymentMethods (MethodName)
VALUES
('Paypal'),
('Credit Card');

INSERT INTO PaymentStatus (StatusName)
VALUES
('Pending'),
('Completed'),
('Refused')

