# UnitsNet Serialization Examples

This project demonstrates how to use UnitsNet with ASP.NET Core for serializing and deserializing units of measurement using custom converters like `AbbreviatedUnitsConverter` and `UnitsNetIQuantityJsonConverter`.

## Project Overview

The project provides a set of API endpoints to handle operations related to UnitsNet quantities and conversions. It includes functionalities such as retrieving default values, constructing quantities, and performing conversions between different units of measurement.

### Key Features:

- **Serialization and Deserialization**: Demonstrates how to serialize and deserialize UnitsNet types using custom JSON converters.
- **API Endpoints**: Provides RESTful API endpoints for various operations on UnitsNet quantities.
- **Units Conversion**: Supports conversion between different units of measurement using UnitsNet.

## Project Structure

- **ASP.Net.UnitsNetSerializationExamples.csproj**: Project file containing package references and project settings.
- **Program.cs**: Configures the ASP.NET Core application, including JSON serialization settings.
- **Controllers/UnitsNetConverterController.cs**: API controller providing endpoints for working with UnitsNet types.

## API Endpoints

### 1. Retrieve Default Mass Value

- **Endpoint**: `GET /mass-unitZero`
- **Description**: Returns the default mass value represented as `Mass.Zero`.

### 2. Construct a Quantity

- **Endpoint**: `GET /construct-quantity`
- **Description**: Creates a quantity based on the provided quantity name, unit name, and value.

### 3. Convert Quantity Using Unit Abbreviation

- **Endpoint**: `POST /convert-to-unit-with-abbreviation`
- **Description**: Converts a quantity to a specified unit using the unit abbreviation.

### 4. Convert Quantity Using Full Type of Unit

- **Endpoint**: `POST /convert-to-unit-with-toFullTypeOfUnit`
- **Description**: Converts a quantity to a specified unit using the full type of the unit.

### 5. Convert Mass and Volume to Density

- **Endpoint**: `GET /conversion-from-massUnit-volumeUnit-to-density`
- **Description**: Converts mass and volume values to density.

### 6. Convert Density Unit

- **Endpoint**: `POST /convert-density-unit`
- **Description**: Converts a density value to a specified density unit.