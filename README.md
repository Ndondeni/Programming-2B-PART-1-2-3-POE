# Marathon Event Management System

## About

The Marathon Event Management System is an application for managing marathon events and the people who take part in them. It brings event information, user roles, categories, enrolments and race results together in one system.

## Features

- Manage users and user roles
- Create and manage marathon events
- Create event categories
- Register participants for events
- Manage participant enrolments
- Link participants to events
- Record and view event results

## Technologies

- C#
- ASP.NET Core
- MySQL
- MySQL Workbench
- Docker
- Azurite Storage Emulator

## Database

The database is named `EventManagementDB`.
But renamed to 'Marathon'

To create the database in MySQL Workbench, run:

```sql
CREATE DATABASE EventManagementDB;

USE EventManagementDB;
```

### Database tables

The system uses the following tables:

| Table | Purpose |
|---|---|
| `Users` | Stores user and participant account information. |
| `Roles` | Stores the roles available in the system. |
| `UserRoles` | Links users to their assigned roles. |
| `Events` | Stores marathon event details. |
| `Categories` | Stores the categories available for events. |
| `Enrolments` | Stores participant registrations/enrolments. |
| `ParticipantEvents` | Links participants to the events they enter. |
| `Results` | Stores participant event results. |

## Entity Relationship Diagram (ERD)

The ERD below shows the main tables and their conceptual relationships.

![Marathon Event Management System ERD](docs/images/marathon-erd.png)

The diagram image is stored at `docs/images/marathon-erd.png`. Keep this folder structure when adding the README and diagram to the repository so the image displays correctly on GitHub.

**Note:** The diagram is a conceptual overview based on the table list above. Make sure the foreign keys and relationship cardinalities match your final SQL script.

## Getting Started

1. Open the solution in Visual Studio.
2. Open MySQL Workbench and connect to your MySQL server.
3. Create the `EventManagementDB` database.
4. Run the project's SQL script to create the tables.
5. Update the application's database connection string in the configuration file.
6. Build and run the ASP.NET Core application.

## Project Purpose

The purpose of this project is to make marathon event administration easier by keeping user, event, category, enrolment and result information organised in one system.
