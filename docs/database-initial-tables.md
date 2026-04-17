# EstateIQ Initial Database Tables

This document defines the first database draft for EstateIQ. These 24 tables will be used as the starting point for the initial implementation and can be refined later as the product grows.

## Scope

- Total tables: 24
- Includes the 10 mandatory system tables
- Covers property management for sale and rent
- Covers map and location support
- Covers user interaction flows
- Supports dashboard and statistics needs

## Mandatory Tables

### `Users`
Stores user accounts and core identity data.

### `Roles`
Defines system roles such as admin, agent, or client.

### `UserRoles`
Maps users to one or more roles.

### `Permissions`
Defines permission-level actions in the system.

### `RolePermissions`
Maps permissions to roles.

### `RefreshTokens`
Stores refresh tokens for authenticated sessions.

### `AuditLogs`
Tracks important system and user actions.

### `Notifications`
Stores in-app notifications and alerts.

### `Settings`
Stores configurable system or user-level settings.

### `Files`
Stores uploaded file metadata used across modules.

## Core Real Estate Tables

### `Properties`
Main property record with core business data.

### `PropertyTypes`
Defines property categories such as apartment, house, land, or commercial.

### `PropertyStatuses`
Defines operational status such as available, sold, rented, or draft.

### `ListingTypes`
Defines listing intent such as sale or rent.

### `PropertyListings`
Represents the publishable listing data connected to a property.

### `PropertyImages`
Stores property image references and display order.

### `Locations`
Stores structured location data for properties and map usage.

### `PropertyFeatures`
Stores property attributes and feature values.

## User Interaction and Business Tables

### `Favorites`
Stores saved properties for users.

### `Inquiries`
Stores lead and contact requests related to properties.

### `Messages`
Stores messages exchanged between users.

### `MessageThreads`
Groups messages into conversation threads.

### `Appointments`
Stores property visit bookings and meeting schedules.

### `PropertyViews`
Stores property view activity for analytics and reporting.

## Why These 24 Tables

These tables were selected because they give a practical and balanced starting point:

- They include the 10 mandatory platform tables
- They cover the main real estate domain cleanly
- They support both sale and rent flows
- They support map and location-based functionality
- They support user interaction and lead handling
- They provide enough structure for dashboard metrics and statistics

## Initial Note

This is the starting schema draft. During implementation, the structure may be extended with extra lookup tables, junction tables, or module-specific tables if new business requirements appear.
