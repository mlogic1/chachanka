# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY Chachanka/Chachanka.csproj ./Chachanka/
RUN dotnet restore ./Chachanka/Chachanka.csproj

# Copy the rest of the application files
COPY Chachanka/ ./Chachanka/

# Build the application
RUN dotnet publish ./Chachanka/Chachanka.csproj -c Release -o /app/publish

# Use the official .NET runtime image as the runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build output from the build stage

COPY --from=build /app/publish ./

# Run the application
ENTRYPOINT ["dotnet", "Chachanka.dll"]