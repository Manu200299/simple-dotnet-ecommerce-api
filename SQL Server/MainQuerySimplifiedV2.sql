-- ISI API Online Store --
/*
 Project Summary
 PT
 Uma simples loja online onde cada utilizador efetua o login e navega pela loja observando os produtos disponiveis. 
 O utilizador pode adicionar produtos ao carrinho onde depois poderá simular o pagamento do valor total da ordem.
 Para além das compras do próprio carrinho do utilizador, será possível partilhar os carrinhos entre utilizadores. Por exemplo:
	> O utilizador X partilha o seu carrinho com o utilizador Y para que este pague o carrinho do utilizador X

	

	--- SQL DataBase Reminders ---

	Q: On a two table relationship, where should the foreign key go?
	A: On the "Many" end table. Ex.: Column A has many B. Therefore column B has A as foreign key

*/

-- Users Table
CREATE TABLE Users(
	UserID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	Username NVARCHAR(100) NOT NULL,
	Email NVARCHAR(150) NOT NULL,
	"Name" NVARCHAR(150) NOT NULL,
	"Address" NVARCHAR(250) NOT NULL,
	PhoneNumber NVARCHAR(50) NOT NULL,
	PasswordHash NVARCHAR(255) NOT NULL, -- Hashed password
	Salt NVARCHAR(255) NOT NULL, -- Additional security for password hashing
	Token NVARCHAR(MAX), -- Optional JWT auth token
	CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
	UpdatedAt DATETIME DEFAULT GETDATE()
);


-- Products Table
CREATE TABLE Products(
	ProductID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	"Name" NVARCHAR(200) NOT NULL,
	"Description" NVARCHAR(255),
	Color NVARCHAR(100),
	Category NVARCHAR(100),
	Price DECIMAL(18, 2) NOT NULL,
	CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
	UpdatedAt DATETIME DEFAULT GETDATE(),
);


-- Orders Table
CREATE TABLE Orders(
	OrderID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	UserID INT NOT NULL, -- Check also for address relationship (bridge table)
	Total DECIMAL (18, 2) NOT NULL,
	OrderDate DATETIME DEFAULT GETDATE(),
	"Status" NVARCHAR(50) NOT NULL, -- can be changed to int (Add text status values for each number) / new table due to normalization
	FOREIGN KEY (UserID) REFERENCES Users(UserID)
	
);


-- OrderDetails Table (For each order line item)
CREATE TABLE OrderDetails(
	OrderDetailID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	OrderID INT NOT NULL,
	ProductID INT NOT NULL,
	Quantity INT NOT NULL,
	Price DECIMAL(18, 2) NOT NULL,
	FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
	FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);


-- Carts Table
CREATE TABLE Carts(
	CartID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	UserID INT NOT NULL,
	ProductID INT NOT NULL,
	Quantity INT NOT NULL,
	IsShared BIT DEFAULT 0, -- 0 = private, 1 = shareable
	SharedToken NVARCHAR(MAX), -- sharing token
	AddedAt DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (UserID) REFERENCES Users(UserID),
	FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);


-- Payments Table
CREATE TABLE Payments(
	PaymentID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	OrderID INT NOT NULL,
	PaymentDate DATETIME DEFAULT GETDATE(),
	Amount DECIMAL(18, 2) NOT NULL,
	PaymentMethod NVARCHAR(100), -- maybe another table due to normalization
	PaymentStatus NVARCHAR(100) NOT NULL,
	FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);
