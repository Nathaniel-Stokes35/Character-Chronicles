# Documentation of the project

There is also a FAQ section on the page that works as an user guide.

# D&D Character Chronicles

D&D Character Chronicles is a web application for creating, organizing, and managing Dungeons & Dragons 5th Edition characters.

The application provides players with a centralized character roster where characters can be created, viewed, edited, and managed. Character data is stored persistently in a PostgreSQL database and presented through a responsive fantasy-themed interface.

## Group Members

- Kevin Bezerra
- Nathaniel Stokes
- Samuel Apusiyine Avike
- Vinicius Eduardo Rocca
- Jayce Odin Nephi Brown

## Features

### Character Management

- Create new D&D 5th Edition characters
- View existing characters as character cards
- Edit character information through an interactive character sheet
- Delete characters
- Track character status and campaign assignment
- Automatically display when a character was last updated

### Ability Score Generation

Character Chronicles includes a built-in D&D ability score roller.

During character creation, the application:

1. Rolls four six-sided dice (4d6)
2. Removes the lowest die
3. Adds the remaining three dice together
4. Repeats the process six times
5. Allows the player to assign the resulting scores to:
   - Strength
   - Dexterity
   - Constitution
   - Intelligence
   - Wisdom
   - Charisma

Once the character has been created, these rolled ability scores are displayed as read-only values on the character sheet.

### Character Sheets

Each character has an editable character sheet containing information such as:

- Character name
- Class
- Level
- Ancestry
- Background
- Alignment
- Armor Class
- Hit Points
- Ability Scores and Modifiers
- Personality Traits
- Ideals
- Bonds
- Flaws
- Equipment
- Features and Traits

### Campaign Support

Characters can be associated with campaigns, allowing character records to be organized around the adventures in which they participate.

The data model also provides the foundation for campaign-specific custom character metrics.

## Technology Stack

D&D Character Chronicles is built using:

- C#
- .NET 10
- ASP.NET Core
- Blazor / Razor Components
- Entity Framework Core
- PostgreSQL
- Npgsql
- ASP.NET Core Identity
- HTML
- CSS

## Database

The application uses PostgreSQL for persistent storage.

Entity Framework Core is used to manage the application's database models, relationships, and migrations.

Current application data includes:

- Users
- Characters
- Campaigns
- Character metric definitions
- Campaign metric definitions
- Character metric values

## Running the Project Locally

### Prerequisites

Make sure the following are installed:

- .NET 10 SDK
- Entity Framework Core CLI tools
- Access to a PostgreSQL database

### Environment Variables

Create a `.env` file or configure the following environment variable:

```env
DATABASE_URL=your_postgresql_connection_string
