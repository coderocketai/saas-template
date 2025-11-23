-- Users table creation script for MariaDB
-- This script creates the users table that matches the User model in CodeRocket.Common

CREATE TABLE IF NOT EXISTS users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Role INT NOT NULL DEFAULT 0 COMMENT 'UserRole enum: 0=Guest, 1=User, 2=Moderator, 3=Administrator, 4=SuperAdmin',
    Email VARCHAR(255) NULL UNIQUE,
    TelegramId VARCHAR(50) NULL UNIQUE,
    DiscordId VARCHAR(50) NULL UNIQUE,
    FirstName VARCHAR(100) NULL,
    LastName VARCHAR(100) NULL,
    DisplayName VARCHAR(100) NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    INDEX idx_users_email (Email),
    INDEX idx_users_telegram_id (TelegramId),
    INDEX idx_users_discord_id (DiscordId),
    INDEX idx_users_role (Role),
    INDEX idx_users_is_deleted (IsDeleted),
    INDEX idx_users_created_at (CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert test data
INSERT INTO users (Role, Email, FirstName, LastName, DisplayName, CreatedBy, UpdatedBy) 
VALUES 
    (4, 'admin@coderocket.com', 'Super', 'Admin', 'SuperAdmin', 'system', 'system'),
    (1, 'user@example.com', 'John', 'Doe', 'JohnDoe', 'admin@coderocket.com', 'admin@coderocket.com'),
    (2, 'moderator@example.com', 'Jane', 'Smith', 'JaneSmith', 'admin@coderocket.com', 'admin@coderocket.com');
