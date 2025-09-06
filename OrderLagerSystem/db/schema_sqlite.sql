
-- Order & Lager – SQLite DDL (MVP)
PRAGMA foreign_keys = ON;

-- =====================
-- Drop (dev convenience)
-- =====================
DROP TABLE IF EXISTS UserRole;
DROP TABLE IF EXISTS InventoryTransaction;
DROP TABLE IF EXISTS Inventory;
DROP TABLE IF EXISTS ReceiptLine;
DROP TABLE IF EXISTS Receipt;
DROP TABLE IF EXISTS OrderStatusHistory;
DROP TABLE IF EXISTS OrderLine;
DROP TABLE IF EXISTS "Order";
DROP TABLE IF EXISTS OrderStatus;
DROP TABLE IF EXISTS Customer;
DROP TABLE IF EXISTS Supplier;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS "Location";
DROP TABLE IF EXISTS "User";
DROP TABLE IF EXISTS Role;
DROP TABLE IF EXISTS Category;

-- =====================
-- Users & Roles
-- =====================
CREATE TABLE "User" (
    UserId        INTEGER PRIMARY KEY AUTOINCREMENT,
    Email         TEXT NOT NULL UNIQUE,
    DisplayName   TEXT NOT NULL,
    -- Lagra aldrig lösenord i klartext. Använd separat identity-lager i produktion.
    PasswordHash  TEXT,
    IsActive      INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
    CreatedAt     TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE Role (
    RoleId    INTEGER PRIMARY KEY AUTOINCREMENT,
    Name      TEXT NOT NULL UNIQUE
);

CREATE TABLE UserRole (
    UserId INTEGER NOT NULL,
    RoleId INTEGER NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES "User"(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Role(RoleId) ON DELETE CASCADE
);

-- =====================
-- Produkt & Lager
-- =====================
CREATE TABLE Category (
    CategoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name       TEXT NOT NULL UNIQUE
);

CREATE TABLE Product (
    ProductId   INTEGER PRIMARY KEY AUTOINCREMENT,
    SKU         TEXT NOT NULL UNIQUE,
    Name        TEXT NOT NULL,
    CategoryId  INTEGER,
    IsActive    INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
    CreatedAt   TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
);

CREATE TABLE "Location" (
    LocationId  INTEGER PRIMARY KEY AUTOINCREMENT,
    Code        TEXT NOT NULL UNIQUE,
    Description TEXT,
    IsActive    INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1))
);

-- Current stock per Product & Location
CREATE TABLE Inventory (
    ProductId  INTEGER NOT NULL,
    LocationId INTEGER NOT NULL,
    OnHandQty  INTEGER NOT NULL DEFAULT 0 CHECK (OnHandQty >= 0),
    PRIMARY KEY (ProductId, LocationId),
    FOREIGN KEY (ProductId)  REFERENCES Product(ProductId),
    FOREIGN KEY (LocationId) REFERENCES "Location"(LocationId)
);

-- Ledger of movements
CREATE TABLE InventoryTransaction (
    InventoryTransactionId INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductId     INTEGER NOT NULL,
    LocationId    INTEGER NOT NULL,
    Quantity      INTEGER NOT NULL, -- positive=in, negative=out
    MovementType  TEXT NOT NULL CHECK (MovementType IN ('RECEIPT','SHIPMENT','ADJUSTMENT','TRANSFER_IN','TRANSFER_OUT')),
    RelatedTable  TEXT,
    RelatedId     INTEGER,
    PerformedBy   INTEGER,
    OccurredAt    TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    FOREIGN KEY (ProductId)   REFERENCES Product(ProductId),
    FOREIGN KEY (LocationId)  REFERENCES "Location"(LocationId),
    FOREIGN KEY (PerformedBy) REFERENCES "User"(UserId)
);
CREATE INDEX IX_InvTxn_ProductOccurred ON InventoryTransaction(ProductId, OccurredAt);

-- =====================
-- Kunder & leverantörer
-- =====================
CREATE TABLE Customer (
    CustomerId  INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,
    Email       TEXT,
    Phone       TEXT
);

CREATE TABLE Supplier (
    SupplierId  INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,
    Email       TEXT,
    Phone       TEXT
);

-- =====================
-- Orders & status
-- =====================
CREATE TABLE OrderStatus (
    OrderStatusId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name          TEXT NOT NULL UNIQUE
);

CREATE TABLE "Order" (
    OrderId         INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderNumber     TEXT NOT NULL UNIQUE,
    CustomerId      INTEGER NOT NULL,
    CurrentStatusId INTEGER NOT NULL,
    CreatedAt       TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CreatedBy       INTEGER,
    FOREIGN KEY (CustomerId)      REFERENCES Customer(CustomerId),
    FOREIGN KEY (CurrentStatusId) REFERENCES OrderStatus(OrderStatusId),
    FOREIGN KEY (CreatedBy)       REFERENCES "User"(UserId)
);
CREATE INDEX IX_Order_CustomerCreated ON "Order"(CustomerId, CreatedAt);

CREATE TABLE OrderLine (
    OrderLineId INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId     INTEGER NOT NULL,
    ProductId   INTEGER NOT NULL,
    Quantity    INTEGER NOT NULL CHECK (Quantity > 0),
    FOREIGN KEY (OrderId)   REFERENCES "Order"(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
);
CREATE INDEX IX_OrderLine_Order ON OrderLine(OrderId);

CREATE TABLE OrderStatusHistory (
    OrderStatusHistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId      INTEGER NOT NULL,
    FromStatusId INTEGER,
    ToStatusId   INTEGER NOT NULL,
    ChangedAt    TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ChangedBy    INTEGER,
    Note         TEXT,
    FOREIGN KEY (OrderId)      REFERENCES "Order"(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (FromStatusId) REFERENCES OrderStatus(OrderStatusId),
    FOREIGN KEY (ToStatusId)   REFERENCES OrderStatus(OrderStatusId),
    FOREIGN KEY (ChangedBy)    REFERENCES "User"(UserId)
);
CREATE INDEX IX_OSH_OrderChangedAt ON OrderStatusHistory(OrderId, ChangedAt);

-- =====================
-- Inleverans (ökar saldo)
-- =====================
CREATE TABLE Receipt (
    ReceiptId   INTEGER PRIMARY KEY AUTOINCREMENT,
    SupplierId  INTEGER,
    ReceivedAt  TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    ReceivedBy  INTEGER,
    Note        TEXT,
    FOREIGN KEY (SupplierId) REFERENCES Supplier(SupplierId),
    FOREIGN KEY (ReceivedBy) REFERENCES "User"(UserId)
);

CREATE TABLE ReceiptLine (
    ReceiptLineId INTEGER PRIMARY KEY AUTOINCREMENT,
    ReceiptId     INTEGER NOT NULL,
    ProductId     INTEGER NOT NULL,
    LocationId    INTEGER NOT NULL,
    Quantity      INTEGER NOT NULL CHECK (Quantity > 0),
    FOREIGN KEY (ReceiptId)  REFERENCES Receipt(ReceiptId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId)  REFERENCES Product(ProductId),
    FOREIGN KEY (LocationId) REFERENCES "Location"(LocationId)
);
CREATE INDEX IX_ReceiptLine_Receipt ON ReceiptLine(ReceiptId);

-- =====================
-- Seeds
-- =====================
INSERT INTO Role (Name) VALUES ('Admin'), ('Warehouse'), ('Sales');
INSERT INTO OrderStatus (Name) VALUES ('Created'), ('Picking'), ('Shipped'), ('Cancelled');
