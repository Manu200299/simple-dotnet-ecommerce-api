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
	UserID INT PRIMARY KEY NOT NULL,
	UsersAddressIDag INT,
	Username NVARCHAR NOT NULL,
	Email NVARCHAR NOT NULL,
	"Password" NVARCHAR NOT NULL,
	CreatedAt DATETIME DEFAULT GETDATE(),
	UpdatedAt DATETIME DEFAULT GETDATE()
);

-- Address Table
CREATE TABLE "Address"(
	AddressID INT PRIMARY KEY NOT NULL,
	FirstName NVARCHAR NOT NULL,
	LastName NVARCHAR NOT NULL,
	PhoneNumber NVARCHAR NOT NULL,
	StreetName NVARCHAR NOT NULL,
	StreetAdditional NVARCHAR, -- Can be null
	PostalCode NVARCHAR NOT NULL,
	District NVARCHAR NOT NULL,
	City NVARCHAR NOT NULL,
	Country NVARCHAR NOT NULL,
	AdditionalNote NVARCHAR, -- Can be null
);

-- UsersAddress (bridge) Table
CREATE TABLE UsersAddress(
	UsersAddress INT PRIMARY KEY NOT NULL,
	UserID INT NOT NULL,
	AddressID INT NOT NULL,
	FOREIGN KEY (UserID) REFERENCES Users(UserID),
	FOREIGN KEY (AddressID) REFERENCES "Address"(AddressID),

);

-- Orders Table
CREATE TABLE Orders(
	OrderID INT PRIMARY KEY NOT NULL,

);

-- OrderDetails Table
CREATE TABLE OrderDetails(
	OrderDetailsID INT PRIMARY KEY NOT NULL,

);

-- Products Table
CREATE TABLE Products(
	ProductID INT PRIMARY KEY NOT NULL,
	ProductDetailsID INT NOT NULL,
	CategoryID INT NOT NULL,

);

-- ProductDetails Table
CREATE TABLE ProductDetails(
	ProductDetailsID INT PRIMARY KEY NOT NULL,
	"Name" NVARCHAR,
	"Description" NVARCHAR,
	Category NVARCHAR,
	Color NVARCHAR,
	
);

CREATE TABLE Stock(
	StockID INT PRIMARY KEY NOT NULL,
	ProductID INT NOT NULL,
	Quantity INT,
	FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- Carts Table
CREATE TABLE Carts();
