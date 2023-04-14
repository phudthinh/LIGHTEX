CREATE DATABASE LIGHTEX
GO

USE LIGHTEX
GO

-- CREATE TABLE
CREATE TABLE ACCOUNT
(
    username NVARCHAR(50) NOT NULL,
    password NVARCHAR(50) NOT NULL,
    full_name NVARCHAR(50) NOT NULL,
    active BIT NOT NULL,
    permission INT NOT NULL,
    last_login DATETIME NOT NULL,
    create_date DATETIME NOT NULL,
    CONSTRAINT PK_ACCOUNT_username PRIMARY KEY (username)
);
GO

CREATE TABLE CUSTOMER
(
    id_customer INT IDENTITY NOT NULL,
    username NVARCHAR(50) NOT NULL,
    email NVARCHAR(50) NULL,
    phone NVARCHAR(12) NULL,
    address NVARCHAR(200) NULL,
    ward NVARCHAR(200) NULL,
    city NVARCHAR(200) NULL,
    avatar VARBINARY(MAX) NULL,
    money FLOAT DEFAULT 0 NOT NULL,
    CONSTRAINT PK_CUSTOMER_id_customer PRIMARY KEY (id_customer),
    FOREIGN KEY (username) REFERENCES ACCOUNT(username)
);
GO

CREATE TABLE CATEGORY
(
    id_category INT IDENTITY NOT NULL,
    name NVARCHAR(50) NOT NULL,
    description NVARCHAR(500) NOT NULL,
    icon VARBINARY(MAX) NOT NULL,
    CONSTRAINT PK_CATEGORY_id_category PRIMARY KEY (id_category)
);
GO

CREATE TABLE BRAND
(
    id_brand INT IDENTITY NOT NULL,
	description NVARCHAR(500) NOT NULL,
    name NVARCHAR(50) NOT NULL
	CONSTRAINT PK_BRAND_id_brand PRIMARY KEY (id_brand),
);
GO

CREATE TABLE PRODUCT
(
    id_product INT IDENTITY NOT NULL,
    id_category INT NOT NULL,
	id_brand INT NOT NULL,
    name NVARCHAR(50) NOT NULL,
    information NVARCHAR(500) NOT NULL,
    price FLOAT DEFAULT 0 NOT NULL,
    image VARBINARY(MAX) NOT NULL,
	effect INT DEFAULT 0 NOT NULL,
    status BIT NOT NULL,
    create_date DATETIME NOT NULL,
    modified_date DATETIME NOT NULL,
    CONSTRAINT PK_PRODUCT_id_product PRIMARY KEY (id_product),
    FOREIGN KEY (id_category) REFERENCES CATEGORY(id_category),
	FOREIGN KEY (id_brand) REFERENCES BRAND(id_brand)
);
GO

CREATE TABLE CART
(
    id_cart INT IDENTITY NOT NULL,
    id_customer INT NOT NULL,
    id_product INT NOT NULL,
    quantity INT NOT NULL,
    CONSTRAINT PK_CART_id_cart PRIMARY KEY (id_cart),
    FOREIGN KEY (id_customer) REFERENCES CUSTOMER(id_customer),
    FOREIGN KEY (id_product) REFERENCES PRODUCT(id_product)
);
GO

CREATE TABLE BILL
(
    id_bill INT IDENTITY NOT NULL,
	id_customer INT NOT NULL,
    id_product INT NOT NULL,
    quantity INT NOT NULL,
	status INT NOT NULL,
	payments INT NOT NULL,
    create_date DATETIME NOT NULL,
    ship_date DATETIME NOT NULL,
    CONSTRAINT PK_BILL_id_bill PRIMARY KEY (id_bill),
	FOREIGN KEY (id_customer) REFERENCES CUSTOMER(id_customer),
    FOREIGN KEY (id_product) REFERENCES PRODUCT(id_product)
);
GO

CREATE TABLE FAVORITE
(
    id_favorite INT IDENTITY NOT NULL,
    id_customer INT NOT NULL,
    id_product INT NOT NULL,
    active BIT NOT NULL,
    CONSTRAINT PK_FAVORITE_id_favorite PRIMARY KEY (id_favorite),
    FOREIGN KEY (id_customer) REFERENCES CUSTOMER(id_customer),
    FOREIGN KEY (id_product) REFERENCES PRODUCT(id_product)
);
GO

CREATE TABLE COMMENT
(
    id_comment INT IDENTITY NOT NULL,
    id_customer INT NOT NULL,
    id_product INT NOT NULL,
    content NVARCHAR(500) NOT NULL,
    star INT DEFAULT 0 NOT NULL,
    CONSTRAINT PK_COMMENT_id_comment PRIMARY KEY (id_comment),
    FOREIGN KEY (id_customer) REFERENCES CUSTOMER(id_customer),
    FOREIGN KEY (id_product) REFERENCES PRODUCT(id_product)
);
GO

CREATE TABLE HISTORY
(
    id_history INT IDENTITY NOT NULL,
    id_customer INT NOT NULL,
    id_product INT NOT NULL,
    CONSTRAINT PK_HISTORY_id_history PRIMARY KEY (id_history),
    FOREIGN KEY (id_customer) REFERENCES CUSTOMER(id_customer),
    FOREIGN KEY (id_product) REFERENCES PRODUCT(id_product)
);
GO