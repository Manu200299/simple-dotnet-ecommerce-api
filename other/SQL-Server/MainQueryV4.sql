-- New simplified database (26/12/2024 -- 23:31)
-- Users Table
CREATE TABLE Users(
    UserID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    [Password] NVARCHAR(255) NOT NULL, -- Hashed password
    Token NVARCHAR(MAX) NOT NULL, -- Optional JWT auth token
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- Address Table
CREATE TABLE [Address](
    AddressID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    PhoneNumber NVARCHAR(30) NOT NULL, -- Maybe needs normalization due to prefix
    StreetName NVARCHAR(255) NOT NULL,
    StreetAdditional NVARCHAR(255), -- Can be null
    PostalCode NVARCHAR(50) NOT NULL,
    District NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    AdditionalNote NVARCHAR(100) -- Can be null
);

-- UsersAddress (bridge) Table
CREATE TABLE UsersAddress(
    UsersAddressID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    UserID INT NOT NULL,
    AddressID INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (AddressID) REFERENCES Address(AddressID) ON DELETE CASCADE
);

-- Category Table
CREATE TABLE Category(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NOT NULL
);

-- Products Table
CREATE TABLE Products(
    ProductID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    CategoryID INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Color NVARCHAR(100) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    Stock INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

-- OrderStatus Table (Normalized Status)
CREATE TABLE OrderStatus(
    StatusID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    StatusName NVARCHAR(50) NOT NULL
);

-- Orders Table
CREATE TABLE Orders(
    OrderID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    UserID INT NOT NULL,
	StatusID INT NOT NULL, -- Now referencing the OrderStatus table
    Total DECIMAL(18, 2) NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (StatusID) REFERENCES OrderStatus(StatusID)
);

-- OrderDetails Table (For each order line item)
CREATE TABLE OrderDetails(
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    ProductPrice DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Carts Table
CREATE TABLE Carts(
    CartID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    UserID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
	-- ProductPrice decimal(18, 2) NOT NULL,
	IsShared BIT DEFAULT 0, -- 0 = private, 1 = shareable
    SharedToken NVARCHAR(MAX), -- Sharing token
    AddedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- PaymentMethods Table (Normalized Payment Methods)
CREATE TABLE PaymentMethods(
    PaymentMethodID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    MethodName NVARCHAR(100) NOT NULL
);

-- PaymentStatusTable
CREATE TABLE PaymentStatus(
	PaymentStatusID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	StatusName NVARCHAR(50) NOT NULL
);

-- Payments Table
CREATE TABLE Payments(
    PaymentID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
    OrderID INT NOT NULL,
	PaymentStatusID INT NOT NULL,
	PaymentMethodID INT NOT NULL, -- Now referencing the PaymentMethods table
	Amount DECIMAL(18, 2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (PaymentMethodID) REFERENCES PaymentMethods(PaymentMethodID),
	FOREIGN KEY (PaymentStatusID) REFERENCES PaymentStatus(PaymentStatusID)
);

