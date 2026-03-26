FROM archlinux:latest

WORKDIR /app

RUN pacman -Syyu --noconfirm && pacman -S --noconfirm dotnet-sdk-8.0 git unzip tar curl nano

COPY SpellCheckingTool.sln ./
COPY SpellCheckingTool.Application/*.csproj ./SpellCheckingTool.Application/
COPY SpellCheckingTool.Presentation/*.csproj ./SpellCheckingTool.Presentation/
COPY SpellCheckingTool.Domain/*.csproj ./SpellCheckingTool.Domain/
COPY SpellCheckingTool.Infrastructure/*.csproj ./SpellCheckingTool.Infrastructure/
COPY TestProject/*.csproj ./TestProject/

RUN dotnet restore SpellCheckingTool.sln
RUN dotnet restore

COPY . ./

CMD ["/bin/bash", "-c", "dotnet build && clear && dotnet run --no-build --project SpellCheckingTool.Presentation"]