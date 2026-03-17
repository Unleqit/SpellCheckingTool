# Start from Arch Linux
FROM archlinux:latest

# Set working directory
WORKDIR /app

# Install base dependencies and .NET SDK
RUN pacman -Syyu --noconfirm \
    && pacman -S --noconfirm dotnet-sdk-8.0 git unzip tar curl

# Copy csproj first to cache restore
COPY SpellCheckingTool.sln ./
COPY SpellCheckingTool.Application/*.csproj ./SpellCheckingTool.Application/
COPY SpellCheckingTool.Presentation/*.csproj ./SpellCheckingTool.Presentation/
COPY SpellCheckingTool.Domain/*.csproj ./SpellCheckingTool.Domain/
COPY SpellCheckingTool.Infrastructure/*.csproj ./SpellCheckingTool.Infrastructure/
COPY TestProject/*.csproj ./TestProject/


RUN dotnet restore SpellCheckingTool.sln
# Restore dependencies
RUN dotnet restore

# Copy the rest of the project
COPY . ./

# Build the project
RUN dotnet build -c Release

# Run the app when container starts
CMD ["dotnet", "run", "--no-build", "--project", "SpellCheckingTool.Presentation"]
