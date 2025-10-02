-- Fix duplicate category issue
-- This script removes duplicate categories and keeps only the first one

-- First, let's see what categories exist
SELECT Id, Name, Description, SortOrder, IsActive, CreationTime
FROM RestaurantCategories
ORDER BY Name, CreationTime;

-- Remove duplicates, keeping the oldest one (first created)
WITH DuplicateCategories AS (
    SELECT Id, Name,
           ROW_NUMBER() OVER (PARTITION BY Name ORDER BY CreationTime ASC) as RowNum
    FROM RestaurantCategories
)
DELETE FROM RestaurantCategories 
WHERE Id IN (
    SELECT Id 
    FROM DuplicateCategories 
    WHERE RowNum > 1
);

-- Verify the fix
SELECT Id, Name, Description, SortOrder, IsActive, CreationTime
FROM RestaurantCategories
ORDER BY Name, CreationTime;

