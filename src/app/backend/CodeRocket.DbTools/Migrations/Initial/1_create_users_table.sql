-- Users table creation script for PostgreSQL
-- This script creates the users table that matches the User model in CodeRocket.Common

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    role INT NOT NULL DEFAULT 0, -- UserRole enum: 0=Guest, 1=User, 2=Moderator, 3=Administrator, 4=SuperAdmin
    email VARCHAR(255) NULL UNIQUE,
    telegram_id VARCHAR(50) NULL UNIQUE,
    discord_id VARCHAR(50) NULL UNIQUE,
    first_name VARCHAR(100) NULL,
    last_name VARCHAR(100) NULL,
    display_name VARCHAR(100) NULL,
    created_by VARCHAR(100) NULL,
    updated_by VARCHAR(100) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_telegram_id ON users(telegram_id);
CREATE INDEX IF NOT EXISTS idx_users_discord_id ON users(discord_id);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_users_is_deleted ON users(is_deleted);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(created_at);

-- Create updated_at trigger function
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for automatic updated_at update
DROP TRIGGER IF EXISTS update_users_updated_at ON users;
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Insert test data
INSERT INTO users (role, email, first_name, last_name, display_name, created_by, updated_by) 
VALUES 
    (4, 'admin@coderocket.com', 'Super', 'Admin', 'SuperAdmin', 'system', 'system'),
    (2, 'moderator@example.com', 'Jane', 'Smith', 'JaneSmith', 'admin@coderocket.com', 'admin@coderocket.com'),
    (1, 'user@example.com', 'John', 'Doe', 'JohnDoe', 'admin@coderocket.com', 'admin@coderocket.com')
ON CONFLICT (email) DO NOTHING;
