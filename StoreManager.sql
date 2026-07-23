-- ============================================================================
--  StoreManager.sql — schema + seed data
--  Module 2 · Setup. Five tables, four relationships.
--
--  Usage (SQL Server / SSMS):
--    1. Right-click Databases -> New Database -> name it "StoreManager"
--    2. Open this file, make sure "StoreManager" is the selected database, Execute
--    3. Expand Tables to verify all five appear, with the seed categories
--
--  Column names are snake_case so they match what the API (EF Core) expects.
--  Re-running is safe: tables and seed rows are only created if missing.
-- ============================================================================

-- --- users --------------------------------------------------------------
IF OBJECT_ID('dbo.users', 'U') IS NULL
CREATE TABLE users (
    id            INT IDENTITY PRIMARY KEY,
    name          NVARCHAR(100) NOT NULL,
    email         NVARCHAR(255) NOT NULL,
    password_hash NVARCHAR(MAX) NOT NULL,
    role          NVARCHAR(20)  NOT NULL,          -- 'admin' or 'cashier'
    created_at    DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

-- --- categories ---------------------------------------------------------
IF OBJECT_ID('dbo.categories', 'U') IS NULL
CREATE TABLE categories (
    id          INT IDENTITY PRIMARY KEY,
    name        NVARCHAR(100) NOT NULL,
    description NVARCHAR(255) NULL
);

-- --- products -----------------------------------------------------------
IF OBJECT_ID('dbo.products', 'U') IS NULL
CREATE TABLE products (
    id             INT IDENTITY PRIMARY KEY,
    category_id    INT           NOT NULL,
    name           NVARCHAR(100) NOT NULL,
    barcode        NVARCHAR(100) NULL,
    unit           NVARCHAR(20)  NOT NULL,          -- 'piece', 'kg' or 'litre'
    cost_price     DECIMAL(18,2) NOT NULL,
    selling_price  DECIMAL(18,2) NOT NULL,
    stock_quantity INT           NOT NULL,
    created_at     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_products_categories FOREIGN KEY (category_id) REFERENCES categories(id)
);

-- --- sales --------------------------------------------------------------
IF OBJECT_ID('dbo.sales', 'U') IS NULL
CREATE TABLE sales (
    id             INT IDENTITY PRIMARY KEY,
    cashier_id     INT           NOT NULL,
    total_amount   DECIMAL(18,2) NOT NULL,
    payment_method NVARCHAR(20)  NOT NULL,          -- 'cash' or 'card'
    notes          NVARCHAR(255) NULL,
    created_at     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_sales_users FOREIGN KEY (cashier_id) REFERENCES users(id)
);

-- --- sale_items ---------------------------------------------------------
-- The most important table: it stores unit_price at the time of sale, so
-- changing a product's price later never rewrites yesterday's sales.
IF OBJECT_ID('dbo.sale_items', 'U') IS NULL
CREATE TABLE sale_items (
    id         INT IDENTITY PRIMARY KEY,
    sale_id    INT           NOT NULL,
    product_id INT           NOT NULL,
    quantity   DECIMAL(18,3) NOT NULL,
    unit_price DECIMAL(18,2) NOT NULL,
    subtotal   DECIMAL(18,2) NOT NULL,
    CONSTRAINT fk_sale_items_sales    FOREIGN KEY (sale_id)    REFERENCES sales(id),
    CONSTRAINT fk_sale_items_products FOREIGN KEY (product_id) REFERENCES products(id)
);

-- ============================================================================
--  Seed data
-- ============================================================================

-- The four starter categories (from the setup lecture).
IF NOT EXISTS (SELECT 1 FROM categories)
INSERT INTO categories (name, description) VALUES
    ('Beverages', 'Drinks and soft drinks'),
    ('Snacks',    'Chips and packaged snacks'),
    ('Dairy',     'Milk, cheese and eggs'),
    ('Cleaning',  'Household cleaning supplies');

-- Optional: a demo admin so you can log in once Slice 1 (Auth) is built.
-- NOTE: password_hash below is a placeholder — replace it with a real hash
-- from your auth code before relying on it.
IF NOT EXISTS (SELECT 1 FROM users)
INSERT INTO users (name, email, password_hash, role) VALUES
    ('Admin', 'admin@storemanager.local', 'CHANGE_ME', 'admin');

-- After running: expand Tables in SSMS, right-click categories ->
-- Select Top 1000, and verify four rows appear.
